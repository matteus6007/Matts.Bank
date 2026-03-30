using MattsBank.Domain.ValueObjects;

namespace MattsBank.Domain.Aggregates
{
    public abstract class AggregateRoot(Identifier id)
    {
        public Identifier Id { get; protected init; } = id;

        public ValueObjects.Version Version { get; protected set; } = 1;

        public ValueObjects.Version PreviousVersion { get; protected set; } = 1;
    }
}
