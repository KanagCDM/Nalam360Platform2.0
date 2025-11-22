using MedWay.Domain.Primitives;

namespace MedWay.ClinicalManagement.Domain.Entities;

/// <summary>
/// Procedure entity - represents medical procedures performed during encounter
/// </summary>
public sealed class Procedure : Entity<Guid>
{
    private Procedure(
        Guid id,
        string procedureCode,
        string procedureName,
        DateTime performedAt)
        : base(id)
    {
        ProcedureCode = procedureCode;
        ProcedureName = procedureName;
        PerformedAt = performedAt;
    }

    private Procedure() : base() { }

    public string ProcedureCode { get; private set; } = null!;
    public string ProcedureName { get; private set; } = null!;
    public DateTime PerformedAt { get; private set; }
    public string? Notes { get; private set; }
    public string? Outcome { get; private set; }

    public static Result<Procedure> Create(
        string procedureCode,
        string procedureName,
        DateTime performedAt,
        string? notes = null,
        string? outcome = null)
    {
        if (string.IsNullOrWhiteSpace(procedureCode))
            return Result.Failure<Procedure>(Error.Validation(nameof(ProcedureCode), "Procedure code is required"));

        if (string.IsNullOrWhiteSpace(procedureName))
            return Result.Failure<Procedure>(Error.Validation(nameof(ProcedureName), "Procedure name is required"));

        var procedure = new Procedure(
            Guid.NewGuid(),
            procedureCode,
            procedureName,
            performedAt)
        {
            Notes = notes,
            Outcome = outcome
        };

        return procedure;
    }
}
