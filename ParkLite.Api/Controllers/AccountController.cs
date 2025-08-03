using Microsoft.AspNetCore.Mvc;
using ParkLite.Api.Interfaces;
using ParkLite.Api.Models;

namespace ParkLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IAccountService accountService) : ControllerBase
{
	private readonly IAccountService _accountService = accountService;

	[HttpGet]
	public IActionResult Get() => Ok(_accountService.GetAllAccountsAsync());

	[HttpGet("{id:int}")]
	public IActionResult Get(int id)
	{
		var account = _accountService.GetByIdAsync(id);
		return account is null ? NotFound() : Ok(account);
	}

	[HttpPost]
	public IActionResult Post(Account account)
	{
		_accountService.AddAsync(account);
		return CreatedAtAction(nameof(Get), new { id = account.Id }, account);
	}

	[HttpPut("{id:int}")]
	public IActionResult Put(int id, Account account)
	{
		if (id != account.Id) return BadRequest();
		_accountService.UpdateAsync(account);
		return NoContent();
	}

	[HttpDelete("{id:int}")]
	public IActionResult Delete(int id)
	{
		_accountService.DeleteAsync(id);
		return NoContent();
	}
}
