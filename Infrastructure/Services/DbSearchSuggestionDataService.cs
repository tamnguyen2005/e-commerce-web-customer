using e_commerce_web_customer.Application.Contracts;
using e_commerce_web_customer.Data;
using e_commerce_web_customer.ViewModels.Shared;

namespace e_commerce_web_customer.Infrastructure.Services;

public sealed class DbSearchSuggestionDataService(EcommerceDbContext dbContext) : ISearchSuggestionDataService
{
    public Task<HeaderSearchViewModel> GetInitialSuggestionsAsync(
        CancellationToken cancellationToken = default)
    {
        _ = dbContext;
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new HeaderSearchViewModel());
    }

    public Task<SearchSuggestionResultsViewModel> SearchAsync(
        string? query,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new SearchSuggestionResultsViewModel
        {
            Query = query?.Trim() ?? string.Empty
        });
    }
}
