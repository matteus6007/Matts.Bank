using ErrorOr;

using MattsBank.Api.Services;
using MattsBank.Domain.Aggregates;
using MattsBank.Domain.Entities;
using MattsBank.Domain.ValueObjects;
using MattsBank.Infrastructure.Repositories;

using NSubstitute;

namespace MattsBank.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly TransactionService _sut;

        public TransactionServiceTests()
        {
            _accountRepository = Substitute.For<IAccountRepository>();
            _transactionRepository = Substitute.For<ITransactionRepository>();
            _sut = new TransactionService(_accountRepository, _transactionRepository);
        }

        [Fact]
        public async Task GetTransactions_WhenAccountDoesNotExist_ReturnsNotFoundError()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";

            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns(Error.NotFound("Account does not exist"));

            // Act
            var actual = await _sut.GetTransactions(accountNumber, sortCode);

            // Assert
            Assert.True(actual.IsError);
            Assert.Equal(ErrorType.NotFound, actual.FirstError.Type);
        }

        [Fact]
        public async Task GetTransactions_WhenAccountExists_ReturnsTransactions()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";
            var aggregate = BankAccountAggregate.Create("Matt", "Jones", accountNumber, sortCode);

            var accountId = aggregate.Id;
            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns(aggregate);
            var transactions = new List<Transaction>
            {
                new Transaction(Guid.NewGuid(), accountId, 100m, 100m, DateTime.UtcNow, TransactionType.Deposit),
                new Transaction(Guid.NewGuid(), accountId, 50m, 50m, DateTime.UtcNow, TransactionType.Withdrawal)
            };
            _transactionRepository.GetByAccountIdAsync(accountId).Returns(transactions);

            // Act
            var actual = await _sut.GetTransactions(accountNumber, sortCode);

            // Assert
            Assert.False(actual.IsError);
            Assert.NotNull(actual.Value);
        }
    }
}
