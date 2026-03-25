using ErrorOr;

using MattsBank.Domain.Aggregates;
using MattsBank.Domain.ValueObjects;

namespace MattsBank.Infrastructure.Repositories
{
    public interface IAccountRepository
    {
        Task<bool> AccountExistsAsync(AccountNumber accountNumber, SortCode sortCode);
        Task<ErrorOr<Success>> CreateAsync(BankAccountAggregate aggregate);
        Task<ErrorOr<Success>> DeleteAsync(AccountNumber accountNumber, SortCode sortCode);
        Task<ErrorOr<BankAccountAggregate>> GetByAccountNumberAsync(AccountNumber accountNumber, SortCode sortCode);
        Task<AccountNumber> GetNextAccountNumberAsync();
        Task<ErrorOr<Success>> UpdateAsync(BankAccountAggregate aggregate);
    }
}
