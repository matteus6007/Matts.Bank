namespace MattsBank.Domain.Aggregates
{
    public abstract class AggregateRoot
    {
        public Guid Id { get; protected init; }
    }
}
