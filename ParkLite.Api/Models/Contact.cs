namespace ParkLite.Api.Models
{
	public class Contact
	{
		public int Id { get; set; }
		public int AccountId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Phone { get; set; }
		public string? Email { get; set; }
	}
}