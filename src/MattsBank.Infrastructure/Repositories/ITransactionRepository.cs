using ErrorOr;

using MattsBank.Domain.Entities;
using MattsBank.Domain.ValueObjects;

namespace MattsBank.Infrastructure.Repositories
{
    public interface ITransactionRepository
    {
        Task CreateAsync(Transaction transaction);
        Task<List<Transaction>> GetByAccountIdAsync(Identifier accountId, DateTime? from = null, DateTime? to = null);
        Task<ErrorOr<Transaction>> GetTransactionById(Identifier transactionId);
    }
}
