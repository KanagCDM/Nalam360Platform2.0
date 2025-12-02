# ProductAdminPortal

> Domain-Agnostic Product Configuration & Subscription Management System

A comprehensive, production-ready platform for designing products, configuring pricing models, managing subscriptions, and calculating usage-based billing with complex pricing rules.

## 🚀 Features

### Core Capabilities
- **Product Designer** - Create/edit/delete products with modular architecture
- **Module Designer** - Define modules within products with entity mappings
- **Entity Designer** - Granular feature/resource management with pricing controls
- **Subscription Management** - Flexible subscription plans (Trial, Standard, Gold, Platinum, etc.)
- **Advanced Pricing Engine**
  - Tiered volume pricing
  - Per-seat / per-user pricing
  - Per-transaction pricing with complexity multipliers (Low/Medium/High/Critical)
  - One-time setup fees
  - Recurring fees (monthly/yearly/quarterly)
  - Discount codes and promotional pricing
- **Usage Tracking & Quotas** - Real-time usage monitoring with soft/hard limits
- **Billing Calculation** - Deterministic invoice generation with line-item breakdowns
- **Simulation & Preview** - Test pricing scenarios before committing
- **Audit Trails** - Complete change history with versioning and rollback
- **RBAC** - Role-based access control for product designers, pricing managers, auditors
- **Multi-Tenant** - Full tenant isolation with shared or dedicated database strategies
- **Import/Export** - CSV/JSON configuration exchange

---

## 📋 Prerequisites

### Development Environment
- **.NET SDK 10.0** or higher
- **Node.js 20+** and **npm/yarn** (for frontend)
- **PostgreSQL 15+** (primary database)
- **Redis 7+** (optional, for caching)
- **Docker & Docker Compose** (recommended)

### Production Requirements
- **Kubernetes cluster** (v1.28+) or **Docker Swarm**
- **PostgreSQL** with read replicas
- **Redis cluster** for distributed caching
- **Object storage** (AWS S3/Azure Blob/GCS) for exports
- **Monitoring** (Prometheus/Grafana or Application Insights)

---

## 🛠️ Quick Start

### Option 1: Docker Compose (Recommended for Development)

```bash
# Clone repository
git clone <repository-url>
cd ProductAdminPortal

# Copy environment template
cp .env.template .env

# Edit .env and configure:
# - POSTGRES_PASSWORD (required)
# - JWT_SECRET (required - min 32 characters)
# - Other settings as needed

# Start all services (backend, frontend, postgres, redis)
docker-compose up -d

# View logs
docker-compose logs -f backend

# Start with development tools (pgAdmin, Redis Commander)
docker-compose --profile tools up -d

# Access services:
# - Frontend: http://localhost:3000
# - Backend API: http://localhost:5000
# - API Swagger: http://localhost:5000/swagger
# - pgAdmin: http://localhost:5050 (admin@productadmin.com / admin123)
# - Redis Commander: http://localhost:8081
# - PostgreSQL: localhost:5432
# - Redis: localhost:6379

# Stop services
docker-compose down

# Remove all data (WARNING: destructive)
docker-compose down -v
```

### Option 2: Manual Setup

#### 1. Database Setup

```bash
# Create PostgreSQL database
createdb productadmin

# Run migrations
cd ProductAdminPortal
psql -d productadmin -f Database/schema.sql

# Seed data
psql -d productadmin -f Database/seed.sql
```

#### 2. Backend Setup (.NET)

```bash
cd ProductAdminPortal

# Restore packages
dotnet restore

# Update connection string in appsettings.json
# ConnectionStrings:DefaultConnection = "Host=localhost;Database=productadmin;Username=postgres;Password=yourpassword"

# Run database migrations
dotnet ef database update

# Start backend
dotnet run

# Backend will be available at http://localhost:5000
```

#### 3. Frontend Setup (React)

```bash
cd frontend

# Install dependencies
npm install

# Configure API endpoint
# Create .env.local file:
echo "REACT_APP_API_URL=http://localhost:5000/api/v1" > .env.local

# Start development server
npm start

# Frontend will be available at http://localhost:3000
```

---

## 🧪 Running Tests

### Backend Tests (.NET)

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/ProductAdminPortal.Tests

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReporter=html

# View coverage report
open coverage/index.html
```

### Frontend Tests (React)

```bash
cd frontend

# Run unit tests
npm test

# Run E2E tests (Playwright)
npm run test:e2e

# Run visual regression tests
npm run test:visual
```

### Integration Tests

```bash
# Start test environment
docker-compose -f docker-compose.test.yml up -d

# Run integration tests
npm run test:integration

# Cleanup
docker-compose -f docker-compose.test.yml down -v
```

---

## 📚 Documentation

### Architecture & Design
- [Database Schema](./Database/schema.sql) - Complete PostgreSQL DDL
- [JSON Schemas](./Documentation/JSON_SCHEMAS.md) - All entity schemas with examples
- [API Specification](./Documentation/API_SPECIFICATION.yaml) - OpenAPI 3.0 spec
- [Pricing Calculation](./Documentation/PRICING_CALCULATION_COMPLETE.md) - Fully worked examples

### Seed Data
- [Products Seed](./SeedData/products_seed.json) - 2 products, 6 modules, 10 entities
- [Subscriptions Seed](./SeedData/subscriptions_seed.json) - 4 plans, pricing rules, discounts
- [Usage Seed](./SeedData/usage_seed.json) - 10 usage records for simulation

### API Examples

#### Create a Product
```bash
curl -X POST http://localhost:5000/api/v1/products \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Hospital Management System",
    "key": "HMS_CORE",
    "description": "Complete hospital management platform",
    "metadata": {
      "industry": "healthcare"
    }
  }'
```

#### Calculate Pricing
```bash
curl -X POST http://localhost:5000/api/v1/pricing/calculate \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "tenant_id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "customer_subscription_id": "cust-sub-001",
    "billing_period_start": "2025-11-01",
    "billing_period_end": "2025-11-30",
    "usage_records": [
      {
        "entity_id": "entity-pat-001",
        "units": 8000,
        "complexity": "low"
      },
      {
        "entity_id": "entity-pat-001",
        "units": 3000,
        "complexity": "medium"
      },
      {
        "entity_id": "entity-pat-001",
        "units": 1500,
        "complexity": "high"
      }
    ],
    "include_details": true
  }'
```

**Response:**
```json
{
  "subscription_plan": {
    "id": "plan-gold-001",
    "name": "Gold Plan",
    "base_fee": 1000.00
  },
  "line_items": [...],
  "subtotal": 2660.00,
  "tax_amount": 478.80,
  "discount_amount": 0.00,
  "total_amount": 3138.80,
  "currency_code": "INR"
}
```

#### Simulate Subscription
```bash
curl -X POST http://localhost:5000/api/v1/simulate \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "subscription_plan_id": "plan-gold-001",
    "projected_usage": [
      {
        "entity_id": "entity-pat-001",
        "units": 12500,
        "complexity_breakdown": {
          "low": 8000,
          "medium": 3000,
          "high": 1500
        }
      }
    ],
    "billing_cycle": "monthly"
  }'
```

---

## 🏗️ Project Structure

```
ProductAdminPortal/
├── Database/
│   ├── schema.sql                 # PostgreSQL DDL
│   ├── seed.sql                   # Initial data
│   └── migrations/                # EF Core migrations
├── Documentation/
│   ├── JSON_SCHEMAS.md            # Complete schemas
│   ├── API_SPECIFICATION.yaml     # OpenAPI 3.0 spec
│   ├── PRICING_CALCULATION_COMPLETE.md
│   └── ARCHITECTURE.md
├── SeedData/
│   ├── products_seed.json
│   ├── subscriptions_seed.json
│   └── usage_seed.json
├── Models/
│   ├── Product.cs
│   ├── Module.cs
│   ├── Entity.cs
│   ├── SubscriptionPlan.cs
│   ├── PricingRule.cs
│   ├── UsageRecord.cs
│   ├── Invoice.cs
│   └── Pricing/                   # Pricing engine models
├── Services/
│   ├── ProductService.cs
│   ├── PricingEngine.cs           # Core pricing calculation
│   ├── BillingService.cs
│   ├── UsageService.cs
│   ├── AuditService.cs
│   └── SimulationService.cs
├── Controllers/
│   ├── ProductsController.cs
│   ├── ModulesController.cs
│   ├── EntitiesController.cs
│   ├── SubscriptionsController.cs
│   ├── PricingController.cs
│   ├── SimulationController.cs
│   └── AuditController.cs
├── frontend/                      # React TypeScript frontend
│   ├── src/
│   │   ├── components/
│   │   │   ├── ProductDesigner/
│   │   │   ├── ModuleEditor/
│   │   │   ├── EntityEditor/
│   │   │   ├── SubscriptionEditor/
│   │   │   ├── PricingMatrix/
│   │   │   └── SimulationPanel/
│   │   ├── services/
│   │   └── hooks/
│   ├── package.json
│   └── tsconfig.json
├── tests/
│   ├── ProductAdminPortal.Tests/  # Unit tests
│   ├── Integration.Tests/         # Integration tests
│   └── E2E.Tests/                 # End-to-end tests
├── docker-compose.yml
├── Dockerfile
├── ProductAdminPortal.csproj
└── README.md
```

---

## 🎯 Pricing Calculation Example

### Scenario
- **Plan:** Gold Plan (₹1,000/month base fee)
- **Entity:** Patient Registration
- **Usage:** 12,500 transactions
  - Low complexity: 8,000
  - Medium complexity: 3,000
  - High complexity: 1,500

### Tiered Pricing
```
Tier 0 (0-1,000):     ₹0.00/txn (included)
Tier 1 (1,001-10,000): ₹0.10/txn
Tier 2 (>10,000):     ₹0.07/txn
```

### Complexity Multipliers
```
Low:    1.0x
Medium: 2.0x
High:   4.0x
```

### Calculation
```
Base Fee:                          ₹1,000.00
Low (Tier 0): 1,000 × ₹0.00 =         ₹0.00
Low (Tier 1): 7,000 × ₹0.10 =       ₹700.00
Medium (Tier 1): 2,000 × ₹0.20 =    ₹400.00
Medium (Tier 2): 1,000 × ₹0.14 =    ₹140.00
High (Tier 2): 1,500 × ₹0.28 =      ₹420.00
                                 ───────────
Subtotal:                          ₹2,660.00
Tax (18% GST):                       ₹478.80
                                 ───────────
TOTAL:                             ₹3,138.80
```

See [PRICING_CALCULATION_COMPLETE.md](./Documentation/PRICING_CALCULATION_COMPLETE.md) for full implementation with unit tests.

---

## 🔐 Security & Access Control

### Authentication
- **JWT Bearer Tokens** - Industry-standard authentication
- **OAuth 2.0** - Social login support (Google, Microsoft)
- **Multi-Factor Authentication** - Optional MFA for sensitive operations

### RBAC Roles
- **Super Admin** - Full system access, tenant management
- **Product Designer** - Create/edit products, modules, entities
- **Pricing Manager** - Manage pricing rules and subscription plans
- **Subscription Manager** - Manage customer subscriptions and usage
- **Billing Admin** - Generate invoices, process payments
- **Auditor** - Read-only access to audit logs and reports
- **Customer User** - Limited access to own subscription and usage

### Data Protection
- **Encryption at Rest** - Database and file storage encryption
- **Encryption in Transit** - TLS 1.3 for all API calls
- **PII Handling** - GDPR-compliant data retention policies
- **Audit Logging** - All changes tracked with user, timestamp, IP

---

## 📊 Monitoring & Observability

### Metrics
- **Request Rate** - API calls per second
- **Response Time** - P50, P95, P99 latencies
- **Error Rate** - 4xx and 5xx errors
- **Pricing Calculations** - Calculation time, complexity

### Logging
- **Structured Logging** - JSON format with correlation IDs
- **Log Levels** - DEBUG, INFO, WARN, ERROR, FATAL
- **Retention** - 30 days hot, 1 year cold storage

### Alerts
- **High Error Rate** - >5% errors in 5 minutes
- **Slow Queries** - Database queries >2 seconds
- **Usage Limits** - Customers approaching quotas
- **Failed Billing** - Invoice generation failures

---

## 🚢 Deployment

### Docker Compose (Development)

```bash
docker-compose up -d
```

### Kubernetes (Production)

```bash
# Create namespace
kubectl create namespace productadmin

# Apply configurations
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/postgres.yaml
kubectl apply -f k8s/redis.yaml
kubectl apply -f k8s/backend.yaml
kubectl apply -f k8s/frontend.yaml
kubectl apply -f k8s/ingress.yaml

# Check status
kubectl get pods -n productadmin
kubectl get services -n productadmin
```

### CI/CD Pipeline (GitHub Actions)

```yaml
# .github/workflows/deploy.yml
name: Deploy
on:
  push:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run tests
        run: dotnet test
  
  build-and-push:
    needs: test
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build Docker image
        run: docker build -t productadmin:${{ github.sha }} .
      - name: Push to registry
        run: docker push productadmin:${{ github.sha }}
  
  deploy:
    needs: build-and-push
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to Kubernetes
        run: kubectl set image deployment/productadmin productadmin=productadmin:${{ github.sha }}
```

---

## 🧩 Extensibility

### Adding New Pricing Models

```csharp
// Services/PricingStrategies/BundlePricingStrategy.cs
public class BundlePricingStrategy : IPricingStrategy
{
    public decimal Calculate(PricingRule rule, int units)
    {
        var bundleSize = rule.Params["bundle_size"];
        var bundlePrice = rule.Params["bundle_price"];
        
        var bundles = (int)Math.Ceiling((decimal)units / bundleSize);
        return bundles * bundlePrice;
    }
}

// Register in Startup.cs
services.AddScoped<IPricingStrategy, BundlePricingStrategy>();
```

### Custom Entity Types

```typescript
// frontend/src/types/entities.ts
export type CustomEntityType = 
  | 'resource'
  | 'action'
  | 'report'
  | 'feature'
  | 'integration'
  | 'custom_workflow'  // New type
  | 'custom_analytics'; // New type
```

---

## 🚀 Deployment

### Production Deployment to Kubernetes

#### Prerequisites
- Kubernetes cluster (1.24+)
- kubectl configured
- Container registry access
- SSL certificate (or cert-manager for Let's Encrypt)

#### Step 1: Build and Push Docker Images

```bash
# Build backend image
docker build -t ghcr.io/yourorg/productadmin:latest -f Dockerfile .

# Build frontend image (when ready)
docker build -t ghcr.io/yourorg/productadmin-frontend:latest -f frontend/Dockerfile ./frontend

# Push to registry
docker push ghcr.io/yourorg/productadmin:latest
docker push ghcr.io/yourorg/productadmin-frontend:latest
```

#### Step 2: Configure Secrets

```bash
# Update k8s/production/deployment.yaml with your secrets:
# - postgres-password
# - jwt-secret (min 32 characters)
# - redis-password

# Apply secrets
kubectl apply -f k8s/production/deployment.yaml
```

#### Step 3: Deploy to Kubernetes

```bash
# Create namespace
kubectl create namespace productadmin-production

# Apply all manifests
kubectl apply -f k8s/production/

# Verify deployment
kubectl get pods -n productadmin-production
kubectl get services -n productadmin-production
kubectl get ingress -n productadmin-production

# Check logs
kubectl logs -f deployment/productadmin-backend -n productadmin-production
```

#### Step 4: Configure DNS

```bash
# Get ingress external IP
kubectl get ingress productadmin-ingress -n productadmin-production

# Point your DNS to the external IP:
# - productadmin.com → INGRESS_IP
# - api.productadmin.com → INGRESS_IP
```

#### Step 5: Verify Deployment

```bash
# Health check
curl https://api.productadmin.com/health

# API endpoint
curl https://api.productadmin.com/api/v1/products

# Frontend
curl https://productadmin.com
```

### Continuous Deployment (GitHub Actions)

The CI/CD pipeline automatically:
1. Runs tests on every PR and push
2. Builds Docker images on main/develop branches
3. Deploys to staging on develop branch
4. Deploys to production on release

**Required GitHub Secrets:**
- `KUBE_CONFIG_STAGING` - Base64-encoded kubeconfig for staging
- `KUBE_CONFIG_PRODUCTION` - Base64-encoded kubeconfig for production
- `SLACK_WEBHOOK` - Slack webhook URL for notifications (optional)

**To create kubeconfig secret:**
```bash
# Encode kubeconfig
cat ~/.kube/config | base64 | pbcopy

# Add to GitHub Repository Settings > Secrets > Actions
# Name: KUBE_CONFIG_PRODUCTION
# Value: (paste base64-encoded config)
```

### Environment-Specific Configuration

#### Staging
- Namespace: `productadmin-staging`
- URL: https://staging.productadmin.com
- Replicas: 2 backend, 1 frontend
- Auto-deploy on `develop` branch

#### Production
- Namespace: `productadmin-production`
- URL: https://productadmin.com
- Replicas: 3 backend, 2 frontend (auto-scaled)
- Manual approval required for deployment
- Auto-deploy on release tags

### Monitoring & Observability

```bash
# View pod status
kubectl get pods -n productadmin-production -w

# View logs (all pods)
kubectl logs -f -l app=productadmin-backend -n productadmin-production

# View HPA status (auto-scaling)
kubectl get hpa -n productadmin-production

# View resource usage
kubectl top pods -n productadmin-production
kubectl top nodes
```

### Database Backup & Recovery

```bash
# Manual backup
kubectl exec -n productadmin-production deployment/postgres -- \
  pg_dump -U postgres productadmin > backup-$(date +%Y%m%d-%H%M%S).sql

# Restore from backup
kubectl exec -i -n productadmin-production deployment/postgres -- \
  psql -U postgres productadmin < backup-20250128-120000.sql

# Automated backups (configure CronJob in k8s/production/)
# See k8s/production/backup-cronjob.yaml
```

### Scaling

```bash
# Manual scaling
kubectl scale deployment productadmin-backend --replicas=5 -n productadmin-production

# Update HPA limits
kubectl edit hpa productadmin-backend-hpa -n productadmin-production

# Scale database (requires StatefulSet conversion)
# See k8s/production/database-statefulset.yaml
```

### Rollback

```bash
# View deployment history
kubectl rollout history deployment/productadmin-backend -n productadmin-production

# Rollback to previous version
kubectl rollout undo deployment/productadmin-backend -n productadmin-production

# Rollback to specific revision
kubectl rollout undo deployment/productadmin-backend --to-revision=3 -n productadmin-production
```

### Security Hardening

1. **Update Secrets:** Change default passwords in production
2. **Enable HTTPS:** Ensure cert-manager is configured for SSL
3. **Network Policies:** Restrict pod-to-pod communication
4. **RBAC:** Use service accounts with minimal permissions
5. **Image Scanning:** Use Trivy or similar for vulnerability scanning
6. **Secret Management:** Consider using Sealed Secrets or external vault

---

## 🤝 Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md) for development guidelines.

### Development Sprint Roadmap

#### Sprint 1-2: Core CRUD + RBAC (2 weeks)
- Product/Module/Entity CRUD operations
- User authentication and authorization
- Role-based access control
- Basic audit logging

#### Sprint 3-4: Subscription & Basic Pricing (2 weeks)
- Subscription plan management
- Simple per-unit pricing
- Flat fee support
- Module-entity mapping

#### Sprint 5-6: Advanced Pricing Engine (2 weeks)
- Tiered pricing implementation
- Complexity multipliers
- Usage ingestion
- Billing calculation API

#### Sprint 7-8: UI Polish & Simulation (2 weeks)
- Product designer UI
- Pricing matrix editor
- Simulation panel
- Real-time preview

#### Sprint 9-10: Audit, Versioning, Deployment (2 weeks)
- Configuration versioning
- Rollback functionality
- Import/export features
- Production deployment

---

## 📄 License

MIT License - See [LICENSE](./LICENSE) for details.

---

## 🆘 Support

- **Documentation:** https://docs.productadminportal.com
- **API Reference:** https://api.productadminportal.com/docs
- **Issues:** https://github.com/yourorg/productadminportal/issues
- **Email:** support@productadminportal.com
- **Slack:** #productadminportal

---

## 🎉 Acknowledgments

Built with:
- **.NET 10** - Backend framework
- **React 18** - Frontend framework
- **PostgreSQL 15** - Primary database
- **Redis 7** - Caching layer
- **Tailwind CSS** - UI styling
- **shadcn/ui** - Component library
- **Playwright** - E2E testing
- **xUnit** - Unit testing

---

**ProductAdminPortal** - Empowering SaaS businesses with flexible product configuration and intelligent pricing.
