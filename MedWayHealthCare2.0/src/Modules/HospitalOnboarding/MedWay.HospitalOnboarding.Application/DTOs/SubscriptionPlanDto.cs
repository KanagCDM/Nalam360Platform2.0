namespace MedWay.HospitalOnboarding.Application.DTOs;

/// <summary>
/// Subscription plan DTO with included facilities.
/// </summary>
public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BaseMonthlyPrice { get; set; }
    public int IncludedUsers { get; set; }
    public int IncludedBranches { get; set; }
    public int? MaxUsers { get; set; }
    public int? MaxBranches { get; set; }
    public decimal PricePerAdditionalUser { get; set; }
    public decimal PricePerAdditionalBranch { get; set; }
    public bool IsPublic { get; set; }
    public List<FacilityDto> IncludedFacilities { get; set; } = new();
}

/// <summary>
/// Facility DTO for facility information.
/// </summary>
public class FacilityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal BaseMonthlyCost { get; set; }
}
