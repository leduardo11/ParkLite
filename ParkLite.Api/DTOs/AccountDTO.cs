using ParkLite.Api.Models;

namespace ParkLite.Api.Dtos;

public class AccountDTO
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public bool IsActive { get; set; }
	public List<Contact> Contacts { get; set; } = [];
	public List<VehicleDTO> Vehicles { get; set; } = [];
}
