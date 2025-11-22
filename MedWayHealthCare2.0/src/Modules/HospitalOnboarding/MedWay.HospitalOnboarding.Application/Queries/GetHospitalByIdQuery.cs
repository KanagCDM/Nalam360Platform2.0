using MedWay.Application.Abstractions;
using MedWay.Domain.Primitives;

namespace MedWay.HospitalOnboarding.Application.Queries;

/// <summary>
/// Query: Get hospital by ID
/// </summary>
public record GetHospitalByIdQuery(Guid HospitalId) : IQuery<Result<HospitalDto>>;

/// <summary>
/// Handler: Retrieve hospital details
/// </summary>
public class GetHospitalByIdHandler : IRequestHandler<GetHospitalByIdQuery, Result<HospitalDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetHospitalByIdHandler> _logger;

    public GetHospitalByIdHandler(
        IUnitOfWork _unitOfWork,
        ILogger<GetHospitalByIdHandler> logger)
    {
        _unitOfWork = _unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HospitalDto>> Handle(GetHospitalByIdQuery request, CancellationToken cancellationToken)
    {
        var hospital = await _unitOfWork.Hospitals.GetByIdAsync(request.HospitalId, cancellationToken);

        if (hospital is null)
            return Result<HospitalDto>.Failure(Error.NotFound("Hospital", request.HospitalId));

        var dto = new HospitalDto
        {
            Id = hospital.Id,
            Name = hospital.Name,
            RegistrationNumber = hospital.RegistrationNumber,
            TenantId = hospital.TenantId,
            TaxNumber = hospital.TaxNumber,
            Address = new AddressDto
            {
                Street = hospital.RegisteredAddress.Street,
                City = hospital.RegisteredAddress.City,
                State = hospital.RegisteredAddress.State,
                PostalCode = hospital.RegisteredAddress.PostalCode,
                Country = hospital.RegisteredAddress.Country
            },
            PrimaryEmail = hospital.PrimaryEmail.Value,
            PrimaryPhone = hospital.PrimaryPhone.Value,
            Status = hospital.Status.ToString(),
            EstablishedDate = hospital.EstablishedDate,
            TrialStartDate = hospital.TrialStartDate,
            TrialEndDate = hospital.TrialEndDate,
            IsInTrial = hospital.IsInTrial,
            CurrentSubscriptionPlanId = hospital.CurrentSubscriptionPlanId,
            SubscriptionStartDate = hospital.SubscriptionStartDate,
            SubscriptionEndDate = hospital.SubscriptionEndDate,
            CreatedAt = hospital.CreatedAt,
            ModifiedAt = hospital.ModifiedAt
        };

        return Result<HospitalDto>.Success(dto);
    }
}

/// <summary>
/// Hospital DTO
/// </summary>
public class HospitalDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string RegistrationNumber { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string? TaxNumber { get; set; }
    public AddressDto Address { get; set; } = null!;
    public string PrimaryEmail { get; set; } = null!;
    public string PrimaryPhone { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime EstablishedDate { get; set; }
    public DateTime? TrialStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public bool IsInTrial { get; set; }
    public Guid? CurrentSubscriptionPlanId { get; set; }
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class AddressDto
{
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = null!;
}
