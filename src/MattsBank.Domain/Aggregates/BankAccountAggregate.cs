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
            Version = Version.Value + 1;

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
        public Balance Balance { get; private set; }

        private BankAccountAggregate(
            Identifier id,
            string firstName,
            string lastName,
            AccountNumber accountNumber,
            SortCode sortCode,
            DateTime openedDate,
            Balance balance,
            ValueObjects.Version version)
            : base(id)
        {
            FirstName = firstName;
            LastName = lastName;
            AccountNumber = accountNumber;
            SortCode = sortCode;
            OpenedDate = openedDate;
            Balance = balance;
            Version = version;
            PreviousVersion = version;
        }

        public static BankAccountAggregate Create(
            string firstName,
            string lastName,
            AccountNumber accountNumber,
            SortCode sortCode)
        => new(Guid.NewGuid(), firstName, lastName, accountNumber, sortCode, DateTime.UtcNow, 0m, 1);

        public static BankAccountAggregate Recreate(Account account)
        {
            var aggregate = new BankAccountAggregate(
                account.Id,
                account.FirstName,
                account.LastName,
                account.AccountNumber,
                account.SortCode,
                account.OpenedDate,
                account.Balance,
                account.Version);

            return aggregate;
        }

        public ErrorOr<Success> Deposit(Amount amount)
        {
            Balance += amount;

            var transaction = new Transaction(
                Guid.NewGuid(),
                Id,
                amount,
                Balance,
                DateTime.UtcNow,
                TransactionType.Deposit);

            AddTransaction(transaction);

            return Result.Success;
        }

        public ErrorOr<Success> Withdraw(Amount amount)
        {
            if (amount.Value > Balance) return Error.Conflict(description: "Insufficient funds.");

            decimal transactionAmount = -1 * amount.Value;

            Balance -= amount;

            var transaction = new Transaction(
                Guid.NewGuid(),
                Id,
                transactionAmount,
                Balance,
                DateTime.UtcNow,
                TransactionType.Withdrawal);

            AddTransaction(transaction);

            return Result.Success;
        }

        public ErrorOr<Success> Reverse(Transaction transactionToReverse)
        {
            decimal reverseAmount = -1 * transactionToReverse.Amount;

            Balance += reverseAmount;

            var transaction = new Transaction(
                Guid.NewGuid(),
                Id,
                reverseAmount,
                Balance,
                DateTime.UtcNow,
                TransactionType.Reversal);

            AddTransaction(transaction);

            return Result.Success;
        }
    }
}
