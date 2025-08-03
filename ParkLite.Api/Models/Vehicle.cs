namespace ParkLite.Api.Models
{
	public class Vehicle
	{
		public int Id { get; set; }
		public int AccountId { get; set; }
		public string? Plate { get; set; } = string.Empty;
		public string? Model { get; set; } = string.Empty;
		public byte[]? Photo { get; set; }
	}
}