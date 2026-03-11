using CoI.Models;

namespace CoI.Interfaces;

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(PaymentRequest request);
}