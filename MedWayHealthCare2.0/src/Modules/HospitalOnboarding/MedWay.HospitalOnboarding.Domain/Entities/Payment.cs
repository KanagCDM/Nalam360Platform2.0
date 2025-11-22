using MedWay.Domain.Primitives;

namespace MedWay.HospitalOnboarding.Domain.Entities;

/// <summary>
/// Payment entity - tracks subscription payments from hospitals
/// </summary>
public sealed class Payment : AggregateRoot<Guid>
{
    public Guid HospitalId { get; private set; }
    public Guid SubscriptionPlanId { get; private set; }
    
    public string InvoiceNumber { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    
    public PaymentStatus Status { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    
    /// <summary>
    /// Payment gateway transaction ID
    /// </summary>
    public string? TransactionId { get; private set; }
    
    /// <summary>
    /// Payment gateway name (e.g., "Stripe", "PayPal", "Razorpay")
    /// </summary>
    public string? PaymentGateway { get; private set; }
    
    /// <summary>
    /// Period this payment covers
    /// </summary>
    public DateTime BillingPeriodStart { get; private set; }
    public DateTime BillingPeriodEnd { get; private set; }
    
    public DateTime? PaidAt { get; private set; }
    public DateTime DueDate { get; private set; }
    
    /// <summary>
    /// Additional metadata from payment gateway (stored as JSON string)
    /// TODO: Consider creating ValueObject or using EF Core owned entity
    /// </summary>
    public string? PaymentMetadata { get; private set; }
    
    /// <summary>
    /// Failure reason if payment failed
    /// </summary>
    public string? FailureReason { get; private set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private Payment() { } // EF Core

    private Payment(
        Guid id,
        Guid hospitalId,
        Guid subscriptionPlanId,
        string invoiceNumber,
        decimal amount,
        string currency,
        DateTime billingPeriodStart,
        DateTime billingPeriodEnd,
        DateTime dueDate)
        : base(id)
    {
        HospitalId = hospitalId;
        SubscriptionPlanId = subscriptionPlanId;
        InvoiceNumber = invoiceNumber;
        Amount = amount;
        Currency = currency;
        BillingPeriodStart = billingPeriodStart;
        BillingPeriodEnd = billingPeriodEnd;
        DueDate = dueDate;
        Status = PaymentStatus.Pending;
        PaymentMethod = PaymentMethod.Unknown;
    }

    /// <summary>
    /// Factory method: Create payment record
    /// </summary>
    public static Result<Payment> Create(
        Guid hospitalId,
        Guid subscriptionPlanId,
        string invoiceNumber,
        decimal amount,
        string currency,
        DateTime billingPeriodStart,
        DateTime billingPeriodEnd,
        DateTime dueDate)
    {
        if (amount <= 0)
            return Result<Payment>.Failure(Error.Validation("Payment.Amount", "Payment amount must be greater than zero"));

        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return Result<Payment>.Failure(Error.Validation("Payment.InvoiceNumber", "Invoice number is required"));

        if (billingPeriodEnd <= billingPeriodStart)
            return Result<Payment>.Failure(Error.Validation("Payment.BillingPeriod", "Billing period end must be after start"));

        var payment = new Payment(
            Guid.NewGuid(),
            hospitalId,
            subscriptionPlanId,
            invoiceNumber,
            amount,
            currency,
            billingPeriodStart,
            billingPeriodEnd,
            dueDate);

        payment.AddDomainEvent(new PaymentCreatedEvent(
            payment.Id,
            hospitalId,
            amount,
            dueDate));

        return Result<Payment>.Success(payment);
    }

    /// <summary>
    /// Mark payment as successful
    /// </summary>
    public Result MarkAsSuccessful(
        string transactionId,
        string paymentGateway,
        PaymentMethod paymentMethod,
        string? paymentMetadata = null)
    {
        if (Status == PaymentStatus.Successful)
            return Result.Failure(Error.Validation("Payment.Status", "Payment is already marked as successful"));

        if (Status == PaymentStatus.Refunded)
            return Result.Failure(Error.Validation("Payment.Status", "Cannot mark refunded payment as successful"));

        Status = PaymentStatus.Successful;
        TransactionId = transactionId;
        PaymentGateway = paymentGateway;
        PaymentMethod = paymentMethod;
        PaymentMetadata = paymentMetadata;
        PaidAt = DateTime.UtcNow;
        FailureReason = null;

        AddDomainEvent(new PaymentSuccessfulEvent(
            Id,
            HospitalId,
            SubscriptionPlanId,
            Amount,
            TransactionId));

        return Result.Success();
    }

    /// <summary>
    /// Mark payment as failed
    /// </summary>
    public Result MarkAsFailed(string failureReason)
    {
        if (Status == PaymentStatus.Successful)
            return Result.Failure(Error.Validation("Payment.Status", "Cannot mark successful payment as failed"));

        if (Status == PaymentStatus.Refunded)
            return Result.Failure(Error.Validation("Payment.Status", "Cannot mark refunded payment as failed"));

        Status = PaymentStatus.Failed;
        FailureReason = failureReason;

        AddDomainEvent(new PaymentFailedEvent(
            Id,
            HospitalId,
            Amount,
            failureReason));

        return Result.Success();
    }

    /// <summary>
    /// Process refund
    /// </summary>
    public Result Refund(string reason, string? refundTransactionId = null)
    {
        if (Status != PaymentStatus.Successful)
            return Result.Failure(Error.Validation("Payment.Status", "Only successful payments can be refunded"));

        Status = PaymentStatus.Refunded;
        FailureReason = $"Refunded: {reason}";
        PaymentMetadata = $"Refund TxnId: {refundTransactionId}";

        AddDomainEvent(new PaymentRefundedEvent(
            Id,
            HospitalId,
            Amount,
            reason));

        return Result.Success();
    }

    /// <summary>
    /// Retry failed payment
    /// </summary>
    public Result Retry()
    {
        if (Status != PaymentStatus.Failed)
            return Result.Failure(Error.Validation("Payment.Status", "Only failed payments can be retried"));

        Status = PaymentStatus.Pending;
        FailureReason = null;

        AddDomainEvent(new PaymentRetryInitiatedEvent(Id, HospitalId, Amount));

        return Result.Success();
    }
}

/// <summary>
/// Payment status
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Successful = 1,
    Failed = 2,
    Refunded = 3
}

/// <summary>
/// Payment methods
/// </summary>
public enum PaymentMethod
{
    Unknown = 0,
    CreditCard = 1,
    DebitCard = 2,
    BankTransfer = 3,
    UPI = 4,
    Wallet = 5,
    NetBanking = 6,
    Cash = 7,
    Cheque = 8
}
