using MedWay.Domain.Primitives;
using MedWay.HospitalOnboarding.Domain.Entities;

namespace MedWay.HospitalOnboarding.Domain.Repositories;

/// <summary>
/// Hospital repository interface
/// </summary>
public interface IHospitalRepository
{
    Task<Hospital?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Hospital?> GetByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<Hospital?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Hospital>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Hospital>> GetByStatusAsync(HospitalStatus status, CancellationToken cancellationToken = default);
    Task<Hospital?> GetWithBranchesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Hospital?> GetWithFacilitiesAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Hospital hospital, CancellationToken cancellationToken = default);
    void Update(Hospital hospital);
    void Remove(Hospital hospital);
}

/// <summary>
/// Branch repository interface
/// </summary>
public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Branch?> GetByBranchCodeAsync(Guid hospitalId, string branchCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Branch>> GetByHospitalIdAsync(Guid hospitalId, CancellationToken cancellationToken = default);
    Task<Branch?> GetWithFacilitiesAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Branch branch, CancellationToken cancellationToken = default);
    void Update(Branch branch);
    void Remove(Branch branch);
}

/// <summary>
/// Global facility repository interface
/// </summary>
public interface IGlobalFacilityRepository
{
    Task<GlobalFacility?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GlobalFacility?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<GlobalFacility>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<GlobalFacility>> GetByCategoryAsync(FacilityCategory category, CancellationToken cancellationToken = default);
    Task AddAsync(GlobalFacility facility, CancellationToken cancellationToken = default);
    void Update(GlobalFacility facility);
    void Remove(GlobalFacility facility);
}

/// <summary>
/// Subscription plan repository interface
/// </summary>
public interface ISubscriptionPlanRepository
{
    Task<SubscriptionPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetWithFacilitiesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubscriptionPlan>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubscriptionPlan>> GetPublicPlansAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default);
    void Update(SubscriptionPlan plan);
    void Remove(SubscriptionPlan plan);
}

/// <summary>
/// Payment repository interface
/// </summary>
public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByHospitalIdAsync(Guid hospitalId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetOverduePaymentsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    void Update(Payment payment);
    void Remove(Payment payment);
}

/// <summary>
/// Unit of Work interface - coordinates repository operations
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IHospitalRepository Hospitals { get; }
    IBranchRepository Branches { get; }
    IGlobalFacilityRepository GlobalFacilities { get; }
    ISubscriptionPlanRepository SubscriptionPlans { get; }
    IPaymentRepository Payments { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
