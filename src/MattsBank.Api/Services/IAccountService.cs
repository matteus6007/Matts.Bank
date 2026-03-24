using ErrorOr;

using MattsBank.Api.Contracts;

namespace MattsBank.Api.Services
{
    public interface IAccountService
    {
        Task<ErrorOr<Account>> CreateAccountAsync(CreateAccountRequest request);
        Task<ErrorOr<Account>> GetAccountAsync(string accountNumber, string sortCode);
        Task<ErrorOr<Success>> DepositAsync(string accountNumber, string sortCode, decimal amount);
        Task<ErrorOr<Success>> WithdrawAsync(string accountNumber, string sortCode, decimal amount);
    }
}
