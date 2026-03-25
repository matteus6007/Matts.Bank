
using ErrorOr;

using MattsBank.Api.Contracts;
using MattsBank.Infrastructure.Repositories;

namespace MattsBank.Api.Services
{
    public class TransactionService(IAccountRepository accountRepository, ITransactionRepository transactionRepository) : ITransactionService
    {
        private readonly IAccountRepository _accountRepository = accountRepository;
        private readonly ITransactionRepository _transactionRepository = transactionRepository;

        public async Task<ErrorOr<List<Transaction>>> GetTransactions(string accountNumber, string sortCode)
        {
            var aggregate = await _accountRepository.GetByAccountNumberAsync(accountNumber, sortCode);

            if (aggregate.IsError)
            {
                return aggregate.Errors;
            }

            var transactions = await _transactionRepository.GetByAccountIdAsync(aggregate.Value.Id);

            return transactions == null ? [] : transactions.Select(MapFrom).ToList();
        }

        // TODO: move to mapper
        private static Transaction MapFrom(Domain.Entities.Transaction transaction)
        {
            var amount = transaction.TransactionType == Domain.ValueObjects.TransactionType.Deposit
                ? transaction.Amount.Value
                : -1 * transaction.Amount.Value;

            return new Transaction
            {
                Id = transaction.Id,
                Amount = amount,
                ClosingBalance = transaction.ClosingBalance.Value,
                TransactionDate = transaction.TransactionDate,
                TransactionType = transaction.TransactionType.ToString()
            };
        }
    }
}
