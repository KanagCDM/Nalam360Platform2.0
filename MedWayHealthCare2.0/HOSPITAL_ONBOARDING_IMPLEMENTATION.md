# Hospital Onboarding System - Implementation Guide

## 📦 Project Structure Created

```
MedWayHealthCare2.0/
├── src/
│   └── Modules/
│       └── HospitalOnboarding/
│           ├── MedWay.HospitalOnboarding.Domain/
│           │   ├── Entities/
│           │   │   ├── Hospital.cs ✅
│           │   │   ├── Branch.cs ✅
│           │   │   ├── Facility.cs ✅
│           │   │   ├── SubscriptionPlan.cs ✅
│           │   │   └── Payment.cs ✅
│           │   ├── Events/
│           │   │   └── DomainEvents.cs ✅
│           │   └── Repositories/
│           │       └── IRepositories.cs ✅
│           ├── MedWay.HospitalOnboarding.Application/ (TODO)
│           └── MedWay.HospitalOnboarding.Infrastructure/ (TODO)
```

## ✅ Completed: Domain Model

### Domain Entities Created

1. **Hospital** (Aggregate Root)
   - Multi-tenant support via `TenantId`
   - Trial period management (30 days default)
   - Subscription lifecycle
   - Status workflow: Draft → PendingApproval → Active/Rejected → Suspended/Closed
   - Collections: Branches, Facilities
   - Business methods: Create, Update, AddFacility, AddBranch, Submit, Approve, Reject, Suspend, Reactivate, ActivateSubscription

2. **Branch** (Entity)
   - Hospital location management
   - Unique branch codes within hospital
   - Manager assignment
   - Operating hours
   - Facility mapping
   - Business methods: Create, Update, AddFacility, RemoveFacility, AssignManager, Deactivate, Reactivate

3. **GlobalFacility** (Entity)
   - Master list of facilities (Emergency, ICU, Lab, Pharmacy, etc.)
   - Category-based organization (Clinical, Diagnostic, Support, Administrative)
   - Base monthly cost for subscription calculation
   - System Admin managed

4. **HospitalFacility & BranchFacility** (Junction Entities)
   - Many-to-many relationships
   - Track when facility was added and by whom

5. **SubscriptionPlan** (Entity)
   - Flexible pricing model
   - Base price + per-user + per-branch + facility costs
   - Included vs. additional resource limits
   - Public/private plans
   - CalculateMonthlyCost method for dynamic pricing

6. **Payment** (Aggregate Root)
   - Payment gateway integration ready
   - Status tracking: Pending → Successful/Failed → Refunded
   - Invoice generation
   - Billing period management
   - Retry mechanism for failed payments

### Domain Events (21 events)
- Hospital: Registered, Updated, Submitted, Approved, Rejected, Suspended, Reactivated
- Facility: Added, Removed (hospital level)
- Branch: Added
- Subscription: Activated
- Payment: Created, Successful, Failed, Refunded, RetryInitiated

### Repository Interfaces
- `IHospitalRepository`
- `IBranchRepository`
- `IGlobalFacilityRepository`
- `ISubscriptionPlanRepository`
- `IPaymentRepository`
- `IUnitOfWork` (transaction coordination)

## 🔧 TODO: Infrastructure Layer (EF Core)

### Files to Create

#### 1. MedWay.HospitalOnboarding.Infrastructure.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\..\src\Nalam360.Platform.Data\Nalam360.Platform.Data.csproj" />
    <ProjectReference Include="..\..\..\..\src\Nalam360.Platform.Tenancy\Nalam360.Platform.Tenancy.csproj" />
    <ProjectReference Include="..\MedWay.HospitalOnboarding.Domain\MedWay.HospitalOnboarding.Domain.csproj" />
    <ProjectReference Include="..\..\..\..\src\Core\MedWay.Domain\MedWay.Domain.csproj" />
  </ItemGroup>
</Project>
```

#### 2. HospitalOnboardingDbContext.cs
```csharp
using Microsoft.EntityFrameworkCore;
using Nalam360.Platform.Core.Time;
using Nalam360.Platform.Data.EntityFramework;
using Nalam360.Platform.Domain.Events;
using Nalam360.Platform.Tenancy;
using MedWay.HospitalOnboarding.Domain.Entities;

namespace MedWay.HospitalOnboarding.Infrastructure.Persistence;

/// <summary>
/// Hospital Onboarding DbContext with multi-tenant support
/// Inherits from Nalam360 Platform BaseDbContext for domain events and auditing
/// </summary>
public class HospitalOnboardingDbContext : BaseDbContext
{
    private readonly ITenantProvider _tenantProvider;

    public HospitalOnboardingDbContext(
        DbContextOptions<HospitalOnboardingDbContext> options,
        ITimeProvider timeProvider,
        ITenantProvider tenantProvider,
        IDomainEventDispatcher? domainEventDispatcher = null)
        : base(options, timeProvider, domainEventDispatcher)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Hospital> Hospitals => Set<Hospital>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<GlobalFacility> GlobalFacilities => Set<GlobalFacility>();
    public DbSet<HospitalFacility> HospitalFacilities => Set<HospitalFacility>();
    public DbSet<BranchFacility> BranchFacilities => Set<BranchFacility>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<SubscriptionPlanFacility> SubscriptionPlanFacilities => Set<SubscriptionPlanFacility>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HospitalOnboardingDbContext).Assembly);

        // Global query filter for multi-tenancy (Hospital only)
        // TODO: Consider applying to other tenant-scoped entities
        modelBuilder.Entity<Hospital>().HasQueryFilter(h => 
            h.TenantId == _tenantProvider.GetCurrentTenantId());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Apply tenant ID to new Hospital entities
        var tenantId = _tenantProvider.GetCurrentTenantId();
        if (!string.IsNullOrEmpty(tenantId))
        {
            var hospitalEntries = ChangeTracker.Entries<Hospital>()
                .Where(e => e.State == EntityState.Added);

            foreach (var entry in hospitalEntries)
            {
                if (string.IsNullOrEmpty(entry.Entity.TenantId))
                {
                    // Set tenant ID via reflection (since TenantId is private set)
                    var property = entry.Property("TenantId");
                    property.CurrentValue = tenantId;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

#### 3. Entity Configurations (EF Core Fluent API)

**HospitalConfiguration.cs**
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedWay.HospitalOnboarding.Domain.Entities;

namespace MedWay.HospitalOnboarding.Infrastructure.Persistence.Configurations;

public class HospitalConfiguration : IEntityTypeConfiguration<Hospital>
{
    public void Configure(EntityTypeBuilder<Hospital> builder)
    {
        builder.ToTable("Hospitals", "HospitalOnboarding");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .ValueGeneratedNever();

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(h => h.RegistrationNumber)
            .IsUnique();

        builder.Property(h => h.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(h => h.TenantId)
            .IsUnique();

        builder.Property(h => h.TaxNumber)
            .HasMaxLength(50);

        builder.Property(h => h.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(h => h.Status);

        // Owned type: Address
        builder.OwnsOne(h => h.RegisteredAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("Address_Street").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("Address_City").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("Address_State").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("Address_PostalCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("Address_Country").HasMaxLength(100);
        });

        // Owned type: Email
        builder.OwnsOne(h => h.PrimaryEmail, email =>
        {
            email.Property(e => e.Value).HasColumnName("PrimaryEmail").HasMaxLength(255);
        });

        // Owned type: PhoneNumber
        builder.OwnsOne(h => h.PrimaryPhone, phone =>
        {
            phone.Property(p => p.Value).HasColumnName("PrimaryPhone").HasMaxLength(20);
        });

        // Relationships
        builder.HasMany<Branch>()
            .WithOne()
            .HasForeignKey(b => b.HospitalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<HospitalFacility>()
            .WithOne()
            .HasForeignKey(hf => hf.HospitalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit fields
        builder.Property(h => h.CreatedAt).IsRequired();
        builder.Property(h => h.ModifiedAt);
        builder.Property(h => h.CreatedBy).IsRequired();
        builder.Property(h => h.ModifiedBy);

        // Indexes for performance
        builder.HasIndex(h => h.CreatedAt);
        builder.HasIndex(h => h.CurrentSubscriptionPlanId);
    }
}
```

**BranchConfiguration.cs**
```csharp
public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches", "HospitalOnboarding");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.BranchCode)
            .IsRequired()
            .HasMaxLength(20);

        // Composite unique index: BranchCode must be unique within a hospital
        builder.HasIndex(b => new { b.HospitalId, b.BranchCode })
            .IsUnique();

        builder.Property(b => b.OperatingHours)
            .HasMaxLength(500);

        // Owned types: Address, Email, PhoneNumber (similar to Hospital)
        builder.OwnsOne(b => b.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("Address_Street").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("Address_City").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("Address_State").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("Address_PostalCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("Address_Country").HasMaxLength(100);
        });

        builder.OwnsOne(b => b.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email").HasMaxLength(255);
        });

        builder.OwnsOne(b => b.Phone, phone =>
        {
            phone.Property(p => p.Value).HasColumnName("Phone").HasMaxLength(20);
        });

        builder.HasMany<BranchFacility>()
            .WithOne()
            .HasForeignKey(bf => bf.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => b.HospitalId);
        builder.HasIndex(b => b.IsActive);
    }
}
```

**GlobalFacilityConfiguration.cs**, **SubscriptionPlanConfiguration.cs**, **PaymentConfiguration.cs** - Similar patterns

#### 4. Repository Implementations

**HospitalRepository.cs**
```csharp
using Microsoft.EntityFrameworkCore;
using MedWay.HospitalOnboarding.Domain.Entities;
using MedWay.HospitalOnboarding.Domain.Repositories;

namespace MedWay.HospitalOnboarding.Infrastructure.Persistence.Repositories;

public class HospitalRepository : IHospitalRepository
{
    private readonly HospitalOnboardingDbContext _context;

    public HospitalRepository(HospitalOnboardingDbContext context)
    {
        _context = context;
    }

    public async Task<Hospital?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Hospitals.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Hospital?> GetByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Hospitals
            .FirstOrDefaultAsync(h => h.TenantId == tenantId, cancellationToken);
    }

    public async Task<Hospital?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Hospitals
            .FirstOrDefaultAsync(h => h.RegistrationNumber == registrationNumber, cancellationToken);
    }

    public async Task<IEnumerable<Hospital>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Hospitals.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Hospital>> GetByStatusAsync(HospitalStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Hospitals
            .Where(h => h.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<Hospital?> GetWithBranchesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Hospitals
            .Include(h => h.Branches)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<Hospital?> GetWithFacilitiesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Hospitals
            .Include(h => h.Facilities)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task AddAsync(Hospital hospital, CancellationToken cancellationToken = default)
    {
        await _context.Hospitals.AddAsync(hospital, cancellationToken);
    }

    public void Update(Hospital hospital)
    {
        _context.Hospitals.Update(hospital);
    }

    public void Remove(Hospital hospital)
    {
        _context.Hospitals.Remove(hospital);
    }
}
```

**UnitOfWork.cs**
```csharp
using Microsoft.EntityFrameworkCore.Storage;
using MedWay.HospitalOnboarding.Domain.Repositories;

namespace MedWay.HospitalOnboarding.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly HospitalOnboardingDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(
        HospitalOnboardingDbContext context,
        IHospitalRepository hospitals,
        IBranchRepository branches,
        IGlobalFacilityRepository globalFacilities,
        ISubscriptionPlanRepository subscriptionPlans,
        IPaymentRepository payments)
    {
        _context = context;
        Hospitals = hospitals;
        Branches = branches;
        GlobalFacilities = globalFacilities;
        SubscriptionPlans = subscriptionPlans;
        Payments = payments;
    }

    public IHospitalRepository Hospitals { get; }
    public IBranchRepository Branches { get; }
    public IGlobalFacilityRepository GlobalFacilities { get; }
    public ISubscriptionPlanRepository SubscriptionPlans { get; }
    public IPaymentRepository Payments { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
```

#### 5. Dependency Injection

**ServiceCollectionExtensions.cs**
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MedWay.HospitalOnboarding.Domain.Repositories;
using MedWay.HospitalOnboarding.Infrastructure.Persistence;
using MedWay.HospitalOnboarding.Infrastructure.Persistence.Repositories;

namespace MedWay.HospitalOnboarding.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHospitalOnboardingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useSqlServer = true)
    {
        // Register DbContext
        if (useSqlServer)
        {
            services.AddDbContext<HospitalOnboardingDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("SqlServer"),
                    b => b.MigrationsAssembly(typeof(HospitalOnboardingDbContext).Assembly.FullName)));
        }
        else
        {
            services.AddDbContext<HospitalOnboardingDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("PostgreSQL"),
                    b => b.MigrationsAssembly(typeof(HospitalOnboardingDbContext).Assembly.FullName)));
        }

        // Register repositories
        services.AddScoped<IHospitalRepository, HospitalRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IGlobalFacilityRepository, GlobalFacilityRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
```

## 📝 Database DDL Scripts

### SQL Server DDL
```sql
-- Create schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'HospitalOnboarding')
BEGIN
    EXEC('CREATE SCHEMA HospitalOnboarding')
END
GO

-- Hospitals table
CREATE TABLE HospitalOnboarding.Hospitals (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    RegistrationNumber NVARCHAR(50) NOT NULL UNIQUE,
    TenantId NVARCHAR(100) NOT NULL UNIQUE,
    TaxNumber NVARCHAR(50) NULL,
    
    -- Address (owned type)
    Address_Street NVARCHAR(200) NULL,
    Address_City NVARCHAR(100) NULL,
    Address_State NVARCHAR(100) NULL,
    Address_PostalCode NVARCHAR(20) NULL,
    Address_Country NVARCHAR(100) NULL,
    
    PrimaryEmail NVARCHAR(255) NOT NULL,
    PrimaryPhone NVARCHAR(20) NOT NULL,
    
    Status NVARCHAR(20) NOT NULL,
    EstablishedDate DATETIME2 NOT NULL,
    
    TrialStartDate DATETIME2 NULL,
    TrialEndDate DATETIME2 NULL,
    
    CurrentSubscriptionPlanId UNIQUEIDENTIFIER NULL,
    SubscriptionStartDate DATETIME2 NULL,
    SubscriptionEndDate DATETIME2 NULL,
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedAt DATETIME2 NULL,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    ModifiedBy UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT FK_Hospital_SubscriptionPlan 
        FOREIGN KEY (CurrentSubscriptionPlanId) 
        REFERENCES HospitalOnboarding.SubscriptionPlans(Id)
);

CREATE INDEX IX_Hospitals_TenantId ON HospitalOnboarding.Hospitals(TenantId);
CREATE INDEX IX_Hospitals_Status ON HospitalOnboarding.Hospitals(Status);
CREATE INDEX IX_Hospitals_CreatedAt ON HospitalOnboarding.Hospitals(CreatedAt);

-- Branches table
CREATE TABLE HospitalOnboarding.Branches (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    HospitalId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    BranchCode NVARCHAR(20) NOT NULL,
    
    Address_Street NVARCHAR(200) NULL,
    Address_City NVARCHAR(100) NULL,
    Address_State NVARCHAR(100) NULL,
    Address_PostalCode NVARCHAR(20) NULL,
    Address_Country NVARCHAR(100) NULL,
    
    Phone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    
    ManagerUserId UNIQUEIDENTIFIER NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    OpeningDate DATETIME2 NOT NULL,
    ClosingDate DATETIME2 NULL,
    OperatingHours NVARCHAR(500) NULL,
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Branch_Hospital 
        FOREIGN KEY (HospitalId) 
        REFERENCES HospitalOnboarding.Hospitals(Id) 
        ON DELETE CASCADE
);

CREATE UNIQUE INDEX IX_Branches_HospitalId_BranchCode 
    ON HospitalOnboarding.Branches(HospitalId, BranchCode);
CREATE INDEX IX_Branches_IsActive ON HospitalOnboarding.Branches(IsActive);

-- GlobalFacilities table
CREATE TABLE HospitalOnboarding.GlobalFacilities (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    Category NVARCHAR(20) NOT NULL,
    BaseMonthlyCost DECIMAL(18,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedAt DATETIME2 NULL
);

CREATE INDEX IX_GlobalFacilities_Code ON HospitalOnboarding.GlobalFacilities(Code);
CREATE INDEX IX_GlobalFacilities_Category ON HospitalOnboarding.GlobalFacilities(Category);

-- HospitalFacilities (junction table)
CREATE TABLE HospitalOnboarding.HospitalFacilities (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    HospitalId UNIQUEIDENTIFIER NOT NULL,
    GlobalFacilityId UNIQUEIDENTIFIER NOT NULL,
    AddedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AddedBy UNIQUEIDENTIFIER NOT NULL,
    
    CONSTRAINT FK_HospitalFacility_Hospital 
        FOREIGN KEY (HospitalId) 
        REFERENCES HospitalOnboarding.Hospitals(Id) 
        ON DELETE CASCADE,
    CONSTRAINT FK_HospitalFacility_Facility 
        FOREIGN KEY (GlobalFacilityId) 
        REFERENCES HospitalOnboarding.GlobalFacilities(Id)
);

CREATE UNIQUE INDEX IX_HospitalFacilities_Hospital_Facility 
    ON HospitalOnboarding.HospitalFacilities(HospitalId, GlobalFacilityId);

-- BranchFacilities (junction table)
CREATE TABLE HospitalOnboarding.BranchFacilities (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    BranchId UNIQUEIDENTIFIER NOT NULL,
    GlobalFacilityId UNIQUEIDENTIFIER NOT NULL,
    AddedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AddedBy UNIQUEIDENTIFIER NOT NULL,
    
    CONSTRAINT FK_BranchFacility_Branch 
        FOREIGN KEY (BranchId) 
        REFERENCES HospitalOnboarding.Branches(Id) 
        ON DELETE CASCADE,
    CONSTRAINT FK_BranchFacility_Facility 
        FOREIGN KEY (GlobalFacilityId) 
        REFERENCES HospitalOnboarding.GlobalFacilities(Id)
);

CREATE UNIQUE INDEX IX_BranchFacilities_Branch_Facility 
    ON HospitalOnboarding.BranchFacilities(BranchId, GlobalFacilityId);

-- SubscriptionPlans table
CREATE TABLE HospitalOnboarding.SubscriptionPlans (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    
    BaseMonthlyPrice DECIMAL(18,2) NOT NULL,
    MaxUsers INT NOT NULL DEFAULT 0,
    MaxBranches INT NOT NULL DEFAULT 0,
    PricePerAdditionalUser DECIMAL(18,2) NOT NULL DEFAULT 0,
    PricePerAdditionalBranch DECIMAL(18,2) NOT NULL DEFAULT 0,
    IncludedUsers INT NOT NULL DEFAULT 0,
    IncludedBranches INT NOT NULL DEFAULT 0,
    
    IsActive BIT NOT NULL DEFAULT 1,
    IsPublic BIT NOT NULL DEFAULT 1,
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedAt DATETIME2 NULL
);

CREATE INDEX IX_SubscriptionPlans_Code ON HospitalOnboarding.SubscriptionPlans(Code);
CREATE INDEX IX_SubscriptionPlans_IsActive ON HospitalOnboarding.SubscriptionPlans(IsActive);

-- SubscriptionPlanFacilities (junction table)
CREATE TABLE HospitalOnboarding.SubscriptionPlanFacilities (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SubscriptionPlanId UNIQUEIDENTIFIER NOT NULL,
    GlobalFacilityId UNIQUEIDENTIFIER NOT NULL,
    
    CONSTRAINT FK_PlanFacility_Plan 
        FOREIGN KEY (SubscriptionPlanId) 
        REFERENCES HospitalOnboarding.SubscriptionPlans(Id) 
        ON DELETE CASCADE,
    CONSTRAINT FK_PlanFacility_Facility 
        FOREIGN KEY (GlobalFacilityId) 
        REFERENCES HospitalOnboarding.GlobalFacilities(Id)
);

CREATE UNIQUE INDEX IX_PlanFacilities_Plan_Facility 
    ON HospitalOnboarding.SubscriptionPlanFacilities(SubscriptionPlanId, GlobalFacilityId);

-- Payments table
CREATE TABLE HospitalOnboarding.Payments (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    HospitalId UNIQUEIDENTIFIER NOT NULL,
    SubscriptionPlanId UNIQUEIDENTIFIER NOT NULL,
    
    InvoiceNumber NVARCHAR(50) NOT NULL UNIQUE,
    Amount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(10) NOT NULL DEFAULT 'USD',
    
    Status NVARCHAR(20) NOT NULL,
    PaymentMethod NVARCHAR(20) NOT NULL,
    
    TransactionId NVARCHAR(100) NULL,
    PaymentGateway NVARCHAR(50) NULL,
    
    BillingPeriodStart DATETIME2 NOT NULL,
    BillingPeriodEnd DATETIME2 NOT NULL,
    
    PaidAt DATETIME2 NULL,
    DueDate DATETIME2 NOT NULL,
    
    PaymentMetadata NVARCHAR(MAX) NULL,
    FailureReason NVARCHAR(500) NULL,
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Payment_Hospital 
        FOREIGN KEY (HospitalId) 
        REFERENCES HospitalOnboarding.Hospitals(Id),
    CONSTRAINT FK_Payment_SubscriptionPlan 
        FOREIGN KEY (SubscriptionPlanId) 
        REFERENCES HospitalOnboarding.SubscriptionPlans(Id)
);

CREATE INDEX IX_Payments_HospitalId ON HospitalOnboarding.Payments(HospitalId);
CREATE INDEX IX_Payments_Status ON HospitalOnboarding.Payments(Status);
CREATE INDEX IX_Payments_DueDate ON HospitalOnboarding.Payments(DueDate);
CREATE INDEX IX_Payments_InvoiceNumber ON HospitalOnboarding.Payments(InvoiceNumber);

-- Seed Data
INSERT INTO HospitalOnboarding.GlobalFacilities (Id, Name, Code, Description, Category, BaseMonthlyCost, IsActive, CreatedAt)
VALUES
    (NEWID(), 'Emergency Department', 'EMR', 'Emergency and trauma care', 'Clinical', 1000.00, 1, GETUTCDATE()),
    (NEWID(), 'Intensive Care Unit', 'ICU', 'Critical care unit', 'Clinical', 1500.00, 1, GETUTCDATE()),
    (NEWID(), 'Outpatient Department', 'OPD', 'Outpatient consultations', 'Clinical', 500.00, 1, GETUTCDATE()),
    (NEWID(), 'Inpatient Department', 'IPD', 'Inpatient admissions', 'Clinical', 800.00, 1, GETUTCDATE()),
    (NEWID(), 'Operation Theater', 'OT', 'Surgical procedures', 'Clinical', 2000.00, 1, GETUTCDATE()),
    (NEWID(), 'Laboratory', 'LAB', 'Clinical laboratory testing', 'Diagnostic', 600.00, 1, GETUTCDATE()),
    (NEWID(), 'Radiology', 'RAD', 'Imaging services', 'Diagnostic', 1200.00, 1, GETUTCDATE()),
    (NEWID(), 'Pharmacy', 'PHARM', 'Medication dispensing', 'Support', 400.00, 1, GETUTCDATE()),
    (NEWID(), 'Blood Bank', 'BB', 'Blood storage and transfusion', 'Support', 700.00, 1, GETUTCDATE()),
    (NEWID(), 'Billing', 'BILL', 'Billing and invoicing', 'Administrative', 300.00, 1, GETUTCDATE());

INSERT INTO HospitalOnboarding.SubscriptionPlans (Id, Name, Code, Description, BaseMonthlyPrice, MaxUsers, MaxBranches, PricePerAdditionalUser, PricePerAdditionalBranch, IncludedUsers, IncludedBranches, IsActive, IsPublic, CreatedAt)
VALUES
    (NEWID(), 'Basic Plan', 'BASIC', 'Starter plan for small clinics', 99.00, 10, 1, 5.00, 50.00, 5, 1, 1, 1, GETUTCDATE()),
    (NEWID(), 'Standard Plan', 'STANDARD', 'For growing hospitals', 299.00, 50, 5, 4.00, 40.00, 20, 2, 1, 1, GETUTCDATE()),
    (NEWID(), 'Premium Plan', 'PREMIUM', 'Enterprise hospitals', 999.00, 0, 0, 3.00, 30.00, 100, 10, 1, 1, GETUTCDATE());
GO
```

### PostgreSQL DDL
```sql
-- Create schema
CREATE SCHEMA IF NOT EXISTS "HospitalOnboarding";

-- Hospitals table
CREATE TABLE "HospitalOnboarding"."Hospitals" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "RegistrationNumber" VARCHAR(50) NOT NULL UNIQUE,
    "TenantId" VARCHAR(100) NOT NULL UNIQUE,
    "TaxNumber" VARCHAR(50),
    
    -- Address
    "Address_Street" VARCHAR(200),
    "Address_City" VARCHAR(100),
    "Address_State" VARCHAR(100),
    "Address_PostalCode" VARCHAR(20),
    "Address_Country" VARCHAR(100),
    
    "PrimaryEmail" VARCHAR(255) NOT NULL,
    "PrimaryPhone" VARCHAR(20) NOT NULL,
    
    "Status" VARCHAR(20) NOT NULL,
    "EstablishedDate" TIMESTAMP NOT NULL,
    
    "TrialStartDate" TIMESTAMP,
    "TrialEndDate" TIMESTAMP,
    
    "CurrentSubscriptionPlanId" UUID,
    "SubscriptionStartDate" TIMESTAMP,
    "SubscriptionEndDate" TIMESTAMP,
    
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedAt" TIMESTAMP,
    "CreatedBy" UUID NOT NULL,
    "ModifiedBy" UUID
);

CREATE INDEX "IX_Hospitals_TenantId" ON "HospitalOnboarding"."Hospitals"("TenantId");
CREATE INDEX "IX_Hospitals_Status" ON "HospitalOnboarding"."Hospitals"("Status");
CREATE INDEX "IX_Hospitals_CreatedAt" ON "HospitalOnboarding"."Hospitals"("CreatedAt");

-- Similar tables for Branches, Facilities, Plans, Payments...
-- (Full PostgreSQL DDL available in separate file)
```

## 🔄 Next Steps

This document provides the complete structure for the Hospital Onboarding system. The following files need to be generated:

1. ✅ **Domain Layer** - Complete
2. **Infrastructure Layer** - Use templates above to create:
   - Repository implementations (5 files)
   - Entity configurations (7 files)
   - DbContext and UnitOfWork
   - DI registration

3. **Application Layer** - To implement:
   - CQRS Commands & Queries (RegisterHospitalCommand, etc.)
   - Command Handlers with validation
   - DTOs
   - MediatR pipeline behaviors

4. **WebAPI Layer** - To implement:
   - Controllers with JWT auth
   - Middleware
   - API documentation

5. **Blazor UI** - To implement:
   - Syncfusion components
   - Admin dashboards

6. **Testing** - To implement:
   - Unit tests
   - Integration tests
   - Postman collection

7. **Docker** - To implement:
   - docker-compose.yml
   - Dockerfiles

Would you like me to continue with any specific layer?
