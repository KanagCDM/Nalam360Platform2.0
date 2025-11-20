# 🧪 Testing Guide

## Overview

This guide covers testing strategies for the Nalam360 Enterprise Platform, including unit tests, integration tests, and component tests.

---

## Test Projects

### Platform Tests
- **Location**: `tests/Nalam360.Platform.Tests/`
- **Framework**: xUnit
- **Target**: .NET 8
- **Coverage**: Platform modules

### UI Tests
- **Location**: `tests/Nalam360Enterprise.UI.Tests/`
- **Framework**: xUnit + bUnit
- **Target**: .NET 9
- **Coverage**: Blazor components

---

## Running Tests

### All Tests

```bash
# Run all tests in solution
dotnet test Nalam360EnterprisePlatform.sln --configuration Release

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Platform Tests Only

```bash
dotnet test tests/Nalam360.Platform.Tests/Nalam360.Platform.Tests.csproj --configuration Release
```

### UI Tests Only

```bash
dotnet test tests/Nalam360Enterprise.UI.Tests/Nalam360Enterprise.UI.Tests/Nalam360Enterprise.UI.Tests.csproj --configuration Release
```

### Specific Test

```bash
dotnet test --filter "FullyQualifiedName~CreateOrderHandler"
```

---

## Unit Testing Platform Modules

### Example: Testing CQRS Handler

```csharp
using Xunit;
using Moq;
using FluentAssertions;

public class CreateOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CreateOrderHandler _handler;
    
    public CreateOrderHandlerTests()
    {
        _mockRepository = new Mock<IOrderRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new CreateOrderHandler(_mockRepository.Object, _mockUnitOfWork.Object);
    }
    
    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateOrderCommand(Guid.NewGuid(), 100m);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithInvalidTotal_ReturnsFailure()
    {
        // Arrange
        var command = new CreateOrderCommand(Guid.NewGuid(), -10m);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Validation");
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Handle_WithNonPositiveTotal_ReturnsFailure(decimal total)
    {
        // Arrange
        var command = new CreateOrderCommand(Guid.NewGuid(), total);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
```

### Example: Testing Domain Entity

```csharp
public class OrderTests
{
    [Fact]
    public void Create_WithValidData_ReturnsOrder()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var total = 100m;
        
        // Act
        var order = Order.Create(customerId, total);
        
        // Assert
        order.Should().NotBeNull();
        order.CustomerId.Should().Be(customerId);
        order.Total.Should().Be(total);
        order.Status.Should().Be(OrderStatus.Pending);
        order.DomainEvents.Should().ContainSingle(e => e is OrderCreatedEvent);
    }
    
    [Fact]
    public void Complete_WhenPending_ChangesStatusToCompleted()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), 100m);
        order.ClearDomainEvents(); // Clear creation event
        
        // Act
        var result = order.Complete();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Completed);
        order.DomainEvents.Should().ContainSingle(e => e is OrderCompletedEvent);
    }
    
    [Fact]
    public void Complete_WhenAlreadyCompleted_ReturnsFailure()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), 100m);
        order.Complete();
        order.ClearDomainEvents();
        
        // Act
        var result = order.Complete();
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Validation");
    }
}
```

### Example: Testing Value Object

```csharp
public class AddressTests
{
    [Fact]
    public void Create_WithValidData_ReturnsAddress()
    {
        // Arrange & Act
        var result = Address.Create("123 Main St", "New York", "10001");
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Street.Should().Be("123 Main St");
        result.Value.City.Should().Be("New York");
        result.Value.ZipCode.Should().Be("10001");
    }
    
    [Theory]
    [InlineData("", "City", "12345")]
    [InlineData(null, "City", "12345")]
    [InlineData("   ", "City", "12345")]
    public void Create_WithInvalidStreet_ReturnsFailure(string street, string city, string zipCode)
    {
        // Act
        var result = Address.Create(street, city, zipCode);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Validation");
    }
    
    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var address1 = Address.Create("123 Main St", "New York", "10001").Value;
        var address2 = Address.Create("123 Main St", "New York", "10001").Value;
        
        // Act & Assert
        address1.Should().Be(address2);
        (address1 == address2).Should().BeTrue();
    }
    
    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var address1 = Address.Create("123 Main St", "New York", "10001").Value;
        var address2 = Address.Create("456 Oak Ave", "New York", "10001").Value;
        
        // Act & Assert
        address1.Should().NotBe(address2);
        (address1 != address2).Should().BeTrue();
    }
}
```

---

## Testing Blazor Components (bUnit)

### Setup Test Context

```csharp
using Bunit;
using Xunit;

public class N360TextBoxTests : TestContext
{
    [Fact]
    public void N360TextBox_Renders_WithPlaceholder()
    {
        // Arrange
        var expectedPlaceholder = "Enter username";
        
        // Act
        var cut = RenderComponent<N360TextBox>(parameters => parameters
            .Add(p => p.Placeholder, expectedPlaceholder));
        
        // Assert
        var input = cut.Find("input");
        input.GetAttribute("placeholder").Should().Be(expectedPlaceholder);
    }
    
    [Fact]
    public void N360TextBox_WithRequiredPermission_ShowsWhenHasPermission()
    {
        // Arrange
        Services.AddSingleton<IPermissionService>(new MockPermissionService(hasPermission: true));
        
        // Act
        var cut = RenderComponent<N360TextBox>(parameters => parameters
            .Add(p => p.RequiredPermission, "users.edit")
            .Add(p => p.HideIfNoPermission, false));
        
        // Assert
        cut.Find("input").Should().NotBeNull();
    }
    
    [Fact]
    public void N360TextBox_WithRequiredPermission_HidesWhenNoPermission()
    {
        // Arrange
        Services.AddSingleton<IPermissionService>(new MockPermissionService(hasPermission: false));
        
        // Act
        var cut = RenderComponent<N360TextBox>(parameters => parameters
            .Add(p => p.RequiredPermission, "users.edit")
            .Add(p => p.HideIfNoPermission, true));
        
        // Assert
        cut.Markup.Should().BeEmpty();
    }
    
    [Fact]
    public void N360TextBox_ValueChanged_TriggersEvent()
    {
        // Arrange
        var valueChanged = false;
        var newValue = "";
        
        var cut = RenderComponent<N360TextBox>(parameters => parameters
            .Add(p => p.Value, "initial")
            .Add(p => p.ValueChanged, value => 
            {
                valueChanged = true;
                newValue = value;
            }));
        
        // Act
        var input = cut.Find("input");
        input.Change("new value");
        
        // Assert
        valueChanged.Should().BeTrue();
        newValue.Should().Be("new value");
    }
}
```

### Testing Grid Component

```csharp
public class N360GridTests : TestContext
{
    [Fact]
    public void N360Grid_Renders_WithDataSource()
    {
        // Arrange
        var dataSource = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Item 1" },
            new TestItem { Id = 2, Name = "Item 2" }
        };
        
        // Act
        var cut = RenderComponent<N360Grid<TestItem>>(parameters => parameters
            .Add(p => p.DataSource, dataSource));
        
        // Assert
        var rows = cut.FindAll("tr");
        rows.Count.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public async Task N360Grid_RowSelected_TriggersCallback()
    {
        // Arrange
        TestItem selectedItem = null;
        var dataSource = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Item 1" }
        };
        
        var cut = RenderComponent<N360Grid<TestItem>>(parameters => parameters
            .Add(p => p.DataSource, dataSource)
            .Add(p => p.OnRowSelected, item => selectedItem = item));
        
        // Act
        var firstRow = cut.Find("tr:nth-child(2)"); // Skip header
        await firstRow.ClickAsync(new());
        
        // Assert
        selectedItem.Should().NotBeNull();
        selectedItem.Id.Should().Be(1);
    }
}
```

---

## Integration Testing

### Testing with In-Memory Database

```csharp
public class OrderRepositoryIntegrationTests : IDisposable
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly ApplicationDbContext _context;
    private readonly OrderRepository _repository;
    
    public OrderRepositoryIntegrationTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(_options);
        _repository = new OrderRepository(_context);
    }
    
    [Fact]
    public async Task AddAsync_AddsOrderToDatabase()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), 100m);
        
        // Act
        await _repository.AddAsync(order, CancellationToken.None);
        await _context.SaveChangesAsync();
        
        // Assert
        var savedOrder = await _repository.GetByIdAsync(order.Id, CancellationToken.None);
        savedOrder.Should().NotBeNull();
        savedOrder.Total.Should().Be(100m);
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
```

---

## Code Coverage

### Generate Coverage Report

```bash
# Install ReportGenerator
dotnet tool install --global dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator `
    -reports:"./**/coverage.cobertura.xml" `
    -targetdir:"./coverage-report" `
    -reporttypes:Html

# Open report
start ./coverage-report/index.html
```

### Coverage Goals

- **Platform Modules**: 80% minimum
- **UI Components**: 70% minimum
- **Critical paths**: 90%+ coverage

---

## Performance Testing

### Benchmark Example

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
public class OrderServiceBenchmarks
{
    private IOrderService _service;
    
    [GlobalSetup]
    public void Setup()
    {
        // Setup mock services
    }
    
    [Benchmark]
    public async Task CreateOrder_Baseline()
    {
        await _service.CreateOrderAsync(Guid.NewGuid(), 100m);
    }
    
    [Benchmark]
    public async Task CreateOrder_WithValidation()
    {
        await _service.CreateOrderWithValidationAsync(Guid.NewGuid(), 100m);
    }
}

// Run benchmarks
BenchmarkRunner.Run<OrderServiceBenchmarks>();
```

---

## Test Organization

```
tests/
├── Nalam360.Platform.Tests/
│   ├── Core/
│   │   ├── Results/
│   │   │   └── ResultTests.cs
│   │   └── Identity/
│   │       └── GuidProviderTests.cs
│   ├── Domain/
│   │   ├── Entities/
│   │   │   └── OrderTests.cs
│   │   └── ValueObjects/
│   │       └── AddressTests.cs
│   ├── Application/
│   │   ├── Commands/
│   │   │   └── CreateOrderHandlerTests.cs
│   │   └── Queries/
│   │       └── GetOrderHandlerTests.cs
│   └── Data/
│       └── Repositories/
│           └── OrderRepositoryTests.cs
└── Nalam360Enterprise.UI.Tests/
    └── Components/
        ├── Inputs/
        │   ├── N360TextBoxTests.cs
        │   └── N360GridTests.cs
        └── Navigation/
            └── N360MenuTests.cs
```

---

## Continuous Testing

Tests run automatically on:
- Every commit (via GitHub Actions)
- Pull requests
- Before package publishing

---

## Best Practices

1. **AAA Pattern**: Arrange, Act, Assert
2. **One assertion per test** (when possible)
3. **Meaningful test names**: `Method_Scenario_ExpectedResult`
4. **Use Theory for multiple test cases**
5. **Mock external dependencies**
6. **Test edge cases and error paths**
7. **Keep tests fast** (< 1 second each)
8. **Clean up resources** (IDisposable)

---

**Last Updated**: November 17, 2025  
**Version**: 1.0.0
