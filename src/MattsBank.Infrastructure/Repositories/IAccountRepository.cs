using MattsBank.Domain.Aggregates;
using MattsBank.Domain.ValueObjects;

namespace MattsBank.Infrastructure.Repositories
{
    public interface IAccountRepository
    {
        Task<bool> AccountExistsAsync(AccountNumber accountNumber, SortCode sortCode);
        Task CreateAsync(BankAccountAggregate aggregate);
        Task DeleteAsync(AccountNumber accountNumber, SortCode sortCode);
        Task<BankAccountAggregate?> GetByAccountNumberAsync(AccountNumber accountNumber, SortCode sortCode);
        Task<int> GetNextAccountNumberAsync();
        Task UpdateAsync(BankAccountAggregate aggregate);
    }
}
