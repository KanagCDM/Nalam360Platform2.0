using MedWay.Application.Abstractions;
using MedWay.Domain;
using MedWay.HospitalOnboarding.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MedWay.HospitalOnboarding.Application.Commands;

/// <summary>
/// Command to activate a subscription for a hospital after trial period.
/// Hospital must be approved and can select a subscription plan.
/// </summary>
public record ActivateSubscriptionCommand(
    Guid HospitalId,
    Guid SubscriptionPlanId,
    int NumberOfUsers,
    int NumberOfBranches,
    List<Guid> AdditionalFacilityIds) : ICommand<Result<decimal>>;

public class ActivateSubscriptionHandler : IRequestHandler<ActivateSubscriptionCommand, Result<decimal>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateSubscriptionHandler> _logger;

    public ActivateSubscriptionHandler(
        IUnitOfWork unitOfWork,
        ILogger<ActivateSubscriptionHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<decimal>> Handle(ActivateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating subscription for hospital {HospitalId} with plan {SubscriptionPlanId}", 
            command.HospitalId, command.SubscriptionPlanId);

        // Retrieve hospital
        var hospital = await _unitOfWork.Hospitals.GetByIdAsync(command.HospitalId, cancellationToken);
        if (hospital == null)
        {
            _logger.LogWarning("Hospital {HospitalId} not found", command.HospitalId);
            return Result<decimal>.Failure(Error.NotFound("Hospital", command.HospitalId));
        }

        // Retrieve subscription plan
        var plan = await _unitOfWork.SubscriptionPlans.GetWithFacilitiesAsync(command.SubscriptionPlanId, cancellationToken);
        if (plan == null)
        {
            _logger.LogWarning("Subscription plan {PlanId} not found", command.SubscriptionPlanId);
            return Result<decimal>.Failure(Error.NotFound("SubscriptionPlan", command.SubscriptionPlanId));
        }

        if (!plan.IsActive)
        {
            return Result<decimal>.Failure(Error.Validation("SubscriptionPlan", "Selected subscription plan is not active"));
        }

        // Validate user and branch limits
        if (command.NumberOfUsers < 1)
        {
            return Result<decimal>.Failure(Error.Validation("NumberOfUsers", "At least 1 user is required"));
        }

        if (command.NumberOfBranches < 1)
        {
            return Result<decimal>.Failure(Error.Validation("NumberOfBranches", "At least 1 branch is required"));
        }

        // Get facility costs for additional facilities
        var facilityCosts = new Dictionary<Guid, decimal>();
        if (command.AdditionalFacilityIds.Any())
        {
            var facilities = await _unitOfWork.GlobalFacilities.GetByIdsAsync(
                command.AdditionalFacilityIds, cancellationToken);
            
            foreach (var facility in facilities)
            {
                facilityCosts[facility.Id] = facility.BaseMonthlyCost;
            }
        }

        // Calculate monthly cost using plan's pricing logic
        var includedFacilityIds = plan.GetIncludedFacilityIds();
        var monthlyCost = plan.CalculateMonthlyCost(
            command.NumberOfUsers,
            command.NumberOfBranches,
            command.AdditionalFacilityIds,
            facilityCosts);

        // Activate subscription on hospital
        var activateResult = hospital.ActivateSubscription(
            command.SubscriptionPlanId,
            monthlyCost,
            command.NumberOfUsers,
            command.NumberOfBranches);

        if (activateResult.IsFailure)
        {
            _logger.LogWarning("Failed to activate subscription for hospital {HospitalId}: {Error}", 
                command.HospitalId, activateResult.Error);
            return Result<decimal>.Failure(activateResult.Error);
        }

        // Persist changes
        _unitOfWork.Hospitals.Update(hospital);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Subscription activated for hospital {HospitalId}. Monthly cost: {MonthlyCost}", 
            command.HospitalId, monthlyCost);

        // TODO: Send subscription confirmation email
        // TODO: Create first payment invoice
        // TODO: Publish SubscriptionActivatedEvent to integration event bus

        return Result<decimal>.Success(monthlyCost);
    }
}
