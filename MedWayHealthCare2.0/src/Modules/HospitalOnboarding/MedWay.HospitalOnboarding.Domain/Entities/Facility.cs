using MedWay.Domain.Primitives;

namespace MedWay.HospitalOnboarding.Domain.Entities;

/// <summary>
/// Global facility entity - master list of all available facilities (e.g., Emergency, ICU, Lab, Pharmacy)
/// System Admins configure this global list
/// </summary>
public sealed class GlobalFacility : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!; // Unique code (e.g., "EMR", "ICU", "LAB")
    public string? Description { get; private set; }
    public FacilityCategory Category { get; private set; }
    public bool IsActive { get; private set; }
    
    /// <summary>
    /// Base monthly cost for this facility (used in subscription calculation)
    /// </summary>
    public decimal BaseMonthlyCost { get; private set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private GlobalFacility() { } // EF Core

    private GlobalFacility(
        Guid id,
        string name,
        string code,
        string? description,
        FacilityCategory category,
        decimal baseMonthlyCost)
        : base(id)
    {
        Name = name;
        Code = code;
        Description = description;
        Category = category;
        BaseMonthlyCost = baseMonthlyCost;
        IsActive = true;
    }

    /// <summary>
    /// Factory method: Create global facility
    /// </summary>
    public static Result<GlobalFacility> Create(
        string name,
        string code,
        string? description,
        FacilityCategory category,
        decimal baseMonthlyCost)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<GlobalFacility>.Failure(Error.Validation("Facility.Name", "Facility name is required"));

        if (string.IsNullOrWhiteSpace(code))
            return Result<GlobalFacility>.Failure(Error.Validation("Facility.Code", "Facility code is required"));

        if (baseMonthlyCost < 0)
            return Result<GlobalFacility>.Failure(Error.Validation("Facility.Cost", "Base monthly cost cannot be negative"));

        var facility = new GlobalFacility(
            Guid.NewGuid(),
            name,
            code,
            description,
            category,
            baseMonthlyCost);

        return Result<GlobalFacility>.Success(facility);
    }

    /// <summary>
    /// Update facility details
    /// </summary>
    public Result Update(
        string name,
        string? description,
        FacilityCategory category,
        decimal baseMonthlyCost)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("Facility.Name", "Facility name is required"));

        if (baseMonthlyCost < 0)
            return Result.Failure(Error.Validation("Facility.Cost", "Base monthly cost cannot be negative"));

        Name = name;
        Description = description;
        Category = category;
        BaseMonthlyCost = baseMonthlyCost;

        return Result.Success();
    }

    /// <summary>
    /// Deactivate facility (prevent new assignments)
    /// </summary>
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Reactivate facility
    /// </summary>
    public void Reactivate() => IsActive = true;
}

/// <summary>
/// Hospital-Facility mapping - tracks which facilities a hospital has
/// </summary>
public sealed class HospitalFacility : Entity<Guid>
{
    public Guid HospitalId { get; private set; }
    public Guid GlobalFacilityId { get; private set; }
    public DateTime AddedAt { get; private set; }
    public Guid AddedBy { get; private set; }

    private HospitalFacility() { } // EF Core

    internal HospitalFacility(Guid hospitalId, Guid globalFacilityId, Guid addedBy)
        : base(Guid.NewGuid())
    {
        HospitalId = hospitalId;
        GlobalFacilityId = globalFacilityId;
        AddedAt = DateTime.UtcNow;
        AddedBy = addedBy;
    }
}

/// <summary>
/// Branch-Facility mapping - tracks which facilities a branch has
/// </summary>
public sealed class BranchFacility : Entity<Guid>
{
    public Guid BranchId { get; private set; }
    public Guid GlobalFacilityId { get; private set; }
    public DateTime AddedAt { get; private set; }
    public Guid AddedBy { get; private set; }

    private BranchFacility() { } // EF Core

    internal BranchFacility(Guid branchId, Guid globalFacilityId, Guid addedBy)
        : base(Guid.NewGuid())
    {
        BranchId = branchId;
        GlobalFacilityId = globalFacilityId;
        AddedAt = DateTime.UtcNow;
        AddedBy = addedBy;
    }
}

/// <summary>
/// Facility categories
/// </summary>
public enum FacilityCategory
{
    Clinical = 1,          // Emergency, ICU, OPD, IPD, Operation Theater
    Diagnostic = 2,        // Laboratory, Radiology, Pathology
    Support = 3,           // Pharmacy, Blood Bank, Dietary
    Administrative = 4     // Billing, Registration, Records
}
