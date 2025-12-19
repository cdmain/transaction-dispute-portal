using TransactionService.Models;

namespace TransactionService.Services;

public interface ITransactionService
{
    Task<PagedResult<Transaction>> GetTransactionsAsync(TransactionQueryParameters parameters);
    Task<Transaction?> GetTransactionByIdAsync(Guid id);
    Task<IEnumerable<Transaction>> GetTransactionsByCustomerIdAsync(string customerId);
    Task<bool> MarkAsDisputedAsync(Guid id);
    Task<bool> UnmarkAsDisputedAsync(Guid id);
    Task<IEnumerable<string>> GetCategoriesAsync();
}
