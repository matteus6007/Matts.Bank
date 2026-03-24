using MattsBank.Domain.ValueObjects;
using MattsBank.Infrastructure.Repositories;

namespace MattsBank.Tests.Repositories
{
    public class TransactionRepositryTests
    {
        private readonly InMemoryTransactionRepository _repository = new();

        [Fact]
        public async Task CreateTransaction_ShouldSucceed()
        {
            // Arrange
            var transaction = new Domain.Entities.Transaction(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new Amount(100m),
                DateTime.UtcNow,
                TransactionType.Deposit);

            await _repository.CreateAsync(transaction);

            // Act
            var transactions = await _repository.GetByAccountIdAsync(transaction.AccountId);

            // Assert
            Assert.NotNull(transactions);
            Assert.Single(transactions);
        }

        [Fact]
        public async Task GetTransactions_ShouldReturnTransactionsWithinDateRange()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var transaction1 = new Domain.Entities.Transaction(
                Guid.NewGuid(),
                accountId,
                new Amount(100m),
                DateTime.UtcNow.AddDays(-1),
                TransactionType.Deposit);
            var transaction2 = new Domain.Entities.Transaction(
                Guid.NewGuid(),
                accountId,
                new Amount(50m),
                DateTime.UtcNow.AddDays(-2),
                TransactionType.Withdrawal);
            await _repository.CreateAsync(transaction1);
            await _repository.CreateAsync(transaction2);
            var fromDate = DateTime.UtcNow.AddDays(-3);
            var toDate = DateTime.UtcNow;

            // Act
            var transactions = await _repository.GetByAccountIdAsync(accountId, fromDate, toDate);

            // Assert
            Assert.NotNull(transactions);
            Assert.Equal(2, transactions.Count);
        }
    }
}
