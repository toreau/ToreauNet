namespace CoI.Models;

public record PaymentResult(bool Success, string TransactionId, string? Message);
