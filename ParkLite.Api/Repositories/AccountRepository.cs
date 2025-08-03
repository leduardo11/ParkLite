using Microsoft.Data.Sqlite;
using ParkLite.Api.Helpers;
using ParkLite.Api.Interfaces;
using ParkLite.Api.Models;

namespace ParkLite.Api.Repositories;

public class AccountRepository(SqliteConnection connection) : IAccountRepository
{
	private readonly SqliteConnection _conn = connection;

	public async Task<Account?> GetByIdAsync(int id)
	{
		using var cmd = SqliteHelper.CreateCommand(_conn, """
			SELECT 
				a.Id, a.Name, a.IsActive,
				c.Id, c.Name, c.Phone, c.Email,
				v.Id, v.Plate, v.Model, v.Photo
			FROM Accounts a
			LEFT JOIN Contacts c ON c.AccountId = a.Id
			LEFT JOIN Vehicles v ON v.AccountId = a.Id
			WHERE a.Id = $id
		""");
		cmd.AddParameter("$id", id);

		using var reader = await cmd.ExecuteReaderAsync();

		Account? account = null;

		while (await reader.ReadAsync())
		{
			account ??= SqliteHelper.CreateAccountFromReader(reader);

			var contact = SqliteHelper.CreateContactFromReader(reader);
			if (contact != null && !account.Contacts.Any(c => c.Id == contact.Id))
				account.Contacts.Add(contact);

			var vehicle = SqliteHelper.CreateVehicleFromReader(reader);
			if (vehicle != null && !account.Vehicles.Any(v => v.Id == vehicle.Id))
				account.Vehicles.Add(vehicle);
		}

		return account;
	}

	public async Task<PaginatedResult<Account>> GetPaginatedAccountsAsync(int limit, int offset, string? search = null)
	{
		var result = new PaginatedResult<Account>();

		using var countCmd = SqliteHelper.CreateCommand(_conn,
			search == null
				? "SELECT COUNT(*) FROM Accounts"
				: "SELECT COUNT(*) FROM Accounts WHERE Name LIKE $search");

		if (search != null) countCmd.AddParameter("$search", $"%{search}%");
		result.Total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

		using var cmd = SqliteHelper.CreateCommand(_conn, $"""
	    SELECT 
	    a.Id, a.Name, a.IsActive,
	    c.Id, c.Name, c.Phone, c.Email,
	    v.Id, v.Plate, v.Model, v.Photo
        FROM (
        SELECT * FROM Accounts
	    {(search != null ? "WHERE Name LIKE $search" : "")}
	    ORDER BY Id
	    LIMIT $limit OFFSET $offset
       ) AS a
        LEFT JOIN Contacts c ON c.AccountId = a.Id
        LEFT JOIN Vehicles v ON v.AccountId = a.Id

""");

		if (search != null) cmd.AddParameter("$search", $"%{search}%");
		cmd.AddParameter("$limit", limit);
		cmd.AddParameter("$offset", offset);

		using var reader = await cmd.ExecuteReaderAsync();

		var accounts = new Dictionary<int, Account>();

		while (await reader.ReadAsync())
		{
			var accountId = reader.GetInt32((int)SqlColumnIndex.AccountId);

			if (!accounts.TryGetValue(accountId, out var account))
			{
				account = SqliteHelper.CreateAccountFromReader(reader);
				accounts[accountId] = account;
			}

			var contact = SqliteHelper.CreateContactFromReader(reader);
			if (contact != null && !account.Contacts.Any(c => c.Id == contact.Id))
				account.Contacts.Add(contact);

			var vehicle = SqliteHelper.CreateVehicleFromReader(reader);
			if (vehicle != null && !account.Vehicles.Any(v => v.Id == vehicle.Id))
				account.Vehicles.Add(vehicle);
		}

		result.Result = accounts.Values;
		return result;
	}

	public async Task AddAsync(Account account)
	{
		using var transaction = _conn.BeginTransaction();

		using var cmd = SqliteHelper.CreateCommand(_conn, "INSERT INTO Accounts (Name, IsActive) VALUES ($name, $isActive)", transaction);
		cmd.AddParameter("$name", account.Name);
		cmd.AddParameter("$isActive", account.IsActive ? 1 : 0);
		await cmd.ExecuteNonQueryAsync();

		account.Id = await SqliteHelper.GetLastInsertRowIdAsync(_conn, transaction);
		await SaveRelatedEntitiesAsync(account, transaction);

		await transaction.CommitAsync();
	}

	public async Task UpdateAsync(Account account)
	{
		using var transaction = _conn.BeginTransaction();

		using var cmd = SqliteHelper.CreateCommand(_conn, "UPDATE Accounts SET Name = $name, IsActive = $isActive WHERE Id = $id", transaction);
		cmd.AddParameter("$name", account.Name);
		cmd.AddParameter("$isActive", account.IsActive ? 1 : 0);
		cmd.AddParameter("$id", account.Id);
		await cmd.ExecuteNonQueryAsync();

		using (var deleteContactsCmd = SqliteHelper.CreateCommand(_conn, "DELETE FROM Contacts WHERE AccountId = $accountId", transaction))
		{
			deleteContactsCmd.AddParameter("$accountId", account.Id);
			await deleteContactsCmd.ExecuteNonQueryAsync();
		}

		using (var deleteVehiclesCmd = SqliteHelper.CreateCommand(_conn, "DELETE FROM Vehicles WHERE AccountId = $accountId", transaction))
		{
			deleteVehiclesCmd.AddParameter("$accountId", account.Id);
			await deleteVehiclesCmd.ExecuteNonQueryAsync();
		}

		await SaveRelatedEntitiesAsync(account, transaction);
		await transaction.CommitAsync();
	}

	public async Task DeleteAsync(int id)
	{
		using var cmd = SqliteHelper.CreateCommand(_conn, "DELETE FROM Accounts WHERE Id = $id");
		cmd.AddParameter("$id", id);
		await cmd.ExecuteNonQueryAsync();
	}

	private async Task SaveRelatedEntitiesAsync(Account account, SqliteTransaction transaction)
	{
		foreach (var contact in account.Contacts)
		{
			using var contactCmd = SqliteHelper.CreateCommand(_conn, "INSERT INTO Contacts (AccountId, Name, Phone, Email) VALUES ($accountId, $name, $phone, $email)", transaction);
			contactCmd.AddParameter("$accountId", account.Id);
			contactCmd.AddParameter("$name", contact.Name);
			contactCmd.AddParameter("$phone", contact.Phone);
			contactCmd.AddParameter("$email", contact.Email);
			await contactCmd.ExecuteNonQueryAsync();

			contact.Id = await SqliteHelper.GetLastInsertRowIdAsync(_conn, transaction);
		}

		foreach (var vehicle in account.Vehicles)
		{
			using var vehicleCmd = SqliteHelper.CreateCommand(_conn, "INSERT INTO Vehicles (AccountId, Plate, Model, Photo) VALUES ($accountId, $plate, $model, $photo)", transaction);
			vehicleCmd.AddParameter("$accountId", account.Id);
			vehicleCmd.AddParameter("$plate", vehicle.Plate);
			vehicleCmd.AddParameter("$model", vehicle.Model);
			vehicleCmd.AddParameter("$photo", vehicle.Photo ?? []);
			await vehicleCmd.ExecuteNonQueryAsync();

			vehicle.Id = await SqliteHelper.GetLastInsertRowIdAsync(_conn, transaction);
		}
	}

	public async Task BatchDeactivateInactiveAccountsAsync(int batchSize = 50, int delayMs = 1000)
	{
		if (batchSize <= 0)
			return;

		const int maxBatches = 100;
		int batchCount = 0;
		int affectedRows;

		do
		{
			if (++batchCount > maxBatches)
				break;

			using var transaction = _conn.BeginTransaction();
			using var cmd = SqliteHelper.CreateCommand(_conn, """
			WITH cte AS (
				SELECT Id FROM Accounts
				WHERE IsActive = 1
				ORDER BY Id
				LIMIT $batchSize
			)
			UPDATE Accounts
			SET IsActive = 0
			WHERE Id IN (SELECT Id FROM cte);
		""", transaction);
			cmd.AddParameter("$batchSize", batchSize);

			affectedRows = await cmd.ExecuteNonQueryAsync();
			await transaction.CommitAsync();

			if (affectedRows > 0)
				await Task.Delay(delayMs);

		} while (affectedRows > 0);
	}
}