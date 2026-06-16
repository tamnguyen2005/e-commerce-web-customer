namespace e_commerce_web_customer.ViewModels.Shared;

public sealed class HeaderAccountViewModel
{
    public bool IsLoggedIn { get; init; }
    public string DisplayName { get; init; } = "Đăng nhập";
    public string FullName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string MemberUrl { get; init; } = "/account/profile";
    public string MemberLogoUrl { get; init; } = "/images/logo-techstore-icon.svg";
    public string ReadStatusText { get; init; } = "Bạn đã đọc tất cả thông báo ✓✓";
    public IReadOnlyList<HeaderAccountNotificationViewModel> Notifications { get; init; } = [];

    public bool HasNotifications => Notifications.Count > 0;
    public bool HasMaskedPhoneNumber => !string.IsNullOrWhiteSpace(MaskedPhoneNumber);
    public string MaskedPhoneNumber => MaskPhoneNumber(PhoneNumber);

    private static string MaskPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return string.Empty;
        }

        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        if (digits.Length < 5)
        {
            return phoneNumber.Trim();
        }

        var prefixLength = Math.Min(3, digits.Length - 2);
        return $"{digits[..prefixLength]}*****{digits[^2..]}";
    }
}

public sealed class HeaderAccountNotificationViewModel
{
    public required string Id { get; init; }
    public required string Category { get; init; }
    public required string LeadText { get; init; }
    public required string HighlightText { get; init; }
    public required string TailText { get; init; }
    public required string TimeAgoText { get; init; }
    public required string DetailUrl { get; init; }
    public bool IsRead { get; init; } = true;
}
