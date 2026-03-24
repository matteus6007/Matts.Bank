using MattsBank.Api.Contracts;
using MattsBank.Api.Options;
using MattsBank.Api.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MattsBank.Api.Controllers
{
    public class TransactionsController(ITransactionService transactionService, IOptions<BankOptions> options) : ApiController
    {
        private readonly ITransactionService _transactionService = transactionService;
        private readonly IOptions<BankOptions> _options = options;

        [HttpGet]
        [Route("{accountNumber}")]
        [ProducesResponseType(typeof(TransactionsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Index(string accountNumber)
        {
            var response = await _transactionService.GetTransactions(accountNumber, _options.Value.SortCode);

            return response.IsError ? Problem(response.Errors) : Ok(new TransactionsResponse { Transactions = response.Value });
        }
    }
}
