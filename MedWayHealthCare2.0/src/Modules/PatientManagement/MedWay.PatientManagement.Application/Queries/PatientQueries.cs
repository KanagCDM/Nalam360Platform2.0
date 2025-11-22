using MedWay.Application.Abstractions;
using MedWay.Domain.Primitives;
using MedWay.PatientManagement.Application.DTOs;

namespace MedWay.PatientManagement.Application.Queries;

public sealed record GetPatientByIdQuery(Guid PatientId) : IQuery<Result<PatientDto>>;

public sealed record GetPatientByMRNQuery(string MRN) : IQuery<Result<PatientDto>>;

public sealed record SearchPatientsQuery(
    string? SearchTerm,
    Guid? BranchId,
    int PageNumber,
    int PageSize) : IQuery<Result<PagedResult<PatientDto>>>;
