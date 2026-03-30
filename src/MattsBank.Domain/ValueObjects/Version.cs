namespace MattsBank.Domain.ValueObjects
{
    public class Version
    {
        public int Value { get; private set; } = 1;

        public static implicit operator Version(int version)
        {
            return version <= 0
                ? throw new ArgumentException("Version must be a positive integer.", nameof(version))
                : new Version { Value = version };
        }

        public override bool Equals(object? obj)
        {
            return obj is Version item && Value.Equals(item.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
