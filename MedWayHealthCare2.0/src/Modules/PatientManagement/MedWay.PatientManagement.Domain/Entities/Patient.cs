using MedWay.Domain.Primitives;
using MedWay.Domain.ValueObjects;
using MedWay.PatientManagement.Domain.Events;
using MedWay.PatientManagement.Domain.ValueObjects;

namespace MedWay.PatientManagement.Domain.Entities;

/// <summary>
/// Patient aggregate root - represents a patient in the hospital system
/// </summary>
public sealed class Patient : AggregateRoot<Guid>
{
    private Patient(
        Guid id,
        MedicalRecordNumber mrn,
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        Gender gender,
        Email email,
        PhoneNumber phoneNumber,
        Address address,
        Guid branchId)
        : base(id)
    {
        MRN = mrn;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        BranchId = branchId;
        RegistrationDate = DateTime.UtcNow;
        IsActive = true;
    }

    // Required for EF Core
    private Patient() : base() { }

    public MedicalRecordNumber MRN { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public Email Email { get; private set; } = null!;
    public PhoneNumber PhoneNumber { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public Guid BranchId { get; private set; }
    public DateTime RegistrationDate { get; private set; }
    public bool IsActive { get; private set; }
    public string? BloodType { get; private set; }
    public string? EmergencyContactName { get; private set; }
    public PhoneNumber? EmergencyContactPhone { get; private set; }
    public string? Allergies { get; private set; }
    public string? MedicalHistory { get; private set; }

    public string FullName => $"{FirstName} {LastName}";
    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year;

    public static Result<Patient> Create(
        MedicalRecordNumber mrn,
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        Gender gender,
        Email email,
        PhoneNumber phoneNumber,
        Address address,
        Guid branchId)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure<Patient>(Error.Validation(nameof(FirstName), "First name is required"));

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure<Patient>(Error.Validation(nameof(LastName), "Last name is required"));

        if (dateOfBirth > DateTime.UtcNow)
            return Result.Failure<Patient>(Error.Validation(nameof(DateOfBirth), "Date of birth cannot be in the future"));

        if (branchId == Guid.Empty)
            return Result.Failure<Patient>(Error.Validation(nameof(BranchId), "Branch ID is required"));

        var patient = new Patient(
            Guid.NewGuid(),
            mrn,
            firstName,
            lastName,
            dateOfBirth,
            gender,
            email,
            phoneNumber,
            address,
            branchId);

        patient.AddDomainEvent(new PatientRegisteredEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            patient.Id,
            patient.MRN.Value,
            patient.FullName,
            patient.BranchId));

        return patient;
    }

    public Result Update(
        string firstName,
        string lastName,
        Email email,
        PhoneNumber phoneNumber,
        Address address)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure(Error.Validation(nameof(FirstName), "First name is required"));

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure(Error.Validation(nameof(LastName), "Last name is required"));

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;

        AddDomainEvent(new PatientUpdatedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            FullName));

        return Result.Success();
    }

    public Result UpdateMedicalInfo(
        string? bloodType,
        string? emergencyContactName,
        PhoneNumber? emergencyContactPhone,
        string? allergies,
        string? medicalHistory)
    {
        BloodType = bloodType;
        EmergencyContactName = emergencyContactName;
        EmergencyContactPhone = emergencyContactPhone;
        Allergies = allergies;
        MedicalHistory = medicalHistory;

        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(Error.Validation(nameof(IsActive), "Patient is already inactive"));

        IsActive = false;

        AddDomainEvent(new PatientDeactivatedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            MRN.Value));

        return Result.Success();
    }

    public Result Reactivate()
    {
        if (IsActive)
            return Result.Failure(Error.Validation(nameof(IsActive), "Patient is already active"));

        IsActive = true;

        return Result.Success();
    }
}
