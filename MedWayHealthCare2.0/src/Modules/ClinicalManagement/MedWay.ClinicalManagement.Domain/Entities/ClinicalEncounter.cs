using MedWay.Domain.Primitives;
using MedWay.ClinicalManagement.Domain.Events;
using MedWay.ClinicalManagement.Domain.ValueObjects;

namespace MedWay.ClinicalManagement.Domain.Entities;

/// <summary>
/// Clinical Encounter aggregate root - represents a patient visit/consultation
/// </summary>
public sealed class ClinicalEncounter : AggregateRoot<Guid>
{
    private readonly List<Diagnosis> _diagnoses = new();
    private readonly List<Prescription> _prescriptions = new();
    private readonly List<Procedure> _procedures = new();

    private ClinicalEncounter(
        Guid id,
        Guid patientId,
        Guid providerId,
        Guid branchId,
        EncounterType encounterType,
        DateTime encounterDateTime)
        : base(id)
    {
        PatientId = patientId;
        ProviderId = providerId;
        BranchId = branchId;
        EncounterType = encounterType;
        EncounterDateTime = encounterDateTime;
        Status = EncounterStatus.InProgress;
        CreatedAt = DateTime.UtcNow;
    }

    private ClinicalEncounter() : base() { }

    public Guid PatientId { get; private set; }
    public Guid ProviderId { get; private set; }
    public Guid BranchId { get; private set; }
    public EncounterType EncounterType { get; private set; }
    public DateTime EncounterDateTime { get; private set; }
    public EncounterStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // SOAP Notes
    public string? Subjective { get; private set; }
    public string? Objective { get; private set; }
    public string? Assessment { get; private set; }
    public string? Plan { get; private set; }

    // Vital Signs
    public VitalSigns? VitalSigns { get; private set; }

    // Chief Complaint
    public string? ChiefComplaint { get; private set; }

    public IReadOnlyCollection<Diagnosis> Diagnoses => _diagnoses.AsReadOnly();
    public IReadOnlyCollection<Prescription> Prescriptions => _prescriptions.AsReadOnly();
    public IReadOnlyCollection<Procedure> Procedures => _procedures.AsReadOnly();

    public static Result<ClinicalEncounter> Create(
        Guid patientId,
        Guid providerId,
        Guid branchId,
        EncounterType encounterType,
        DateTime encounterDateTime,
        string? chiefComplaint)
    {
        if (patientId == Guid.Empty)
            return Result.Failure<ClinicalEncounter>(
                Error.Validation(nameof(PatientId), "Patient ID is required"));

        if (providerId == Guid.Empty)
            return Result.Failure<ClinicalEncounter>(
                Error.Validation(nameof(ProviderId), "Provider ID is required"));

        if (branchId == Guid.Empty)
            return Result.Failure<ClinicalEncounter>(
                Error.Validation(nameof(BranchId), "Branch ID is required"));

        var encounter = new ClinicalEncounter(
            Guid.NewGuid(),
            patientId,
            providerId,
            branchId,
            encounterType,
            encounterDateTime)
        {
            ChiefComplaint = chiefComplaint
        };

        encounter.AddDomainEvent(new EncounterStartedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            encounter.Id,
            encounter.PatientId,
            encounter.ProviderId));

        return encounter;
    }

    public Result RecordVitalSigns(VitalSigns vitalSigns)
    {
        if (Status == EncounterStatus.Completed)
            return Result.Failure(Error.Validation(nameof(Status), "Cannot modify completed encounter"));

        VitalSigns = vitalSigns;
        return Result.Success();
    }

    public Result UpdateSOAPNotes(
        string? subjective,
        string? objective,
        string? assessment,
        string? plan)
    {
        if (Status == EncounterStatus.Completed)
            return Result.Failure(Error.Validation(nameof(Status), "Cannot modify completed encounter"));

        Subjective = subjective;
        Objective = objective;
        Assessment = assessment;
        Plan = plan;

        return Result.Success();
    }

    public Result AddDiagnosis(Diagnosis diagnosis)
    {
        if (Status == EncounterStatus.Completed)
            return Result.Failure(Error.Validation(nameof(Status), "Cannot modify completed encounter"));

        _diagnoses.Add(diagnosis);

        AddDomainEvent(new DiagnosisAddedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            diagnosis.Id,
            diagnosis.ICDCode));

        return Result.Success();
    }

    public Result AddPrescription(Prescription prescription)
    {
        if (Status == EncounterStatus.Completed)
            return Result.Failure(Error.Validation(nameof(Status), "Cannot modify completed encounter"));

        _prescriptions.Add(prescription);

        AddDomainEvent(new PrescriptionIssuedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            prescription.Id,
            prescription.MedicationName));

        return Result.Success();
    }

    public Result AddProcedure(Procedure procedure)
    {
        if (Status == EncounterStatus.Completed)
            return Result.Failure(Error.Validation(nameof(Status), "Cannot modify completed encounter"));

        _procedures.Add(procedure);
        return Result.Success();
    }

    public Result Complete()
    {
        if (Status == EncounterStatus.Completed)
            return Result.Failure(Error.Validation(nameof(Status), "Encounter is already completed"));

        if (string.IsNullOrWhiteSpace(Assessment))
            return Result.Failure(Error.Validation(nameof(Assessment), "Assessment is required to complete encounter"));

        Status = EncounterStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        AddDomainEvent(new EncounterCompletedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            PatientId,
            ProviderId));

        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (Status == EncounterStatus.Completed)
            return Result.Failure(Error.Validation(nameof(Status), "Cannot cancel completed encounter"));

        if (Status == EncounterStatus.Cancelled)
            return Result.Failure(Error.Validation(nameof(Status), "Encounter is already cancelled"));

        Status = EncounterStatus.Cancelled;

        AddDomainEvent(new EncounterCancelledEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            reason));

        return Result.Success();
    }
}
