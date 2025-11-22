using MedWay.Domain.Primitives;
using MedWay.PharmacyManagement.Domain.Events;
using MedWay.PharmacyManagement.Domain.ValueObjects;

namespace MedWay.PharmacyManagement.Domain.Entities;

/// <summary>
/// Medication Order aggregate root - represents a prescription to be dispensed
/// </summary>
public sealed class MedicationOrder : AggregateRoot<Guid>
{
    private MedicationOrder(
        Guid id,
        Guid prescriptionId,
        Guid patientId,
        Guid pharmacyId,
        string medicationName,
        string dosage,
        int quantity)
        : base(id)
    {
        PrescriptionId = prescriptionId;
        PatientId = patientId;
        PharmacyId = pharmacyId;
        MedicationName = medicationName;
        Dosage = dosage;
        Quantity = quantity;
        Status = MedicationOrderStatus.Pending;
        OrderedAt = DateTime.UtcNow;
    }

    private MedicationOrder() : base() { }

    public Guid PrescriptionId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid PharmacyId { get; private set; }
    public string MedicationName { get; private set; } = null!;
    public string Dosage { get; private set; } = null!;
    public int Quantity { get; private set; }
    public MedicationOrderStatus Status { get; private set; }
    public DateTime OrderedAt { get; private set; }
    public DateTime? DispensedAt { get; private set; }
    public Guid? DispensedBy { get; private set; }
    public string? Instructions { get; private set; }
    public decimal? Price { get; private set; }

    public static Result<MedicationOrder> Create(
        Guid prescriptionId,
        Guid patientId,
        Guid pharmacyId,
        string medicationName,
        string dosage,
        int quantity,
        string? instructions = null)
    {
        if (prescriptionId == Guid.Empty)
            return Result.Failure<MedicationOrder>(
                Error.Validation(nameof(PrescriptionId), "Prescription ID is required"));

        if (patientId == Guid.Empty)
            return Result.Failure<MedicationOrder>(
                Error.Validation(nameof(PatientId), "Patient ID is required"));

        if (string.IsNullOrWhiteSpace(medicationName))
            return Result.Failure<MedicationOrder>(
                Error.Validation(nameof(MedicationName), "Medication name is required"));

        if (quantity <= 0)
            return Result.Failure<MedicationOrder>(
                Error.Validation(nameof(Quantity), "Quantity must be positive"));

        var order = new MedicationOrder(
            Guid.NewGuid(),
            prescriptionId,
            patientId,
            pharmacyId,
            medicationName,
            dosage,
            quantity)
        {
            Instructions = instructions
        };

        order.AddDomainEvent(new MedicationOrderCreatedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            order.Id,
            order.PatientId,
            order.MedicationName));

        return order;
    }

    public Result Dispense(Guid pharmacistId, decimal price)
    {
        if (Status != MedicationOrderStatus.Pending)
            return Result.Failure(Error.Validation(nameof(Status), "Only pending orders can be dispensed"));

        if (pharmacistId == Guid.Empty)
            return Result.Failure(Error.Validation(nameof(pharmacistId), "Pharmacist ID is required"));

        Status = MedicationOrderStatus.Dispensed;
        DispensedAt = DateTime.UtcNow;
        DispensedBy = pharmacistId;
        Price = price;

        AddDomainEvent(new MedicationDispensedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            PatientId,
            MedicationName,
            pharmacistId));

        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (Status == MedicationOrderStatus.Dispensed)
            return Result.Failure(Error.Validation(nameof(Status), "Cannot cancel dispensed order"));

        if (Status == MedicationOrderStatus.Cancelled)
            return Result.Failure(Error.Validation(nameof(Status), "Order is already cancelled"));

        Status = MedicationOrderStatus.Cancelled;

        AddDomainEvent(new MedicationOrderCancelledEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            reason));

        return Result.Success();
    }
}

public enum MedicationOrderStatus
{
    Pending = 1,
    Dispensed = 2,
    Cancelled = 3
}
