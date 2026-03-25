using MattsBank.Domain.ValueObjects;

namespace MattsBank.Domain.Entities
{
    public record Account(
        Guid Id,
        AccountNumber AccountNumber,
        SortCode SortCode,
        string FirstName,
        string LastName,
        DateTime OpenedDate,
        Balance Balance) : Entity(Id);
}
