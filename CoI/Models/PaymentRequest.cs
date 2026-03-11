namespace CoI.Models;

public record PaymentRequest(string OrderId, decimal Amount, string Currency);