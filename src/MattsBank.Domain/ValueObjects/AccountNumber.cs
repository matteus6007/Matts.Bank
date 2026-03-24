namespace MattsBank.Domain.ValueObjects
{
    public class AccountNumber
    {
        public int Number { get; set; }

        public static implicit operator AccountNumber(string accountNumber)
        {
            if (accountNumber.Length < 8) throw new ArgumentException("Account number must be at least 8 characters long.", nameof(accountNumber));
            if (int.TryParse(accountNumber, out _) == false) throw new ArgumentException("Account number must be numeric.", nameof(accountNumber));
    
            return new AccountNumber { Number = int.Parse(accountNumber) };
        }

        public static implicit operator AccountNumber(int accountNumber) => new AccountNumber { Number = accountNumber };

        public override bool Equals(object? obj)
        {
            var item = obj as AccountNumber;

            return item == null ? false : Number.Equals(item.Number);
        }

        public override int GetHashCode()
        {
            return Number.GetHashCode();
        }

        public override string ToString()
        {
            return Number.ToString("D8");
        }
    }
}
