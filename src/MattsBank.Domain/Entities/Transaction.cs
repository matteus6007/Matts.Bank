using MattsBank.Domain.ValueObjects;

namespace MattsBank.Domain.Entities
{
    public record Transaction(
        Identifier Id,
        Identifier AccountId,
        decimal Amount,
        Balance ClosingBalance,
        DateTime TransactionDate,
        TransactionType TransactionType,
        Identifier? OriginalTransactionId) : Entity(Id);
}
