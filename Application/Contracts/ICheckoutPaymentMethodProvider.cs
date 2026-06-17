using e_commerce_web_customer.ViewModels.Checkout;

namespace e_commerce_web_customer.Application.Contracts;

public interface ICheckoutPaymentMethodProvider
{
    Task<IReadOnlyList<CheckoutPaymentMethodViewModel>> GetActivePaymentMethodsAsync(
        CancellationToken cancellationToken = default);
}
