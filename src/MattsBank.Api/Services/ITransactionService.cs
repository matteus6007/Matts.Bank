using ErrorOr;

using MattsBank.Api.Contracts;

namespace MattsBank.Api.Services
{
    public interface ITransactionService
    {
        Task<ErrorOr<List<Transaction>>> GetTransactions(string accountNumber, string sortCode);
    }
}
