namespace MattsBank.Domain.Aggregates
{
    public abstract class AggregateRoot
    {
        public Guid Id { get; protected init; }

        public ValueObjects.Version Version { get; protected set; } = 1;

        public ValueObjects.Version PreviousVersion { get; protected set; } = 1;
    }
}
