using MedWay.Domain.Primitives;

namespace MedWay.PatientManagement.Domain.Events;

public sealed record PatientRegisteredEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid PatientId,
    string MRN,
    string PatientName,
    Guid BranchId) : IDomainEvent;

public sealed record PatientUpdatedEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid PatientId,
    string PatientName) : IDomainEvent;

public sealed record PatientDeactivatedEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid PatientId,
    string MRN) : IDomainEvent;
