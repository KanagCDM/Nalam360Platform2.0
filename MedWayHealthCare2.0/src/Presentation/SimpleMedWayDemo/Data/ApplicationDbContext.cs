using Microsoft.EntityFrameworkCore;

namespace SimpleMedWayDemo.Data;

/// <summary>
/// Main database context for the MedWay Healthcare System
/// Supports both SQL Server and PostgreSQL
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Core Entities
    public DbSet<Models.Hospital> Hospitals { get; set; }
    
    // Product Hierarchy
    public DbSet<Models.Product> Products { get; set; }
    public DbSet<Models.ProductModule> ProductModules { get; set; }
    public DbSet<Models.ModuleEntity> ModuleEntities { get; set; }
    
    // Subscription Management
    public DbSet<Models.SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<Models.HospitalSubscription> HospitalSubscriptions { get; set; }
    public DbSet<Models.SubscriptionUpgradeHistory> SubscriptionUpgradeHistories { get; set; }
    
    // Mapping Tables
    public DbSet<Models.SubscriptionProduct> SubscriptionProducts { get; set; }
    public DbSet<Models.SubscriptionModule> SubscriptionModules { get; set; }
    public DbSet<Models.SubscriptionPlanEntity> SubscriptionPlanEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureHospital(modelBuilder);
        ConfigureProductHierarchy(modelBuilder);
        ConfigureSubscriptions(modelBuilder);
        ConfigureMappings(modelBuilder);
        SeedData(modelBuilder);
    }

    private void ConfigureHospital(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.Hospital>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RegistrationNumber).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RegistrationNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AdminEmail).IsRequired().HasMaxLength(100);
            
            // Relationships
            entity.HasMany(e => e.Subscriptions)
                  .WithOne(e => e.Hospital)
                  .HasForeignKey(e => e.HospitalId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureProductHierarchy(ModelBuilder modelBuilder)
    {
        // Product Configuration
        modelBuilder.Entity<Models.Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Version).HasMaxLength(20);

            entity.HasMany(e => e.Modules)
                  .WithOne(e => e.Product)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Product Module Configuration
        modelBuilder.Entity<Models.ProductModule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProductId, e.Code }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);

            // Removed FK relationship with ModuleEntity - use simple string matching instead
        });

        // Module Entity Configuration
        modelBuilder.Entity<Models.ModuleEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EntityCode).IsUnique();
            entity.HasIndex(e => e.ModuleName);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ModuleName).IsRequired().HasMaxLength(50);
            
            // Note: ModuleName is a simple string reference, not a formal FK
            // This allows flexibility in entity management without strict FK constraints
        });
    }

    private void ConfigureSubscriptions(ModelBuilder modelBuilder)
    {
        // Subscription Plan Configuration
        modelBuilder.Entity<Models.SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Tier).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MonthlyPrice).HasPrecision(18, 2);
            entity.Property(e => e.YearlyPrice).HasPrecision(18, 2);

            entity.HasMany(e => e.IncludedEntities)
                  .WithOne(e => e.SubscriptionPlan)
                  .HasForeignKey(e => e.SubscriptionPlanId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Hospital Subscription Configuration
        modelBuilder.Entity<Models.HospitalSubscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.HospitalId, e.Status });
            entity.HasIndex(e => e.EndDate);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.Property(e => e.CurrentStorageUsedGB).HasPrecision(18, 2);

            entity.HasOne(e => e.Hospital)
                  .WithMany(e => e.Subscriptions)
                  .HasForeignKey(e => e.HospitalId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SubscriptionPlan)
                  .WithMany()
                  .HasForeignKey(e => e.SubscriptionPlanId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.UpgradeHistory)
                  .WithOne(e => e.HospitalSubscription)
                  .HasForeignKey(e => e.HospitalSubscriptionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Subscription Upgrade History Configuration
        modelBuilder.Entity<Models.SubscriptionUpgradeHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ChangeDate);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.ProrationAmount).HasPrecision(18, 2);

            entity.HasOne(e => e.FromPlan)
                  .WithMany()
                  .HasForeignKey(e => e.FromPlanId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ToPlan)
                  .WithMany()
                  .HasForeignKey(e => e.ToPlanId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureMappings(ModelBuilder modelBuilder)
    {
        // Subscription Product Mapping
        modelBuilder.Entity<Models.SubscriptionProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SubscriptionPlanId, e.ProductId }).IsUnique();

            entity.HasOne(e => e.SubscriptionPlan)
                  .WithMany()
                  .HasForeignKey(e => e.SubscriptionPlanId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany(e => e.SubscriptionProducts)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Subscription Module Mapping
        modelBuilder.Entity<Models.SubscriptionModule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SubscriptionPlanId, e.ProductModuleId }).IsUnique();

            entity.HasOne(e => e.SubscriptionPlan)
                  .WithMany()
                  .HasForeignKey(e => e.SubscriptionPlanId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ProductModule)
                  .WithMany(e => e.SubscriptionModules)
                  .HasForeignKey(e => e.ProductModuleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Subscription Plan Entity Mapping
        modelBuilder.Entity<Models.SubscriptionPlanEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SubscriptionPlanId, e.ModuleEntityId }).IsUnique();

            entity.HasOne(e => e.SubscriptionPlan)
                  .WithMany(e => e.IncludedEntities)
                  .HasForeignKey(e => e.SubscriptionPlanId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ModuleEntity)
                  .WithMany()
                  .HasForeignKey(e => e.ModuleEntityId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Subscription Plans
        var plans = Models.PredefinedPlans.GetAllPlans();
        for (int i = 0; i < plans.Count; i++)
        {
            plans[i].Id = i + 1;
        }
        modelBuilder.Entity<Models.SubscriptionPlan>().HasData(plans);

        // Seed Products
        var products = Models.PredefinedProducts.GetAllProducts();
        for (int i = 0; i < products.Count; i++)
        {
            products[i].Id = i + 1;
        }
        modelBuilder.Entity<Models.Product>().HasData(products);

        // Seed Modules for HMS Product (ID = 1)
        var modules = Models.PredefinedModules.GetHMSModules();
        for (int i = 0; i < modules.Count; i++)
        {
            modules[i].Id = i + 1;
            modules[i].ProductId = 1; // MedWay HMS
        }
        modelBuilder.Entity<Models.ProductModule>().HasData(modules);

        // Seed Entities
        var entities = Models.PredefinedEntities.GetAllEntities();
        for (int i = 0; i < entities.Count; i++)
        {
            entities[i].Id = i + 1;
        }
        modelBuilder.Entity<Models.ModuleEntity>().HasData(entities);

        // Seed Subscription-Entity Mappings
        SeedSubscriptionEntityMappings(modelBuilder, plans, entities);
    }

    private void SeedSubscriptionEntityMappings(
        ModelBuilder modelBuilder, 
        List<Models.SubscriptionPlan> plans, 
        List<Models.ModuleEntity> entities)
    {
        var mappings = Models.PredefinedPlanEntityMappings.GetPlanEntityMappings();
        var planEntityMappings = new List<Models.SubscriptionPlanEntity>();
        int id = 1;

        foreach (var plan in plans)
        {
            if (mappings.TryGetValue(plan.Tier, out var entityCodes))
            {
                foreach (var entityCode in entityCodes)
                {
                    var entity = entities.FirstOrDefault(e => e.EntityCode == entityCode);
                    if (entity != null)
                    {
                        planEntityMappings.Add(new Models.SubscriptionPlanEntity
                        {
                            Id = id++,
                            SubscriptionPlanId = plan.Id,
                            ModuleEntityId = entity.Id,
                            IsEnabled = true
                        });
                    }
                }
            }
        }

        modelBuilder.Entity<Models.SubscriptionPlanEntity>().HasData(planEntityMappings);
    }
}
