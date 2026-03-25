using ErrorOr;

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
            var aggregate = BankAccountAggregate.Create("John", "Doe", "12345678", "123456");

            // Act
            await _sut.CreateAsync(aggregate);

            // Assert            
            var result = await _sut.GetByAccountNumberAsync("12345678", "123456");

            Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Equal("John", result.Value.FirstName);
            Assert.Equal("Doe", result.Value.LastName);
            Assert.Equal(0m, result.Value.Balance.Value);
            Assert.Equal(1, result.Value.Version);
        }

        [Fact]
        public async Task CreateDuplicateAccount_ShouldReturnConflict()
        {
            // Arrange
            var aggregate = BankAccountAggregate.Create("John", "Doe", "12345678", "123456");

            // Act
            await _sut.CreateAsync(aggregate);

            var result = await _sut.CreateAsync(aggregate);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(ErrorType.Conflict, result.FirstError.Type);
        }

        [Fact]
        public async Task UpdateAccount_ShouldSucceed()
        {
            // Arrange
            var aggregate = BankAccountAggregate.Create("John", "Doe", "12345678", "123456");

            await _sut.CreateAsync(aggregate);

            // Act
            aggregate.Deposit(10m);

            await _sut.UpdateAsync(aggregate);

            // Assert
            var result = await _sut.GetByAccountNumberAsync("12345678", "123456");

            Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Version);
            Assert.Equal(10m, result.Value.Balance.Value);
        }

        [Fact]
        public async Task UpdateAccount_VersionIsOutOfDate_ShouldReturnConflict()
        {
            // Arrange
            var aggregate1 = BankAccountAggregate.Create("John", "Doe", "12345678", "123456");

            await _sut.CreateAsync(aggregate1);

            aggregate1.Deposit(10m);

            var aggregate2 = await _sut.GetByAccountNumberAsync("12345678", "123456");
            aggregate2.Value.Withdraw(10m);

            // Act
            var result1 = await _sut.UpdateAsync(aggregate1);
            var result2 = await _sut.UpdateAsync(aggregate2.Value);

            // Assert
            Assert.False(result1.IsError);
            Assert.True(result2.IsError);
            Assert.Equal(ErrorType.Conflict, result2.FirstError.Type);
            Assert.Equal("Account has been modified by another transaction", result2.FirstError.Description);
        }
    }
}
