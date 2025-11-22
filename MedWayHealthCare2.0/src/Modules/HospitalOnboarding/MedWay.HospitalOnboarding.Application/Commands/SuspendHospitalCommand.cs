using MedWay.Application.Abstractions;
using MedWay.Domain;
using MedWay.HospitalOnboarding.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MedWay.HospitalOnboarding.Application.Commands;

/// <summary>
/// Command to suspend an active hospital (e.g., non-payment, policy violation).
/// Only SystemAdmin or SuperAdmin can suspend.
/// </summary>
public record SuspendHospitalCommand(
    Guid HospitalId, 
    string Reason, 
    string SuspendedBy) : ICommand<Result>;

public class SuspendHospitalHandler : IRequestHandler<SuspendHospitalCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SuspendHospitalHandler> _logger;

    public SuspendHospitalHandler(
        IUnitOfWork unitOfWork,
        ILogger<SuspendHospitalHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(SuspendHospitalCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Suspending hospital {HospitalId} by {SuspendedBy}. Reason: {Reason}", 
            command.HospitalId, command.SuspendedBy, command.Reason);

        // Validate reason
        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            return Result.Failure(Error.Validation("Reason", "Suspension reason is required"));
        }

        // Retrieve hospital
        var hospital = await _unitOfWork.Hospitals.GetByIdAsync(command.HospitalId, cancellationToken);
        if (hospital == null)
        {
            _logger.LogWarning("Hospital {HospitalId} not found", command.HospitalId);
            return Result.Failure(Error.NotFound("Hospital", command.HospitalId));
        }

        // Suspend hospital (domain logic validates status)
        var suspendResult = hospital.Suspend(command.Reason);
        if (suspendResult.IsFailure)
        {
            _logger.LogWarning("Failed to suspend hospital {HospitalId}: {Error}", 
                command.HospitalId, suspendResult.Error);
            return suspendResult;
        }

        // Persist changes
        _unitOfWork.Hospitals.Update(hospital);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Hospital {HospitalId} suspended successfully", command.HospitalId);

        // TODO: Send suspension notification email to hospital admin
        // TODO: Publish HospitalSuspendedEvent to integration event bus
        // TODO: Disable user access for hospital staff

        return Result.Success();
    }
}
