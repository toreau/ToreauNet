using CoI.Interfaces;
using CoI.Models;
using Microsoft.Extensions.Logging;

namespace CoI.Services;

public sealed class PaymentService
{
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IPaymentGatewayFactory paymentGatewayFactory, ILogger<PaymentService> logger)
    {
        _paymentGatewayFactory = paymentGatewayFactory ?? throw new ArgumentNullException(nameof(paymentGatewayFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Starting payment processing for OrderId {OrderId} (Amount: {Amount}, Currency: {Currency})",
            request.OrderId,
            request.Amount,
            request.Currency);

        var gateway = _paymentGatewayFactory.GetGateway(request);

        var result = await gateway.ProcessPaymentAsync(request, cancellationToken);

        _logger.LogInformation(
            "Payment processed via {GatewayName} for OrderId {OrderId}. Success: {Success}",
            gateway.Name,
            request.OrderId,
            result.Success);

        return result;
    }
}