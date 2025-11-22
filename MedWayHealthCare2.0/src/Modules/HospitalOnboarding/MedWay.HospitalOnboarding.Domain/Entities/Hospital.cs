using MedWay.Domain.Primitives;
using MedWay.Domain.ValueObjects;

namespace MedWay.HospitalOnboarding.Domain.Entities;

/// <summary>
/// Hospital aggregate root - represents a healthcare organization with multi-tenancy support
/// </summary>
public sealed class Hospital : AggregateRoot<Guid>
{
    private readonly List<Branch> _branches = new();
    private readonly List<HospitalFacility> _facilities = new();

    public string Name { get; private set; } = null!;
    public string RegistrationNumber { get; private set; } = null!;
    public string? TaxNumber { get; private set; }
    public Address RegisteredAddress { get; private set; } = null!;
    public Email PrimaryEmail { get; private set; } = null!;
    public PhoneNumber PrimaryPhone { get; private set; } = null!;
    
    /// <summary>
    /// Tenant ID for multi-tenancy support - each hospital is a separate tenant
    /// </summary>
    public string TenantId { get; private set; } = null!;
    
    public HospitalStatus Status { get; private set; }
    public DateTime EstablishedDate { get; private set; }
    
    /// <summary>
    /// Trial period management
    /// </summary>
    public DateTime? TrialStartDate { get; private set; }
    public DateTime? TrialEndDate { get; private set; }
    public bool IsInTrial => TrialEndDate.HasValue && DateTime.UtcNow <= TrialEndDate.Value;
    
    /// <summary>
    /// Subscription details
    /// </summary>
    public Guid? CurrentSubscriptionPlanId { get; private set; }
    public DateTime? SubscriptionStartDate { get; private set; }
    public DateTime? SubscriptionEndDate { get; private set; }
    
    public IReadOnlyCollection<Branch> Branches => _branches.AsReadOnly();
    public IReadOnlyCollection<HospitalFacility> Facilities => _facilities.AsReadOnly();
    
    /// <summary>
    /// Audit fields
    /// </summary>
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public Guid CreatedBy { get; private set; }
    public Guid? ModifiedBy { get; private set; }

    private Hospital() { } // EF Core

    private Hospital(
        Guid id,
        string name,
        string registrationNumber,
        string tenantId,
        Address registeredAddress,
        Email primaryEmail,
        PhoneNumber primaryPhone,
        DateTime establishedDate,
        Guid createdBy)
        : base(id)
    {
        Name = name;
        RegistrationNumber = registrationNumber;
        TenantId = tenantId;
        RegisteredAddress = registeredAddress;
        PrimaryEmail = primaryEmail;
        PrimaryPhone = primaryPhone;
        EstablishedDate = establishedDate;
        Status = HospitalStatus.Draft;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Factory method: Create hospital with trial period
    /// </summary>
    public static Result<Hospital> Create(
        string name,
        string registrationNumber,
        string tenantId,
        Address registeredAddress,
        Email primaryEmail,
        PhoneNumber primaryPhone,
        DateTime establishedDate,
        Guid createdBy,
        int trialDays = 30)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Hospital>.Failure(Error.Validation("Hospital.Name", "Hospital name is required"));

        if (string.IsNullOrWhiteSpace(registrationNumber))
            return Result<Hospital>.Failure(Error.Validation("Hospital.RegistrationNumber", "Registration number is required"));

        if (string.IsNullOrWhiteSpace(tenantId))
            return Result<Hospital>.Failure(Error.Validation("Hospital.TenantId", "Tenant ID is required"));

        var hospital = new Hospital(
            Guid.NewGuid(),
            name,
            registrationNumber,
            tenantId,
            registeredAddress,
            primaryEmail,
            primaryPhone,
            establishedDate,
            createdBy);

        // Start trial period
        hospital.TrialStartDate = DateTime.UtcNow;
        hospital.TrialEndDate = DateTime.UtcNow.AddDays(trialDays);

        hospital.AddDomainEvent(new HospitalRegisteredEvent(
            hospital.Id,
            hospital.TenantId,
            hospital.Name,
            hospital.PrimaryEmail.Value,
            hospital.TrialEndDate.Value));

        return Result<Hospital>.Success(hospital);
    }

    /// <summary>
    /// Update hospital basic information
    /// </summary>
    public Result Update(
        string name,
        string? taxNumber,
        Address registeredAddress,
        Email primaryEmail,
        PhoneNumber primaryPhone,
        Guid modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("Hospital.Name", "Hospital name is required"));

        Name = name;
        TaxNumber = taxNumber;
        RegisteredAddress = registeredAddress;
        PrimaryEmail = primaryEmail;
        PrimaryPhone = primaryPhone;
        ModifiedBy = modifiedBy;

        AddDomainEvent(new HospitalUpdatedEvent(Id, TenantId, Name));

        return Result.Success();
    }

    /// <summary>
    /// Add facility to hospital (from global facility list)
    /// </summary>
    public Result AddFacility(Guid globalFacilityId, Guid userId)
    {
        if (_facilities.Any(f => f.GlobalFacilityId == globalFacilityId))
            return Result.Failure(Error.Conflict("Hospital.Facility", "Facility already added to hospital"));

        var hospitalFacility = new HospitalFacility(Id, globalFacilityId, userId);
        _facilities.Add(hospitalFacility);

        AddDomainEvent(new HospitalFacilityAddedEvent(Id, globalFacilityId));

        return Result.Success();
    }

    /// <summary>
    /// Remove facility from hospital
    /// </summary>
    public Result RemoveFacility(Guid globalFacilityId)
    {
        var facility = _facilities.FirstOrDefault(f => f.GlobalFacilityId == globalFacilityId);
        if (facility is null)
            return Result.Failure(Error.NotFound("Hospital.Facility", globalFacilityId));

        _facilities.Remove(facility);
        AddDomainEvent(new HospitalFacilityRemovedEvent(Id, globalFacilityId));

        return Result.Success();
    }

    /// <summary>
    /// Add branch to hospital
    /// </summary>
    public Result AddBranch(Branch branch)
    {
        if (_branches.Any(b => b.BranchCode == branch.BranchCode))
            return Result.Failure(Error.Conflict("Hospital.Branch", $"Branch with code '{branch.BranchCode}' already exists"));

        _branches.Add(branch);
        AddDomainEvent(new BranchAddedEvent(Id, branch.Id, branch.Name));

        return Result.Success();
    }

    /// <summary>
    /// Activate subscription plan
    /// </summary>
    public Result ActivateSubscription(Guid subscriptionPlanId, int durationMonths)
    {
        if (Status == HospitalStatus.Suspended || Status == HospitalStatus.Closed)
            return Result.Failure(Error.Validation("Hospital.Status", "Cannot activate subscription for suspended or closed hospital"));

        CurrentSubscriptionPlanId = subscriptionPlanId;
        SubscriptionStartDate = DateTime.UtcNow;
        SubscriptionEndDate = DateTime.UtcNow.AddMonths(durationMonths);
        Status = HospitalStatus.Active;

        AddDomainEvent(new SubscriptionActivatedEvent(
            Id,
            subscriptionPlanId,
            SubscriptionStartDate.Value,
            SubscriptionEndDate.Value));

        return Result.Success();
    }

    /// <summary>
    /// Suspend hospital (payment failure, violation, etc.)
    /// </summary>
    public Result Suspend(string reason, Guid userId)
    {
        if (Status == HospitalStatus.Closed)
            return Result.Failure(Error.Validation("Hospital.Status", "Cannot suspend a closed hospital"));

        Status = HospitalStatus.Suspended;
        ModifiedBy = userId;

        AddDomainEvent(new HospitalSuspendedEvent(Id, reason));

        return Result.Success();
    }

    /// <summary>
    /// Reactivate suspended hospital
    /// </summary>
    public Result Reactivate(Guid userId)
    {
        if (Status != HospitalStatus.Suspended)
            return Result.Failure(Error.Validation("Hospital.Status", "Only suspended hospitals can be reactivated"));

        Status = HospitalStatus.Active;
        ModifiedBy = userId;

        AddDomainEvent(new HospitalReactivatedEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Submit hospital for approval (after trial registration)
    /// </summary>
    public Result Submit()
    {
        if (Status != HospitalStatus.Draft)
            return Result.Failure(Error.Validation("Hospital.Status", "Only draft hospitals can be submitted"));

        Status = HospitalStatus.PendingApproval;
        AddDomainEvent(new HospitalSubmittedEvent(Id, Name));

        return Result.Success();
    }

    /// <summary>
    /// Approve hospital registration
    /// </summary>
    public Result Approve(Guid approvedBy)
    {
        if (Status != HospitalStatus.PendingApproval)
            return Result.Failure(Error.Validation("Hospital.Status", "Only pending hospitals can be approved"));

        Status = HospitalStatus.Active;
        ModifiedBy = approvedBy;
        
        AddDomainEvent(new HospitalApprovedEvent(Id, Name, approvedBy));

        return Result.Success();
    }

    /// <summary>
    /// Reject hospital registration
    /// </summary>
    public Result Reject(string reason, Guid rejectedBy)
    {
        if (Status != HospitalStatus.PendingApproval)
            return Result.Failure(Error.Validation("Hospital.Status", "Only pending hospitals can be rejected"));

        Status = HospitalStatus.Rejected;
        ModifiedBy = rejectedBy;

        AddDomainEvent(new HospitalRejectedEvent(Id, reason, rejectedBy));

        return Result.Success();
    }
}

/// <summary>
/// Hospital lifecycle status
/// </summary>
public enum HospitalStatus
{
    Draft = 0,
    PendingApproval = 1,
    Active = 2,
    Suspended = 3,
    Rejected = 4,
    Closed = 5
}
