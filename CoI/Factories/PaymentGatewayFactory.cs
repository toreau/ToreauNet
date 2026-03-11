using CoI.Interfaces;
using CoI.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoI.Factories;

public sealed class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPaymentGatewayResolver _paymentGatewayResolver;
    private readonly ILogger<PaymentGatewayFactory> _logger;

    public PaymentGatewayFactory(
        IServiceProvider serviceProvider,
        IPaymentGatewayResolver paymentGatewayResolver,
        ILogger<PaymentGatewayFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _paymentGatewayResolver = paymentGatewayResolver ?? throw new ArgumentNullException(nameof(paymentGatewayResolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IPaymentGateway GetGateway(PaymentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var gatewayKey = _paymentGatewayResolver.Resolve(request);

        var gateway = _serviceProvider.GetRequiredKeyedService<IPaymentGateway>(gatewayKey);

        _logger.LogInformation(
            "Selected gateway {GatewayName} using key {GatewayKey} for OrderId {OrderId} (Currency: {Currency})",
            gateway.Name,
            gatewayKey,
            request.OrderId,
            request.Currency);

        return gateway;
    }
}