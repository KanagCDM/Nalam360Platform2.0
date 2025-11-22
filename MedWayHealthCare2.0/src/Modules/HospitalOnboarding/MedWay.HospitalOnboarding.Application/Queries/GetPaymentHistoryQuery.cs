using MedWay.Application.Abstractions;
using MedWay.Domain;
using MedWay.HospitalOnboarding.Application.DTOs;
using MedWay.HospitalOnboarding.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MedWay.HospitalOnboarding.Application.Queries;

/// <summary>
/// Query to get payment history for a specific hospital.
/// Returns all payments ordered by creation date (newest first).
/// </summary>
public record GetPaymentHistoryQuery(Guid HospitalId) : IQuery<Result<List<PaymentDto>>>;

public class GetPaymentHistoryHandler : IRequestHandler<GetPaymentHistoryQuery, Result<List<PaymentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPaymentHistoryHandler> _logger;

    public GetPaymentHistoryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetPaymentHistoryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<PaymentDto>>> Handle(
        GetPaymentHistoryQuery query, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving payment history for hospital {HospitalId}", query.HospitalId);

        // Verify hospital exists
        var hospital = await _unitOfWork.Hospitals.GetByIdAsync(query.HospitalId, cancellationToken);
        if (hospital == null)
        {
            _logger.LogWarning("Hospital {HospitalId} not found", query.HospitalId);
            return Result<List<PaymentDto>>.Failure(Error.NotFound("Hospital", query.HospitalId));
        }

        // Get all payments for hospital
        var payments = await _unitOfWork.Payments.GetByHospitalIdAsync(query.HospitalId, cancellationToken);

        // Map to DTOs
        var paymentDtos = payments
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                HospitalId = p.HospitalId,
                SubscriptionPlanId = p.SubscriptionPlanId,
                InvoiceNumber = p.InvoiceNumber,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status.ToString(),
                PaymentMethod = p.PaymentMethod.ToString(),
                TransactionId = p.TransactionId,
                PaymentGateway = p.PaymentGateway,
                BillingPeriodStart = p.BillingPeriodStart,
                BillingPeriodEnd = p.BillingPeriodEnd,
                DueDate = p.DueDate,
                PaidAt = p.PaidAt,
                FailureReason = p.FailureReason,
                CreatedAt = p.CreatedAt
            }).ToList();

        _logger.LogInformation("Retrieved {Count} payments for hospital {HospitalId}", 
            paymentDtos.Count, query.HospitalId);

        return Result<List<PaymentDto>>.Success(paymentDtos);
    }
}
