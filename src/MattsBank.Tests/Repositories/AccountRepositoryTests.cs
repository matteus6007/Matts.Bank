using MattsBank.Domain.Aggregates;
using MattsBank.Infrastructure.Repositories;

namespace MattsBank.Tests.Repositories
{
    public class AccountRepositoryTests
    {
        private readonly ITransactionRepository _transactionRepository = new InMemoryTransactionRepository();
        private readonly InMemoryAccountRepository _sut;

        public AccountRepositoryTests()
        {
            _sut = new InMemoryAccountRepository(_transactionRepository);
        }

        [Fact]
        public async Task GetAccountNumberAsync_ShouldReturnNextAvailableAccountNumber()
        {
            // Act
            var accountNumber = await _sut.GetNextAccountNumberAsync();

            // Assert
            Assert.Equal(1, accountNumber);
        }

        [Fact]
        public async Task CreateAccount_ShouldSucceed()
        {
            // Arrange
            var accountAggregate = BankAccountAggregate.Create("John", "Doe", "12345678", "123456");

            // Act
            await _sut.CreateAsync(accountAggregate);

            // Assert            
            var retrievedAccount = await _sut.GetByAccountNumberAsync("12345678", "123456");

            Assert.NotNull(retrievedAccount);
            Assert.Equal("John", retrievedAccount!.FirstName);
            Assert.Equal("Doe", retrievedAccount!.LastName);
            Assert.Equal(0m, retrievedAccount!.Balance.Value);
        }

        [Fact]
        public async Task CreateDuplicateAccount_ShouldThrowException()
        {
            // Arrange
            var accountAggregate = BankAccountAggregate.Create("John", "Doe", "12345678", "123456");

            // Act
            await _sut.CreateAsync(accountAggregate);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(accountAggregate));
        }

        [Fact]
        public async Task UpdateAccount_ShouldSucceed()
        {
            // Arrange
            var accountAggregate = BankAccountAggregate.Create("John", "Doe", "12345678", "123456");

            await _sut.CreateAsync(accountAggregate);

            // Act
            accountAggregate.Deposit(10m);

            await _sut.UpdateAsync(accountAggregate);

            // Assert
            var updatedAccount = await _sut.GetByAccountNumberAsync("12345678", "123456");

            Assert.NotNull(updatedAccount);
            Assert.Equal(10m, updatedAccount!.Balance.Value);
        }
    }
}
