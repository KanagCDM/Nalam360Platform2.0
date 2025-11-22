using MedWay.Domain.Primitives;

namespace MedWay.ClinicalManagement.Domain.Entities;

/// <summary>
/// Prescription entity - represents medication prescribed during encounter
/// </summary>
public sealed class Prescription : Entity<Guid>
{
    private Prescription(
        Guid id,
        string medicationName,
        string dosage,
        string frequency,
        int durationDays,
        string route)
        : base(id)
    {
        MedicationName = medicationName;
        Dosage = dosage;
        Frequency = frequency;
        DurationDays = durationDays;
        Route = route;
        PrescribedAt = DateTime.UtcNow;
        Status = PrescriptionStatus.Active;
    }

    private Prescription() : base() { }

    public string MedicationName { get; private set; } = null!;
    public string Dosage { get; private set; } = null!;
    public string Frequency { get; private set; } = null!;
    public int DurationDays { get; private set; }
    public string Route { get; private set; } = null!;
    public DateTime PrescribedAt { get; private set; }
    public PrescriptionStatus Status { get; private set; }
    public string? Instructions { get; private set; }
    public int? RefillsAllowed { get; private set; }

    public static Result<Prescription> Create(
        string medicationName,
        string dosage,
        string frequency,
        int durationDays,
        string route,
        string? instructions = null,
        int? refillsAllowed = null)
    {
        if (string.IsNullOrWhiteSpace(medicationName))
            return Result.Failure<Prescription>(Error.Validation(nameof(MedicationName), "Medication name is required"));

        if (string.IsNullOrWhiteSpace(dosage))
            return Result.Failure<Prescription>(Error.Validation(nameof(Dosage), "Dosage is required"));

        if (string.IsNullOrWhiteSpace(frequency))
            return Result.Failure<Prescription>(Error.Validation(nameof(Frequency), "Frequency is required"));

        if (durationDays <= 0)
            return Result.Failure<Prescription>(Error.Validation(nameof(DurationDays), "Duration must be positive"));

        var prescription = new Prescription(
            Guid.NewGuid(),
            medicationName,
            dosage,
            frequency,
            durationDays,
            route)
        {
            Instructions = instructions,
            RefillsAllowed = refillsAllowed
        };

        return prescription;
    }

    public Result Discontinue()
    {
        if (Status == PrescriptionStatus.Discontinued)
            return Result.Failure(Error.Validation(nameof(Status), "Prescription is already discontinued"));

        Status = PrescriptionStatus.Discontinued;
        return Result.Success();
    }
}

public enum PrescriptionStatus
{
    Active = 1,
    Completed = 2,
    Discontinued = 3
}
