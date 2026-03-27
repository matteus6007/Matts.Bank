using MattsBank.Domain.ValueObjects;

namespace MattsBank.Domain.Entities
{
    public record Transaction(
        Guid Id,
        Guid AccountId,
        decimal Amount,
        Balance ClosingBalance,
        DateTime TransactionDate,
        TransactionType TransactionType) : Entity(Id);
}
