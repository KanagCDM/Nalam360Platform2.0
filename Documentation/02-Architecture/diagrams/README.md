# Nalam360 Enterprise Platform - Diagram Index

> **Comprehensive Visual Documentation Suite**  
> 63 detailed diagrams covering architecture, flows, data models, and patterns

---

## 📊 Document Overview

| Category | File | Diagrams | Purpose |
|----------|------|----------|---------|
| **Sequence** | [SEQUENCE_DIAGRAMS.md](SEQUENCE_DIAGRAMS.md) | 15 | Detailed interaction flows for CQRS, messaging, security |
| **Architecture** | [ARCHITECTURE_DIAGRAMS.md](ARCHITECTURE_DIAGRAMS.md) | 8 | System structure, layers, deployment topology |
| **Database** | [DATABASE_DIAGRAMS.md](DATABASE_DIAGRAMS.md) | 7 | ERDs for domain models, multi-tenancy, RBAC, audit |
| **State Machines** | [STATE_DIAGRAMS.md](STATE_DIAGRAMS.md) | 7 | Business process lifecycles and state transitions |
| **Integration** | [INTEGRATION_PATTERNS.md](INTEGRATION_PATTERNS.md) | 8 | API patterns, event-driven integration, resilience |
| **UI Components** | [UI_COMPONENT_FLOWS.md](UI_COMPONENT_FLOWS.md) | 10 | Blazor component interactions and validation flows ✅ |
| **DevOps** | [DEVOPS_DIAGRAMS.md](DEVOPS_DIAGRAMS.md) | 8 | CI/CD pipelines, monitoring, deployment strategies ✅ |

**Total Diagrams:** 63 ✅ **COMPLETE**  
**Format:** Mermaid (GitHub-compatible)  
**Last Updated:** November 18, 2025

---

## 🎯 Quick Navigation

### For Developers
- **Start Here**: [Architecture Diagrams](ARCHITECTURE_DIAGRAMS.md) - System overview and clean architecture
- **CQRS Implementation**: [Sequence Diagrams #1-2](SEQUENCE_DIAGRAMS.md#1-cqrs-command-flow) - Command/query flows
- **Data Access**: [Sequence Diagrams #4-5](SEQUENCE_DIAGRAMS.md#4-repository-pattern) - Repository and Unit of Work
- **Database Schema**: [Database Diagrams](DATABASE_DIAGRAMS.md) - All ERDs

### For Architects
- **System Design**: [Architecture Diagrams #1-3](ARCHITECTURE_DIAGRAMS.md#1-system-context-diagram) - Context, layers, modules
- **Event-Driven**: [Architecture Diagrams #5](ARCHITECTURE_DIAGRAMS.md#5-event-driven-architecture) + [Sequence #6-7](SEQUENCE_DIAGRAMS.md#6-messaging-with-outbox-pattern)
- **Microservices**: [Architecture Diagrams #6](ARCHITECTURE_DIAGRAMS.md#6-microservices-topology)
- **Deployment**: [Architecture Diagrams #8](ARCHITECTURE_DIAGRAMS.md#8-deployment-architecture)

### For Product Managers
- **Business Flows**: [State Diagrams #1-3](STATE_DIAGRAMS.md#1-order-lifecycle) - Order, payment, user registration
- **Feature Rollout**: [State Diagrams #4](STATE_DIAGRAMS.md#4-feature-flag-lifecycle)
- **UI Interactions**: [UI Component Flows](UI_COMPONENT_FLOWS.md)

### For DevOps Engineers
- **CI/CD**: [DevOps Diagrams #1-3](DEVOPS_DIAGRAMS.md) - Build, test, deployment pipelines
- **Monitoring**: [DevOps Diagrams #4-5](DEVOPS_DIAGRAMS.md) - Observability and alerting
- **Deployment**: [Architecture Diagrams #8](ARCHITECTURE_DIAGRAMS.md#8-deployment-architecture) + [DevOps #7](DEVOPS_DIAGRAMS.md)

### For Security Teams
- **RBAC**: [Sequence Diagrams #10](SEQUENCE_DIAGRAMS.md#10-rbac-authorization-flow) + [Database Diagrams #3](DATABASE_DIAGRAMS.md#3-rbac-schema)
- **Audit**: [Database Diagrams #4](DATABASE_DIAGRAMS.md#4-audit-log-schema)
- **Multi-Tenancy**: [Sequence Diagrams #15](SEQUENCE_DIAGRAMS.md#15-multi-tenancy-resolution) + [Database #2](DATABASE_DIAGRAMS.md#2-multi-tenancy-schema)

---

## 📁 Detailed Contents

### 1. Sequence Diagrams (15)

**[SEQUENCE_DIAGRAMS.md](SEQUENCE_DIAGRAMS.md)** - Interaction flows showing how components communicate

| # | Diagram | Use Case |
|---|---------|----------|
| 1 | CQRS Command Flow | Creating/updating entities via mediator pipeline |
| 2 | CQRS Query Flow | Querying data with caching and specifications |
| 3 | Domain Event Dispatch | Publishing events from aggregates |
| 4 | Repository Pattern | Generic data access with specifications |
| 5 | Unit of Work Pattern | Transactional coordination and event dispatch |
| 6 | Messaging with Outbox | Reliable event publishing (transactional outbox) |
| 7 | RabbitMQ Publishing | Event bus with retry and confirmations |
| 8 | Circuit Breaker | Resilience pattern for external calls |
| 9 | Feature Flag Evaluation | Dynamic feature toggling with rollout |
| 10 | RBAC Authorization | Role-based access control with permissions |
| 11 | Database Migration | Schema versioning with rollback |
| 12 | Validation Pipeline | FluentValidation and attribute validation |
| 13 | Cloud Storage Upload | Azure Blob upload with SAS URLs |
| 14 | Distributed Tracing | OpenTelemetry trace propagation |
| 15 | Multi-Tenancy Resolution | Tenant identification and data isolation |

---

### 2. Architecture Diagrams (8)

**[ARCHITECTURE_DIAGRAMS.md](ARCHITECTURE_DIAGRAMS.md)** - High-level system structure and deployment

| # | Diagram | Use Case |
|---|---------|----------|
| 1 | System Context | Platform in ecosystem with external systems |
| 2 | Clean Architecture Layers | Dependency rule and layer responsibilities |
| 3 | Module Structure | 15 platform modules and their dependencies |
| 4 | CQRS Architecture | Command/query separation with read/write models |
| 5 | Event-Driven Architecture | Event flow from domain to integration events |
| 6 | Microservices Topology | Service mesh, API gateway, observability |
| 7 | Data Flow Architecture | Data ingestion, processing, storage, analytics |
| 8 | Deployment Architecture | Azure cloud deployment with HA and DR |

---

### 3. Database Diagrams (7)

**[DATABASE_DIAGRAMS.md](DATABASE_DIAGRAMS.md)** - Entity Relationship Diagrams (ERDs)

| # | Diagram | Entities | Purpose |
|---|---------|----------|---------|
| 1 | Core Domain Model | 5 entities | Customer, Order, OrderItem, Product, Category |
| 2 | Multi-Tenancy Schema | 5 entities | Tenant, User, TenantUser, TenantSetting, TenantFeature |
| 3 | RBAC Schema | 6 entities | Role, Permission, UserRole, RolePermission, UserPermission |
| 4 | Audit Log Schema | 4 entities | AuditLog, AuditLogDetail, DataAccessLog, SecurityEvent |
| 5 | Outbox Pattern Schema | 4 entities | OutboxMessage, OutboxDeliveryLog, InboxMessage, IdempotencyKey |
| 6 | Feature Flags Schema | 5 entities | FeatureFlag, Variant, Rule, Override, Evaluation |
| 7 | Migration History Schema | 4 entities | MigrationHistory, MigrationScript, SchemaVersion, MigrationLock |

---

### 4. State Diagrams (7)

**[STATE_DIAGRAMS.md](STATE_DIAGRAMS.md)** - Business process state machines

| # | Diagram | States | Process |
|---|---------|--------|---------|
| 1 | Order Lifecycle | 16 states | Draft → Confirmed → Shipped → Delivered → Completed |
| 2 | Payment Processing | 12 states | Initiated → Authorized → Captured → Settled |
| 3 | User Registration | 11 states | Started → EmailVerified → Active |
| 4 | Feature Flag Lifecycle | 8 states | Draft → Testing → Canary → Rollout → Permanent |
| 5 | Outbox Message Processing | 7 states | Pending → Publishing → Published → Completed |
| 6 | Circuit Breaker States | 3 main states | Closed → Open → HalfOpen |
| 7 | Tenant Activation | 10 states | Registration → Provisioning → Active → Suspended |

---

### 5. Integration Patterns (8)

**[INTEGRATION_PATTERNS.md](INTEGRATION_PATTERNS.md)** - API and event integration patterns

| # | Diagram | Pattern | Purpose |
|---|---------|---------|---------|
| 1 | REST API Gateway | API Gateway | Centralized routing and auth |
| 2 | GraphQL API | GraphQL | Flexible querying |
| 3 | Webhook Integration | Webhooks | Event push to subscribers |
| 4 | Message Bus Integration | Pub/Sub | Decoupled messaging |
| 5 | Saga Pattern | Distributed Transactions | Long-running workflows |
| 6 | API Rate Limiting | Throttling | Protect from abuse |
| 7 | API Versioning | Versioning | Backward compatibility |
| 8 | Service-to-Service Auth | OAuth2 | Machine-to-machine auth |

---

### 6. UI Component Flows (10)

**[UI_COMPONENT_FLOWS.md](UI_COMPONENT_FLOWS.md)** - Blazor component interactions

| # | Diagram | Component | Purpose |
|---|---------|-----------|---------|
| 1 | Form Validation Flow | N360Form | Multi-stage validation |
| 2 | Data Grid with CRUD | N360DataGrid | Server-side operations |
| 3 | File Upload Flow | N360FileUpload | Multi-file upload with preview |
| 4 | Permission Check Flow | N360Button | RBAC integration |
| 5 | Theme Switching | ThemeService | Dynamic theming |
| 6 | Modal Dialog Flow | N360Modal | Async confirmation |
| 7 | Autocomplete Search | N360Autocomplete | Debounced server search |
| 8 | Notification System | N360Toast | Queued notifications |
| 9 | Wizard Component | N360Wizard | Multi-step forms |
| 10 | Chart Data Refresh | N360Chart | Real-time data updates |

---

### 7. DevOps Diagrams (8)

**[DEVOPS_DIAGRAMS.md](DEVOPS_DIAGRAMS.md)** - CI/CD and operations

| # | Diagram | Pipeline | Purpose |
|---|---------|----------|---------|
| 1 | CI Pipeline | Build | Compile, test, package |
| 2 | CD Pipeline | Deploy | Multi-environment deployment |
| 3 | GitFlow Workflow | Branching | Git branching strategy |
| 4 | Monitoring Stack | Observability | Metrics, logs, traces |
| 5 | Alerting Rules | Alerting | Incident detection |
| 6 | Backup Strategy | Backup | Data protection |
| 7 | Blue-Green Deployment | Deployment | Zero-downtime releases |
| 8 | Disaster Recovery | DR | Failover and recovery |

---

## 🛠️ Working with Diagrams

### Rendering

All diagrams use **Mermaid** syntax and render natively in:

✅ **GitHub/GitLab** - View directly in markdown  
✅ **VS Code** - Install [Markdown Preview Mermaid Support](https://marketplace.visualstudio.com/items?itemName=bierner.markdown-mermaid)  
✅ **Online** - Copy to [mermaid.live](https://mermaid.live/)  
✅ **Documentation Sites** - Docusaurus, MkDocs, VuePress, etc.

### Exporting

Export diagrams to images:

```bash
# Using Mermaid CLI
npm install -g @mermaid-js/mermaid-cli
mmdc -i SEQUENCE_DIAGRAMS.md -o output.png

# Using Docker
docker run --rm -v $(pwd):/data minlag/mermaid-cli -i /data/SEQUENCE_DIAGRAMS.md -o /data/output.png
```

### Customization

Modify diagram themes:

```javascript
%%{init: {'theme':'dark', 'themeVariables': { 'primaryColor':'#bb2528'}}}%%
```

Available themes: `default`, `dark`, `forest`, `neutral`

---

## 📚 Learning Path

### Beginner
1. Read [System Context](ARCHITECTURE_DIAGRAMS.md#1-system-context-diagram) - understand what we're building
2. Review [Clean Architecture](ARCHITECTURE_DIAGRAMS.md#2-clean-architecture-layers) - learn the structure
3. Study [CQRS Command Flow](SEQUENCE_DIAGRAMS.md#1-cqrs-command-flow) - see how commands work
4. Explore [Core Domain Model](DATABASE_DIAGRAMS.md#1-core-domain-model) - understand data

### Intermediate
1. Master [CQRS Architecture](ARCHITECTURE_DIAGRAMS.md#4-cqrs-architecture) - command/query separation
2. Learn [Domain Event Dispatch](SEQUENCE_DIAGRAMS.md#3-domain-event-dispatch) - event-driven design
3. Study [Repository Pattern](SEQUENCE_DIAGRAMS.md#4-repository-pattern) - data access
4. Review [State Diagrams](STATE_DIAGRAMS.md) - business processes

### Advanced
1. Implement [Event-Driven Architecture](ARCHITECTURE_DIAGRAMS.md#5-event-driven-architecture) - full event sourcing
2. Deploy [Microservices Topology](ARCHITECTURE_DIAGRAMS.md#6-microservices-topology) - distributed systems
3. Master [Integration Patterns](INTEGRATION_PATTERNS.md) - enterprise integration
4. Optimize [DevOps Pipelines](DEVOPS_DIAGRAMS.md) - production operations

---

## 🎨 Diagram Conventions

### Color Coding

- 🔴 **Red/Pink** (#ffcccc) - Write operations, commands
- 🟢 **Green** (#ccffcc) - Read operations, queries
- 🔵 **Blue** (#ccccff) - Events, messaging
- 🟡 **Yellow** (#ffffcc) - Caching, optimization
- 🟠 **Orange** (#ffddaa) - External systems
- 🟣 **Purple** (#ddccff) - Security, auth

### Icons & Symbols

- `🔒` - Security/encryption
- `📊` - Analytics/reporting
- `⚡` - Performance-critical
- `🔄` - Retry/resilience
- `📝` - Audit/logging
- `🚀` - Deployment
- `⚠️` - Warning/attention
- `✅` - Validated/complete

### Naming Conventions

- **PascalCase**: Classes, entities, services
- **camelCase**: Variables, parameters
- **UPPER_SNAKE_CASE**: Constants, enums
- **kebab-case**: URLs, file names

---

## 📖 Additional Resources

### Related Documentation

- [PLATFORM_GUIDE.md](../../PLATFORM_GUIDE.md) - Module inventory and status
- [COMPONENT_INVENTORY.md](../../COMPONENT_INVENTORY.md) - UI components catalog
- [QUICK_REFERENCE.md](../../QUICK_REFERENCE.md) - Code patterns and examples
- [CONTRIBUTING.md](../../CONTRIBUTING.md) - Development guidelines

### External References

- [Mermaid Documentation](https://mermaid.js.org/)
- [C4 Model](https://c4model.com/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)

---

## 🤝 Contributing

### Adding New Diagrams

1. Choose the appropriate category file
2. Follow existing diagram structure
3. Use consistent styling and naming
4. Add to this index with description
5. Update table of contents

### Updating Existing Diagrams

1. Maintain backward compatibility
2. Document breaking changes
3. Update related diagrams
4. Increment version number
5. Add changelog entry

### Review Checklist

- [ ] Diagram renders correctly in GitHub
- [ ] All participants/entities labeled clearly
- [ ] Notes explain complex logic
- [ ] Alternative paths shown (success/failure)
- [ ] Consistent with other diagrams
- [ ] Index updated with new diagram

---

## 📊 Diagram Statistics

### By Category
- **Flows**: 15 sequence + 8 integration = 23 (37%)
- **Structure**: 8 architecture + 7 database = 15 (24%)
- **Process**: 7 state + 10 UI = 17 (27%)
- **Operations**: 8 DevOps (13%)

### By Complexity
- **Simple** (< 10 elements): 18 diagrams
- **Medium** (10-20 elements): 28 diagrams
- **Complex** (> 20 elements): 17 diagrams

### Coverage
- ✅ CQRS patterns: 100%
- ✅ Data access: 100%
- ✅ Security/auth: 100%
- ✅ Messaging: 100%
- ✅ Resilience: 100%
- ✅ Multi-tenancy: 100%
- ✅ DevOps: 100%

---

**Document Version:** 1.0  
**Total Diagrams:** 63  
**Last Updated:** November 18, 2025  
**Maintained By:** Nalam360 Platform Team

---

## Quick Links

- 📊 [Sequence Diagrams](SEQUENCE_DIAGRAMS.md)
- 🏗️ [Architecture Diagrams](ARCHITECTURE_DIAGRAMS.md)
- 💾 [Database Diagrams](DATABASE_DIAGRAMS.md)
- 🔄 [State Diagrams](STATE_DIAGRAMS.md)
- 🔌 [Integration Patterns](INTEGRATION_PATTERNS.md)
- 🎨 [UI Component Flows](UI_COMPONENT_FLOWS.md)
- 🚀 [DevOps Diagrams](DEVOPS_DIAGRAMS.md)
