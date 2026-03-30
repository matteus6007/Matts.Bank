using ErrorOr;

using MattsBank.Domain.Entities;
using MattsBank.Domain.ValueObjects;

namespace MattsBank.Infrastructure.Repositories
{
    public class InMemoryTransactionRepository : ITransactionRepository
    {
        private readonly List<Transaction> _transactions = [];

        public async Task CreateAsync(Transaction transaction)
        {
            await Task.CompletedTask;

            if (_transactions.Any(x => x.Id == transaction.Id)) throw new InvalidOperationException("Transaction already exists.");

            _transactions.Add(transaction);
        }

        public async Task<List<Transaction>> GetByAccountIdAsync(Identifier accountId, DateTime? from = null, DateTime? to = null)
        {
            await Task.CompletedTask;

            if (!to.HasValue)
            {
                to = DateTime.UtcNow;
            }

            if (!from.HasValue)
            {
                // TODO: get default from configuration
                from = to.Value.AddDays(-7);
            }

            if (from > to)
            {
                throw new ArgumentException("From date cannot be greater than to date.");
            }

            return _transactions.OrderByDescending(x => x.TransactionDate).Where(x => x.AccountId.Equals(accountId) && x.TransactionDate >= from && x.TransactionDate <= to).ToList();
        }

        public async Task<ErrorOr<Transaction>> GetTransactionById(Identifier transactionId)
        {
            await Task.CompletedTask;
            
            var transaction = _transactions.FirstOrDefault(x => x.Id.Equals(transactionId));

            if (transaction == null)
            {
                return Error.NotFound(description: $"Transaction with id {transactionId} not found.");
            }

            return transaction;
        }
    }
}
