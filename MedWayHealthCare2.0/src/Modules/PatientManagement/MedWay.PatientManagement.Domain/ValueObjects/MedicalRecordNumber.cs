using MedWay.Domain.Primitives;

namespace MedWay.PatientManagement.Domain.ValueObjects;

/// <summary>
/// Medical Record Number (MRN) value object - unique identifier for patients
/// </summary>
public sealed class MedicalRecordNumber : ValueObject
{
    public string Value { get; private set; }

    private MedicalRecordNumber(string value)
    {
        Value = value;
    }

    public static Result<MedicalRecordNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<MedicalRecordNumber>(
                Error.Validation(nameof(MedicalRecordNumber), "MRN is required"));

        if (value.Length < 6 || value.Length > 20)
            return Result.Failure<MedicalRecordNumber>(
                Error.Validation(nameof(MedicalRecordNumber), "MRN must be between 6 and 20 characters"));

        return new MedicalRecordNumber(value.ToUpperInvariant());
    }

    public static Result<MedicalRecordNumber> Generate(string branchCode, int sequence)
    {
        if (string.IsNullOrWhiteSpace(branchCode) || branchCode.Length != 3)
            return Result.Failure<MedicalRecordNumber>(
                Error.Validation(nameof(branchCode), "Branch code must be 3 characters"));

        var mrn = $"{branchCode}{DateTime.UtcNow:yyyyMMdd}{sequence:D6}";
        return new MedicalRecordNumber(mrn);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
