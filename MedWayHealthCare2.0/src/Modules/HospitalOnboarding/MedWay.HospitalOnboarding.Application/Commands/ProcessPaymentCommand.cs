using MedWay.Application.Abstractions;
using MedWay.Domain;
using MedWay.HospitalOnboarding.Domain.Entities;
using MedWay.HospitalOnboarding.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Nalam360.Platform.Core.Abstractions;

namespace MedWay.HospitalOnboarding.Application.Commands;

/// <summary>
/// Command to process a subscription payment for a hospital.
/// Creates payment record and initiates payment gateway transaction.
/// </summary>
public record ProcessPaymentCommand(
    Guid HospitalId,
    Guid? SubscriptionPlanId,
    decimal Amount,
    string Currency,
    PaymentMethod PaymentMethod,
    string? PaymentGateway,
    Dictionary<string, string>? PaymentMetadata) : ICommand<Result<Guid>>;

public class ProcessPaymentHandler : IRequestHandler<ProcessPaymentCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGuidProvider _guidProvider;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger<ProcessPaymentHandler> _logger;

    public ProcessPaymentHandler(
        IUnitOfWork unitOfWork,
        IGuidProvider guidProvider,
        ITimeProvider timeProvider,
        ILogger<ProcessPaymentHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _guidProvider = guidProvider;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing payment for hospital {HospitalId}. Amount: {Amount} {Currency}", 
            command.HospitalId, command.Amount, command.Currency);

        // Retrieve hospital
        var hospital = await _unitOfWork.Hospitals.GetByIdAsync(command.HospitalId, cancellationToken);
        if (hospital == null)
        {
            _logger.LogWarning("Hospital {HospitalId} not found", command.HospitalId);
            return Result<Guid>.Failure(Error.NotFound("Hospital", command.HospitalId));
        }

        // Validate subscription plan if provided
        if (command.SubscriptionPlanId.HasValue)
        {
            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(
                command.SubscriptionPlanId.Value, cancellationToken);
            
            if (plan == null)
            {
                return Result<Guid>.Failure(Error.NotFound("SubscriptionPlan", command.SubscriptionPlanId.Value));
            }
        }

        // Generate invoice number: INV-{HospitalId}-{YYYYMMDD}-{Random4Digits}
        var invoiceNumber = $"INV-{command.HospitalId.ToString().Substring(0, 8).ToUpper()}-{_timeProvider.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

        // Calculate billing period (current month)
        var billingStart = new DateTime(_timeProvider.UtcNow.Year, _timeProvider.UtcNow.Month, 1);
        var billingEnd = billingStart.AddMonths(1).AddDays(-1);

        // Calculate due date (15 days from now)
        var dueDate = _timeProvider.UtcNow.AddDays(15);

        // Create payment entity
        var paymentId = _guidProvider.NewGuid();
        var paymentResult = Payment.Create(
            paymentId,
            command.HospitalId,
            command.SubscriptionPlanId,
            invoiceNumber,
            command.Amount,
            command.Currency,
            command.PaymentMethod,
            billingStart,
            billingEnd,
            dueDate,
            command.PaymentGateway,
            command.PaymentMetadata);

        if (paymentResult.IsFailure)
        {
            _logger.LogWarning("Failed to create payment: {Error}", paymentResult.Error);
            return Result<Guid>.Failure(paymentResult.Error);
        }

        var payment = paymentResult.Value;

        // TODO: Integrate with payment gateway (Stripe, Razorpay, etc.)
        // For now, simulate immediate success for development
        // var gatewayResult = await _paymentGatewayService.ProcessPaymentAsync(payment, cancellationToken);
        
        // Simulate payment processing
        var transactionId = $"TXN-{_guidProvider.NewGuid().ToString().Substring(0, 16).ToUpper()}";
        
        // Mark as successful (in production, this would be based on gateway response)
        var successResult = payment.MarkAsSuccessful(transactionId, _timeProvider.UtcNow);
        if (successResult.IsFailure)
            return Result<Guid>.Failure(successResult.Error);

        // Persist payment
        await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment {PaymentId} processed successfully for hospital {HospitalId}. Transaction: {TransactionId}", 
            payment.Id, command.HospitalId, transactionId);

        // TODO: Send payment receipt email
        // TODO: Update subscription status if payment was for activation
        // TODO: Publish PaymentSuccessfulEvent to integration event bus

        return Result<Guid>.Success(payment.Id);
    }
}
