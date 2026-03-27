

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
        ITransactionRepository transactionRepository,
        IOptions<BankOptions> options) : IAccountService
    {
        private readonly IAccountRepository _accountRepository = accountRepository;
        private readonly ITransactionRepository _transactionRepository = transactionRepository;
        private readonly BankOptions _bankOptions = options.Value;

        public async Task<ErrorOr<Account>> CreateAccountAsync(CreateAccountRequest request)
        {
            var accountNumber = await _accountRepository.GetNextAccountNumberAsync();

            var accountAggregate = BankAccountAggregate.Create(request.FirstName, request.LastName, accountNumber, _bankOptions.SortCode);

            var response = await _accountRepository.CreateAsync(accountAggregate);

            if (response.IsError) return response.Errors;

            return MapFrom(accountAggregate);
        }

        public async Task<ErrorOr<Success>> DepositAsync(string accountNumber, string sortCode, decimal amount)
        {
            // TODO: handle retrying transactions in case of concurrency exceptions (optimistic concurrency control)
            var aggregate = await _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode);

            if (aggregate.IsError) return aggregate.Errors;

            var response = aggregate.Value.Deposit(amount);

            if (response.IsError) return response.Errors;

            await _accountRepository.UpdateAsync(aggregate.Value);

            return Result.Success;
        }

        public async Task<ErrorOr<Account>> GetAccountAsync(string accountNumber, string sortCode)
        {
            var aggregate = await _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode);

            if (aggregate.IsError) return aggregate.Errors;

            return MapFrom(aggregate.Value);
        }

        public async Task<ErrorOr<Success>> WithdrawAsync(string accountNumber, string sortCode, decimal amount)
        {
            // TODO: handle retrying transactions in case of concurrency exceptions (optimistic concurrency control)
            var aggregate = await _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode);

            if (aggregate.IsError) return aggregate.Errors;

            var response = aggregate.Value.Withdraw(amount);

            if (response.IsError) return response.Errors;

            await _accountRepository.UpdateAsync(aggregate.Value);

            return Result.Success;
        }

        public async Task<ErrorOr<Success>> ReverseAsync(string accountNumber, string sortCode, Guid transactionId)
        {
            var aggregate = await _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode);

            if (aggregate.IsError) return aggregate.Errors;

            var transaction = await _transactionRepository.GetTransactionById(transactionId);

            if (transaction.IsError) return transaction.Errors;

            var response = aggregate.Value.Reverse(transaction.Value);

            if (response.IsError) return response.Errors;

            await _accountRepository.UpdateAsync(aggregate.Value);

            return Result.Success;
        }

        // TODO: move to mapper
        private static Account MapFrom(BankAccountAggregate aggregate)
        {
            return new Account
            {
                AccountNumber = aggregate.AccountNumber.ToString(),
                SortCode = aggregate.SortCode.ToString(),
                FirstName = aggregate.FirstName,
                LastName = aggregate.LastName,
                OpenedDate = aggregate.OpenedDate,
                Balance = aggregate.Balance,
            };
        }
    }
}
