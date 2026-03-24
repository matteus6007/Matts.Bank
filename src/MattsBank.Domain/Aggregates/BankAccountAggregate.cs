using ErrorOr;

using MattsBank.Domain.Entities;
using MattsBank.Domain.ValueObjects;

namespace MattsBank.Domain.Aggregates
{
    public partial class BankAccountAggregate
    {
        private readonly List<Transaction> _transactions = [];

        internal void AddTransaction(Transaction transaction)
        {
            _transactions.Add(transaction);
        }        

        public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();
    }

    public partial class BankAccountAggregate : AggregateRoot
    {
        public AccountNumber AccountNumber { get; private init; }
        public SortCode SortCode { get; private init; }
        public string FirstName { get; private init; }
        public string LastName { get; private init; }
        public DateTime OpenedDate { get; private init; }
        public decimal Balance => _transactions.Sum(t =>t.TransactionType == TransactionType.Deposit ? t.Amount.Value : -t.Amount.Value);

        private BankAccountAggregate(
            Guid id,
            string firstName,
            string lastName,
            AccountNumber accountNumber,
            SortCode sortCode,
            DateTime openedDate)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            AccountNumber = accountNumber;
            SortCode = sortCode;
            OpenedDate = openedDate;
        }

        public static BankAccountAggregate Create(
            string firstName,
            string lastName,
            AccountNumber accountNumber,
            SortCode sortCode)
        => new(Guid.NewGuid(), firstName, lastName, accountNumber, sortCode, DateTime.UtcNow);

        public static BankAccountAggregate Recreate(Account account, IEnumerable<Transaction> transactions)
        {
            var aggregate = new BankAccountAggregate(
                account.Id,
                account.FirstName,
                account.LastName,
                account.AccountNumber,
                account.SortCode,
                account.OpenedDate);

            foreach (var transaction in transactions)
            {
                aggregate.AddTransaction(transaction);
            }

            return aggregate;
        }

        public ErrorOr<Success> Deposit(Amount amount)
        {
            var transaction = new Transaction(
                Guid.NewGuid(),
                Id,
                amount,
                DateTime.UtcNow,
                TransactionType.Deposit);

            AddTransaction(transaction);

            return Result.Success;
        }

        public ErrorOr<Success> Withdraw(Amount amount)
        {
            if (amount.Value > Balance) return Error.Conflict(description: "Insufficient funds.");

            var transaction = new Transaction(
                Guid.NewGuid(),
                Id,
                amount,
                DateTime.UtcNow,
                TransactionType.Withdrawal);

            AddTransaction(transaction);

            return Result.Success;
        }       
    }
}
