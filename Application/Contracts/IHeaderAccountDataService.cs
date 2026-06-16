using e_commerce_web_customer.ViewModels.Shared;

namespace e_commerce_web_customer.Application.Contracts;

public interface IHeaderAccountDataService
{
    Task<HeaderAccountViewModel> GetAccountAsync(
        string email,
        string displayName,
        string? phoneNumber = null,
        CancellationToken cancellationToken = default);
}
