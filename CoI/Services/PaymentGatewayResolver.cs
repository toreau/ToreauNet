using CoI.Interfaces;
using CoI.Models;

namespace CoI.Services;

public sealed class PaymentGatewayResolver : IPaymentGatewayResolver
{
    public PaymentGatewayKey Resolve(PaymentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.OrderId))
            throw new ArgumentException("OrderId is required.", nameof(request));

        if (request.Amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(request), "Amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new ArgumentException("Currency is required.", nameof(request));

        return request.Currency.ToUpperInvariant() switch
        {
            "NOK" => PaymentGatewayKey.Vipps,
            "USD" or "EUR" => PaymentGatewayKey.PayPal,
            _ => throw new InvalidOperationException(
                $"No payment gateway is configured for currency '{request.Currency}'.")
        };
    }
}