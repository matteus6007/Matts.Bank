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

        public async Task CreateAsync(BankAccountAggregate aggregate)
        {
            if (await AccountExistsAsync(aggregate.AccountNumber, aggregate.SortCode)) throw new InvalidOperationException("Account already exists.");

            var account = new Account(
                aggregate.Id,
                aggregate.AccountNumber,
                aggregate.SortCode,
                aggregate.FirstName,
                aggregate.LastName,
                aggregate.OpenedDate,
                aggregate.Balance);

            _accounts.Add(account);
        }

        public async Task DeleteAsync(AccountNumber accountNumber, SortCode sortCode)
        {
            await Task.CompletedTask;

            var account = _accounts.FirstOrDefault(x =>
                x.AccountNumber.Equals(accountNumber)
                && x.SortCode.Equals(sortCode));

            if (account is null)
            {
                throw new InvalidOperationException("Account does not exist.");
            }

            _accounts.Remove(account);
        }

        public async Task<BankAccountAggregate?> GetByAccountNumberAsync(AccountNumber accountNumber, SortCode sortCode)
        {
            await Task.CompletedTask;

            var account = _accounts.FirstOrDefault(x => 
                x.AccountNumber.Equals(accountNumber)
                && x.SortCode.Equals(sortCode));

            if (account is null)
            {
                return null;
            }

            var aggregate = BankAccountAggregate.Recreate(account);

            return aggregate;
        }

        public async Task<int> GetNextAccountNumberAsync()
        {
            await Task.CompletedTask;

            var lastAccountNumber = _accounts.Count == 0 ? 0 : _accounts.Max(x => x.AccountNumber.Number);
            lastAccountNumber++;

            return lastAccountNumber;
        }

        public async Task UpdateAsync(BankAccountAggregate aggregate)
        {
            await Task.CompletedTask;

            var existing = _accounts.FirstOrDefault(x => x.Id == aggregate.Id);
            
            if (existing is null)
            {
                throw new InvalidOperationException("Account does not exist.");            
            }

            var index = _accounts.IndexOf(existing);
            _accounts[index] = new Account(
                aggregate.Id,
                aggregate.AccountNumber,
                aggregate.SortCode,                
                aggregate.FirstName,
                aggregate.LastName,
                aggregate.OpenedDate,
                aggregate.Balance);

            foreach (var transaction in aggregate.Transactions)
            {
                await _transactionRepository.CreateAsync(transaction);
            }
        }
    }
}
