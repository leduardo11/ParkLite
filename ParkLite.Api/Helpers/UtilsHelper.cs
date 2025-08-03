using ParkLite.Api.Dtos;
using ParkLite.Api.Models;

namespace ParkLite.Api.Helpers
{
	public static class UtilsHelper
	{
		public static Account MapDtoToAccount(AccountDTO dto) => new(dto.Id, dto.Name, dto.IsActive)
		{
			Contacts = dto.Contacts,
			Vehicles = [.. dto.Vehicles.Select(v => new Vehicle
		  {
			Id = v.Id,
			AccountId = v.AccountId,
			Plate = v.Plate,
			Model = v.Model,
			Photo = TryParseBase64(v.Photo)
			})]
		};

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