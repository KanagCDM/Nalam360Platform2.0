using MedWay.Domain.Primitives;

namespace MedWay.PharmacyManagement.Domain.Events;

public sealed record MedicationOrderCreatedEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid OrderId,
    Guid PatientId,
    string MedicationName) : IDomainEvent;

public sealed record MedicationDispensedEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid OrderId,
    Guid PatientId,
    string MedicationName,
    Guid PharmacistId) : IDomainEvent;

public sealed record MedicationOrderCancelledEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid OrderId,
    string Reason) : IDomainEvent;
