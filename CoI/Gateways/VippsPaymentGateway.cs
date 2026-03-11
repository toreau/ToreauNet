using CoI.Interfaces;
using CoI.Models;

namespace CoI.Gateways;

public sealed class VippsPaymentGateway : IPaymentGateway
{
    public string Name => "Vipps";

    public Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = new PaymentResult(
            Success: true,
            TransactionId: $"VIPPS-{Guid.NewGuid():N}",
            Message: null);

        return Task.FromResult(result);
    }
}