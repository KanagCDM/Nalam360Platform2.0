using MedWay.Domain.Primitives;

namespace MedWay.ClinicalManagement.Domain.Events;

public sealed record EncounterStartedEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid EncounterId,
    Guid PatientId,
    Guid ProviderId) : IDomainEvent;

public sealed record EncounterCompletedEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid EncounterId,
    Guid PatientId,
    Guid ProviderId) : IDomainEvent;

public sealed record EncounterCancelledEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid EncounterId,
    string Reason) : IDomainEvent;

public sealed record DiagnosisAddedEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid EncounterId,
    Guid DiagnosisId,
    string ICDCode) : IDomainEvent;

public sealed record PrescriptionIssuedEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid EncounterId,
    Guid PrescriptionId,
    string MedicationName) : IDomainEvent;
