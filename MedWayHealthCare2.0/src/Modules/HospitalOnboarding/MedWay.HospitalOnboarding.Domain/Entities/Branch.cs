using MedWay.Domain.Primitives;
using MedWay.Domain.ValueObjects;

namespace MedWay.HospitalOnboarding.Domain.Entities;

/// <summary>
/// Branch entity - represents a physical location of a hospital
/// </summary>
public sealed class Branch : Entity<Guid>
{
    private readonly List<BranchFacility> _facilities = new();

    public Guid HospitalId { get; private set; }
    public string Name { get; private set; } = null!;
    public string BranchCode { get; private set; } = null!; // Unique identifier within hospital
    public Address Address { get; private set; } = null!;
    public PhoneNumber Phone { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    
    /// <summary>
    /// Branch manager or admin user ID
    /// </summary>
    public Guid? ManagerUserId { get; private set; }
    
    public bool IsActive { get; private set; }
    public DateTime OpeningDate { get; private set; }
    public DateTime? ClosingDate { get; private set; }
    
    /// <summary>
    /// Operating hours (JSON stored as string, TODO: create ValueObject)
    /// </summary>
    public string? OperatingHours { get; private set; }
    
    public IReadOnlyCollection<BranchFacility> Facilities => _facilities.AsReadOnly();
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private Branch() { } // EF Core

    private Branch(
        Guid id,
        Guid hospitalId,
        string name,
        string branchCode,
        Address address,
        PhoneNumber phone,
        Email email,
        DateTime openingDate)
        : base(id)
    {
        HospitalId = hospitalId;
        Name = name;
        BranchCode = branchCode;
        Address = address;
        Phone = phone;
        Email = email;
        OpeningDate = openingDate;
        IsActive = true;
    }

    /// <summary>
    /// Factory method: Create branch
    /// </summary>
    public static Result<Branch> Create(
        Guid hospitalId,
        string name,
        string branchCode,
        Address address,
        PhoneNumber phone,
        Email email,
        DateTime openingDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Branch>.Failure(Error.Validation("Branch.Name", "Branch name is required"));

        if (string.IsNullOrWhiteSpace(branchCode))
            return Result<Branch>.Failure(Error.Validation("Branch.BranchCode", "Branch code is required"));

        var branch = new Branch(
            Guid.NewGuid(),
            hospitalId,
            name,
            branchCode,
            address,
            phone,
            email,
            openingDate);

        return Result<Branch>.Success(branch);
    }

    /// <summary>
    /// Update branch details
    /// </summary>
    public Result Update(
        string name,
        Address address,
        PhoneNumber phone,
        Email email,
        string? operatingHours)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("Branch.Name", "Branch name is required"));

        Name = name;
        Address = address;
        Phone = phone;
        Email = email;
        OperatingHours = operatingHours;

        return Result.Success();
    }

    /// <summary>
    /// Assign branch manager
    /// </summary>
    public Result AssignManager(Guid managerUserId)
    {
        ManagerUserId = managerUserId;
        return Result.Success();
    }

    /// <summary>
    /// Add facility to branch
    /// </summary>
    public Result AddFacility(Guid globalFacilityId, Guid userId)
    {
        if (_facilities.Any(f => f.GlobalFacilityId == globalFacilityId))
            return Result.Failure(Error.Conflict("Branch.Facility", "Facility already added to branch"));

        var branchFacility = new BranchFacility(Id, globalFacilityId, userId);
        _facilities.Add(branchFacility);

        return Result.Success();
    }

    /// <summary>
    /// Remove facility from branch
    /// </summary>
    public Result RemoveFacility(Guid globalFacilityId)
    {
        var facility = _facilities.FirstOrDefault(f => f.GlobalFacilityId == globalFacilityId);
        if (facility is null)
            return Result.Failure(Error.NotFound("Branch.Facility", globalFacilityId));

        _facilities.Remove(facility);
        return Result.Success();
    }

    /// <summary>
    /// Deactivate branch
    /// </summary>
    public Result Deactivate(DateTime closingDate)
    {
        if (!IsActive)
            return Result.Failure(Error.Validation("Branch.Status", "Branch is already inactive"));

        IsActive = false;
        ClosingDate = closingDate;

        return Result.Success();
    }

    /// <summary>
    /// Reactivate branch
    /// </summary>
    public Result Reactivate()
    {
        if (IsActive)
            return Result.Failure(Error.Validation("Branch.Status", "Branch is already active"));

        IsActive = true;
        ClosingDate = null;

        return Result.Success();
    }
}
