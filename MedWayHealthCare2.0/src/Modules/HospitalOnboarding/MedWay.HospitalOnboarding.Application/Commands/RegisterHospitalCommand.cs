using MedWay.Application.Abstractions;
using MedWay.Domain.Primitives;
using MedWay.Domain.ValueObjects;

namespace MedWay.HospitalOnboarding.Application.Commands;

/// <summary>
/// Command: Register new hospital with trial period
/// </summary>
public record RegisterHospitalCommand(
    string Name,
    string RegistrationNumber,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    string PrimaryEmail,
    string PrimaryPhone,
    DateTime EstablishedDate,
    string? TaxNumber = null,
    int TrialDays = 30) : ICommand<Result<Guid>>;

/// <summary>
/// Handler: Process hospital registration
/// </summary>
public class RegisterHospitalHandler : IRequestHandler<RegisterHospitalCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<RegisterHospitalHandler> _logger;

    public RegisterHospitalHandler(
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        IGuidProvider guidProvider,
        ILogger<RegisterHospitalHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _guidProvider = guidProvider;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RegisterHospitalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if registration number already exists
            var existingHospital = await _unitOfWork.Hospitals
                .GetByRegistrationNumberAsync(request.RegistrationNumber, cancellationToken);
            
            if (existingHospital is not null)
                return Result<Guid>.Failure(Error.Conflict(
                    "Hospital.RegistrationNumber", 
                    $"Hospital with registration number '{request.RegistrationNumber}' already exists"));

            // Generate unique tenant ID (hospital code + GUID suffix)
            var tenantId = $"HSP_{request.RegistrationNumber}_{_guidProvider.NewGuid().ToString("N")[..8]}".ToUpperInvariant();

            // Create value objects
            var addressResult = Address.Create(
                request.Street,
                request.City,
                request.State,
                request.PostalCode,
                request.Country);
            if (addressResult.IsFailure)
                return Result<Guid>.Failure(addressResult.Error);

            var emailResult = Email.Create(request.PrimaryEmail);
            if (emailResult.IsFailure)
                return Result<Guid>.Failure(emailResult.Error);

            var phoneResult = PhoneNumber.Create(request.PrimaryPhone);
            if (phoneResult.IsFailure)
                return Result<Guid>.Failure(phoneResult.Error);

            // Get current user ID (from claims/context)
            // TODO: Inject ICurrentUserService to get authenticated user ID
            var currentUserId = Guid.NewGuid(); // Placeholder

            // Create hospital
            var hospitalResult = Hospital.Create(
                request.Name,
                request.RegistrationNumber,
                tenantId,
                addressResult.Value,
                emailResult.Value,
                phoneResult.Value,
                request.EstablishedDate,
                currentUserId,
                request.TrialDays);

            if (hospitalResult.IsFailure)
                return Result<Guid>.Failure(hospitalResult.Error);

            var hospital = hospitalResult.Value;

            // Set tax number if provided
            if (!string.IsNullOrWhiteSpace(request.TaxNumber))
            {
                // Use reflection or add public setter - for now, stored in Update method
                var updateResult = hospital.Update(
                    hospital.Name,
                    request.TaxNumber,
                    hospital.RegisteredAddress,
                    hospital.PrimaryEmail,
                    hospital.PrimaryPhone,
                    currentUserId);
                
                if (updateResult.IsFailure)
                    return Result<Guid>.Failure(updateResult.Error);
            }

            // Persist
            await _unitOfWork.Hospitals.AddAsync(hospital, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Hospital registered: {HospitalId}, TenantId: {TenantId}, Name: {Name}",
                hospital.Id,
                hospital.TenantId,
                hospital.Name);

            // TODO: Send welcome email with trial details
            // TODO: Create default admin user for hospital
            // TODO: Publish integration event for other modules

            return Result<Guid>.Success(hospital.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering hospital: {RegistrationNumber}", request.RegistrationNumber);
            return Result<Guid>.Failure(Error.Internal("Hospital.Registration", "An error occurred during registration"));
        }
    }
}
