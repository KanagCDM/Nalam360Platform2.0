using Microsoft.EntityFrameworkCore;
using ProductAdminPortal.Models;
using ProductAdminPortal.Models.Domain;

namespace ProductAdminPortal.Data;

/// <summary>
/// Application database context for ProductAdminPortal (New Domain-Agnostic Version)
/// Uses Models.Domain namespace for new domain-agnostic entities
/// </summary>
public class ProductAdminDbContext : DbContext
{
    public ProductAdminDbContext(DbContextOptions<ProductAdminDbContext> options)
        : base(options)
    {
    }

    // Identity & Tenancy
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;

    // Products (using Domain namespace explicitly)
    public DbSet<Models.Domain.Product> Products { get; set; } = null!;
    public DbSet<Models.Domain.Module> Modules { get; set; } = null!;
    public DbSet<Models.Domain.Entity> Entities { get; set; } = null!;
    public DbSet<Models.Domain.ModuleEntity> ModuleEntities { get; set; } = null!;

    // Subscriptions
    public DbSet<Models.Domain.SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
    public DbSet<Models.Domain.SubscriptionProduct> SubscriptionProducts { get; set; } = null!;
    public DbSet<Models.Domain.SubscriptionModule> SubscriptionModules { get; set; } = null!;
    public DbSet<Models.Domain.SubscriptionEntity> SubscriptionEntities { get; set; } = null!;

    // Pricing
    public DbSet<PricingRule> PricingRules { get; set; } = null!;
    public DbSet<PricingTier> PricingTiers { get; set; } = null!;
    public DbSet<ComplexityMultiplier> ComplexityMultipliers { get; set; } = null!;
    public DbSet<DiscountCode> DiscountCodes { get; set; } = null!;

    // Customers
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<CustomerSubscription> CustomerSubscriptions { get; set; } = null!;
    public DbSet<UsageRecord> UsageRecords { get; set; } = null!;

    // Billing & Audit
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<ConfigurationVersion> ConfigurationVersions { get; set; } = null!;
    public DbSet<UsageAlert> UsageAlerts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure composite keys for junction tables
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<Models.Domain.ModuleEntity>()
            .HasKey(me => new { me.ModuleId, me.EntityId });

        modelBuilder.Entity<Models.Domain.SubscriptionProduct>()
            .HasKey(sp => new { sp.SubscriptionPlanId, sp.ProductId });

        modelBuilder.Entity<Models.Domain.SubscriptionModule>()
            .HasKey(sm => new { sm.SubscriptionPlanId, sm.ModuleId });

        modelBuilder.Entity<Models.Domain.SubscriptionEntity>()
            .HasKey(se => new { se.SubscriptionPlanId, se.EntityId });

        // Configure indexes for performance
        // Multi-tenant isolation indexes
        modelBuilder.Entity<Models.Domain.Product>()
            .HasIndex(p => p.TenantId);

        modelBuilder.Entity<Models.Domain.Module>()
            .HasIndex(m => m.ProductId);

        modelBuilder.Entity<Models.Domain.Entity>()
            .HasIndex(e => e.TenantId);

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.TenantId);

        modelBuilder.Entity<UsageRecord>()
            .HasIndex(ur => ur.CustomerSubscriptionId);

        // Unique constraints
        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.Subdomain)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Models.Domain.Product>()
            .HasIndex(p => new { p.TenantId, p.Code })
            .IsUnique();

        modelBuilder.Entity<DiscountCode>()
            .HasIndex(dc => dc.Code)
            .IsUnique();

        // Audit log indexes
        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => new { al.TenantId, al.EntityType, al.EntityId });

        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => al.CreatedAt);

        // Configure delete behaviors
        modelBuilder.Entity<Models.Domain.Product>()
            .HasMany(p => p.Modules)
            .WithOne(m => m.Product)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Customer>()
            .HasMany(c => c.CustomerSubscriptions)
            .WithOne(cs => cs.Customer)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.InvoiceLineItems)
            .WithOne(ili => ili.Invoice)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PricingRule>()
            .HasMany(pr => pr.PricingTiers)
            .WithOne(pt => pt.PricingRule)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure decimal precision
        modelBuilder.Entity<Models.Domain.SubscriptionPlan>()
            .Property(sp => sp.Price)
            .HasPrecision(15, 2);

        modelBuilder.Entity<PricingTier>()
            .Property(pt => pt.UnitPrice)
            .HasPrecision(15, 4);

        modelBuilder.Entity<ComplexityMultiplier>()
            .Property(cm => cm.Multiplier)
            .HasPrecision(15, 4);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.Subtotal)
            .HasPrecision(15, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.TaxAmount)
            .HasPrecision(15, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.TotalAmount)
            .HasPrecision(15, 2);

        modelBuilder.Entity<InvoiceLineItem>()
            .Property(ili => ili.UnitPrice)
            .HasPrecision(15, 4);

        modelBuilder.Entity<InvoiceLineItem>()
            .Property(ili => ili.Amount)
            .HasPrecision(15, 2);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-update timestamps
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
