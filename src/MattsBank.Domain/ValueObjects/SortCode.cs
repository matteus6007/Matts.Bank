namespace MattsBank.Domain.ValueObjects
{
    public class SortCode(int bank, int branch)
    {
        public int Bank { get; private set; } = bank;
        public int Branch { get; private set; } = branch;

        public static implicit operator SortCode(string sortCode)
        {
            sortCode = sortCode.Replace("-", "");

            if (sortCode.Length < 6) throw new ArgumentException("Sort code must be at least 6 characters long.", nameof(sortCode));
            if (int.TryParse(sortCode, out _) == false) throw new ArgumentException("Sort code must be numeric.", nameof(sortCode));

            var bank = sortCode.Substring(0, 2);
            var branch = sortCode.Substring(2, 4);

            return new SortCode(int.Parse(bank), int.Parse(branch));
        }

        public override bool Equals(object? obj)
        {
            return obj is SortCode item && Bank.Equals(item.Bank) && Branch.Equals(item.Branch);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Bank, Branch);
        }

        public override string ToString()
        {
            return $"{Bank:D2}{Branch:D4}";
        }
    }
}
