using e_commerce_web_customer.Application.Contracts;
using e_commerce_web_customer.Data;
using e_commerce_web_customer.ViewModels.Shared;
using Microsoft.EntityFrameworkCore;

namespace e_commerce_web_customer.Infrastructure.Account.Db;

public sealed class DbHeaderAccountDataService(EcommerceDbContext dbContext) : IHeaderAccountDataService
{
    public async Task<HeaderAccountViewModel> GetAccountAsync(
        string email,
        string displayName,
        string? phoneNumber = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedEmail = email.Trim();
        var emailLower = normalizedEmail.ToLowerInvariant();
        var user = await dbContext.Users
            .AsNoTracking()
            .Where(account => account.Email.ToLower() == emailLower)
            .Select(account => new
            {
                account.FullName,
                account.Phone
            })
            .FirstOrDefaultAsync(cancellationToken);

        var resolvedDisplayName = string.IsNullOrWhiteSpace(user?.FullName)
            ? displayName.Trim()
            : user.FullName.Trim();
        var resolvedPhoneNumber = string.IsNullOrWhiteSpace(phoneNumber)
            ? user?.Phone?.Trim()
            : phoneNumber.Trim();

        return new HeaderAccountViewModel
        {
            IsLoggedIn = true,
            DisplayName = ToButtonName(resolvedDisplayName),
            FullName = resolvedDisplayName,
            Email = normalizedEmail,
            PhoneNumber = resolvedPhoneNumber,
            Notifications = []
        };
    }

    private static string ToButtonName(string fullName)
    {
        var lastPart = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        return string.IsNullOrWhiteSpace(lastPart) ? fullName : lastPart;
    }
}
