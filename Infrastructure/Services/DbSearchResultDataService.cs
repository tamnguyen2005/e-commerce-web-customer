using e_commerce_web_customer.Application.Contracts;
using e_commerce_web_customer.Application.Search;
using e_commerce_web_customer.Data;
using e_commerce_web_customer.ViewModels.Search;

namespace e_commerce_web_customer.Infrastructure.Services;

public sealed class DbSearchResultDataService(EcommerceDbContext dbContext) : ISearchResultDataService
{
    public Task<SearchResultPageViewModel> SearchAsync(
        SearchResultRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = dbContext;
        _ = request;

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new SearchResultPageViewModel
        {
            Query = request.Query?.Trim() ?? string.Empty,
            TotalCount = 0,
            InitialProductCount = 25,
            Categories = [],
            SortOptions = [],
            Products = []
        });
    }
}
