# Nalam360 Enterprise Platform - AI Agent Instructions

## Project Overview

This is a **dual-focus enterprise platform** consisting of:
1. **Platform Modules**: 14 modular .NET 8 NuGet packages implementing Clean Architecture, DDD, and CQRS
2. **UI Library**: Enterprise Blazor component library (Nalam360Enterprise.UI) targeting .NET 9 with 144 components (126 base + 18 AI-powered healthcare)

**Key Principle**: Use **Railway-Oriented Programming** with `Result<T>` instead of exceptions for business logic failures.

**Current Status**: ✅ Production Ready - 0 compilation errors, 100% requirements met, full CI/CD automation

## Architecture Patterns

### CQRS with Result Pattern
All commands and queries return `Result<T>` or `Result` from `Nalam360.Platform.Core.Results`:

```csharp
// Command Definition
public record CreateOrderCommand(Guid CustomerId, decimal Total) : ICommand<Result<Guid>>;

// Handler Implementation
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        // Use Result.Success() or Result.Failure() - never throw for business failures
        if (customer == null)
            return Result<Guid>.Failure(Error.NotFound("Customer", cmd.CustomerId));
        
        return Result<Guid>.Success(order.Id);
    }
}

// Usage
var result = await _mediator.Send(new CreateOrderCommand(customerId, 100m));
```

### Domain-Driven Design
- **Entities**: Inherit from `Entity<TId>` in `Nalam360.Platform.Domain.Primitives`
- **Aggregates**: Inherit from `AggregateRoot<TId>` - always use domain events via `AddDomainEvent()`
- **Value Objects**: Inherit from `ValueObject` - override `GetEqualityComponents()`
- **Domain Events**: Implement `IDomainEvent`, collected in aggregates and dispatched after persistence

### Module Organization
Platform modules follow strict layering (Core → Domain → Application → Data):
- **Core**: Time providers, GUID generators, Result types, Either monad
- **Domain**: DDD primitives, domain events
- **Application**: Mediator, commands/queries, pipeline behaviors (logging, validation)
- **Data**: Repository, UnitOfWork, Specification pattern, EF Core helpers

## Project Structure

```
src/
├── Nalam360.Platform.*/          # Platform modules (.NET 8)
│   └── DependencyInjection/      # Each has ServiceCollectionExtensions.cs
└── Nalam360Enterprise.UI/        # Blazor components (.NET 9)
    ├── Components/               # 39 Syncfusion-wrapped components
    ├── Core/Theming/            # ThemeService, ThemeConfiguration
    ├── Core/Security/           # PermissionService, AuditService
    └── Core/Forms/              # ValidationRules (Yup/Zod-like API)
```

## Development Workflows

### Building
```powershell
# Build entire solution (14 platform modules + UI library)
dotnet build Nalam360EnterprisePlatform.sln --configuration Release

# Run example API
dotnet run --project examples/Nalam360.Platform.Example.Api
```

### Testing
- Unit tests in `tests/Nalam360.Platform.Tests/`
- UI tests in `tests/Nalam360Enterprise.UI.Tests/` using bUnit

### Adding Platform Features
1. Place in appropriate layer (Core/Domain/Application/Data)
2. Return `Result<T>` for operations that can fail
3. Use `IMediator.Send()` for CQRS commands/queries
4. Register services via `ServiceCollectionExtensions.cs` in each module

### Creating UI Components
All components in `Nalam360Enterprise.UI/Components/` follow this pattern:
- **PREFER Syncfusion components** when available for professional, production-ready UI (e.g., `SfTextBox` → `N360TextBox`, `SfChat` → `N360Chat`)
- Use custom HTML/CSS only when Syncfusion doesn't provide the functionality
- Syncfusion provides enterprise-grade features: accessibility, RTL, themes, validation, keyboard navigation
- Include RBAC support: `RequiredPermission`, `HideIfNoPermission` parameters
- Include audit: `EnableAudit`, `AuditResource`, `AuditAction` parameters
- Use `ValidationRules` for schema-driven validation
- Call `GetHtmlAttributes()` for accessibility/RTL/theming

**Component Selection Priority:**
1. **First**: Check Syncfusion Blazor component catalog - use if available
2. **Second**: Wrap/enhance existing Syncfusion component with enterprise features
3. **Third**: Build custom component only if Syncfusion doesn't offer equivalent

Example component signature:
```csharp
@using Syncfusion.Blazor.Chat // Import Syncfusion namespace
@inherits N360ComponentBase

[Parameter] public string? RequiredPermission { get; set; }
[Parameter] public bool EnableAudit { get; set; }
[Parameter] public ValidationRules? ValidationRules { get; set; }
```

## Key Conventions

### Namespace Structure
- Platform modules: `Nalam360.Platform.{Module}` (.NET 8)
- UI library: `Nalam360Enterprise.UI.*` (.NET 9)
- Mix of two naming schemes for historical reasons

### Dependency Injection
Register services fluently:
```csharp
builder.Services.AddPlatformCore();
builder.Services.AddPlatformApplication(Assembly.GetExecutingAssembly());
builder.Services.AddPlatformData<YourDbContext>();
builder.Services.AddNalam360EnterpriseUI<CustomPermissionService>();
```

### Pipeline Behaviors
The Application layer auto-wires:
- `LoggingBehavior<,>`: Logs all commands/queries via ILogger
- `ValidationBehavior<,>`: Validates using FluentValidation, returns Result.Failure on errors

### Component Registration
UI components use Syncfusion services - already registered via `AddNalam360EnterpriseUI()` which calls `AddSyncfusionBlazor()`.

## Common Pitfalls

1. **Don't mix .NET versions**: Platform modules are .NET 8, UI is .NET 9
2. **Don't throw exceptions for business failures**: Use `Result.Failure(Error.*)` instead
3. **Don't forget CancellationToken**: All async methods accept `CancellationToken ct`
4. **Don't skip permission checks**: UI components require `PermissionService` injection
5. **Always dispatch domain events**: Call `ClearDomainEvents()` after persisting aggregates

## Essential Files & Documentation

### Primary Documentation (Start Here)
- `Documentation/README.md`: Master index for all organized documentation
- `Documentation/01-Getting-Started/README.md`: Project overview and quick start
- `Documentation/01-Getting-Started/QUICK_REFERENCE.md`: Common patterns and code examples
- `Documentation/01-Getting-Started/CONTRIBUTING.md`: Development setup and coding guidelines

### Architecture & Design
- `Documentation/02-Architecture/PLATFORM_GUIDE.md`: Complete 14 module inventory with build status
- `Documentation/02-Architecture/DESIGN_SYSTEM.md`: UI design system with 50+ design tokens
- `Documentation/02-Architecture/diagrams/`: Architecture, database, DevOps, sequence diagrams (10 files)

### Component Library
- `Documentation/03-Components/COMPONENT_INVENTORY.md`: All 144 UI components (126 base + 18 AI-powered)
- `Documentation/03-Components/COMPONENT_ANALYSIS.md`: Component architecture and patterns
- `Documentation/03-Components/FORM_SCHEMA_GUIDE.md`: Yup/Zod-like validation API

### Testing & Quality
- `Documentation/04-Testing/TESTING_GUIDE.md`: Comprehensive testing guide
- `Documentation/04-Testing/AI_TESTING_GUIDE.md`: AI services testing (consolidated guide)
- `Documentation/04-Testing/VISUAL_TESTING.md`: Playwright visual regression tests

### Deployment & Production
- `Documentation/05-Deployment/DEPLOYMENT_GUIDE.md`: Production deployment guide
- `Documentation/05-Deployment/PACKAGE_PUBLISHING_GUIDE.md`: NuGet publishing workflow
- `Documentation/05-Deployment/PRODUCTION_CHECKLIST.md`: Go-live checklist

### AI/ML Integration
- `Documentation/06-AI-ML/AI_INTEGRATION_GUIDE.md`: Azure OpenAI integration
- `Documentation/06-AI-ML/AI_SERVICES_USAGE_GUIDE.md`: Complete API reference (500+ lines)

### Compliance & Status
- `Documentation/08-Compliance/REQUIREMENTS_ANALYSIS.md`: Complete requirements compliance (1866 lines)
- `Documentation/09-Project-Status/PROJECT_STATUS.md`: Current project status (consolidated)
- `Documentation/09-Project-Status/BUILD_STATUS.md`: Latest build information
- `Documentation/09-Project-Status/CHANGELOG.md`: Version history

### Code Organization
- `src/*/DependencyInjection/ServiceCollectionExtensions.cs`: Module registration patterns for all 14 platform modules

### Interactive Documentation Site
- **URL:** http://localhost:5032 (dev) | GitHub Pages (production)
- **Location:** `docs/Nalam360Enterprise.Docs.Web/`
- **Features:** 5-tab component docs, live playground, modern professional design
