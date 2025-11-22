using MedWay.Domain.Primitives;
using MedWay.AppointmentManagement.Domain.Events;
using MedWay.AppointmentManagement.Domain.ValueObjects;

namespace MedWay.AppointmentManagement.Domain.Entities;

/// <summary>
/// Appointment aggregate root - represents a scheduled patient appointment
/// </summary>
public sealed class Appointment : AggregateRoot<Guid>
{
    private Appointment(
        Guid id,
        Guid patientId,
        Guid providerId,
        Guid branchId,
        AppointmentType appointmentType,
        TimeSlot timeSlot,
        string reason)
        : base(id)
    {
        PatientId = patientId;
        ProviderId = providerId;
        BranchId = branchId;
        AppointmentType = appointmentType;
        TimeSlot = timeSlot;
        Reason = reason;
        Status = AppointmentStatus.Scheduled;
        CreatedAt = DateTime.UtcNow;
    }

    private Appointment() : base() { }

    public Guid PatientId { get; private set; }
    public Guid ProviderId { get; private set; }
    public Guid BranchId { get; private set; }
    public AppointmentType AppointmentType { get; private set; }
    public TimeSlot TimeSlot { get; private set; } = null!;
    public string Reason { get; private set; } = null!;
    public AppointmentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? Notes { get; private set; }
    public bool IsRecurring { get; private set; }
    public RecurrencePattern? RecurrencePattern { get; private set; }

    public static Result<Appointment> Create(
        Guid patientId,
        Guid providerId,
        Guid branchId,
        AppointmentType appointmentType,
        TimeSlot timeSlot,
        string reason)
    {
        if (patientId == Guid.Empty)
            return Result.Failure<Appointment>(
                Error.Validation(nameof(PatientId), "Patient ID is required"));

        if (providerId == Guid.Empty)
            return Result.Failure<Appointment>(
                Error.Validation(nameof(ProviderId), "Provider ID is required"));

        if (branchId == Guid.Empty)
            return Result.Failure<Appointment>(
                Error.Validation(nameof(BranchId), "Branch ID is required"));

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure<Appointment>(
                Error.Validation(nameof(Reason), "Reason is required"));

        var appointment = new Appointment(
            Guid.NewGuid(),
            patientId,
            providerId,
            branchId,
            appointmentType,
            timeSlot,
            reason);

        appointment.AddDomainEvent(new AppointmentScheduledEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            appointment.Id,
            appointment.PatientId,
            appointment.ProviderId,
            appointment.TimeSlot.StartTime));

        return appointment;
    }

    public Result Reschedule(TimeSlot newTimeSlot)
    {
        if (Status == AppointmentStatus.Cancelled)
            return Result.Failure(Error.Validation(nameof(Status), "Cannot reschedule cancelled appointment"));

        if (Status == AppointmentStatus.Completed)
            return Result.Failure(Error.Validation(nameof(Status), "Cannot reschedule completed appointment"));

        var oldTimeSlot = TimeSlot;
        TimeSlot = newTimeSlot;

        AddDomainEvent(new AppointmentRescheduledEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            oldTimeSlot.StartTime,
            newTimeSlot.StartTime));

        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (Status == AppointmentStatus.Cancelled)
            return Result.Failure(Error.Validation(nameof(Status), "Appointment is already cancelled"));

        if (Status == AppointmentStatus.Completed)
            return Result.Failure(Error.Validation(nameof(Status), "Cannot cancel completed appointment"));

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(Error.Validation(nameof(CancellationReason), "Cancellation reason is required"));

        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason;

        AddDomainEvent(new AppointmentCancelledEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            reason));

        return Result.Success();
    }

    public Result MarkAsCheckedIn()
    {
        if (Status != AppointmentStatus.Scheduled)
            return Result.Failure(Error.Validation(nameof(Status), "Only scheduled appointments can be checked in"));

        Status = AppointmentStatus.CheckedIn;

        AddDomainEvent(new PatientCheckedInEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            PatientId));

        return Result.Success();
    }

    public Result MarkAsInProgress()
    {
        if (Status != AppointmentStatus.CheckedIn)
            return Result.Failure(Error.Validation(nameof(Status), "Patient must be checked in first"));

        Status = AppointmentStatus.InProgress;
        return Result.Success();
    }

    public Result Complete()
    {
        if (Status == AppointmentStatus.Completed)
            return Result.Failure(Error.Validation(nameof(Status), "Appointment is already completed"));

        if (Status == AppointmentStatus.Cancelled)
            return Result.Failure(Error.Validation(nameof(Status), "Cannot complete cancelled appointment"));

        Status = AppointmentStatus.Completed;

        AddDomainEvent(new AppointmentCompletedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            PatientId,
            ProviderId));

        return Result.Success();
    }

    public Result MarkAsNoShow()
    {
        if (Status != AppointmentStatus.Scheduled)
            return Result.Failure(Error.Validation(nameof(Status), "Only scheduled appointments can be marked as no-show"));

        if (TimeSlot.EndTime > DateTime.UtcNow)
            return Result.Failure(Error.Validation(nameof(TimeSlot), "Cannot mark future appointment as no-show"));

        Status = AppointmentStatus.NoShow;

        AddDomainEvent(new AppointmentNoShowEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            PatientId));

        return Result.Success();
    }

    public Result SetRecurrence(RecurrencePattern pattern)
    {
        if (Status != AppointmentStatus.Scheduled)
            return Result.Failure(Error.Validation(nameof(Status), "Only scheduled appointments can be set as recurring"));

        IsRecurring = true;
        RecurrencePattern = pattern;

        return Result.Success();
    }
}
