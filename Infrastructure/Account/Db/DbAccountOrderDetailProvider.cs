using System.Globalization;
using e_commerce_web_customer.Application.Account;
using e_commerce_web_customer.Data;
using e_commerce_web_customer.Models.Entities;
using e_commerce_web_customer.Models.Enums;
using e_commerce_web_customer.ViewModels.Account;
using Microsoft.EntityFrameworkCore;

namespace e_commerce_web_customer.Infrastructure.Account.Db;

public sealed class DbAccountOrderDetailProvider(EcommerceDbContext dbContext) : IAccountOrderDetailProvider
{
    private const string FallbackImage = "/images/logo-techstore-icon.svg";
    private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");

    public async Task<AccountOrderDetailViewModel?> GetOrderDetailAsync(
        string? email,
        string? displayName,
        string? phoneNumber,
        string orderCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email?.Trim().ToLowerInvariant() ?? string.Empty;
        var normalizedCode = NormalizeOrderCode(orderCode);
        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(normalizedCode))
        {
            return null;
        }

        var order = await dbContext.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.User)
            .Include(item => item.OrderItems)
                .ThenInclude(item => item.ProductVariant)
                    .ThenInclude(variant => variant!.Product)
            .Include(item => item.OrderItems)
                .ThenInclude(item => item.ProductVariant)
                    .ThenInclude(variant => variant!.ProductVariantImages)
            .Where(item => item.User != null && item.User.Email.ToLower() == normalizedEmail)
            .FirstOrDefaultAsync(
                item => item.OrderCode.ToLower() == normalizedCode.ToLower(),
                cancellationToken);

        if (order is null)
        {
            return null;
        }

        var summaryData = await dbContext.Orders
            .AsNoTracking()
            .Where(item => item.UserId == order.UserId)
            .Where(item => item.OrderStatus != OrderStatus.Cancelled)
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Count = group.Count(),
                Total = group.Sum(item => item.TotalAmount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var user = order.User;
        var resolvedPhone = string.IsNullOrWhiteSpace(order.ShippingPhone)
            ? phoneNumber?.Trim() ?? string.Empty
            : order.ShippingPhone.Trim();
        var summaryPhone = string.IsNullOrWhiteSpace(user?.Phone)
            ? phoneNumber?.Trim() ?? string.Empty
            : user.Phone.Trim();
        var customerName = string.IsNullOrWhiteSpace(order.ShippingContactName)
            ? displayName?.Trim() ?? user?.FullName ?? "Khách hàng TechStore"
            : order.ShippingContactName.Trim();
        var paidAmount = ResolvePaidAmount(order);
        var remainingAmount = Math.Max(0m, order.TotalAmount - paidAmount);
        var discount = Math.Max(0m, order.VoucherDiscount);
        var itemQuantity = order.OrderItems.Sum(item => Math.Max(1, item.Quantity));

        return new AccountOrderDetailViewModel
        {
            Summary = new AccountProfileSummaryViewModel
            {
                FullName = string.IsNullOrWhiteSpace(user?.FullName)
                    ? displayName?.Trim() ?? user?.Username ?? "Thành viên TechStore"
                    : user.FullName.Trim(),
                Email = user?.Email ?? normalizedEmail,
                PhoneNumber = summaryPhone,
                MaskedPhoneNumber = MaskPhoneNumber(summaryPhone),
                AvatarUrl = NormalizeImageUrl(user?.AvatarImage),
                GenderText = user is null ? "-" : GetGenderText(user.Gender),
                PasswordUpdatedAtText = user is null ? "-" : FormatDateTime(user.UpdatedAt ?? user.CreatedAt),
                OrderCountText = (summaryData?.Count ?? 0).ToString("N0", ViCulture),
                TotalSpentText = FormatCurrency(summaryData?.Total ?? 0m)
            },
            OrderCode = "#" + order.OrderCode.TrimStart('#'),
            OrderedDateText = order.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy", ViCulture),
            StatusText = GetStatusText(order.OrderStatus),
            StatusTone = GetStatusTone(order.OrderStatus),
            Items = order.OrderItems.Select(item => ToItemViewModel(item, order.CreatedAt)).ToList(),
            Steps = CreateSteps(order.OrderStatus),
            Customer = new AccountOrderCustomerViewModel
            {
                FullName = customerName,
                PhoneNumber = string.IsNullOrWhiteSpace(resolvedPhone) ? "-" : resolvedPhone,
                AddressText = FormatShippingAddress(order),
                NoteText = "-"
            },
            Support = new AccountOrderSupportViewModel(),
            Payment = new AccountOrderPaymentViewModel
            {
                ProductQuantity = itemQuantity,
                SubtotalText = FormatCurrency(order.SubtotalAmount),
                DiscountText = discount > 0m ? "-" + FormatCurrency(discount) : FormatCurrency(0m),
                HasDiscount = discount > 0m,
                ShippingFeeText = order.ShippingFee <= 0m ? "Miễn phí" : FormatCurrency(order.ShippingFee),
                IsFreeShipping = order.ShippingFee <= 0m,
                TotalText = FormatCurrency(order.TotalAmount),
                PaidText = FormatCurrency(paidAmount),
                RemainingText = FormatCurrency(remainingAmount)
            }
        };
    }

    private static AccountOrderDetailItemViewModel ToItemViewModel(OrderItem item, DateTime orderedAt)
    {
        var variant = item.ProductVariant;
        var product = variant?.Product;
        var image = variant?.ProductVariantImages
            .OrderBy(image => image.Position)
            .FirstOrDefault();

        return new AccountOrderDetailItemViewModel
        {
            ProductName = product?.Name ?? "Sản phẩm TechStore",
            ProductImageUrl = NormalizeImageUrl(image?.ImagePath),
            ProductImageAlt = image?.AltText ?? product?.Name ?? "Sản phẩm TechStore",
            UnitPriceText = FormatCurrency(item.UnitPrice),
            ColorText = variant?.ColorName ?? string.Empty,
            Quantity = Math.Max(1, item.Quantity),
            WarrantyText = "Thời hạn bảo hành đến: " + orderedAt.ToLocalTime().AddYears(5).AddDays(-1).ToString("dd/MM/yyyy", ViCulture)
        };
    }

    private static IReadOnlyList<AccountOrderStepViewModel> CreateSteps(OrderStatus status)
    {
        var firstDone = status != OrderStatus.Cancelled && status != OrderStatus.Returned;
        var readyDone = status is OrderStatus.Confirmed or OrderStatus.Processing or OrderStatus.Shipping or OrderStatus.Completed;
        var receivedDone = status == OrderStatus.Completed;

        return
        [
            new() { Label = "Đặt hàng thành công", IsDone = firstDone },
            new() { Label = "Sẵn hàng", IsDone = readyDone },
            new() { Label = "Đã nhận hàng", IsDone = receivedDone }
        ];
    }

    private static decimal ResolvePaidAmount(Order order)
    {
        if (order.PaymentStatus == PaymentStatus.Paid || order.OrderStatus == OrderStatus.Completed)
        {
            return order.TotalAmount;
        }

        return 0m;
    }

    private static string FormatShippingAddress(Order order)
    {
        var parts = new[]
        {
            order.ShippingDetail,
            order.ShippingWard,
            order.ShippingProvince
        }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim());
        var address = string.Join(", ", parts);
        return string.IsNullOrWhiteSpace(address) ? "-" : address;
    }

    private static string GetStatusText(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Chờ xác nhận",
        OrderStatus.Confirmed => "Đang xử lý",
        OrderStatus.Processing => "Đang xử lý",
        OrderStatus.Shipping => "Đang vận chuyển",
        OrderStatus.Completed => "Đã nhận hàng",
        OrderStatus.Cancelled => "Đã hủy",
        OrderStatus.Returned => "Đã hủy",
        _ => "Đang xử lý"
    };

    private static string GetStatusTone(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "pending",
        OrderStatus.Cancelled or OrderStatus.Returned => "danger",
        _ => "success"
    };

    private static string GetGenderText(Gender gender) => gender switch
    {
        Gender.Male => "Nam",
        Gender.Female => "Nữ",
        Gender.Other => "Khác",
        _ => "-"
    };

    private static string NormalizeOrderCode(string orderCode) =>
        orderCode.Trim().TrimStart('#');

    private static string FormatCurrency(decimal value) =>
        value.ToString("N0", ViCulture) + "đ";

    private static string FormatDateTime(DateTime value) =>
        value.ToLocalTime().ToString("dd/MM/yyyy HH:mm", ViCulture);

    private static string MaskPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return string.Empty;
        }

        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return digits.Length < 5 ? phoneNumber.Trim() : $"{digits[..3]}*****{digits[^2..]}";
    }

    private static string NormalizeImageUrl(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return FallbackImage;
        }

        if (imagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || imagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || imagePath.StartsWith('/'))
        {
            return imagePath;
        }

        return "/" + imagePath.TrimStart('/');
    }
}
