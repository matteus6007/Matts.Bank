using MattsBank.Domain.ValueObjects;

namespace MattsBank.Domain.Entities
{
    public record Account(
        Identifier Id,
        AccountNumber AccountNumber,
        SortCode SortCode,
        string FirstName,
        string LastName,
        DateTime OpenedDate,
        Balance Balance,
        ValueObjects.Version Version) : Entity(Id);
}
