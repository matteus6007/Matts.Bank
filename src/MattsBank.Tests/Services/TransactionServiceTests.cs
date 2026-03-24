using ErrorOr;

using MattsBank.Api.Services;
using MattsBank.Domain.Aggregates;
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

            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns((BankAccountAggregate?)null);

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
            var transactions = new List<Domain.Entities.Transaction>
            {
                new Domain.Entities.Transaction(Guid.NewGuid(), accountId, new Amount(100), DateTime.UtcNow, TransactionType.Deposit),
                new Domain.Entities.Transaction(Guid.NewGuid(), accountId, new Amount(50), DateTime.UtcNow, TransactionType.Withdrawal)
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
