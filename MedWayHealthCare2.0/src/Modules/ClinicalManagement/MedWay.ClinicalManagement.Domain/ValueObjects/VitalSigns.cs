using MedWay.Domain.Primitives;

namespace MedWay.ClinicalManagement.Domain.ValueObjects;

/// <summary>
/// Vital Signs value object
/// </summary>
public sealed class VitalSigns : ValueObject
{
    public decimal? Temperature { get; private set; }
    public int? HeartRate { get; private set; }
    public int? RespiratoryRate { get; private set; }
    public BloodPressure? BloodPressure { get; private set; }
    public decimal? OxygenSaturation { get; private set; }
    public decimal? Weight { get; private set; }
    public decimal? Height { get; private set; }
    public decimal? BMI { get; private set; }
    public DateTime RecordedAt { get; private set; }

    private VitalSigns(
        decimal? temperature,
        int? heartRate,
        int? respiratoryRate,
        BloodPressure? bloodPressure,
        decimal? oxygenSaturation,
        decimal? weight,
        decimal? height)
    {
        Temperature = temperature;
        HeartRate = heartRate;
        RespiratoryRate = respiratoryRate;
        BloodPressure = bloodPressure;
        OxygenSaturation = oxygenSaturation;
        Weight = weight;
        Height = height;
        RecordedAt = DateTime.UtcNow;

        if (weight.HasValue && height.HasValue && height.Value > 0)
        {
            BMI = weight.Value / ((height.Value / 100) * (height.Value / 100));
        }
    }

    public static Result<VitalSigns> Create(
        decimal? temperature = null,
        int? heartRate = null,
        int? respiratoryRate = null,
        BloodPressure? bloodPressure = null,
        decimal? oxygenSaturation = null,
        decimal? weight = null,
        decimal? height = null)
    {
        if (temperature.HasValue && (temperature < 35 || temperature > 42))
            return Result.Failure<VitalSigns>(
                Error.Validation(nameof(Temperature), "Temperature must be between 35 and 42 °C"));

        if (heartRate.HasValue && (heartRate < 40 || heartRate > 200))
            return Result.Failure<VitalSigns>(
                Error.Validation(nameof(HeartRate), "Heart rate must be between 40 and 200 bpm"));

        if (oxygenSaturation.HasValue && (oxygenSaturation < 0 || oxygenSaturation > 100))
            return Result.Failure<VitalSigns>(
                Error.Validation(nameof(OxygenSaturation), "Oxygen saturation must be between 0 and 100%"));

        return new VitalSigns(
            temperature,
            heartRate,
            respiratoryRate,
            bloodPressure,
            oxygenSaturation,
            weight,
            height);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Temperature;
        yield return HeartRate;
        yield return RespiratoryRate;
        yield return BloodPressure;
        yield return OxygenSaturation;
        yield return Weight;
        yield return Height;
    }
}

public sealed class BloodPressure : ValueObject
{
    public int Systolic { get; private set; }
    public int Diastolic { get; private set; }

    private BloodPressure(int systolic, int diastolic)
    {
        Systolic = systolic;
        Diastolic = diastolic;
    }

    public static Result<BloodPressure> Create(int systolic, int diastolic)
    {
        if (systolic < 70 || systolic > 250)
            return Result.Failure<BloodPressure>(
                Error.Validation(nameof(Systolic), "Systolic BP must be between 70 and 250 mmHg"));

        if (diastolic < 40 || diastolic > 150)
            return Result.Failure<BloodPressure>(
                Error.Validation(nameof(Diastolic), "Diastolic BP must be between 40 and 150 mmHg"));

        if (diastolic >= systolic)
            return Result.Failure<BloodPressure>(
                Error.Validation(nameof(Diastolic), "Diastolic BP must be less than Systolic BP"));

        return new BloodPressure(systolic, diastolic);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Systolic;
        yield return Diastolic;
    }

    public override string ToString() => $"{Systolic}/{Diastolic}";
}

public enum EncounterType
{
    Outpatient = 1,
    Inpatient = 2,
    Emergency = 3,
    Telemedicine = 4
}

public enum EncounterStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}
