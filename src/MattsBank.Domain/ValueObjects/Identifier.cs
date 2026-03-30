namespace MattsBank.Domain.ValueObjects
{
    public class Identifier
    {
        public Guid Value { get; private set; }

        public static implicit operator Identifier(Guid id)
        {
            return id.Equals(Guid.Empty) ? throw new ArgumentException("Identifier cannot be empty.", nameof(id)) : new Identifier { Value = id };
        }

        public override bool Equals(object? obj)
        {
            return obj is Identifier item && Value.Equals(item.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
