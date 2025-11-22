using MedWay.Application.Abstractions;
using MedWay.Domain;
using MedWay.HospitalOnboarding.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MedWay.HospitalOnboarding.Application.Commands;

/// <summary>
/// Command to reject a hospital registration with reason.
/// Only SystemAdmin or SuperAdmin can reject.
/// </summary>
public record RejectHospitalCommand(
    Guid HospitalId, 
    string Reason, 
    string RejectedBy) : ICommand<Result>;

public class RejectHospitalHandler : IRequestHandler<RejectHospitalCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RejectHospitalHandler> _logger;

    public RejectHospitalHandler(
        IUnitOfWork unitOfWork,
        ILogger<RejectHospitalHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RejectHospitalCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Rejecting hospital {HospitalId} by {RejectedBy}. Reason: {Reason}", 
            command.HospitalId, command.RejectedBy, command.Reason);

        // Validate reason
        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            return Result.Failure(Error.Validation("Reason", "Rejection reason is required"));
        }

        if (command.Reason.Length < 10)
        {
            return Result.Failure(Error.Validation("Reason", "Rejection reason must be at least 10 characters"));
        }

        // Retrieve hospital
        var hospital = await _unitOfWork.Hospitals.GetByIdAsync(command.HospitalId, cancellationToken);
        if (hospital == null)
        {
            _logger.LogWarning("Hospital {HospitalId} not found", command.HospitalId);
            return Result.Failure(Error.NotFound("Hospital", command.HospitalId));
        }

        // Reject hospital (domain logic validates status)
        var rejectResult = hospital.Reject(command.Reason);
        if (rejectResult.IsFailure)
        {
            _logger.LogWarning("Failed to reject hospital {HospitalId}: {Error}", 
                command.HospitalId, rejectResult.Error);
            return rejectResult;
        }

        // Persist changes
        _unitOfWork.Hospitals.Update(hospital);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Hospital {HospitalId} rejected successfully", command.HospitalId);

        // TODO: Send rejection email with reason to hospital admin
        // TODO: Publish HospitalRejectedEvent to integration event bus

        return Result.Success();
    }
}
