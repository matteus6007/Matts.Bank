using ErrorOr;

using MattsBank.Domain.Entities;

namespace MattsBank.Infrastructure.Repositories
{
    public interface ITransactionRepository
    {
        Task CreateAsync(Transaction transaction);
        Task<List<Transaction>> GetByAccountIdAsync(Guid accountId, DateTime? from = null, DateTime? to = null);
        Task<ErrorOr<Transaction>> GetTransactionById(Guid transactionId);
    }
}
