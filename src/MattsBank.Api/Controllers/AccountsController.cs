using MattsBank.Api.Contracts;
using MattsBank.Api.Options;
using MattsBank.Api.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MattsBank.Api.Controllers
{
    public class AccountsController(IAccountService accountService, IOptions<BankOptions> options) : ApiController
    {
        [HttpPost]
        [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(CreateAccountRequest request)
        {
            var response = await accountService.CreateAccountAsync(request);

            return CreatedAtAction(nameof(Get), new { response.Value.AccountNumber }, new AccountResponse { Account = response.Value });
        }

        [HttpGet]
        [Route("{accountNumber}")]
        [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(string accountNumber)
        {
            if (accountNumber.Length < 8) return Problem(title: "Account number must be at least 8 characters long.", statusCode: 400);

            var response = await accountService.GetAccountAsync(accountNumber, options.Value.SortCode);

            return response.IsError ? Problem(response.Errors) : Ok(new AccountResponse { Account = response.Value });
        }

        [HttpPut]
        [Route("{accountNumber}/deposit")]
        public async Task<IActionResult> Deposit(string accountNumber, [FromQuery] decimal amount)
        {
            if (accountNumber.Length < 8) return Problem(title: "Account number must be at least 8 characters long.", statusCode: 400);
            if (amount < 0) return Problem(title: "Amount must be a positive value.", statusCode: 400);

            var response = await accountService.DepositAsync(accountNumber, options.Value.SortCode, amount);
            return response.IsError ? Problem(response.Errors) : NoContent();
        }

        [HttpPut]
        [Route("{accountNumber}/withdraw")]
        public async Task<IActionResult> Withdraw(string accountNumber, [FromQuery] decimal amount)
        {
            if (accountNumber.Length < 8) return Problem(title: "Account number must be at least 8 characters long.", statusCode: 400);
            if (amount < 0) return Problem(title: "Amount must be a positive value.", statusCode: 400);

            var response = await accountService.WithdrawAsync(accountNumber, options.Value.SortCode, amount);
            return response.IsError ? Problem(response.Errors) : NoContent();
        }
    }
}
