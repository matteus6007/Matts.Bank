
using ErrorOr;

using MattsBank.Api.Contracts;
using MattsBank.Api.Options;
using MattsBank.Domain.Aggregates;
using MattsBank.Infrastructure.Repositories;

using Microsoft.Extensions.Options;

namespace MattsBank.Api.Services
{
    public class AccountService(
        IAccountRepository accountRepository,
        IOptions<BankOptions> options) : IAccountService
    {
        private readonly IAccountRepository _accountRepository = accountRepository;
        private readonly BankOptions _bankOptions = options.Value;

        public async Task<ErrorOr<Account>> CreateAccountAsync(CreateAccountRequest request)
        {
            var accountNumber = await _accountRepository.GetNextAccountNumberAsync();

            var accountAggregate = BankAccountAggregate.Create(request.FirstName, request.LastName, accountNumber, _bankOptions.SortCode);

            await _accountRepository.CreateAsync(accountAggregate);

            return MapFrom(accountAggregate);
        }

        public async Task<ErrorOr<Success>> DepositAsync(string accountNumber, string sortCode, decimal amount)
        {
            var accountAggregate = await _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode);

            if (accountAggregate == null) return Error.NotFound(description: "Account not found.");

            var response = accountAggregate.Deposit(new Domain.ValueObjects.Amount(amount));

            if (response.IsError)
            {
                return response.Errors.First();
            }

            await _accountRepository.UpdateAsync(accountAggregate);

            return Result.Success;
        }

        public async Task<ErrorOr<Account>> GetAccountAsync(string accountNumber, string sortCode)
        {
            var accountAggregate = await _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode);

            if (accountAggregate == null) return Error.NotFound(description: "Account not found.");

            return MapFrom(accountAggregate);
        }

        public async Task<ErrorOr<Success>> WithdrawAsync(string accountNumber, string sortCode, decimal amount)
        {
            var accountAggregate = await _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode);

            if (accountAggregate == null) return Error.NotFound(description: "Account not found.");

            var response = accountAggregate.Withdraw(new Domain.ValueObjects.Amount(amount));

            if (response.IsError)
            {
                return response.Errors.First();
            }

            await _accountRepository.UpdateAsync(accountAggregate);

            return Result.Success;
        }

        // TODO: move to mapper
        private static Account MapFrom(BankAccountAggregate accountAggregate)
        {
            return new Account
            {
                AccountNumber = accountAggregate.AccountNumber.ToString(),
                SortCode = accountAggregate.SortCode.ToString(),
                FirstName = accountAggregate.FirstName,
                LastName = accountAggregate.LastName,
                OpenedDate = accountAggregate.OpenedDate,
                Balance = accountAggregate.Balance,
            };
        }
    }
}
