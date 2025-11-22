using MedWay.Application.Abstractions;
using MedWay.Domain;
using MedWay.HospitalOnboarding.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MedWay.HospitalOnboarding.Application.Commands;

/// <summary>
/// Command to approve a hospital registration after review.
/// Only SystemAdmin or SuperAdmin can approve.
/// </summary>
public record ApproveHospitalCommand(Guid HospitalId, string ApprovedBy) : ICommand<Result>;

public class ApproveHospitalHandler : IRequestHandler<ApproveHospitalCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApproveHospitalHandler> _logger;

    public ApproveHospitalHandler(
        IUnitOfWork unitOfWork,
        ILogger<ApproveHospitalHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ApproveHospitalCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving hospital {HospitalId} by {ApprovedBy}", 
            command.HospitalId, command.ApprovedBy);

        // Retrieve hospital
        var hospital = await _unitOfWork.Hospitals.GetByIdAsync(command.HospitalId, cancellationToken);
        if (hospital == null)
        {
            _logger.LogWarning("Hospital {HospitalId} not found", command.HospitalId);
            return Result.Failure(Error.NotFound("Hospital", command.HospitalId));
        }

        // Approve hospital (domain logic validates status)
        var approveResult = hospital.Approve();
        if (approveResult.IsFailure)
        {
            _logger.LogWarning("Failed to approve hospital {HospitalId}: {Error}", 
                command.HospitalId, approveResult.Error);
            return approveResult;
        }

        // Persist changes (domain events will be dispatched automatically)
        _unitOfWork.Hospitals.Update(hospital);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Hospital {HospitalId} approved successfully", command.HospitalId);

        // TODO: Send approval email to hospital admin
        // TODO: Publish HospitalApprovedEvent to integration event bus

        return Result.Success();
    }
}
