using ParkLite.Api.Dtos;
using ParkLite.Api.Models;

namespace ParkLite.Api.Interfaces;

public interface IAccountService
{
	Task<IEnumerable<Account>> GetAllAccountsAsync();
	Task<Account?> GetByIdAsync(int id);
	Task AddAsync(AccountDTO dto);
	Task UpdateAsync(AccountDTO dto);
	Task DeleteAsync(int id);
	Task BatchDeactivateInactiveAccountsAsync(int batchSize = 50, int delayMs = 1000);
}