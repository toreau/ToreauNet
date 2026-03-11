using CoI.Factories;
using CoI.Gateways;
using CoI.Interfaces;
using CoI.Models;
using CoI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
    });
});

services.AddKeyedTransient<IPaymentGateway, VippsPaymentGateway>(PaymentGatewayKey.Vipps);
services.AddKeyedTransient<IPaymentGateway, PayPalPaymentGateway>(PaymentGatewayKey.PayPal);

services.AddTransient<IPaymentGatewayResolver, PaymentGatewayResolver>();
services.AddTransient<IPaymentGatewayFactory, PaymentGatewayFactory>();
services.AddTransient<PaymentService>();

await using var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("Startup");

logger.LogInformation("Application starting up");

var paymentService = serviceProvider.GetRequiredService<PaymentService>();

var nokRequest = new PaymentRequest(
    OrderId: Guid.NewGuid().ToString(),
    Amount: 100.00m,
    Currency: "NOK");

var nokResult = await paymentService.ProcessPaymentAsync(nokRequest);

logger.LogInformation(
    "Finished NOK payment. Success: {Success}, TransactionId: {TransactionId}, Error: {Error}",
    nokResult.Success,
    nokResult.TransactionId,
    nokResult.Message);

var usdRequest = new PaymentRequest(
    OrderId: Guid.NewGuid().ToString(),
    Amount: 200.00m,
    Currency: "USD");

var usdResult = await paymentService.ProcessPaymentAsync(usdRequest);

logger.LogInformation(
    "Finished USD payment. Success: {Success}, TransactionId: {TransactionId}, Error: {Error}",
    usdResult.Success,
    usdResult.TransactionId,
    usdResult.Message);

var unsupportedRequest = new PaymentRequest(
    OrderId: Guid.NewGuid().ToString(),
    Amount: 300.00m,
    Currency: "JPY");

try
{
    var unsupportedResult = await paymentService.ProcessPaymentAsync(unsupportedRequest);

    logger.LogInformation(
        "Finished JPY payment. Success: {Success}, TransactionId: {TransactionId}, Error: {Error}",
        unsupportedResult.Success,
        unsupportedResult.TransactionId,
        unsupportedResult.Message);
}
catch (InvalidOperationException ex)
{
    logger.LogError(ex, "Failed to process payment for unsupported currency");
}