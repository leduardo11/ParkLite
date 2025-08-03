using Microsoft.Data.Sqlite;
using ParkLite.Api.Dtos;
using ParkLite.Api.Models;

namespace ParkLite.Api.Helpers
{
	public enum SqlColumnIndex
	{
		AccountId = 0,
		AccountName = 1,
		AccountIsActive = 2,
		ContactId = 3,
		ContactName = 4,
		ContactPhone = 5,
		ContactEmail = 6,
		VehicleId = 7,
		VehiclePlate = 8,
		VehicleModel = 9,
		VehiclePhoto = 10
	}

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
				reader.GetInt32((int)SqlColumnIndex.AccountId),
				reader.GetString((int)SqlColumnIndex.AccountName),
				reader.GetInt32((int)SqlColumnIndex.AccountIsActive) == 1
			)
			{
				Contacts = [],
				Vehicles = []
			};
		}

		public static Contact? CreateContactFromReader(SqliteDataReader reader)
		{
			if (reader.IsDBNull((int)SqlColumnIndex.ContactId))
				return null;

			return new Contact
			{
				Id = reader.GetInt32((int)SqlColumnIndex.ContactId),
				AccountId = reader.GetInt32((int)SqlColumnIndex.AccountId),
				Name = reader.GetString((int)SqlColumnIndex.ContactName),
				Phone = reader.IsDBNull((int)SqlColumnIndex.ContactPhone) ? null : reader.GetString((int)SqlColumnIndex.ContactPhone),
				Email = reader.IsDBNull((int)SqlColumnIndex.ContactEmail) ? null : reader.GetString((int)SqlColumnIndex.ContactEmail),
			};
		}

		public static Vehicle? CreateVehicleFromReader(SqliteDataReader reader)
		{
			if (reader.IsDBNull((int)SqlColumnIndex.VehicleId))
				return null;

			return new Vehicle
			{
				Id = reader.GetInt32((int)SqlColumnIndex.VehicleId),
				AccountId = reader.GetInt32((int)SqlColumnIndex.AccountId),
				Plate = reader.IsDBNull((int)SqlColumnIndex.VehiclePlate) ? null : reader.GetString((int)SqlColumnIndex.VehiclePlate),
				Model = reader.IsDBNull((int)SqlColumnIndex.VehicleModel) ? null : reader.GetString((int)SqlColumnIndex.VehicleModel),
				Photo = reader.IsDBNull((int)SqlColumnIndex.VehiclePhoto)
					? null
					: (byte[])reader.GetValue((int)SqlColumnIndex.VehiclePhoto),
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
			Plate = v.Plate ?? String.Empty,
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