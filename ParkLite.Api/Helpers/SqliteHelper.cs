using Microsoft.Data.Sqlite;
using ParkLite.Api.Models;

namespace ParkLite.Api.Helpers;

public static class SqliteHelper
{
	public static async Task<int> GetLastInsertRowIdAsync(SqliteConnection conn, SqliteTransaction? transaction = null)
	{
		using var cmd = conn.CreateCommand();
		cmd.CommandText = "SELECT last_insert_rowid()";
		if (transaction != null)
			cmd.Transaction = transaction;
		var result = await cmd.ExecuteScalarAsync();
		return (int)(long)result!;
	}

	public static SqliteCommand CreateCommand(SqliteConnection conn, string commandText, SqliteTransaction? transaction = null)
	{
		var cmd = conn.CreateCommand();
		cmd.CommandText = commandText;
		if (transaction != null)
			cmd.Transaction = transaction;
		return cmd;
	}

	public static void AddParameter(this SqliteCommand cmd, string name, object? value)
	{
		cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
	}

	public static Account CreateAccountFromReader(SqliteDataReader reader)
	{
		return new(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2) == 1);
	}

	public static Contact? CreateContactFromReader(SqliteDataReader reader)
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

	public static Vehicle? CreateVehicleFromReader(SqliteDataReader reader)
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

}
