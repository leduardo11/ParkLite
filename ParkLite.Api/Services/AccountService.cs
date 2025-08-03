using ParkLite.Api.Dtos;
using ParkLite.Api.Helpers;
using ParkLite.Api.Interfaces;

namespace ParkLite.Api.Services;

public class AccountService(IAccountRepository repository) : IAccountService
{
	private readonly IAccountRepository _repository = repository;

	public async Task<IEnumerable<AccountDTO>> GetAllAccountsAsync()
	{
		var accounts = await _repository.GetAllAsync();
		return accounts.Select(SqliteHelper.MapAccountToDTO);
	}

	public async Task<AccountDTO?> GetByIdAsync(int id)
	{
		var account = await _repository.GetByIdAsync(id);
		return account == null ? null : SqliteHelper.MapAccountToDTO(account);
	}

	public async Task AddAsync(AccountDTO dto)
	{
		var account = SqliteHelper.MapDTOToAccount(dto);
		await _repository.AddAsync(account);
	}

	public async Task UpdateAsync(AccountDTO dto)
	{
		var account = SqliteHelper.MapDTOToAccount(dto);
		await _repository.UpdateAsync(account);
	}

	public Task DeleteAsync(int id) => _repository.DeleteAsync(id);

	public Task BatchDeactivateInactiveAccountsAsync(int batchSize = 50, int delayMs = 1000) =>
		_repository.BatchDeactivateInactiveAccountsAsync(batchSize, delayMs);
}
