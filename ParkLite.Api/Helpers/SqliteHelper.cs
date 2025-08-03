using Microsoft.Data.Sqlite;
using ParkLite.Api.Dtos;
using ParkLite.Api.Models;

namespace ParkLite.Api.Helpers
{
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
			return new Account(
				reader.GetInt32(0),
				reader.GetString(1),
				reader.GetInt32(2) == 1
			)
			{
				Contacts = [],
				Vehicles = []
			};
		}

		public static Contact? CreateContactFromReader(SqliteDataReader reader)
		{
			if (reader.IsDBNull(3))
				return null;

			return new Contact
			{
				Id = reader.GetInt32(3),
				AccountId = reader.GetInt32(0),
				Name = reader.GetString(4),
				Phone = reader.IsDBNull(5) ? null : reader.GetString(5),
				Email = reader.IsDBNull(6) ? null : reader.GetString(6),
			};
		}

		public static Vehicle? CreateVehicleFromReader(SqliteDataReader reader)
		{
			if (reader.IsDBNull(6))
				return null;

			return new Vehicle
			{
				Id = reader.GetInt32(6),
				AccountId = reader.GetInt32(0),
				Plate = reader.GetString(7),
				Model = reader.IsDBNull(8) ? null : reader.GetString(8),
				Photo = reader.IsDBNull(9) ? null : TryParseBase64(reader.GetString(9)),
			};
		}

		public static string? ConvertPhotoToBase64(byte[]? photo)
		{
			if (photo == null)
				return null;
			return Convert.ToBase64String(photo);
		}

		public static byte[]? ConvertPhotoFromBase64(string? base64)
		{
			if (string.IsNullOrEmpty(base64))
				return null;
			return Convert.FromBase64String(base64);
		}

		public static Vehicle MapDtoToVehicle(VehicleDTO dto)
		{
			return new Vehicle
			{
				Id = dto.Id,
				AccountId = dto.AccountId,
				Plate = dto.Plate,
				Model = dto.Model,
				Photo = ConvertPhotoFromBase64(dto.Photo)
			};
		}

		public static VehicleDTO MapVehicleToDTO(Vehicle v) => new()
		{
			Id = v.Id,
			Plate = v.Plate,
			Model = v.Model,
			Photo = v.Photo != null ? Convert.ToBase64String(v.Photo) : null
		};

		public static AccountDTO MapAccountToDTO(Account account)
		{
			return new AccountDTO
			{
				Id = account.Id,
				Name = account.Name,
				IsActive = account.IsActive,
				Contacts = account.Contacts,
				Vehicles = [.. account.Vehicles.Select(MapVehicleToDTO)]
			};
		}

		public static Account MapDTOToAccount(AccountDTO dto)
		{
			return new Account(dto.Id, dto.Name, dto.IsActive)
			{
				Contacts = dto.Contacts,
				Vehicles = [.. dto.Vehicles.Select(MapDtoToVehicle)]
			};
		}

		public static byte[]? TryParseBase64(string? input)
		{
			if (string.IsNullOrWhiteSpace(input)) return null;

			try
			{
				var parts = input.Split(',');
				var base64 = parts.Length > 1 ? parts[1] : parts[0];
				return Convert.FromBase64String(base64);
			}
			catch
			{
				return null;
			}
		}
	}
}
