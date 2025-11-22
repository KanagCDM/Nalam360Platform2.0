using MedWay.Domain.Primitives;

namespace MedWay.AppointmentManagement.Domain.ValueObjects;

/// <summary>
/// Time Slot value object - represents a scheduled time block
/// </summary>
public sealed class TimeSlot : ValueObject
{
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;

    private TimeSlot(DateTime startTime, DateTime endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }

    public static Result<TimeSlot> Create(DateTime startTime, DateTime endTime)
    {
        if (startTime >= endTime)
            return Result.Failure<TimeSlot>(
                Error.Validation(nameof(StartTime), "Start time must be before end time"));

        if (startTime < DateTime.UtcNow.AddMinutes(-30))
            return Result.Failure<TimeSlot>(
                Error.Validation(nameof(StartTime), "Cannot schedule appointments in the past"));

        var duration = (endTime - startTime).TotalMinutes;
        if (duration < 15 || duration > 480)
            return Result.Failure<TimeSlot>(
                Error.Validation(nameof(EndTime), "Duration must be between 15 minutes and 8 hours"));

        return new TimeSlot(startTime, endTime);
    }

    public static Result<TimeSlot> CreateFromDuration(DateTime startTime, int durationMinutes)
    {
        if (durationMinutes < 15 || durationMinutes > 480)
            return Result.Failure<TimeSlot>(
                Error.Validation(nameof(durationMinutes), "Duration must be between 15 minutes and 8 hours"));

        return Create(startTime, startTime.AddMinutes(durationMinutes));
    }

    public bool OverlapsWith(TimeSlot other)
    {
        return StartTime < other.EndTime && EndTime > other.StartTime;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartTime;
        yield return EndTime;
    }

    public override string ToString() =>
        $"{StartTime:g} - {EndTime:t} ({DurationMinutes} min)";
}

/// <summary>
/// Recurrence Pattern value object
/// </summary>
public sealed class RecurrencePattern : ValueObject
{
    public RecurrenceFrequency Frequency { get; private set; }
    public int Interval { get; private set; }
    public DateTime? EndDate { get; private set; }
    public int? OccurrenceCount { get; private set; }
    public DayOfWeek[]? DaysOfWeek { get; private set; }

    private RecurrencePattern(
        RecurrenceFrequency frequency,
        int interval,
        DateTime? endDate,
        int? occurrenceCount,
        DayOfWeek[]? daysOfWeek)
    {
        Frequency = frequency;
        Interval = interval;
        EndDate = endDate;
        OccurrenceCount = occurrenceCount;
        DaysOfWeek = daysOfWeek;
    }

    public static Result<RecurrencePattern> Create(
        RecurrenceFrequency frequency,
        int interval = 1,
        DateTime? endDate = null,
        int? occurrenceCount = null,
        DayOfWeek[]? daysOfWeek = null)
    {
        if (interval < 1)
            return Result.Failure<RecurrencePattern>(
                Error.Validation(nameof(Interval), "Interval must be at least 1"));

        if (endDate.HasValue && endDate.Value <= DateTime.UtcNow)
            return Result.Failure<RecurrencePattern>(
                Error.Validation(nameof(EndDate), "End date must be in the future"));

        if (occurrenceCount.HasValue && occurrenceCount.Value < 1)
            return Result.Failure<RecurrencePattern>(
                Error.Validation(nameof(OccurrenceCount), "Occurrence count must be at least 1"));

        if (frequency == RecurrenceFrequency.Weekly && (daysOfWeek == null || daysOfWeek.Length == 0))
            return Result.Failure<RecurrencePattern>(
                Error.Validation(nameof(DaysOfWeek), "Days of week required for weekly recurrence"));

        return new RecurrencePattern(frequency, interval, endDate, occurrenceCount, daysOfWeek);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Frequency;
        yield return Interval;
        yield return EndDate;
        yield return OccurrenceCount;
        if (DaysOfWeek != null)
        {
            foreach (var day in DaysOfWeek)
                yield return day;
        }
    }
}

public enum RecurrenceFrequency
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3
}

public enum AppointmentType
{
    Consultation = 1,
    FollowUp = 2,
    Procedure = 3,
    Vaccination = 4,
    Checkup = 5,
    Telemedicine = 6
}

public enum AppointmentStatus
{
    Scheduled = 1,
    CheckedIn = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5,
    NoShow = 6
}
