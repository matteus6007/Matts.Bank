using MattsBank.Domain.ValueObjects;

namespace MattsBank.Domain.Entities
{
    public record Transaction(
        Guid Id,
        Guid AccountId,
        Amount Amount,
        DateTime TransactionDate,
        TransactionType TransactionType)
    {
    }
}
