using e_commerce_web_customer.Application.Account;
using e_commerce_web_customer.Application.Contracts;
using e_commerce_web_customer.ViewModels.Shared;

namespace e_commerce_web_customer.Infrastructure.Account.Db;

public sealed class DbHeaderAccountProvider(
    IHeaderAccountDataService headerAccountDataService) : IHeaderAccountProvider
{
    public Task<HeaderAccountViewModel> GetAccountAsync(
        bool isLoggedIn,
        string? email,
        string? displayName,
        string? phoneNumber = null,
        CancellationToken cancellationToken = default)
    {
        if (!isLoggedIn)
        {
            return Task.FromResult(new HeaderAccountViewModel());
        }

        var resolvedEmail = email?.Trim();
        var resolvedDisplayName = string.IsNullOrWhiteSpace(displayName)
            ? resolvedEmail ?? "Thành viên"
            : displayName.Trim();
        var resolvedPhoneNumber = string.IsNullOrWhiteSpace(phoneNumber)
            ? null
            : phoneNumber.Trim();

        if (string.IsNullOrWhiteSpace(resolvedEmail))
        {
            return Task.FromResult(new HeaderAccountViewModel
            {
                IsLoggedIn = true,
                DisplayName = ToButtonName(resolvedDisplayName),
                FullName = resolvedDisplayName,
                PhoneNumber = resolvedPhoneNumber
            });
        }

        return headerAccountDataService.GetAccountAsync(
            resolvedEmail,
            resolvedDisplayName,
            resolvedPhoneNumber,
            cancellationToken);
    }

    private static string ToButtonName(string fullName)
    {
        var lastPart = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        return string.IsNullOrWhiteSpace(lastPart) ? fullName : lastPart;
    }
}
