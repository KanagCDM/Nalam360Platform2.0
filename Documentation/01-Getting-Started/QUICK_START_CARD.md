# 🚀 Nalam360 Enterprise Platform - Quick Start Card

## Installation

```bash
# Install Platform Core
dotnet add package Nalam360.Platform.Core

# Install UI Library
dotnet add package Nalam360Enterprise.UI

# Install Syncfusion (required for UI)
dotnet add package Syncfusion.Blazor
```

## Basic Setup

### Platform Configuration (Program.cs)

```csharp
using Nalam360.Platform.Core;
using Nalam360.Platform.Application;

var builder = WebApplication.CreateBuilder(args);

// Add Platform modules
builder.Services.AddPlatformCore();
builder.Services.AddPlatformApplication(typeof(Program).Assembly);

var app = builder.Build();
app.Run();
```

### UI Configuration (Program.cs - Blazor)

```csharp
using Nalam360Enterprise.UI;

builder.Services.AddNalam360EnterpriseUI<YourPermissionService>(theme =>
{
    theme.CurrentTheme = Theme.Light;
    theme.SyncfusionThemeName = "material";
});
```

## Common Patterns

### 1. CQRS Command

```csharp
// Command
public record CreateOrderCommand(Guid CustomerId, decimal Total) 
    : ICommand<Result<Guid>>;

// Handler
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        // Validation
        if (cmd.Total <= 0)
            return Result<Guid>.Failure(Error.Validation("Total", "Must be positive"));
        
        // Business logic
        var order = Order.Create(cmd.CustomerId, cmd.Total);
        await _repository.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result<Guid>.Success(order.Id);
    }
}
```

### 2. CQRS Query

```csharp
// Query
public record GetOrderQuery(Guid OrderId) : IQuery<Result<OrderDto>>;

// Handler
public class GetOrderHandler : IRequestHandler<GetOrderQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(GetOrderQuery query, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(query.OrderId, ct);
        
        if (order is null)
            return Result<OrderDto>.Failure(Error.NotFound("Order", query.OrderId));
        
        return Result<OrderDto>.Success(order.ToDto());
    }
}
```

### 3. Domain Entity

```csharp
public class Order : AggregateRoot<Guid>
{
    public Guid CustomerId { get; private set; }
    public decimal Total { get; private set; }
    public OrderStatus Status { get; private set; }
    
    private Order() { } // EF Core
    
    public static Order Create(Guid customerId, decimal total)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Total = total,
            Status = OrderStatus.Pending
        };
        
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId));
        return order;
    }
    
    public Result Complete()
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure(Error.Validation("Status", "Order already completed"));
        
        Status = OrderStatus.Completed;
        AddDomainEvent(new OrderCompletedEvent(Id));
        
        return Result.Success();
    }
}
```

### 4. Value Object

```csharp
public class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string ZipCode { get; private set; }
    
    private Address() { }
    
    public static Result<Address> Create(string street, string city, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            return Result<Address>.Failure(Error.Validation("Street", "Required"));
        
        return Result<Address>.Success(new Address 
        { 
            Street = street, 
            City = city, 
            ZipCode = zipCode 
        });
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return ZipCode;
    }
}
```

### 5. Repository Pattern

```csharp
public interface IOrderRepository : IRepository<Order, Guid>
{
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken ct);
}

public class OrderRepository : Repository<Order, Guid>, IOrderRepository
{
    public OrderRepository(DbContext context) : base(context) { }
    
    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(
        Guid customerId, 
        CancellationToken ct)
    {
        return await _dbSet
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(ct);
    }
}
```

### 6. UI Component Usage

```razor
@page "/orders"
@using Nalam360Enterprise.UI.Components.Data
@using Nalam360Enterprise.UI.Components.Inputs

<N360TextBox @bind-Value="@searchTerm"
             Placeholder="Search orders..."
             RequiredPermission="orders.search"
             EnableAudit="true"
             AuditResource="Orders" />

<N360Grid TItem="OrderDto"
          DataSource="@orders"
          EnablePaging="true"
          PageSize="20"
          EnableFiltering="true"
          EnableSorting="true"
          RequiredPermission="orders.view"
          OnRowSelected="@HandleRowSelected">
    <GridColumns>
        <GridColumn Field="@nameof(OrderDto.Id)" HeaderText="ID" />
        <GridColumn Field="@nameof(OrderDto.CustomerName)" HeaderText="Customer" />
        <GridColumn Field="@nameof(OrderDto.Total)" HeaderText="Total" Format="C2" />
        <GridColumn Field="@nameof(OrderDto.Status)" HeaderText="Status" />
    </GridColumns>
</N360Grid>

@code {
    private List<OrderDto> orders = new();
    private string searchTerm = "";
    
    protected override async Task OnInitializedAsync()
    {
        var result = await Mediator.Send(new GetOrdersQuery());
        if (result.IsSuccess)
            orders = result.Value.ToList();
    }
    
    private void HandleRowSelected(OrderDto order)
    {
        NavigationManager.NavigateTo($"/orders/{order.Id}");
    }
}
```

## Result<T> Error Handling

```csharp
// Success
return Result<Order>.Success(order);
return Result.Success();

// Failure
return Result.Failure(Error.NotFound("Order", orderId));
return Result.Failure(Error.Validation("Total", "Must be positive"));
return Result.Failure(Error.Unauthorized("Insufficient permissions"));
return Result.Failure(Error.Conflict("Order already exists"));

// Usage
var result = await Mediator.Send(new CreateOrderCommand(...));

if (result.IsSuccess)
{
    var orderId = result.Value;
    // Continue with success flow
}
else
{
    var error = result.Error;
    // Handle error: error.Code, error.Message
}

// Chaining
var result = await CreateOrder()
    .Then(order => ProcessPayment(order))
    .Then(payment => SendConfirmation(payment));
```

## Component Parameters

### Common Parameters (All Components)

```razor
<N360TextBox 
    @* Value Binding *@
    @bind-Value="@value"
    
    @* RBAC *@
    RequiredPermission="users.edit"
    HideIfNoPermission="true"
    
    @* Audit *@
    EnableAudit="true"
    AuditResource="UserProfile"
    AuditAction="Edit"
    
    @* Styling *@
    CssClass="custom-class"
    Width="300px"
    
    @* Accessibility *@
    AriaLabel="Username input"
    TabIndex="1"
    
    @* Theming *@
    IsRtl="false"
    
    @* State *@
    Disabled="false"
    ReadOnly="false"
    
    @* Validation *@
    ValidationRules="@rules"
    
    @* Events *@
    ValueChanged="@OnValueChanged"
    OnFocus="@OnFocus"
    OnBlur="@OnBlur" />
```

## Validation Rules

```csharp
var rules = new ValidationRules()
    .Required("Username is required")
    .MinLength(3, "At least 3 characters")
    .MaxLength(50, "Maximum 50 characters")
    .Pattern(@"^[a-zA-Z0-9_]+$", "Only alphanumeric and underscore")
    .Custom(async value => 
    {
        var exists = await CheckUsernameExists(value);
        return exists 
            ? ValidationResult.Error("Username already taken") 
            : ValidationResult.Success();
    });
```

## Dependency Injection

```csharp
// Register custom services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddTransient<IEmailService, SendGridEmailService>();

// Use in components/handlers
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    
    public CreateOrderHandler(
        IOrderRepository repository,
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }
    
    // Handler implementation...
}
```

## Testing

### Unit Test Example

```csharp
[Fact]
public async Task CreateOrder_WithValidData_ReturnsSuccess()
{
    // Arrange
    var command = new CreateOrderCommand(Guid.NewGuid(), 100m);
    var handler = new CreateOrderHandler(_mockRepository.Object, _mockUnitOfWork.Object);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotEqual(Guid.Empty, result.Value);
}
```

### Component Test (bUnit)

```csharp
[Fact]
public void N360TextBox_WithRequiredPermission_HidesWhenNoPermission()
{
    // Arrange
    var ctx = new TestContext();
    ctx.Services.AddSingleton<IPermissionService>(new MockPermissionService(hasPermission: false));
    
    // Act
    var cut = ctx.RenderComponent<N360TextBox>(parameters => parameters
        .Add(p => p.RequiredPermission, "users.edit")
        .Add(p => p.HideIfNoPermission, true));
    
    // Assert
    Assert.Empty(cut.Markup); // Component should be hidden
}
```

## Helpful Commands

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Create migration (EF Core)
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# Pack NuGet
dotnet pack --configuration Release

# Run example API
dotnet run --project examples/Nalam360.Platform.Example.Api
```

## Troubleshooting

### Issue: Component not found
**Solution**: Add `@using Nalam360Enterprise.UI.Components.*` to `_Imports.razor`

### Issue: Permission check always fails
**Solution**: Ensure `IPermissionService` is registered and returns true for the permission

### Issue: Audit logging not working
**Solution**: Verify `IAuditService` is registered and `EnableAudit="true"` on component

### Issue: Theme not applying
**Solution**: Check `ThemeService` configuration and ensure Syncfusion theme CSS is included

## Documentation Links

- 📖 **README.md** - Project overview
- 📚 **PLATFORM_GUIDE.md** - Platform module details
- 🎨 **COMPONENT_INVENTORY.md** - All UI components
- 🚀 **DEPLOYMENT_GUIDE.md** - Deployment instructions
- ✅ **COMPLETION_REPORT.md** - Project status

## Support

- 📧 GitHub Issues: https://github.com/KanagCDM/Nalam360EnterprisePlatform/issues
- 💬 Documentation: See README.md
- 🌐 Website: https://nalam360.com (if applicable)

---

**Quick Reference Card** | Version 1.0.0 | Last Updated: November 17, 2025
