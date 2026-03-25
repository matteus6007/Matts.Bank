using ErrorOr;

using MattsBank.Domain.Aggregates;
using MattsBank.Domain.Entities;
using MattsBank.Domain.ValueObjects;

namespace MattsBank.Infrastructure.Repositories
{
    public class InMemoryAccountRepository(ITransactionRepository transactionRepository) : IAccountRepository
    {
        private readonly List<Account> _accounts = [];
        private readonly ITransactionRepository _transactionRepository = transactionRepository;

        public Task<bool> AccountExistsAsync(AccountNumber accountNumber, SortCode sortCode)
        {
            return _accounts.Any(x => 
                x.AccountNumber.Equals(accountNumber)
                && x.SortCode.Equals(sortCode)) ? Task.FromResult(true) : Task.FromResult(false);
        }

        public async Task<ErrorOr<Success>> CreateAsync(BankAccountAggregate aggregate)
        {
            if (await AccountExistsAsync(aggregate.AccountNumber, aggregate.SortCode))
            {
                return Error.Conflict(description: "Account already exists");
            }

            var account = new Account(
                aggregate.Id,
                aggregate.AccountNumber,
                aggregate.SortCode,
                aggregate.FirstName,
                aggregate.LastName,
                aggregate.OpenedDate,
                aggregate.Balance,
                aggregate.Version);

            _accounts.Add(account);

            return Result.Success;
        }

        public async Task<ErrorOr<Success>> DeleteAsync(AccountNumber accountNumber, SortCode sortCode)
        {
            await Task.CompletedTask;

            var account = _accounts.FirstOrDefault(x =>
                x.AccountNumber.Equals(accountNumber)
                && x.SortCode.Equals(sortCode));

            if (account is null)
            {
                return Error.NotFound(description: "Account does not exist");
            }

            _accounts.Remove(account);

            return Result.Success;
        }

        public async Task<ErrorOr<BankAccountAggregate>> GetByAccountNumberAsync(AccountNumber accountNumber, SortCode sortCode)
        {
            await Task.CompletedTask;

            var account = _accounts.FirstOrDefault(x => 
                x.AccountNumber.Equals(accountNumber)
                && x.SortCode.Equals(sortCode));

            if (account is null)
            {
                return Error.NotFound(description: "Account does not exist");
            }

            var aggregate = BankAccountAggregate.Recreate(account);

            return aggregate;
        }

        public async Task<AccountNumber> GetNextAccountNumberAsync()
        {
            await Task.CompletedTask;

            var lastAccountNumber = _accounts.Count == 0 ? 0 : _accounts.Max(x => x.AccountNumber.Number);
            lastAccountNumber++;

            return lastAccountNumber;
        }

        public async Task<ErrorOr<Success>> UpdateAsync(BankAccountAggregate aggregate)
        {
            await Task.CompletedTask;

            var existing = _accounts.FirstOrDefault(x => x.Id == aggregate.Id);
            
            if (existing is null)
            {
               return Error.NotFound(description: "Account does not exist");
            }

            if (existing.Version != aggregate.PreviousVersion)
            {
                return Error.Conflict(description: "Account has been modified by another transaction");
            }

            var index = _accounts.IndexOf(existing);
            _accounts[index] = new Account(
                aggregate.Id,
                aggregate.AccountNumber,
                aggregate.SortCode,                
                aggregate.FirstName,
                aggregate.LastName,
                aggregate.OpenedDate,
                aggregate.Balance,
                aggregate.Version);

            foreach (var transaction in aggregate.Transactions)
            {
                await _transactionRepository.CreateAsync(transaction);
            }

            return Result.Success;
        }
    }
}
