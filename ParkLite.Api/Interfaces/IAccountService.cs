using ParkLite.Api.Models;

namespace ParkLite.Api.Interfaces;

public interface IAccountService
{
	Task<IEnumerable<Account>> GetAllAccountsAsync();
	Task<Account?> GetByIdAsync(int id);
	Task AddAsync(Account account);
	Task UpdateAsync(Account account);
	Task DeleteAsync(int id);
}