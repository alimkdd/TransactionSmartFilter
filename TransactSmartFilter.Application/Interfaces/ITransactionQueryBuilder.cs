using TransactSmartFilter.Domain.Models;

namespace TransactSmartFilter.Application.Interfaces;

public interface ITransactionQueryBuilder
{
    IQueryable<Transaction> Build();
    ITransactionQueryBuilder ForUser(int userId, bool isAdmin = false, List<int> sharedAccountIds = null);
    ITransactionQueryBuilder WithDateRange(DateTime? from, DateTime? to);
    ITransactionQueryBuilder WithAmountRange(decimal? min, decimal? max, decimal? exact = null);
    ITransactionQueryBuilder WithTypes(List<int> typeIds);
    ITransactionQueryBuilder WithStatuses(List<int> statusIds);
    ITransactionQueryBuilder SearchRecipient(string recipientQuery);
    ITransactionQueryBuilder SearchDescription(string descriptionQuery, DateTime? fromDate = null);
    ITransactionQueryBuilder WithPaymentMethods(List<int> paymentMethodIds);
    ITransactionQueryBuilder WithTags(List<int> tagIds);
    ITransactionQueryBuilder SortBy(string sortBy, string sortDirection);
    ITransactionQueryBuilder Paginate(int page, int pageSize);
    ITransactionQueryBuilder LimitResults(int maxResults = 10000);
}