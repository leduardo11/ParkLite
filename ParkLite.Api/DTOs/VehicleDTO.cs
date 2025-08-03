namespace ParkLite.Api.Dtos;

public class VehicleDTO
{
	public int Id { get; set; }
	public int AccountId { get; set; }
	public string Plate { get; set; } = string.Empty;
	public string? Model { get; set; }
	public string? Photo { get; set; }
}
