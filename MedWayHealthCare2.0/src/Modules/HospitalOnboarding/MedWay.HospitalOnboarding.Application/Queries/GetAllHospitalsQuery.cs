using MedWay.Application.Abstractions;
using MedWay.Domain;
using MedWay.HospitalOnboarding.Application.DTOs;
using MedWay.HospitalOnboarding.Domain.Entities;
using MedWay.HospitalOnboarding.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MedWay.HospitalOnboarding.Application.Queries;

/// <summary>
/// Query to get all hospitals with pagination and optional status filter.
/// Returns paginated list with metadata.
/// </summary>
public record GetAllHospitalsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    HospitalStatus? Status = null,
    string? SearchTerm = null) : IQuery<Result<PagedResult<HospitalSummaryDto>>>;

public class GetAllHospitalsHandler : IRequestHandler<GetAllHospitalsQuery, Result<PagedResult<HospitalSummaryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllHospitalsHandler> _logger;

    public GetAllHospitalsHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllHospitalsHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<HospitalSummaryDto>>> Handle(
        GetAllHospitalsQuery query, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving hospitals. Page: {PageNumber}, Size: {PageSize}, Status: {Status}", 
            query.PageNumber, query.PageSize, query.Status);

        // Validate pagination
        if (query.PageNumber < 1)
            return Result<PagedResult<HospitalSummaryDto>>.Failure(Error.Validation("PageNumber", "Page number must be at least 1"));

        if (query.PageSize < 1 || query.PageSize > 100)
            return Result<PagedResult<HospitalSummaryDto>>.Failure(Error.Validation("PageSize", "Page size must be between 1 and 100"));

        // Get hospitals (with optional status filter)
        var hospitals = query.Status.HasValue
            ? await _unitOfWork.Hospitals.GetByStatusAsync(query.Status.Value, cancellationToken)
            : await _unitOfWork.Hospitals.GetAllAsync(cancellationToken);

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchLower = query.SearchTerm.ToLower();
            hospitals = hospitals.Where(h => 
                h.Name.ToLower().Contains(searchLower) ||
                h.RegistrationNumber.ToLower().Contains(searchLower) ||
                h.TenantId.ToLower().Contains(searchLower)).ToList();
        }

        // Calculate pagination
        var totalCount = hospitals.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);
        var skip = (query.PageNumber - 1) * query.PageSize;

        var pagedHospitals = hospitals
            .OrderByDescending(h => h.CreatedAt)
            .Skip(skip)
            .Take(query.PageSize)
            .ToList();

        // Map to DTOs
        var hospitalDtos = pagedHospitals.Select(h => new HospitalSummaryDto
        {
            Id = h.Id,
            Name = h.Name,
            RegistrationNumber = h.RegistrationNumber,
            TenantId = h.TenantId,
            Status = h.Status.ToString(),
            Email = h.PrimaryEmail.Value,
            Phone = h.PrimaryPhone.Value,
            City = h.RegisteredAddress.City,
            State = h.RegisteredAddress.State,
            Country = h.RegisteredAddress.Country,
            IsInTrial = h.IsInTrial,
            TrialEndDate = h.TrialEndDate,
            SubscriptionPlanId = h.SubscriptionPlanId,
            MonthlySubscriptionCost = h.MonthlySubscriptionCost,
            CreatedAt = h.CreatedAt
        }).ToList();

        var result = new PagedResult<HospitalSummaryDto>
        {
            Items = hospitalDtos,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = query.PageNumber > 1,
            HasNextPage = query.PageNumber < totalPages
        };

        _logger.LogInformation("Retrieved {Count} hospitals (page {Page} of {TotalPages})", 
            hospitalDtos.Count, query.PageNumber, totalPages);

        return Result<PagedResult<HospitalSummaryDto>>.Success(result);
    }
}
