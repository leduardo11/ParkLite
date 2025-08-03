using ParkLite.Api.Interfaces;
using ParkLite.Api.Models;

namespace ParkLite.Api.Services;

public class AccountService(IAccountRepository repository) : IAccountService
{
	private readonly IAccountRepository _repository = repository;

	public IEnumerable<Account> GetAllAccounts() => _repository.GetAll();

	public Account? GetById(int id) => _repository.GetById(id);

	public void Add(Account account) => _repository.Add(account);

	public void Update(Account account) => _repository.Update(account);

	public void Delete(int id) => _repository.Delete(id);
}
