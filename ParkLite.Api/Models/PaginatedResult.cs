namespace ParkLite.Api.Models
{
	public class PaginatedResult<T>
	{
		public int Total { get; set; }
		public IEnumerable<T> Result { get; set; } = [];
	}
}