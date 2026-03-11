using CoI.Models;

namespace CoI.Interfaces;

public interface IPaymentGateway
{
    string Name { get; }
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);
}