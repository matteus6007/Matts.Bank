namespace MattsBank.Domain.ValueObjects
{
    public class Version
    {
        public int Value { get; private set; } = 1;

        public static implicit operator Version(int version)
        {
            if (version <= 0) throw new ArgumentException("Version must be a positive integer.", nameof(version));

            return new Version { Value = version };
        }

        public override bool Equals(object? obj)
        {
            var item = obj as Version;

            return item == null ? false : Value.Equals(item.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
