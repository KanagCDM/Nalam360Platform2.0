using MedWay.Domain.Primitives;

namespace MedWay.HospitalOnboarding.Domain.Entities;

/// <summary>
/// Subscription plan entity - defines pricing tiers based on facilities, users, branches
/// System Admins configure subscription plans
/// </summary>
public sealed class SubscriptionPlan : Entity<Guid>
{
    private readonly List<SubscriptionPlanFacility> _includedFacilities = new();

    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!; // e.g., "BASIC", "STANDARD", "PREMIUM"
    public string? Description { get; private set; }
    
    /// <summary>
    /// Base monthly price (before facility and user multipliers)
    /// </summary>
    public decimal BaseMonthlyPrice { get; private set; }
    
    /// <summary>
    /// Maximum allowed users (0 = unlimited)
    /// </summary>
    public int MaxUsers { get; private set; }
    
    /// <summary>
    /// Maximum allowed branches (0 = unlimited)
    /// </summary>
    public int MaxBranches { get; private set; }
    
    /// <summary>
    /// Additional cost per user beyond base included users
    /// </summary>
    public decimal PricePerAdditionalUser { get; private set; }
    
    /// <summary>
    /// Additional cost per branch beyond base included branches
    /// </summary>
    public decimal PricePerAdditionalBranch { get; private set; }
    
    /// <summary>
    /// Number of users included in base price
    /// </summary>
    public int IncludedUsers { get; private set; }
    
    /// <summary>
    /// Number of branches included in base price
    /// </summary>
    public int IncludedBranches { get; private set; }
    
    public bool IsActive { get; private set; }
    public bool IsPublic { get; private set; } // Public plans shown on website, private for custom enterprise deals
    
    public IReadOnlyCollection<SubscriptionPlanFacility> IncludedFacilities => _includedFacilities.AsReadOnly();
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private SubscriptionPlan() { } // EF Core

    private SubscriptionPlan(
        Guid id,
        string name,
        string code,
        string? description,
        decimal baseMonthlyPrice,
        int maxUsers,
        int maxBranches,
        decimal pricePerAdditionalUser,
        decimal pricePerAdditionalBranch,
        int includedUsers,
        int includedBranches,
        bool isPublic)
        : base(id)
    {
        Name = name;
        Code = code;
        Description = description;
        BaseMonthlyPrice = baseMonthlyPrice;
        MaxUsers = maxUsers;
        MaxBranches = maxBranches;
        PricePerAdditionalUser = pricePerAdditionalUser;
        PricePerAdditionalBranch = pricePerAdditionalBranch;
        IncludedUsers = includedUsers;
        IncludedBranches = includedBranches;
        IsPublic = isPublic;
        IsActive = true;
    }

    /// <summary>
    /// Factory method: Create subscription plan
    /// </summary>
    public static Result<SubscriptionPlan> Create(
        string name,
        string code,
        string? description,
        decimal baseMonthlyPrice,
        int maxUsers,
        int maxBranches,
        decimal pricePerAdditionalUser,
        decimal pricePerAdditionalBranch,
        int includedUsers,
        int includedBranches,
        bool isPublic)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<SubscriptionPlan>.Failure(Error.Validation("SubscriptionPlan.Name", "Plan name is required"));

        if (string.IsNullOrWhiteSpace(code))
            return Result<SubscriptionPlan>.Failure(Error.Validation("SubscriptionPlan.Code", "Plan code is required"));

        if (baseMonthlyPrice < 0)
            return Result<SubscriptionPlan>.Failure(Error.Validation("SubscriptionPlan.Price", "Base monthly price cannot be negative"));

        var plan = new SubscriptionPlan(
            Guid.NewGuid(),
            name,
            code,
            description,
            baseMonthlyPrice,
            maxUsers,
            maxBranches,
            pricePerAdditionalUser,
            pricePerAdditionalBranch,
            includedUsers,
            includedBranches,
            isPublic);

        return Result<SubscriptionPlan>.Success(plan);
    }

    /// <summary>
    /// Update plan details (cannot change code)
    /// </summary>
    public Result Update(
        string name,
        string? description,
        decimal baseMonthlyPrice,
        int maxUsers,
        int maxBranches,
        decimal pricePerAdditionalUser,
        decimal pricePerAdditionalBranch,
        int includedUsers,
        int includedBranches,
        bool isPublic)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("SubscriptionPlan.Name", "Plan name is required"));

        if (baseMonthlyPrice < 0)
            return Result.Failure(Error.Validation("SubscriptionPlan.Price", "Base monthly price cannot be negative"));

        Name = name;
        Description = description;
        BaseMonthlyPrice = baseMonthlyPrice;
        MaxUsers = maxUsers;
        MaxBranches = maxBranches;
        PricePerAdditionalUser = pricePerAdditionalUser;
        PricePerAdditionalBranch = pricePerAdditionalBranch;
        IncludedUsers = includedUsers;
        IncludedBranches = includedBranches;
        IsPublic = isPublic;

        return Result.Success();
    }

    /// <summary>
    /// Add facility to plan (included facilities have no additional cost)
    /// </summary>
    public Result AddFacility(Guid globalFacilityId)
    {
        if (_includedFacilities.Any(f => f.GlobalFacilityId == globalFacilityId))
            return Result.Failure(Error.Conflict("SubscriptionPlan.Facility", "Facility already included in plan"));

        var planFacility = new SubscriptionPlanFacility(Id, globalFacilityId);
        _includedFacilities.Add(planFacility);

        return Result.Success();
    }

    /// <summary>
    /// Remove facility from plan
    /// </summary>
    public Result RemoveFacility(Guid globalFacilityId)
    {
        var facility = _includedFacilities.FirstOrDefault(f => f.GlobalFacilityId == globalFacilityId);
        if (facility is null)
            return Result.Failure(Error.NotFound("SubscriptionPlan.Facility", globalFacilityId));

        _includedFacilities.Remove(facility);
        return Result.Success();
    }

    /// <summary>
    /// Calculate total monthly cost for a hospital
    /// </summary>
    public decimal CalculateMonthlyCost(
        int actualUsers,
        int actualBranches,
        IEnumerable<Guid> additionalFacilityIds,
        Dictionary<Guid, decimal> facilityCosts)
    {
        decimal total = BaseMonthlyPrice;

        // Add cost for additional users
        if (actualUsers > IncludedUsers)
        {
            var additionalUsers = actualUsers - IncludedUsers;
            total += additionalUsers * PricePerAdditionalUser;
        }

        // Add cost for additional branches
        if (actualBranches > IncludedBranches)
        {
            var additionalBranches = actualBranches - IncludedBranches;
            total += additionalBranches * PricePerAdditionalBranch;
        }

        // Add cost for additional facilities (not included in plan)
        var includedFacilityIds = _includedFacilities.Select(f => f.GlobalFacilityId).ToHashSet();
        foreach (var facilityId in additionalFacilityIds)
        {
            if (!includedFacilityIds.Contains(facilityId) && facilityCosts.ContainsKey(facilityId))
            {
                total += facilityCosts[facilityId];
            }
        }

        return total;
    }

    public void Deactivate() => IsActive = false;
    public void Reactivate() => IsActive = true;
}

/// <summary>
/// Junction table: Subscription Plan - Facility
/// </summary>
public sealed class SubscriptionPlanFacility : Entity<Guid>
{
    public Guid SubscriptionPlanId { get; private set; }
    public Guid GlobalFacilityId { get; private set; }

    private SubscriptionPlanFacility() { } // EF Core

    internal SubscriptionPlanFacility(Guid subscriptionPlanId, Guid globalFacilityId)
        : base(Guid.NewGuid())
    {
        SubscriptionPlanId = subscriptionPlanId;
        GlobalFacilityId = globalFacilityId;
    }
}
