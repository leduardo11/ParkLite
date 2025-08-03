using Microsoft.AspNetCore.Mvc;
using ParkLite.Api.Dtos;
using ParkLite.Api.Interfaces;

namespace ParkLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IAccountService accountService) : ControllerBase
{
	private readonly IAccountService _accountService = accountService;
   
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
	    var result = await _accountService.GetPaginatedAccountsAsync(limit, offset);
	    return Ok(result);
    }


	[HttpGet("{id:int}")]
	public async Task<IActionResult> Get(int id)
	{
		var account = await _accountService.GetByIdAsync(id);
		return account is null ? NotFound() : Ok(new { result = account });
	}

	[HttpPost]
	public async Task<IActionResult> Post(AccountDTO account)
	{
		await _accountService.AddAsync(account);
		return CreatedAtAction(nameof(Get), new { id = account.Id }, new { result = account });
	}

	[HttpPut("{id:int}")]
	public async Task<IActionResult> Put(int id, AccountDTO account)
	{
		if (id != account.Id) return BadRequest();
		await _accountService.UpdateAsync(account);
		return NoContent();
	}

	[HttpDelete("{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		await _accountService.DeleteAsync(id);
		return NoContent();
	}

	[HttpPost("batch-deactivate")]
	public async Task<IActionResult> BatchDeactivate(int batchSize = 50, int delayMs = 1000)
	{
		await _accountService.BatchDeactivateInactiveAccountsAsync(batchSize, delayMs);
		return NoContent();
	}
}
