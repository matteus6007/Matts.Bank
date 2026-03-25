using ErrorOr;

using MattsBank.Api.Services;
using MattsBank.Domain.Aggregates;
using MattsBank.Infrastructure.Repositories;

using Microsoft.Extensions.Options;

using NSubstitute;

namespace MattsBank.Tests.Services
{
    public class AccountServiceTests
    {
        private readonly IAccountRepository _accountRepository;
        private readonly AccountService _sut;

        public AccountServiceTests()
        {
            _accountRepository = Substitute.For<IAccountRepository>();

            var options = Options.Create(new Api.Options.BankOptions { SortCode = "123456" });

            _sut = new AccountService(_accountRepository, options);
        }

        [Fact]
        public async Task CreateAccountAsync_Should_Create_Account_Successfully()
        {
            // Arrange
            var request = new Api.Contracts.CreateAccountRequest
            {
                FirstName = "John",
                LastName = "Doe"
            };
            _accountRepository.GetNextAccountNumberAsync().Returns(12345678);

            // Act
            var result = await _sut.CreateAccountAsync(request);

            // Assert
            Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Equal("John", result.Value.FirstName);
            Assert.Equal("Doe", result.Value.LastName);
            Assert.Equal("12345678", result.Value.AccountNumber);
            Assert.Equal("123456", result.Value.SortCode);
            Assert.Equal(0m, result.Value.Balance);
        }

        [Fact]
        public async Task GetAccountAsync_Should_Return_Account_Successfully()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";
            var aggregate = BankAccountAggregate.Create("Matt", "Jones", accountNumber, sortCode);
            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns(aggregate);

            // Act
            var result = await _sut.GetAccountAsync(accountNumber, sortCode);

            // Assert
            Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Equal("Matt", result.Value.FirstName);
            Assert.Equal("Jones", result.Value.LastName);
            Assert.Equal(accountNumber, result.Value.AccountNumber);
            Assert.Equal(sortCode, result.Value.SortCode);
        }

        [Fact]
        public async Task GetAccountAsync_AccountDoesNotExist_Should_Return_NotFound()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";

            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns((BankAccountAggregate?)null);

            // Act
            var result = await _sut.GetAccountAsync(accountNumber, sortCode);

            // Assert  
            Assert.True(result.IsError);
            Assert.True(result.Errors.First().Type == ErrorType.NotFound);
            Assert.Equal("Account not found.", result.Errors.First().Description);
        }

        [Fact]
        public async Task DepositAsync_Should_Deposit_Amount_Successfully()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";
            var amount = 100m;
            var aggregate = BankAccountAggregate.Create("Matt", "Jones", accountNumber, sortCode);
            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns(aggregate);

            // Act
            var result = await _sut.DepositAsync(accountNumber, sortCode, amount);

            // Assert
            Assert.False(result.IsError);
        }

        [Fact]
        public async Task DepositAsync_WhenAccountDoesNotExist_Should_Return_NotFound()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";
            var amount = 100m;
            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns((BankAccountAggregate?)null);

            // Act
            var result = await _sut.DepositAsync(accountNumber, sortCode, amount);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(ErrorType.NotFound, result.Errors.First().Type);
            Assert.Equal("Account not found.", result.Errors.First().Description);
        }

        [Fact]
        public async Task WithdrawAsync_Should_Withdraw_Amount_Successfully()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";
            var amount = 100m;
            var aggregate = BankAccountAggregate.Create("Matt", "Jones", accountNumber, sortCode);
            aggregate.Deposit(200m);
            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns(aggregate);

            // Act
            var result = await _sut.WithdrawAsync(accountNumber, sortCode, amount);

            // Assert
            Assert.False(result.IsError);
        }

        [Fact]
        public async Task WithdrawAsync_WhenAccountDoesNotExist_Should_Return_NotFound()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";
            var amount = 100m;
            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns((BankAccountAggregate?)null);

            // Act
            var result = await _sut.WithdrawAsync(accountNumber, sortCode, amount);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(ErrorType.NotFound, result.Errors.First().Type);
            Assert.Equal("Account not found.", result.Errors.First().Description);
        }

        [Fact]
        public async Task WithdrawAsync_WhenNotEnoughFund_Should_Return_Conflict()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";
            var amount = 100m;
            var aggregate = BankAccountAggregate.Create("Matt", "Jones", accountNumber, sortCode);
            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns(aggregate);

            // Act
            var result = await _sut.WithdrawAsync(accountNumber, sortCode, amount);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(ErrorType.Conflict, result.Errors.First().Type);
            Assert.Equal("Insufficient funds.", result.Errors.First().Description);
        }
    }
}
