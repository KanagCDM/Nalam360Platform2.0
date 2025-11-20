# Nalam360 Platform - Data Access Module

Enterprise-grade data access layer with Repository, UnitOfWork, Specification patterns, Dapper for high performance, and automated database migrations.

## Features

### 🗄️ EF Core Repository Pattern
- Generic repository with full CRUD operations
- Specification pattern for complex queries
- Soft delete support
- Audit trail (CreatedAt, UpdatedAt, DeletedAt)
- Multi-tenancy filtering

### ⚡ Dapper Integration
- High-performance query execution
- Bulk operations (insert, update, delete)
- Stored procedure execution
- Multiple result sets
- Result<T> pattern for Railway-Oriented Programming

### 🔄 Database Migrations
- Automatic migration generation from EF Core models
- Transaction-based execution with rollback
- Version tracking with checksums
- Forward (up) and rollback (down) scripts
- Migration history auditing

### 🎯 Unit of Work Pattern
- Transaction management across repositories
- Automatic domain event dispatching
- Change tracking with EF Core

### 🔍 Specification Pattern
- Composable query specifications
- Type-safe LINQ expressions
- Reusable business rules

## Installation

```bash
dotnet add package Nalam360.Platform.Data
```

## Quick Start

### 1. EF Core Repository

```csharp
using Nalam360.Platform.Data.EntityFramework;
using Nalam360.Platform.Data.Repositories;

// Configure DbContext
public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITimeProvider timeProvider,
        IDomainEventDispatcher? eventDispatcher = null)
        : base(options, timeProvider, eventDispatcher)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.HasOne<Customer>().WithMany().HasForeignKey(e => e.CustomerId);
        });
    }
}

// Register services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IRepository<Order, Guid>, EfRepository<Order, Guid>>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

// Use in code
public class OrderService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Result<Guid>> CreateOrderAsync(CreateOrderCommand cmd)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = cmd.CustomerId,
            Total = cmd.Total,
            Status = OrderStatus.Pending
        };
        
        await _orderRepository.AddAsync(order);
        
        var saveResult = await _unitOfWork.SaveChangesAsync();
        if (saveResult.IsFailure)
            return Result<Guid>.Failure(saveResult.Error!);
        
        return Result<Guid>.Success(order.Id);
    }
    
    public async Task<IReadOnlyList<Order>> GetPendingOrdersAsync()
    {
        var spec = new Specification<Order>(o => o.Status == OrderStatus.Pending);
        return await _orderRepository.GetAllAsync(spec);
    }
}
```

### 2. Dapper for High Performance

```csharp
using Nalam360.Platform.Data.Dapper;

// Register Dapper repository
builder.Services.AddDapperRepository<Order>(connectionString);

// Or with connection factory
builder.Services.AddDapperRepository<Order>(sp =>
{
    var connStr = sp.GetRequiredService<IConfiguration>()
        .GetConnectionString("Database");
    return new SqlConnection(connStr);
});

// Use in code
public class ReportingService
{
    private readonly IDapperRepository<Order> _dapperRepo;
    
    // High-performance queries
    public async Task<Result<IEnumerable<OrderSummary>>> GetOrderSummaryAsync(
        DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT 
                o.CustomerId,
                c.CustomerName,
                COUNT(*) AS OrderCount,
                SUM(o.Total) AS TotalAmount
            FROM Orders o
            INNER JOIN Customers c ON o.CustomerId = c.Id
            WHERE o.OrderDate BETWEEN @StartDate AND @EndDate
            GROUP BY o.CustomerId, c.CustomerName
            ORDER BY TotalAmount DESC";
        
        return await _dapperRepo.QueryAsync<OrderSummary>(sql, new
        {
            StartDate = startDate,
            EndDate = endDate
        });
    }
    
    // Bulk operations
    public async Task<Result<int>> BulkInsertOrdersAsync(
        IEnumerable<Order> orders)
    {
        // Efficiently insert thousands of records
        return await _dapperRepo.BulkInsertAsync(orders);
    }
    
    // Stored procedures
    public async Task<Result<IEnumerable<MonthlyReport>>> GenerateMonthlyReportAsync(
        int year, int month)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Year", year);
        parameters.Add("@Month", month);
        parameters.Add("@TotalOrders", dbType: DbType.Int32, 
            direction: ParameterDirection.Output);
        
        var result = await _dapperRepo
            .ExecuteStoredProcedureWithOutputAsync<MonthlyReport>(
                "sp_GenerateMonthlyReport", parameters);
        
        if (result.IsSuccess)
        {
            var totalOrders = parameters.Get<int>("@TotalOrders");
            Console.WriteLine($"Total orders: {totalOrders}");
        }
        
        return result;
    }
}
```

### 3. Database Migrations

```csharp
using Nalam360.Platform.Data.Migrations;

// Register migration services
builder.Services.AddMigrations<ApplicationDbContext>(options =>
{
    options.MigrationsDirectory = "Migrations";
    options.AppliedBy = Environment.UserName;
});

// Or use file-based migrations
builder.Services.AddFileBasedMigrations<ApplicationDbContext>(
    migrationsPath: "Database/Migrations");

// Use in code
public class DatabaseInitializer
{
    private readonly IMigrationService _migrationService;
    
    public async Task InitializeDatabaseAsync()
    {
        // Check pending migrations
        var pendingResult = await _migrationService.GetPendingMigrationsAsync();
        if (pendingResult.IsSuccess && pendingResult.Value!.Any())
        {
            Console.WriteLine($"Found {pendingResult.Value.Count} pending migrations");
            
            // Apply all pending migrations
            var applyResult = await _migrationService.ApplyAllPendingAsync();
            if (applyResult.IsSuccess)
            {
                Console.WriteLine("All migrations applied successfully");
            }
        }
        
        // Generate new migration from model changes
        var migrationResult = await _migrationService.GenerateMigrationAsync(
            name: "AddOrderTables",
            description: "Add Order and OrderItem tables");
        
        if (migrationResult.IsSuccess)
        {
            Console.WriteLine($"Migration created: {migrationResult.Value!.Id}");
        }
    }
    
    public async Task RollbackLastMigrationAsync()
    {
        var rollbackResult = await _migrationService.RollbackLastMigrationAsync();
        if (rollbackResult.IsSuccess)
        {
            Console.WriteLine("Rollback completed successfully");
        }
    }
}
```

## Advanced Usage

### Specification Pattern

```csharp
// Define reusable specifications
public class ActiveCustomersSpecification : Specification<Customer>
{
    public ActiveCustomersSpecification()
        : base(c => c.IsActive && !c.IsDeleted)
    {
    }
}

public class CustomersByRegionSpecification : Specification<Customer>
{
    public CustomersByRegionSpecification(string region)
        : base(c => c.Region == region)
    {
    }
}

// Combine specifications
var activeInRegion = new ActiveCustomersSpecification()
    .And(new CustomersByRegionSpecification("North"));

var customers = await _repository.GetAllAsync(activeInRegion);
```

### Multi-Tenancy

```csharp
public class ApplicationDbContext : BaseDbContext
{
    private readonly ITenantProvider _tenantProvider;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Add tenant filter to all entities implementing ITenantEntity
        modelBuilder.Entity<Order>().HasQueryFilter(e => 
            e.TenantId == _tenantProvider.GetTenantId());
    }
}
```

### Custom Repository Methods

```csharp
public class OrderRepository : EfRepository<Order, Guid>
{
    public OrderRepository(DbContext context) : base(context)
    {
    }
    
    public async Task<IReadOnlyList<Order>> GetOrdersByCustomerAsync(
        Guid customerId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.OrderItems)
            .ToListAsync(ct);
    }
    
    public async Task<decimal> GetTotalRevenueAsync(
        DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await DbSet
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .SumAsync(o => o.Total, ct);
    }
}
```

### Bulk Operations with Dapper

```csharp
public class BulkDataService
{
    private readonly IDapperRepository<Product> _dapperRepo;
    
    public async Task<Result<int>> ImportProductsAsync(
        IEnumerable<Product> products)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var transaction = connection.BeginTransaction();
        
        try
        {
            // Bulk insert 10,000+ records efficiently
            var insertResult = await _dapperRepo.BulkInsertAsync(
                products, transaction);
            
            if (insertResult.IsFailure)
            {
                transaction.Rollback();
                return insertResult;
            }
            
            // Update inventory counts
            var updateSql = @"
                UPDATE Inventory 
                SET QuantityOnHand = p.InitialQuantity
                FROM Inventory i
                INNER JOIN Products p ON i.ProductId = p.Id";
            
            var updateResult = await _dapperRepo.ExecuteAsync(
                updateSql, transaction: transaction);
            
            transaction.Commit();
            return insertResult;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
```

### Migration Scripts

Migrations are stored as SQL scripts with up/down support:

```sql
-- Migration: 20250118_001_AddOrderTables
-- Up Script
CREATE TABLE [dbo].[Orders] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [CustomerId] UNIQUEIDENTIFIER NOT NULL,
    [OrderDate] DATETIME2 NOT NULL,
    [Total] DECIMAL(18,2) NOT NULL,
    [Status] INT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    [DeletedAt] DATETIME2 NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0
);

CREATE INDEX [IX_Orders_CustomerId] ON [dbo].[Orders] ([CustomerId]);
CREATE INDEX [IX_Orders_OrderDate] ON [dbo].[Orders] ([OrderDate]);

-- Down Script
DROP INDEX [IX_Orders_OrderDate] ON [dbo].[Orders];
DROP INDEX [IX_Orders_CustomerId] ON [dbo].[Orders];
DROP TABLE [dbo].[Orders];
```

## Performance Considerations

### When to Use EF Core
- ✅ CRUD operations on entities
- ✅ Change tracking needed
- ✅ Navigation properties required
- ✅ Complex object graphs
- ✅ Domain event dispatching

### When to Use Dapper
- ⚡ High-performance read queries
- ⚡ Bulk operations (1000+ records)
- ⚡ Reporting and analytics
- ⚡ Stored procedure execution
- ⚡ Custom SQL with optimizations

### Best Practices

1. **Use Specifications**: Encapsulate query logic in reusable specifications
2. **Leverage Bulk Operations**: Use Dapper for importing large datasets
3. **Transaction Management**: Always use UnitOfWork for multi-entity operations
4. **Migration Checksums**: Validate migration integrity before applying
5. **Async/Await**: All operations support async for better scalability

## Testing

```csharp
public class OrderRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddOrder()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        
        var context = new ApplicationDbContext(options, 
            new SystemTimeProvider(), null);
        var repository = new EfRepository<Order, Guid>(context);
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Total = 100m
        };
        
        // Act
        await repository.AddAsync(order);
        await context.SaveChangesAsync();
        
        // Assert
        var retrieved = await repository.GetByIdAsync(order.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(100m, retrieved.Total);
    }
}
```

## Configuration Options

```csharp
builder.Services.AddMigrations<ApplicationDbContext>(options =>
{
    // Directory for migration files
    options.MigrationsDirectory = "Database/Migrations";
    
    // Who applied the migration (for auditing)
    options.AppliedBy = Environment.UserName;
    
    // Validate checksums on apply
    options.ValidateChecksums = true;
    
    // Migration table name
    options.MigrationTableName = "__MigrationHistory";
});
```

## API Reference

See inline XML documentation for complete API reference.

## License

MIT License - See LICENSE file for details
