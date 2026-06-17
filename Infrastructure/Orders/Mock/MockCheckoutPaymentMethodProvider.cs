using e_commerce_web_customer.Application.Contracts;
using e_commerce_web_customer.ViewModels.Checkout;

namespace e_commerce_web_customer.Infrastructure.Orders.Mock;

public sealed class MockCheckoutPaymentMethodProvider : ICheckoutPaymentMethodProvider
{
    private static readonly IReadOnlyList<CheckoutPaymentMethodViewModel> Methods =
    [
        new()
        {
            Id = 1,
            Name = "Tiền mặt",
            Description = "Thanh toán khi nhận hàng",
            IconKey = "cod"
        },
        new()
        {
            Id = 2,
            Name = "Chuyển khoản",
            Description = "Ngân hàng nội địa",
            IconKey = "banktransfer"
        },
        new()
        {
            Id = 3,
            Name = "MoMo",
            Description = "Ví điện tử",
            IconKey = "momo"
        },
        new()
        {
            Id = 4,
            Name = "VNPay",
            Description = "Cổng thanh toán",
            IconKey = "vnpay"
        },
        new()
        {
            Id = 5,
            Name = "ZaloPay",
            Description = "Ví điện tử",
            IconKey = "zalopay"
        }
    ];

    public Task<IReadOnlyList<CheckoutPaymentMethodViewModel>> GetActivePaymentMethodsAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Methods);
    }
}
