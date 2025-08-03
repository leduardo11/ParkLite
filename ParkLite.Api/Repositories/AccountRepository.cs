using Microsoft.Data.Sqlite;
using ParkLite.Api.Interfaces;
using ParkLite.Api.Models;

namespace ParkLite.Api.Repositories;

public class AccountRepository(SqliteConnection connection) : IAccountRepository
{
	private readonly SqliteConnection _conn = connection;

	private static Account CreateAccountFromReader(SqliteDataReader reader)
		=> new(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2) == 1);

	private static Contact? CreateContactFromReader(SqliteDataReader reader)
	{
		if (reader.IsDBNull(3)) return null;

		return new Contact
		{
			Id = reader.GetInt32(3),
			AccountId = reader.GetInt32(0),
			Name = reader.GetString(4),
			Phone = reader.IsDBNull(5) ? null : reader.GetString(5),
		};
	}

	private static Vehicle? CreateVehicleFromReader(SqliteDataReader reader)
	{
		if (reader.IsDBNull(6)) return null;

		return new Vehicle
		{
			Id = reader.GetInt32(6),
			AccountId = reader.GetInt32(0),
			Plate = reader.GetString(7),
			Model = reader.IsDBNull(8) ? null : reader.GetString(8),
		};
	}

	private static async Task<int> GetLastInsertRowIdAsync(SqliteConnection conn, SqliteTransaction? transaction = null)
	{
		using var cmd = conn.CreateCommand();
		cmd.CommandText = "SELECT last_insert_rowid()";
		if (transaction != null)
			cmd.Transaction = transaction;
		var result = await cmd.ExecuteScalarAsync();
		return (int)(long)result!;
	}

	public async Task<Account?> GetByIdAsync(int id)
	{
		using var cmd = _conn.CreateCommand();
		cmd.CommandText = """
			SELECT 
				a.Id AS AccountId, a.Name AS AccountName, a.IsActive,
				c.Id AS ContactId, c.Name AS ContactName, c.Phone,
				v.Id AS VehicleId, v.Plate, v.Model
			FROM Accounts a
			LEFT JOIN Contacts c ON c.AccountId = a.Id
			LEFT JOIN Vehicles v ON v.AccountId = a.Id
			WHERE a.Id = $id
		""";
		cmd.Parameters.AddWithValue("$id", id);

		using var reader = await cmd.ExecuteReaderAsync();

		Account? account = null;

		while (await reader.ReadAsync())
		{
			account ??= CreateAccountFromReader(reader);

			var contact = CreateContactFromReader(reader);
			if (contact != null && !account.Contacts.Any(c => c.Id == contact.Id))
				account.Contacts.Add(contact);

			var vehicle = CreateVehicleFromReader(reader);
			if (vehicle != null && !account.Vehicles.Any(v => v.Id == vehicle.Id))
				account.Vehicles.Add(vehicle);
		}

		return account;
	}

	public async Task<IEnumerable<Account>> GetAllAsync()
	{
		using var cmd = _conn.CreateCommand();
		cmd.CommandText = """
			SELECT 
				a.Id AS AccountId, a.Name AS AccountName, a.IsActive,
				c.Id AS ContactId, c.Name AS ContactName, c.Phone,
				v.Id AS VehicleId, v.Plate, v.Model
			FROM Accounts a
			LEFT JOIN Contacts c ON c.AccountId = a.Id
			LEFT JOIN Vehicles v ON v.AccountId = a.Id
			ORDER BY a.Id
		""";

		using var reader = await cmd.ExecuteReaderAsync();

		var accounts = new Dictionary<int, Account>();

		while (await reader.ReadAsync())
		{
			var accountId = reader.GetInt32(0);

			if (!accounts.TryGetValue(accountId, out var account))
			{
				account = CreateAccountFromReader(reader);
				accounts[accountId] = account;
			}

			var contact = CreateContactFromReader(reader);
			if (contact != null && !account!.Contacts.Any(c => c.Id == contact.Id))
				account.Contacts.Add(contact);

			var vehicle = CreateVehicleFromReader(reader);
			if (vehicle != null && !account!.Vehicles.Any(v => v.Id == vehicle.Id))
				account.Vehicles.Add(vehicle);
		}

		return accounts.Values;
	}

	public async Task AddAsync(Account account)
	{
		using var transaction = _conn.BeginTransaction();

		using var cmd = _conn.CreateCommand();
		cmd.CommandText = "INSERT INTO Accounts (Name, IsActive) VALUES ($name, $isActive)";
		cmd.Parameters.AddWithValue("$name", account.Name);
		cmd.Parameters.AddWithValue("$isActive", account.IsActive ? 1 : 0);
		cmd.Transaction = transaction;
		await cmd.ExecuteNonQueryAsync();

		account.Id = await GetLastInsertRowIdAsync(_conn, transaction);
		await SaveRelatedEntitiesAsync(account, transaction);

		await transaction.CommitAsync();
	}

	public async Task UpdateAsync(Account account)
	{
		using var transaction = _conn.BeginTransaction();

		using var cmd = _conn.CreateCommand();
		cmd.CommandText = "UPDATE Accounts SET Name = $name, IsActive = $isActive WHERE Id = $id";
		cmd.Parameters.AddWithValue("$name", account.Name);
		cmd.Parameters.AddWithValue("$isActive", account.IsActive ? 1 : 0);
		cmd.Parameters.AddWithValue("$id", account.Id);
		cmd.Transaction = transaction;
		await cmd.ExecuteNonQueryAsync();

		using (var deleteContactsCmd = _conn.CreateCommand())
		{
			deleteContactsCmd.CommandText = "DELETE FROM Contacts WHERE AccountId = $accountId";
			deleteContactsCmd.Parameters.AddWithValue("$accountId", account.Id);
			deleteContactsCmd.Transaction = transaction;
			await deleteContactsCmd.ExecuteNonQueryAsync();
		}

		using (var deleteVehiclesCmd = _conn.CreateCommand())
		{
			deleteVehiclesCmd.CommandText = "DELETE FROM Vehicles WHERE AccountId = $accountId";
			deleteVehiclesCmd.Parameters.AddWithValue("$accountId", account.Id);
			deleteVehiclesCmd.Transaction = transaction;
			await deleteVehiclesCmd.ExecuteNonQueryAsync();
		}

		await SaveRelatedEntitiesAsync(account, transaction);
		await transaction.CommitAsync();
	}

	public async Task DeleteAsync(int id)
	{
		using var cmd = _conn.CreateCommand();
		cmd.CommandText = "DELETE FROM Accounts WHERE Id = $id";
		cmd.Parameters.AddWithValue("$id", id);
		await cmd.ExecuteNonQueryAsync();
	}

	public async Task BatchDeactivateInactiveAccountsAsync(int batchSize = 50, int delayMs = 1000)
	{
		if (batchSize <= 0)
			return;

		int affectedRows;
		do
		{
			using var transaction = _conn.BeginTransaction();
			using var cmd = _conn.CreateCommand();
			cmd.Transaction = transaction;

			cmd.CommandText = """
				WITH cte AS (
					SELECT Id FROM Accounts
					WHERE IsActive = 1
					ORDER BY Id
					LIMIT $batchSize
				)
				UPDATE Accounts
				SET IsActive = 0
				WHERE Id IN (SELECT Id FROM cte);
			""";
			cmd.Parameters.AddWithValue("$batchSize", batchSize);

			affectedRows = await cmd.ExecuteNonQueryAsync();
			await transaction.CommitAsync();

			if (affectedRows > 0)
				await Task.Delay(delayMs);

		} while (affectedRows > 0);
	}

	private async Task SaveRelatedEntitiesAsync(Account account, SqliteTransaction transaction)
	{
		foreach (var contact in account.Contacts)
		{
			using var contactCmd = _conn.CreateCommand();
			contactCmd.CommandText = "INSERT INTO Contacts (AccountId, Name, Phone) VALUES ($accountId, $name, $phone)";
			contactCmd.Parameters.AddWithValue("$accountId", account.Id);
			contactCmd.Parameters.AddWithValue("$name", contact.Name);
			contactCmd.Parameters.AddWithValue("$phone", (object?)contact.Phone ?? DBNull.Value);
			contactCmd.Transaction = transaction;
			await contactCmd.ExecuteNonQueryAsync();

			contact.Id = await GetLastInsertRowIdAsync(_conn, transaction);
		}

		foreach (var vehicle in account.Vehicles)
		{
			using var vehicleCmd = _conn.CreateCommand();
			vehicleCmd.CommandText = "INSERT INTO Vehicles (AccountId, Plate, Model) VALUES ($accountId, $plate, $model)";
			vehicleCmd.Parameters.AddWithValue("$accountId", account.Id);
			vehicleCmd.Parameters.AddWithValue("$plate", vehicle.Plate);
			vehicleCmd.Parameters.AddWithValue("$model", (object?)vehicle.Model ?? DBNull.Value);
			vehicleCmd.Transaction = transaction;
			await vehicleCmd.ExecuteNonQueryAsync();

			vehicle.Id = await GetLastInsertRowIdAsync(_conn, transaction);
		}
	}
}