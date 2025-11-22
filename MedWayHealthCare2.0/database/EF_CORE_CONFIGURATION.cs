using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace MedWay.HospitalOnboarding.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework Core configuration for Hospital Onboarding schema
    /// Maps to PostgreSQL/SQL Server multi-tenant database
    /// </summary>
    /// 
    // ============================================================================
    // ENUMS
    // ============================================================================
    
    public enum HospitalStatus
    {
        PendingApproval,
        Active,
        Rejected,
        Suspended,
        Inactive
    }
    
    public enum SubscriptionStatus
    {
        Trial,
        Active,
        Expired,
        Suspended,
        Cancelled
    }
    
    public enum PaymentStatus
    {
        Pending,
        Successful,
        Failed,
        Refunded,
        PartiallyRefunded
    }
    
    public enum PaymentMethod
    {
        CreditCard,
        DebitCard,
        BankTransfer,
        UPI,
        Wallet,
        Cheque
    }
    
    public enum BillingCycle
    {
        Monthly,
        Quarterly,
        HalfYearly,
        Yearly
    }
    
    public enum FacilityCategory
    {
        Diagnostic,
        Treatment,
        Surgical,
        Emergency,
        ICU,
        Pharmacy,
        Laboratory,
        Radiology,
        Other
    }
    
    // ============================================================================
    // ENTITIES
    // ============================================================================
    
    public class Hospital
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        
        // Registration Information
        public string RegistrationNumber { get; set; }
        public string Name { get; set; }
        public string LegalName { get; set; }
        
        // Contact Information
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? AlternatePhone { get; set; }
        public string? Website { get; set; }
        
        // Address
        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; } = "India";
        public string PostalCode { get; set; }
        
        // Business Information
        public string? TaxId { get; set; }
        public string? LicenseNumber { get; set; }
        public string? AccreditationDetails { get; set; } // JSON
        
        // Status & Workflow
        public HospitalStatus Status { get; set; } = HospitalStatus.PendingApproval;
        public DateTimeOffset? ApprovedAt { get; set; }
        public Guid? ApprovedBy { get; set; }
        public DateTimeOffset? RejectedAt { get; set; }
        public Guid? RejectedBy { get; set; }
        public string? RejectionReason { get; set; }
        public DateTimeOffset? SuspendedAt { get; set; }
        public Guid? SuspendedBy { get; set; }
        public string? SuspensionReason { get; set; }
        
        // Subscription Information
        public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Trial;
        public Guid? SubscriptionPlanId { get; set; }
        public DateTimeOffset? SubscriptionStartDate { get; set; }
        public DateTimeOffset? SubscriptionEndDate { get; set; }
        public DateTimeOffset? TrialEndDate { get; set; }
        
        // Capacity & Billing
        public int TotalBeds { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalStaff { get; set; }
        public decimal MonthlySubscriptionCost { get; set; }
        
        // Metadata
        public string? Settings { get; set; } // JSON
        public string? Metadata { get; set; } // JSON
        
        // Audit Fields
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? CreatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? UpdatedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
        
        // Navigation Properties
        public SubscriptionPlan? SubscriptionPlan { get; set; }
        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<HospitalFacility> HospitalFacilities { get; set; } = new List<HospitalFacility>();
    }
    
    public class Branch
    {
        public Guid Id { get; set; }
        public Guid HospitalId { get; set; }
        
        // Branch Information
        public string BranchCode { get; set; }
        public string Name { get; set; }
        
        // Contact Information
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? AlternatePhone { get; set; }
        
        // Address
        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; } = "India";
        public string PostalCode { get; set; }
        
        // Operational Information
        public bool IsMainBranch { get; set; }
        public bool IsActive { get; set; } = true;
        public int TotalBeds { get; set; }
        public string? OperatingHours { get; set; } // JSON
        
        // Manager Information
        public Guid? BranchManagerId { get; set; }
        public string? BranchManagerName { get; set; }
        public string? BranchManagerEmail { get; set; }
        public string? BranchManagerPhone { get; set; }
        
        // Metadata
        public string? Metadata { get; set; } // JSON
        
        // Audit Fields
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? CreatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? UpdatedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
        
        // Navigation Properties
        public Hospital Hospital { get; set; }
        public ICollection<BranchFacility> BranchFacilities { get; set; } = new List<BranchFacility>();
    }
    
    public class SubscriptionPlan
    {
        public Guid Id { get; set; }
        
        // Plan Information
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        
        // Pricing
        public decimal BasePrice { get; set; }
        public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
        public string Currency { get; set; } = "INR";
        
        // Capacity Limits
        public int MaxUsers { get; set; } = 10;
        public int MaxBranches { get; set; } = 1;
        public int? MaxBeds { get; set; }
        
        // Per-Unit Pricing
        public decimal PricePerAdditionalUser { get; set; }
        public decimal PricePerAdditionalBranch { get; set; }
        public decimal PricePerAdditionalBed { get; set; }
        
        // Features
        public string IncludedFeatures { get; set; } = "[]"; // JSON array
        
        // Plan Settings
        public bool IsTrial { get; set; }
        public int TrialDurationDays { get; set; } = 30;
        public bool IsPublic { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public bool IsRecommended { get; set; }
        public int DisplayOrder { get; set; }
        
        // Metadata
        public string? Metadata { get; set; } // JSON
        
        // Audit Fields
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? CreatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? UpdatedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
        
        // Navigation Properties
        public ICollection<Hospital> Hospitals { get; set; } = new List<Hospital>();
    }
    
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid HospitalId { get; set; }
        
        // Payment Information
        public string PaymentReference { get; set; }
        public string InvoiceNumber { get; set; }
        
        // Amount Details
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "INR";
        
        // Payment Details
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public string? TransactionId { get; set; }
        
        // Dates
        public DateTimeOffset? PaymentDate { get; set; }
        public DateTimeOffset DueDate { get; set; }
        
        // Billing Period
        public DateTimeOffset BillingPeriodStart { get; set; }
        public DateTimeOffset BillingPeriodEnd { get; set; }
        
        // Payment Gateway Details
        public string? GatewayName { get; set; }
        public string? GatewayResponse { get; set; } // JSON
        
        // Refund Information
        public decimal RefundAmount { get; set; }
        public DateTimeOffset? RefundDate { get; set; }
        public string? RefundReason { get; set; }
        public string? RefundReference { get; set; }
        
        // Retry Information
        public int RetryCount { get; set; }
        public DateTimeOffset? LastRetryAt { get; set; }
        public DateTimeOffset? NextRetryAt { get; set; }
        
        // Invoice
        public string? InvoiceUrl { get; set; }
        public DateTimeOffset? InvoiceGeneratedAt { get; set; }
        
        // Notes
        public string? Notes { get; set; }
        public string? Metadata { get; set; } // JSON
        
        // Audit Fields
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? CreatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? UpdatedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
        
        // Navigation Properties
        public Hospital Hospital { get; set; }
    }
    
    public class GlobalFacility
    {
        public Guid Id { get; set; }
        
        // Facility Information
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public FacilityCategory Category { get; set; }
        
        // Classification
        public bool IsStandard { get; set; } = true;
        public bool RequiresCertification { get; set; }
        public string? CertificationDetails { get; set; }
        
        // Display
        public string? IconName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Metadata
        public string? Metadata { get; set; } // JSON
        
        // Audit Fields
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? CreatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? UpdatedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
        
        // Navigation Properties
        public ICollection<HospitalFacility> HospitalFacilities { get; set; } = new List<HospitalFacility>();
    }
    
    public class HospitalFacility
    {
        public Guid Id { get; set; }
        public Guid HospitalId { get; set; }
        public Guid FacilityId { get; set; }
        
        // Facility Details
        public bool IsAvailable { get; set; } = true;
        public int? Capacity { get; set; }
        public string? Notes { get; set; }
        
        // Metadata
        public string? Metadata { get; set; } // JSON
        
        // Audit Fields
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? CreatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? UpdatedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
        
        // Navigation Properties
        public Hospital Hospital { get; set; }
        public GlobalFacility Facility { get; set; }
        public ICollection<BranchFacility> BranchFacilities { get; set; } = new List<BranchFacility>();
    }
    
    public class BranchFacility
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public Guid HospitalFacilityId { get; set; }
        
        // Facility Details
        public bool IsAvailable { get; set; } = true;
        public int? Capacity { get; set; }
        public string? FloorNumber { get; set; }
        public string? LocationDetails { get; set; }
        public string? OperatingHours { get; set; } // JSON
        
        // Responsible Staff
        public Guid? InchargeStaffId { get; set; }
        public string? InchargeStaffName { get; set; }
        
        // Metadata
        public string? Metadata { get; set; } // JSON
        
        // Audit Fields
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? CreatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? UpdatedBy { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
        
        // Navigation Properties
        public Branch Branch { get; set; }
        public HospitalFacility HospitalFacility { get; set; }
    }
    
    // ============================================================================
    // DB CONTEXT
    // ============================================================================
    
    public class HospitalOnboardingDbContext : DbContext
    {
        private readonly Guid? _currentTenantId;
        
        public HospitalOnboardingDbContext(DbContextOptions<HospitalOnboardingDbContext> options)
            : base(options)
        {
        }
        
        public HospitalOnboardingDbContext(
            DbContextOptions<HospitalOnboardingDbContext> options,
            Guid? currentTenantId)
            : base(options)
        {
            _currentTenantId = currentTenantId;
        }
        
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<GlobalFacility> GlobalFacilities { get; set; }
        public DbSet<HospitalFacility> HospitalFacilities { get; set; }
        public DbSet<BranchFacility> BranchFacilities { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Set default schema
            modelBuilder.HasDefaultSchema("hospital_onboarding");
            
            // Apply all configurations
            modelBuilder.ApplyConfiguration(new HospitalConfiguration());
            modelBuilder.ApplyConfiguration(new BranchConfiguration());
            modelBuilder.ApplyConfiguration(new SubscriptionPlanConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentConfiguration());
            modelBuilder.ApplyConfiguration(new GlobalFacilityConfiguration());
            modelBuilder.ApplyConfiguration(new HospitalFacilityConfiguration());
            modelBuilder.ApplyConfiguration(new BranchFacilityConfiguration());
            
            // Global query filters for soft delete and multi-tenancy
            modelBuilder.Entity<Hospital>().HasQueryFilter(h => !h.IsDeleted);
            modelBuilder.Entity<Branch>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<Payment>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<GlobalFacility>().HasQueryFilter(f => !f.IsDeleted);
            modelBuilder.Entity<HospitalFacility>().HasQueryFilter(hf => !hf.IsDeleted);
            modelBuilder.Entity<BranchFacility>().HasQueryFilter(bf => !bf.IsDeleted);
            
            // Tenant isolation filter (if tenant context is set)
            if (_currentTenantId.HasValue)
            {
                modelBuilder.Entity<Hospital>()
                    .HasQueryFilter(h => !h.IsDeleted && h.TenantId == _currentTenantId.Value);
            }
        }
        
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }
        
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }
        
        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
            
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Property("CreatedAt").CurrentValue == null)
                        entry.Property("CreatedAt").CurrentValue = DateTimeOffset.UtcNow;
                }
                
                if (entry.State == EntityState.Modified)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTimeOffset.UtcNow;
                }
            }
        }
    }
    
    // ============================================================================
    // ENTITY CONFIGURATIONS
    // ============================================================================
    
    public class HospitalConfiguration : IEntityTypeConfiguration<Hospital>
    {
        public void Configure(EntityTypeBuilder<Hospital> builder)
        {
            builder.ToTable("hospitals", "hospital_onboarding");
            
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Id).HasColumnName("id");
            
            builder.HasIndex(h => h.TenantId).HasDatabaseName("idx_hospitals_tenant_id");
            builder.HasIndex(h => h.RegistrationNumber).IsUnique().HasDatabaseName("idx_hospitals_registration_number");
            builder.HasIndex(h => h.Email).IsUnique().HasDatabaseName("idx_hospitals_email");
            
            builder.Property(h => h.RegistrationNumber).HasMaxLength(50).IsRequired();
            builder.Property(h => h.Name).HasMaxLength(200).IsRequired();
            builder.Property(h => h.Email).HasMaxLength(100).IsRequired();
            
            builder.Property(h => h.Status)
                .HasConversion<string>()
                .HasMaxLength(50);
            
            builder.Property(h => h.SubscriptionStatus)
                .HasConversion<string>()
                .HasMaxLength(50);
            
            // Relationships
            builder.HasOne(h => h.SubscriptionPlan)
                .WithMany(sp => sp.Hospitals)
                .HasForeignKey(h => h.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.SetNull);
            
            builder.HasMany(h => h.Branches)
                .WithOne(b => b.Hospital)
                .HasForeignKey(b => b.HospitalId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class BranchConfiguration : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {
            builder.ToTable("branches", "hospital_onboarding");
            
            builder.HasKey(b => b.Id);
            builder.HasIndex(b => new { b.HospitalId, b.BranchCode })
                .IsUnique()
                .HasDatabaseName("uq_branch_code_per_hospital");
            
            builder.Property(b => b.BranchCode).HasMaxLength(50).IsRequired();
            builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
        }
    }
    
    public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
        {
            builder.ToTable("subscription_plans", "hospital_onboarding");
            
            builder.HasKey(sp => sp.Id);
            builder.HasIndex(sp => sp.Code).IsUnique();
            
            builder.Property(sp => sp.Code).HasMaxLength(50).IsRequired();
            builder.Property(sp => sp.Name).HasMaxLength(200).IsRequired();
            builder.Property(sp => sp.BasePrice).HasPrecision(10, 2);
            
            builder.Property(sp => sp.BillingCycle)
                .HasConversion<string>()
                .HasMaxLength(50);
        }
    }
    
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("payments", "hospital_onboarding");
            
            builder.HasKey(p => p.Id);
            builder.HasIndex(p => p.PaymentReference).IsUnique();
            builder.HasIndex(p => p.InvoiceNumber).IsUnique();
            
            builder.Property(p => p.Amount).HasPrecision(10, 2);
            builder.Property(p => p.TotalAmount).HasPrecision(10, 2);
            
            builder.Property(p => p.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(50);
            
            builder.Property(p => p.PaymentStatus)
                .HasConversion<string>()
                .HasMaxLength(50);
            
            builder.HasOne(p => p.Hospital)
                .WithMany(h => h.Payments)
                .HasForeignKey(p => p.HospitalId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class GlobalFacilityConfiguration : IEntityTypeConfiguration<GlobalFacility>
    {
        public void Configure(EntityTypeBuilder<GlobalFacility> builder)
        {
            builder.ToTable("global_facilities", "hospital_onboarding");
            
            builder.HasKey(gf => gf.Id);
            builder.HasIndex(gf => gf.Code).IsUnique();
            
            builder.Property(gf => gf.Code).HasMaxLength(50).IsRequired();
            builder.Property(gf => gf.Name).HasMaxLength(200).IsRequired();
            
            builder.Property(gf => gf.Category)
                .HasConversion<string>()
                .HasMaxLength(50);
        }
    }
    
    public class HospitalFacilityConfiguration : IEntityTypeConfiguration<HospitalFacility>
    {
        public void Configure(EntityTypeBuilder<HospitalFacility> builder)
        {
            builder.ToTable("hospital_facilities", "hospital_onboarding");
            
            builder.HasKey(hf => hf.Id);
            builder.HasIndex(hf => new { hf.HospitalId, hf.FacilityId })
                .IsUnique()
                .HasDatabaseName("uq_hospital_facility");
            
            builder.HasOne(hf => hf.Hospital)
                .WithMany(h => h.HospitalFacilities)
                .HasForeignKey(hf => hf.HospitalId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasOne(hf => hf.Facility)
                .WithMany(gf => gf.HospitalFacilities)
                .HasForeignKey(hf => hf.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class BranchFacilityConfiguration : IEntityTypeConfiguration<BranchFacility>
    {
        public void Configure(EntityTypeBuilder<BranchFacility> builder)
        {
            builder.ToTable("branch_facilities", "hospital_onboarding");
            
            builder.HasKey(bf => bf.Id);
            builder.HasIndex(bf => new { bf.BranchId, bf.HospitalFacilityId })
                .IsUnique()
                .HasDatabaseName("uq_branch_hospital_facility");
            
            builder.HasOne(bf => bf.Branch)
                .WithMany(b => b.BranchFacilities)
                .HasForeignKey(bf => bf.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasOne(bf => bf.HospitalFacility)
                .WithMany(hf => hf.BranchFacilities)
                .HasForeignKey(bf => bf.HospitalFacilityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
