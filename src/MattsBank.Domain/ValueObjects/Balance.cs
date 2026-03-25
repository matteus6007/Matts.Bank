namespace MattsBank.Domain.ValueObjects
{
    public class Balance(decimal balance)
    {
        public decimal Value { get; private set; } = balance;

        public static implicit operator decimal(Balance balance) => balance.Value;
        public static implicit operator Balance(decimal balance) => new(balance);
    }
}
