using MedWay.Application.Abstractions;
using MedWay.Domain;
using MedWay.HospitalOnboarding.Application.DTOs;
using MedWay.HospitalOnboarding.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MedWay.HospitalOnboarding.Application.Queries;

/// <summary>
/// Query to get all subscription plans.
/// Can filter to show only public plans (for display on website).
/// </summary>
public record GetSubscriptionPlansQuery(bool PublicOnly = false) : IQuery<Result<List<SubscriptionPlanDto>>>;

public class GetSubscriptionPlansHandler : IRequestHandler<GetSubscriptionPlansQuery, Result<List<SubscriptionPlanDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetSubscriptionPlansHandler> _logger;

    public GetSubscriptionPlansHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetSubscriptionPlansHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<SubscriptionPlanDto>>> Handle(
        GetSubscriptionPlansQuery query, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving subscription plans. PublicOnly: {PublicOnly}", query.PublicOnly);

        // Get plans
        var plans = query.PublicOnly
            ? await _unitOfWork.SubscriptionPlans.GetPublicPlansAsync(cancellationToken)
            : await _unitOfWork.SubscriptionPlans.GetAllAsync(cancellationToken);

        // Filter active plans only
        plans = plans.Where(p => p.IsActive).ToList();

        // Map to DTOs
        var planDtos = new List<SubscriptionPlanDto>();

        foreach (var plan in plans.OrderBy(p => p.BaseMonthlyPrice))
        {
            // Get plan with facilities
            var planWithFacilities = await _unitOfWork.SubscriptionPlans.GetWithFacilitiesAsync(
                plan.Id, cancellationToken);

            if (planWithFacilities == null) continue;

            var includedFacilityIds = planWithFacilities.GetIncludedFacilityIds();
            var facilities = await _unitOfWork.GlobalFacilities.GetByIdsAsync(
                includedFacilityIds, cancellationToken);

            planDtos.Add(new SubscriptionPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Code = plan.Code,
                Description = plan.Description,
                BaseMonthlyPrice = plan.BaseMonthlyPrice,
                IncludedUsers = plan.IncludedUsers,
                IncludedBranches = plan.IncludedBranches,
                MaxUsers = plan.MaxUsers,
                MaxBranches = plan.MaxBranches,
                PricePerAdditionalUser = plan.PricePerAdditionalUser,
                PricePerAdditionalBranch = plan.PricePerAdditionalBranch,
                IsPublic = plan.IsPublic,
                IncludedFacilities = facilities.Select(f => new FacilityDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Code = f.Code,
                    Category = f.Category.ToString(),
                    BaseMonthlyCost = f.BaseMonthlyCost
                }).ToList()
            });
        }

        _logger.LogInformation("Retrieved {Count} subscription plans", planDtos.Count);

        return Result<List<SubscriptionPlanDto>>.Success(planDtos);
    }
}
