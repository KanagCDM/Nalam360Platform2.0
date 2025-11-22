using MedWay.Domain.Primitives;

namespace MedWay.AppointmentManagement.Domain.Events;

public sealed record AppointmentScheduledEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid AppointmentId,
    Guid PatientId,
    Guid ProviderId,
    DateTime ScheduledTime) : IDomainEvent;

public sealed record AppointmentRescheduledEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid AppointmentId,
    DateTime OldTime,
    DateTime NewTime) : IDomainEvent;

public sealed record AppointmentCancelledEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid AppointmentId,
    string Reason) : IDomainEvent;

public sealed record PatientCheckedInEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid AppointmentId,
    Guid PatientId) : IDomainEvent;

public sealed record AppointmentCompletedEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid AppointmentId,
    Guid PatientId,
    Guid ProviderId) : IDomainEvent;

public sealed record AppointmentNoShowEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid AppointmentId,
    Guid PatientId) : IDomainEvent;
