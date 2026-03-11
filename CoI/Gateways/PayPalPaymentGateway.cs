using CoI.Interfaces;
using CoI.Models;

namespace CoI.Gateways;

public sealed class PayPalPaymentGateway : IPaymentGateway
{
    public string Name => "PayPal";

    public Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = new PaymentResult(
            Success: true,
            TransactionId: $"PAYPAL-{Guid.NewGuid():N}",
            Message: null);

        return Task.FromResult(result);
    }
}