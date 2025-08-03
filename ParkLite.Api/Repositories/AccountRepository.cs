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

	private static int GetLastInsertRowId(SqliteConnection conn, SqliteTransaction? transaction = null)
	{
		using var cmd = conn.CreateCommand();
		cmd.CommandText = "SELECT last_insert_rowid()";
		if (transaction != null)
			cmd.Transaction = transaction;
		return (int)(long)cmd.ExecuteScalar()!;
	}

	public Account? GetById(int id)
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

		using var reader = cmd.ExecuteReader();

		Account? account = null;

		while (reader.Read())
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

	public IEnumerable<Account> GetAll()
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

		using var reader = cmd.ExecuteReader();

		var accounts = new Dictionary<int, Account>();

		while (reader.Read())
		{
			var accountId = reader.GetInt32(0);

			if (!accounts.TryGetValue(accountId, out var account))
			{
				account = CreateAccountFromReader(reader);
				accounts[accountId] = account;
			}

			var contact = CreateContactFromReader(reader);
			if (contact != null && !account.Contacts.Any(c => c.Id == contact.Id))
				account.Contacts.Add(contact);

			var vehicle = CreateVehicleFromReader(reader);
			if (vehicle != null && !account.Vehicles.Any(v => v.Id == vehicle.Id))
				account.Vehicles.Add(vehicle);
		}

		return accounts.Values;
	}

	public void Add(Account account)
	{
		using var transaction = _conn.BeginTransaction();

		using var cmd = _conn.CreateCommand();
		cmd.CommandText = "INSERT INTO Accounts (Name, IsActive) VALUES ($name, $isActive)";
		cmd.Parameters.AddWithValue("$name", account.Name);
		cmd.Parameters.AddWithValue("$isActive", account.IsActive ? 1 : 0);
		cmd.Transaction = transaction;
		cmd.ExecuteNonQuery();

		account.Id = GetLastInsertRowId(_conn, transaction);

		foreach (var contact in account.Contacts)
		{
			using var contactCmd = _conn.CreateCommand();
			contactCmd.CommandText = "INSERT INTO Contacts (AccountId, Name, Phone) VALUES ($accountId, $name, $phone)";
			contactCmd.Parameters.AddWithValue("$accountId", account.Id);
			contactCmd.Parameters.AddWithValue("$name", contact.Name);
			contactCmd.Parameters.AddWithValue("$phone", (object?)contact.Phone ?? DBNull.Value);
			contactCmd.Transaction = transaction;
			contactCmd.ExecuteNonQuery();

			contact.Id = GetLastInsertRowId(_conn, transaction);
		}

		foreach (var vehicle in account.Vehicles)
		{
			using var vehicleCmd = _conn.CreateCommand();
			vehicleCmd.CommandText = "INSERT INTO Vehicles (AccountId, Plate, Model) VALUES ($accountId, $plate, $model)";
			vehicleCmd.Parameters.AddWithValue("$accountId", account.Id);
			vehicleCmd.Parameters.AddWithValue("$plate", vehicle.Plate);
			vehicleCmd.Parameters.AddWithValue("$model", (object?)vehicle.Model ?? DBNull.Value);
			vehicleCmd.Transaction = transaction;
			vehicleCmd.ExecuteNonQuery();

			vehicle.Id = GetLastInsertRowId(_conn, transaction);
		}

		transaction.Commit();
	}

	public void Update(Account account)
	{
		using var transaction = _conn.BeginTransaction();

		using var cmd = _conn.CreateCommand();
		cmd.CommandText = "UPDATE Accounts SET Name = $name, IsActive = $isActive WHERE Id = $id";
		cmd.Parameters.AddWithValue("$name", account.Name);
		cmd.Parameters.AddWithValue("$isActive", account.IsActive ? 1 : 0);
		cmd.Parameters.AddWithValue("$id", account.Id);
		cmd.Transaction = transaction;
		cmd.ExecuteNonQuery();

		using (var deleteContactsCmd = _conn.CreateCommand())
		{
			deleteContactsCmd.CommandText = "DELETE FROM Contacts WHERE AccountId = $accountId";
			deleteContactsCmd.Parameters.AddWithValue("$accountId", account.Id);
			deleteContactsCmd.Transaction = transaction;
			deleteContactsCmd.ExecuteNonQuery();
		}

		foreach (var contact in account.Contacts)
		{
			using var insertContactCmd = _conn.CreateCommand();
			insertContactCmd.CommandText = "INSERT INTO Contacts (AccountId, Name, Phone) VALUES ($accountId, $name, $phone)";
			insertContactCmd.Parameters.AddWithValue("$accountId", account.Id);
			insertContactCmd.Parameters.AddWithValue("$name", contact.Name);
			insertContactCmd.Parameters.AddWithValue("$phone", (object?)contact.Phone ?? DBNull.Value);
			insertContactCmd.Transaction = transaction;
			insertContactCmd.ExecuteNonQuery();

			contact.Id = GetLastInsertRowId(_conn, transaction);
		}

		using (var deleteVehiclesCmd = _conn.CreateCommand())
		{
			deleteVehiclesCmd.CommandText = "DELETE FROM Vehicles WHERE AccountId = $accountId";
			deleteVehiclesCmd.Parameters.AddWithValue("$accountId", account.Id);
			deleteVehiclesCmd.Transaction = transaction;
			deleteVehiclesCmd.ExecuteNonQuery();
		}

		foreach (var vehicle in account.Vehicles)
		{
			using var insertVehicleCmd = _conn.CreateCommand();
			insertVehicleCmd.CommandText = "INSERT INTO Vehicles (AccountId, Plate, Model) VALUES ($accountId, $plate, $model)";
			insertVehicleCmd.Parameters.AddWithValue("$accountId", account.Id);
			insertVehicleCmd.Parameters.AddWithValue("$plate", vehicle.Plate);
			insertVehicleCmd.Parameters.AddWithValue("$model", (object?)vehicle.Model ?? DBNull.Value);
			insertVehicleCmd.Transaction = transaction;
			insertVehicleCmd.ExecuteNonQuery();

			vehicle.Id = GetLastInsertRowId(_conn, transaction);
		}

		transaction.Commit();
	}

	public void Delete(int id)
	{
		using var cmd = _conn.CreateCommand();
		cmd.CommandText = "DELETE FROM Accounts WHERE Id = $id";
		cmd.Parameters.AddWithValue("$id", id);
		cmd.ExecuteNonQuery();
	}

	public void BatchDeactivateInactiveAccounts(int batchSize = 50, int delayMs = 1000)
	{
		int affectedRows;
		do
		{
			using var transaction = _conn.BeginTransaction();
			using var cmd = _conn.CreateCommand();
			cmd.Transaction = transaction;

			cmd.CommandText = $@"
            WITH cte AS (
                SELECT Id FROM Accounts
                WHERE IsActive = 1
                ORDER BY Id
                LIMIT {batchSize}
            )
            UPDATE Accounts
            SET IsActive = 0
            WHERE Id IN (SELECT Id FROM cte);
        ";

			affectedRows = cmd.ExecuteNonQuery();
			transaction.Commit();

			if (affectedRows > 0)
				Thread.Sleep(delayMs);

		} while (affectedRows > 0);
	}
}
