using ParkLite.Api.Models;

namespace ParkLite.Api.Interfaces
{
	public interface IAccountRepository
	{
		Task<PaginatedResult<Account>> GetPaginatedAccountsAsync(int limit, int offset);
		Task<Account?> GetByIdAsync(int id);
		Task AddAsync(Account account);
		Task UpdateAsync(Account account);
		Task DeleteAsync(int id);
		Task BatchDeactivateInactiveAccountsAsync(int batchSize = 50, int delayMs = 1000);
	}
}