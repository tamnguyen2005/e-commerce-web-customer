using e_commerce_web_customer.ViewModels.Account;

namespace e_commerce_web_customer.Application.Account;

public interface IAccountOrderDetailProvider
{
    Task<AccountOrderDetailViewModel?> GetOrderDetailAsync(
        string? email,
        string? displayName,
        string? phoneNumber,
        string orderCode,
        CancellationToken cancellationToken = default);
}
