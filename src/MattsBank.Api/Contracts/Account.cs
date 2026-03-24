namespace MattsBank.Api.Contracts
{
    public class Account
    {
        public required string AccountNumber { get; set; }
        public required string SortCode { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string Name => $"{FirstName} {LastName}".Trim();
        public DateTime OpenedDate { get; set; }
        public decimal Balance { get; set; } = 0m;
    }
}
