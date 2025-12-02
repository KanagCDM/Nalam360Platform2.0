# ProductAdminPortal Backend - Complete Implementation Guide

## 🎉 Overview

The **ProductAdminPortal** backend is now fully implemented with a complete REST API for domain-agnostic product configuration and subscription management. This system supports:

- ✅ Multi-tenant product catalog management
- ✅ Flexible subscription plans with tiered pricing
- ✅ Advanced pricing engine with complexity multipliers
- ✅ Usage tracking and limit enforcement
- ✅ Invoice generation and billing
- ✅ Complete audit trail with configuration versioning

---

## 📁 Project Structure

```
ProductAdminPortal/
├── Controllers/               # REST API endpoints
│   ├── ProductsController.cs      # Product CRUD + modules
│   ├── PricingController.cs       # Calculate + simulate pricing
│   ├── BillingController.cs       # Invoice generation
│   ├── UsageController.cs         # Usage tracking + alerts
│   └── AuditController.cs         # Audit logs + versioning
├── Services/                  # Business logic layer
│   ├── IServices.cs              # Service interfaces
│   ├── ProductService.cs         # Product management
│   ├── PricingService.cs         # Pricing calculations
│   ├── BillingService.cs         # Invoice generation
│   ├── UsageService.cs           # Usage tracking
│   └── AuditService.cs           # Audit logging
├── DTOs/                      # API contracts
│   ├── ProductDTOs.cs            # Product request/response
│   └── PricingDTOs.cs            # Pricing request/response
├── Models/Domain/             # Domain entities (24 models)
│   ├── ProductModels.cs          # Product, Module, Entity
│   ├── SubscriptionModels.cs     # Plans + junctions
│   ├── PricingModels.cs          # Rules, tiers, multipliers
│   ├── CustomerModels.cs         # Customers + usage
│   └── BillingModels.cs          # Invoices, audit, versions
├── Data/
│   ├── ApplicationDbContext.cs   # Legacy database (SQLite)
│   └── ProductAdminDbContext.cs  # New database (PostgreSQL)
└── Program.cs                 # Startup configuration
```

---

## 🚀 Getting Started

### Prerequisites

1. **.NET 10 SDK** - Download from https://dotnet.microsoft.com/download
2. **PostgreSQL 15+** - For ProductAdmin database
3. **Redis 7+** (optional) - For caching
4. **Visual Studio 2022** or **VS Code** with C# extension

### Installation

```bash
# 1. Navigate to project directory
cd MedWayHealthCare2.0/src/Presentation/ProductAdminPortal

# 2. Restore NuGet packages
dotnet restore

# 3. Start PostgreSQL (if using Docker)
docker run -d \
  --name postgres-productadmin \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=productadmin \
  -p 5432:5432 \
  postgres:15

# 4. Update connection string in appsettings.json (if needed)
# Default: "Host=localhost;Port=5432;Database=productadmin;Username=postgres;Password=postgres"

# 5. Run the application
dotnet run

# Expected output:
# ✅ Legacy database initialized successfully
# ✅ ProductAdmin database initialized successfully
# 🚀 ProductAdminPortal is running!
# 📖 Blazor UI: https://localhost:5096
# 📖 API Docs: https://localhost:5096/api-docs
```

### Access Points

- **Blazor UI**: https://localhost:5096
- **Swagger API Docs**: https://localhost:5096/api-docs
- **Base API URL**: https://localhost:5096/api/v1

---

## 📚 API Endpoints Reference

### 🛍️ Products API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/products` | List all products for tenant |
| `POST` | `/api/v1/products` | Create new product |
| `GET` | `/api/v1/products/{id}` | Get product by ID |
| `PUT` | `/api/v1/products/{id}` | Update product |
| `DELETE` | `/api/v1/products/{id}` | Delete product (soft) |
| `GET` | `/api/v1/products/{id}/modules` | Get product modules |
| `POST` | `/api/v1/products/{id}/modules` | Add module to product |

**Example: Create Product**
```bash
curl -X POST https://localhost:5096/api/v1/products \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -H "X-User-Id: 22222222-2222-2222-2222-222222222222" \
  -d '{
    "name": "Enterprise CRM",
    "code": "ECRM",
    "description": "Customer relationship management system",
    "industry": "SaaS",
    "version": "1.0.0"
  }'
```

**Response:**
```json
{
  "id": "33333333-3333-3333-3333-333333333333",
  "name": "Enterprise CRM",
  "code": "ECRM",
  "description": "Customer relationship management system",
  "industry": "SaaS",
  "version": "1.0.0",
  "isActive": true,
  "createdAt": "2025-06-15T10:30:00Z",
  "modules": []
}
```

---

### 💰 Pricing API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/v1/pricing/calculate` | Calculate actual pricing for subscription |
| `POST` | `/api/v1/pricing/simulate` | Simulate pricing for hypothetical usage |

**Example: Calculate Pricing**
```bash
curl -X POST https://localhost:5096/api/v1/pricing/calculate \
  -H "Content-Type: application/json" \
  -d '{
    "customerSubscriptionId": "44444444-4444-4444-4444-444444444444",
    "billingPeriodStart": "2025-06-01T00:00:00Z",
    "billingPeriodEnd": "2025-06-30T23:59:59Z",
    "usageRecords": [
      {
        "entityId": "55555555-5555-5555-5555-555555555555",
        "units": 1250,
        "complexity": "medium"
      }
    ]
  }'
```

**Response:**
```json
{
  "customerSubscriptionId": "44444444-4444-4444-4444-444444444444",
  "billingPeriodStart": "2025-06-01T00:00:00Z",
  "billingPeriodEnd": "2025-06-30T23:59:59Z",
  "subtotal": 2659.49,
  "taxAmount": 478.71,
  "discountAmount": 0.00,
  "totalAmount": 3138.20,
  "lineItems": [
    {
      "description": "Enterprise Plan - Base Fee",
      "quantity": 1,
      "unitPrice": 999.00,
      "amount": 999.00,
      "metadata": { "type": "base_fee" }
    },
    {
      "description": "Transaction (medium complexity)",
      "quantity": 1250,
      "unitPrice": 1.33,
      "amount": 1660.49,
      "metadata": {
        "entityId": "55555555-5555-5555-5555-555555555555",
        "complexity": "medium",
        "multiplier": 1.5
      }
    }
  ]
}
```

---

### 🧾 Billing API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/v1/billing/invoices` | Generate invoice |
| `GET` | `/api/v1/billing/invoices/{id}` | Get invoice by ID |
| `GET` | `/api/v1/billing/customers/{customerId}/invoices` | Get customer invoices |
| `POST` | `/api/v1/billing/invoices/{id}/mark-paid` | Mark invoice as paid |

**Example: Generate Invoice**
```bash
curl -X POST https://localhost:5096/api/v1/billing/invoices \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -H "X-User-Id: 22222222-2222-2222-2222-222222222222" \
  -d '{
    "customerSubscriptionId": "44444444-4444-4444-4444-444444444444",
    "billingPeriodStart": "2025-06-01T00:00:00Z",
    "billingPeriodEnd": "2025-06-30T23:59:59Z"
  }'
```

**Response:** Returns invoice ID (GUID)

---

### 📊 Usage API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/v1/usage/record` | Record usage event |
| `GET` | `/api/v1/usage/subscriptions/{id}/records` | Get usage records |
| `GET` | `/api/v1/usage/subscriptions/{id}/summary` | Get usage summary |
| `GET` | `/api/v1/usage/subscriptions/{id}/alerts` | Get active alerts |
| `POST` | `/api/v1/usage/alerts/{id}/resolve` | Resolve usage alert |

**Example: Record Usage**
```bash
curl -X POST https://localhost:5096/api/v1/usage/record \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -H "X-User-Id: 22222222-2222-2222-2222-222222222222" \
  -d '{
    "customerSubscriptionId": "44444444-4444-4444-4444-444444444444",
    "entityId": "55555555-5555-5555-5555-555555555555",
    "units": 50,
    "complexity": "high",
    "metadata": {
      "transaction_type": "api_call",
      "endpoint": "/api/v1/customers"
    }
  }'
```

---

### 🔍 Audit API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/audit/logs` | Get audit logs (filterable) |
| `GET` | `/api/v1/audit/versions` | Get configuration versions |
| `GET` | `/api/v1/audit/versions/{id}` | Get specific version |
| `POST` | `/api/v1/audit/versions` | Create configuration snapshot |
| `POST` | `/api/v1/audit/versions/{id}/rollback` | Rollback to version |

**Example: Get Audit Logs**
```bash
curl "https://localhost:5096/api/v1/audit/logs?entityType=Product&startDate=2025-06-01&pageSize=20" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111"
```

---

## 🔧 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=MedWayDb.db",
    "ProductAdminConnection": "Host=localhost;Port=5432;Database=productadmin;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### Environment Variables (Production)

```bash
# Database
export ConnectionStrings__ProductAdminConnection="Host=prod-db.example.com;Port=5432;Database=productadmin;Username=app_user;Password=secure_password"

# Redis
export ConnectionStrings__Redis="prod-redis.example.com:6379,password=redis_password"

# ASP.NET
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://+:80;https://+:443
```

---

## 🏗️ Architecture

### Service Layer Pattern

Each service follows the same pattern:

```csharp
public interface IProductService
{
    Task<ProductResponse> GetProductByIdAsync(Guid id, Guid tenantId, CancellationToken ct);
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request, Guid tenantId, Guid userId, CancellationToken ct);
    // ...
}

public class ProductService : IProductService
{
    private readonly ProductAdminDbContext _context;
    private readonly IAuditService _auditService;

    public ProductService(ProductAdminDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }
    
    // Implementation with:
    // - Multi-tenant filtering
    // - Audit logging
    // - DTO mapping
    // - Exception handling
}
```

### Multi-Tenancy

All operations are tenant-scoped via `X-Tenant-Id` header:

```csharp
var products = await _context.Products
    .Where(p => p.TenantId == tenantId && p.IsActive)
    .ToListAsync(cancellationToken);
```

### Audit Trail

All mutations are automatically logged:

```csharp
await _auditService.LogActionAsync(
    "create",
    "Product",
    product.Id,
    null, // IP address (can be captured from HttpContext)
    new Dictionary<string, object>
    {
        { "product_code", request.Code },
        { "product_name", request.Name }
    },
    tenantId,
    userId,
    cancellationToken);
```

---

## 🧪 Testing

### Manual API Testing

Use Swagger UI at https://localhost:5096/api-docs to test all endpoints interactively.

### Sample Test Scenario

```bash
# 1. Create a product
PRODUCT_ID=$(curl -X POST https://localhost:5096/api/v1/products \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: $TENANT_ID" \
  -H "X-User-Id: $USER_ID" \
  -d '{"name":"Test CRM","code":"TCRM","industry":"SaaS","version":"1.0"}' \
  | jq -r '.id')

# 2. Add a module
curl -X POST https://localhost:5096/api/v1/products/$PRODUCT_ID/modules \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: $TENANT_ID" \
  -H "X-User-Id: $USER_ID" \
  -d '{"name":"Customer Management","code":"CUST","isRequired":true}'

# 3. Get audit logs
curl "https://localhost:5096/api/v1/audit/logs?entityType=Product&entityId=$PRODUCT_ID" \
  -H "X-Tenant-Id: $TENANT_ID"
```

---

## 📖 Next Steps

### Phase 1: Backend Testing ✅ READY
- Write unit tests for services
- Integration tests for API endpoints
- Load testing for pricing engine

### Phase 2: Authentication & Authorization
- Implement JWT authentication
- Add RBAC middleware
- Tenant isolation enforcement

### Phase 3: Frontend Development
- Product Designer UI (React/Blazor)
- Pricing Simulation Dashboard
- Customer Portal

### Phase 4: Production Deployment
- Use Docker Compose or Kubernetes
- Configure production PostgreSQL
- Setup Redis cluster
- Deploy CI/CD pipeline

---

## 🐛 Troubleshooting

### Database Connection Issues

**Error:** `Npgsql.PostgresException: Connection refused`

**Solution:**
```bash
# Check if PostgreSQL is running
docker ps | grep postgres

# Start PostgreSQL
docker start postgres-productadmin

# Verify connection
psql -h localhost -U postgres -d productadmin
```

### Missing NuGet Packages

**Error:** `The type or namespace 'Npgsql' could not be found`

**Solution:**
```bash
dotnet restore
dotnet build
```

### API Returns 404

**Issue:** Controllers not registered

**Solution:** Ensure `Program.cs` has:
```csharp
builder.Services.AddControllers();
app.MapControllers();
```

---

## 📝 License

This project is part of the MedWayHealthCare platform and follows the same licensing terms.

---

## 👥 Contributors

- Backend Architecture: AI Assistant
- Domain Model Design: AI Assistant
- Pricing Engine: AI Assistant
- API Implementation: AI Assistant

---

## 📞 Support

For issues or questions:
1. Check API documentation at `/api-docs`
2. Review audit logs for debugging
3. Enable EF Core SQL logging in appsettings.json:
   ```json
   "Microsoft.EntityFrameworkCore.Database.Command": "Information"
   ```
