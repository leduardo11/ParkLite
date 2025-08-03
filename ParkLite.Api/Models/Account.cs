namespace ParkLite.Api.Models
{
	public class Account(int id, string name, bool isActive)
	{
		public int Id { get; set; } = id;
		public string Name { get; set; } = name;
		public bool IsActive { get; set; } = isActive;

		public List<Contact> Contacts { get; set; } = [];
		public List<Vehicle> Vehicles { get; set; } = [];
	}
}