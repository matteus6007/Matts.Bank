using ErrorOr;

using MattsBank.Api.Services;
using MattsBank.Domain.Aggregates;
using MattsBank.Domain.Entities;
using MattsBank.Domain.ValueObjects;
using MattsBank.Infrastructure.Repositories;

using Microsoft.Extensions.Options;

using NSubstitute;

namespace MattsBank.Tests.Services
{
    public class AccountServiceTests
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly AccountService _sut;

        public AccountServiceTests()
        {
            _accountRepository = Substitute.For<IAccountRepository>();
            _transactionRepository = Substitute.For<ITransactionRepository>();

            var options = Options.Create(new Api.Options.BankOptions { SortCode = "123456" });

            _sut = new AccountService(_accountRepository, _transactionRepository, options);
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

            var account = new Account(
                Guid.NewGuid(),
                accountNumber,
                sortCode,
                "Matt",
                "Jones",
                DateTime.UtcNow,
                0,
                new Domain.ValueObjects.Version());
            var aggregate = BankAccountAggregate.Recreate(account);

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

            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns(Error.NotFound(description: "Account does not exist"));

            // Act
            var result = await _sut.GetAccountAsync(accountNumber, sortCode);

            // Assert  
            Assert.True(result.IsError);
            Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
            Assert.Equal("Account does not exist", result.FirstError.Description);
        }

        [Fact]
        public async Task DepositAsync_Should_Deposit_Amount_Successfully()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";
            var amount = 100m;

            var account = new Account(
                Guid.NewGuid(),
                accountNumber,
                sortCode,
                "Matt",
                "Jones",
                DateTime.UtcNow,
                0,
                new Domain.ValueObjects.Version());
            var aggregate = BankAccountAggregate.Recreate(account);

            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns(aggregate);

            // Act
            var result = await _sut.DepositAsync(accountNumber, sortCode, amount);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(100m, aggregate.Balance.Value);
            Assert.Equal(100m, aggregate.Transactions[0].Amount);
            Assert.Equal(TransactionType.Deposit, aggregate.Transactions[0].TransactionType);
        }

        [Fact]
        public async Task DepositAsync_WhenAccountDoesNotExist_Should_Return_NotFound()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";
            var amount = 100m;
            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns(Error.NotFound(description: "Account does not exist"));

            // Act
            var result = await _sut.DepositAsync(accountNumber, sortCode, amount);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
            Assert.Equal("Account does not exist", result.FirstError.Description);
        }

        [Fact]
        public async Task WithdrawAsync_Should_Withdraw_Amount_Successfully()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";
            var amount = 100m;

            var account = new Account(
                Guid.NewGuid(),
                accountNumber,
                sortCode,
                "Matt",
                "Jones",
                DateTime.UtcNow,
                200m,
                new Domain.ValueObjects.Version());
            var aggregate = BankAccountAggregate.Recreate(account);

            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns(aggregate);

            // Act
            var result = await _sut.WithdrawAsync(accountNumber, sortCode, amount);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(100m, aggregate.Balance.Value);
            Assert.Equal(-100m, aggregate.Transactions[0].Amount);
            Assert.Equal(TransactionType.Withdrawal, aggregate.Transactions[0].TransactionType);
        }

        [Fact]
        public async Task WithdrawAsync_WhenAccountDoesNotExist_Should_Return_NotFound()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";
            var amount = 100m;
            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns(Error.NotFound(description: "Account does not exist"));

            // Act
            var result = await _sut.WithdrawAsync(accountNumber, sortCode, amount);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
            Assert.Equal("Account does not exist", result.FirstError.Description);
        }

        [Fact]
        public async Task WithdrawAsync_WhenNotEnoughFund_Should_Return_Conflict()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";
            var amount = 100m;

            var account = new Account(
                Guid.NewGuid(),
                accountNumber,
                sortCode,
                "Matt",
                "Jones",
                DateTime.UtcNow,
                0,
                new Domain.ValueObjects.Version());
            var aggregate = BankAccountAggregate.Recreate(account);

            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns(aggregate);

            // Act
            var result = await _sut.WithdrawAsync(accountNumber, sortCode, amount);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(ErrorType.Conflict, result.FirstError.Type);
            Assert.Equal("Insufficient funds.", result.FirstError.Description);
        }

        [Fact]
        public async Task ReverseAsync_WhenDepositHasBeenMade_Should_Return_Success()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";

            var account = new Account(
                Guid.NewGuid(),
                accountNumber,
                sortCode,
                "Matt",
                "Jones",
                DateTime.UtcNow,
                100m,
                new Domain.ValueObjects.Version());
            var aggregate = BankAccountAggregate.Recreate(account);

            var transaction = new Transaction(Guid.NewGuid(), account.Id, 100m, 100m, DateTime.UtcNow, TransactionType.Deposit, null);

            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns(aggregate);
            _transactionRepository.GetTransactionById(transaction.Id).Returns(transaction);

            // Act
            var result = await _sut.ReverseAsync(accountNumber, sortCode, transaction.Id.Value);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(0m, aggregate.Balance.Value);
            Assert.Equal(-100m, aggregate.Transactions[0].Amount);
            Assert.Equal(TransactionType.Reversal, aggregate.Transactions[0].TransactionType);
            Assert.Equal(transaction.Id, aggregate.Transactions[0].OriginalTransactionId);
        }

        [Fact]
        public async Task ReverseAsync_WhenWithdrawalHasBeenMade_Should_Return_Success()
        {
            // Arrange
            var accountNumber = "12345678";
            var sortCode = "123456";

            var account = new Account(
                Guid.NewGuid(),
                accountNumber,
                sortCode,
                "Matt",
                "Jones",
                DateTime.UtcNow,
                100m,
                new Domain.ValueObjects.Version());
            var aggregate = BankAccountAggregate.Recreate(account);

            var transaction = new Transaction(Guid.NewGuid(), account.Id, -10m, 100m, DateTime.UtcNow, TransactionType.Withdrawal, null);

            _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode).Returns(aggregate);
            _transactionRepository.GetTransactionById(transaction.Id).Returns(transaction);

            // Act
            var result = await _sut.ReverseAsync(accountNumber, sortCode, transaction.Id.Value);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(110m, aggregate.Balance.Value);
            Assert.Equal(10m, aggregate.Transactions[0].Amount);
            Assert.Equal(TransactionType.Reversal, aggregate.Transactions[0].TransactionType);
            Assert.Equal(transaction.Id, aggregate.Transactions[0].OriginalTransactionId);
        }
    }
}
