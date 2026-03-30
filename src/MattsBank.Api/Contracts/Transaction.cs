namespace MattsBank.Api.Contracts
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public decimal ClosingBalance { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public Guid? OriginalTransactionId { get; set; }
    }
}
