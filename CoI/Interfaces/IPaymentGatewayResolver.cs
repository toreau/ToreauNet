using CoI.Models;

namespace CoI.Interfaces;

public interface IPaymentGatewayResolver
{
    PaymentGatewayKey Resolve(PaymentRequest request);
}