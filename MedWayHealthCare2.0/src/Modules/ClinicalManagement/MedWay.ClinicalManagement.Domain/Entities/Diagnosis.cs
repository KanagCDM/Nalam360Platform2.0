using MedWay.Domain.Primitives;

namespace MedWay.ClinicalManagement.Domain.Entities;

/// <summary>
/// Diagnosis entity - represents a medical diagnosis within an encounter
/// </summary>
public sealed class Diagnosis : Entity<Guid>
{
    private Diagnosis(
        Guid id,
        string icdCode,
        string description,
        DiagnosisType diagnosisType,
        DiagnosisSeverity severity)
        : base(id)
    {
        ICDCode = icdCode;
        Description = description;
        DiagnosisType = diagnosisType;
        Severity = severity;
        DiagnosedAt = DateTime.UtcNow;
    }

    private Diagnosis() : base() { }

    public string ICDCode { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DiagnosisType DiagnosisType { get; private set; }
    public DiagnosisSeverity Severity { get; private set; }
    public DateTime DiagnosedAt { get; private set; }
    public string? Notes { get; private set; }

    public static Result<Diagnosis> Create(
        string icdCode,
        string description,
        DiagnosisType diagnosisType,
        DiagnosisSeverity severity,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(icdCode))
            return Result.Failure<Diagnosis>(Error.Validation(nameof(ICDCode), "ICD code is required"));

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<Diagnosis>(Error.Validation(nameof(Description), "Description is required"));

        var diagnosis = new Diagnosis(
            Guid.NewGuid(),
            icdCode,
            description,
            diagnosisType,
            severity)
        {
            Notes = notes
        };

        return diagnosis;
    }
}

public enum DiagnosisType
{
    Primary = 1,
    Secondary = 2,
    Differential = 3
}

public enum DiagnosisSeverity
{
    Mild = 1,
    Moderate = 2,
    Severe = 3,
    Critical = 4
}
