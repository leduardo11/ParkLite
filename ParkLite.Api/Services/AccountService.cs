using ParkLite.Api.Interfaces;
using ParkLite.Api.Models;

namespace ParkLite.Api.Services;

public class AccountService(IAccountRepository repository) : IAccountService
{
	private readonly IAccountRepository _repository = repository;

	public Task<IEnumerable<Account>> GetAllAccountsAsync() => _repository.GetAllAsync();

	public Task<Account?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

	public Task AddAsync(Account account) => _repository.AddAsync(account);

	public Task UpdateAsync(Account account) => _repository.UpdateAsync(account);

	public Task DeleteAsync(int id) => _repository.DeleteAsync(id);

	public Task BatchDeactivateInactiveAccountsAsync(int batchSize = 50, int delayMs = 1000) =>
	_repository.BatchDeactivateInactiveAccountsAsync(batchSize, delayMs);
}