namespace MattsBank.Domain.ValueObjects
{
    public class Amount
    {
        public decimal Value { get; private set; }

        public Amount(decimal amount)
        {
            if (amount < 0) throw new ArgumentException("Amount cannot be negative.", nameof(amount));

            Value = amount;
        }

        public static implicit operator decimal(Amount amount) => amount.Value;
        public static implicit operator Amount(decimal amount) => new(amount);
    }
}
