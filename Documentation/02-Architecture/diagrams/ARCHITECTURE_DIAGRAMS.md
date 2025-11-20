# Nalam360 Enterprise Platform - Architecture Diagrams

This document contains high-level architecture diagrams showing system structure, component relationships, and deployment patterns.

**Format:** Mermaid  
**Date:** November 18, 2025

---

## Table of Contents

1. [System Context Diagram](#1-system-context-diagram)
2. [Clean Architecture Layers](#2-clean-architecture-layers)
3. [Module Structure](#3-module-structure)
4. [CQRS Architecture](#4-cqrs-architecture)
5. [Event-Driven Architecture](#5-event-driven-architecture)
6. [Microservices Topology](#6-microservices-topology)
7. [Data Flow Architecture](#7-data-flow-architecture)
8. [Deployment Architecture](#8-deployment-architecture)

---

## 1. System Context Diagram

**Description:** High-level view of the platform and external systems.

```mermaid
C4Context
    title System Context - Nalam360 Enterprise Platform

    Person(user, "End User", "Application user accessing features")
    Person(admin, "Administrator", "System admin managing platform")
    
    System_Boundary(platform, "Nalam360 Platform") {
        System(api, "Platform APIs", "REST APIs exposing business capabilities")
        System(ui, "Enterprise UI", "Blazor component library")
    }
    
    System_Ext(identity, "Identity Provider", "Azure AD / Auth0")
    System_Ext(storage, "Cloud Storage", "Azure Blob / AWS S3")
    System_Ext(messaging, "Message Broker", "RabbitMQ / Azure Service Bus")
    System_Ext(monitoring, "Observability", "Application Insights / Jaeger")
    System_Ext(email, "Email Service", "SendGrid / SMTP")
    System_Ext(payment, "Payment Gateway", "Stripe / PayPal")
    
    Rel(user, ui, "Uses", "HTTPS")
    Rel(ui, api, "Calls", "REST/JSON")
    Rel(admin, api, "Manages", "HTTPS")
    
    Rel(api, identity, "Authenticates", "OAuth2/OIDC")
    Rel(api, storage, "Stores files", "SDK")
    Rel(api, messaging, "Publishes events", "AMQP")
    Rel(api, monitoring, "Sends telemetry", "OTLP")
    Rel(api, email, "Sends emails", "SMTP/API")
    Rel(api, payment, "Processes payments", "REST")
```

---

## 2. Clean Architecture Layers

**Description:** Dependency flow in Clean Architecture with the Dependency Rule.

```mermaid
graph TB
    subgraph "Presentation Layer"
        UI[Blazor Components]
        API[Web APIs/Controllers]
    end
    
    subgraph "Application Layer"
        Commands[Commands/Queries]
        Handlers[Command/Query Handlers]
        Behaviors[Pipeline Behaviors]
        DTOs[DTOs/ViewModels]
    end
    
    subgraph "Domain Layer"
        Entities[Entities/Aggregates]
        ValueObjects[Value Objects]
        Events[Domain Events]
        Interfaces[Repository Interfaces]
    end
    
    subgraph "Infrastructure Layer"
        Data[EF Core/Dapper]
        Repos[Repository Implementations]
        External[External Service Clients]
        Messaging[Event Bus]
    end
    
    subgraph "Core Layer"
        Results[Result Pattern]
        Time[Time Providers]
        Security[Crypto Services]
    end
    
    UI --> Commands
    API --> Commands
    Commands --> Handlers
    Handlers --> Entities
    Handlers --> Interfaces
    Behaviors --> Handlers
    DTOs --> Commands
    
    Repos --> Interfaces
    Data --> Repos
    External --> Interfaces
    Messaging --> Events
    
    Entities --> ValueObjects
    Entities --> Events
    Handlers --> Results
    
    style Domain Layer fill:#e1f5ff
    style Core Layer fill:#fff4e1
    
    classDef dependency stroke:#ff6b6b,stroke-width:3px
    class Interfaces,Results dependency
```

---

## 3. Module Structure

**Description:** Platform modules and their dependencies.

```mermaid
graph LR
    subgraph "Foundation"
        Core[Platform.Core]
        Domain[Platform.Domain]
    end
    
    subgraph "Application"
        App[Platform.Application]
        Validation[Platform.Validation]
    end
    
    subgraph "Infrastructure"
        Data[Platform.Data]
        Integration[Platform.Integration]
        Messaging[Platform.Messaging]
        Caching[Platform.Caching]
    end
    
    subgraph "Cross-Cutting"
        Security[Platform.Security]
        Observability[Platform.Observability]
        Resilience[Platform.Resilience]
        Serialization[Platform.Serialization]
    end
    
    subgraph "Features"
        FeatureFlags[Platform.FeatureFlags]
        Tenancy[Platform.Tenancy]
        Documentation[Platform.Documentation]
    end
    
    subgraph "UI"
        EnterpriseUI[Enterprise.UI]
    end
    
    App --> Domain
    App --> Core
    Validation --> Core
    
    Data --> Domain
    Data --> Core
    Integration --> Core
    Messaging --> Core
    Caching --> Core
    
    Security --> Core
    Observability --> Core
    Resilience --> Core
    Serialization --> Core
    
    FeatureFlags --> Core
    Tenancy --> Domain
    Documentation --> Core
    
    EnterpriseUI --> App
    EnterpriseUI --> Security
    
    style Core fill:#ffe1e1
    style Domain fill:#e1ffe1
    style EnterpriseUI fill:#e1e1ff
```

---

## 4. CQRS Architecture

**Description:** Command Query Responsibility Segregation pattern implementation.

```mermaid
graph TB
    subgraph "Client Layer"
        WebUI[Web UI]
        Mobile[Mobile App]
        API[External API Client]
    end
    
    subgraph "API Gateway"
        Gateway[API Gateway/BFF]
    end
    
    subgraph "Command Side - Write Model"
        CommandAPI[Command API]
        CommandHandler[Command Handlers]
        WriteDB[(Write Database)]
        DomainModel[Domain Model<br/>Aggregates/Entities]
    end
    
    subgraph "Query Side - Read Model"
        QueryAPI[Query API]
        QueryHandler[Query Handlers]
        ReadDB[(Read Database<br/>Denormalized)]
        ViewModels[View Models/DTOs]
    end
    
    subgraph "Event Bus"
        EventBus[RabbitMQ/Service Bus]
    end
    
    subgraph "Projections"
        Projector[Event Projectors]
    end
    
    WebUI --> Gateway
    Mobile --> Gateway
    API --> Gateway
    
    Gateway --> CommandAPI
    Gateway --> QueryAPI
    
    CommandAPI --> CommandHandler
    CommandHandler --> DomainModel
    DomainModel --> WriteDB
    
    QueryAPI --> QueryHandler
    QueryHandler --> ReadDB
    QueryHandler --> ViewModels
    
    CommandHandler --> EventBus
    EventBus --> Projector
    Projector --> ReadDB
    
    style CommandAPI fill:#ffcccc
    style QueryAPI fill:#ccffcc
    style EventBus fill:#ccccff
```

---

## 5. Event-Driven Architecture

**Description:** Event flow from domain events to integration events and external systems.

```mermaid
graph TB
    subgraph "Domain Layer"
        Aggregate[Aggregate Root]
        DomainEvent[Domain Event]
    end
    
    subgraph "Application Layer"
        EventHandler[Domain Event Handler]
        IntegrationEvent[Integration Event]
    end
    
    subgraph "Infrastructure"
        Outbox[(Outbox Table)]
        OutboxProcessor[Outbox Processor]
        EventBus[Event Bus]
    end
    
    subgraph "External Systems"
        ServiceA[Microservice A]
        ServiceB[Microservice B]
        Webhook[Webhook Subscribers]
    end
    
    subgraph "Internal Consumers"
        EmailHandler[Email Handler]
        NotificationHandler[Notification Handler]
        AuditHandler[Audit Logger]
    end
    
    Aggregate -->|Raises| DomainEvent
    DomainEvent -->|Triggers| EventHandler
    EventHandler -->|Creates| IntegrationEvent
    IntegrationEvent -->|Saves to| Outbox
    
    OutboxProcessor -->|Polls| Outbox
    OutboxProcessor -->|Publishes| EventBus
    
    EventBus -->|Routes to| ServiceA
    EventBus -->|Routes to| ServiceB
    EventBus -->|Triggers| Webhook
    
    EventBus -->|Consumes| EmailHandler
    EventBus -->|Consumes| NotificationHandler
    EventBus -->|Consumes| AuditHandler
    
    style DomainEvent fill:#ffe1e1
    style IntegrationEvent fill:#e1ffe1
    style EventBus fill:#e1e1ff
```

---

## 6. Microservices Topology

**Description:** Microservices deployment with API gateway and service mesh.

```mermaid
graph TB
    subgraph "Edge Layer"
        LoadBalancer[Load Balancer]
        CDN[CDN]
    end
    
    subgraph "API Gateway Layer"
        Gateway[API Gateway<br/>Kong/Ocelot]
        AuthService[Auth Service]
    end
    
    subgraph "Service Mesh"
        ServiceMesh[Istio/Linkerd]
    end
    
    subgraph "Business Services"
        OrderService[Order Service]
        InventoryService[Inventory Service]
        PaymentService[Payment Service]
        CustomerService[Customer Service]
        NotificationService[Notification Service]
    end
    
    subgraph "Data Layer"
        OrderDB[(Order DB)]
        InventoryDB[(Inventory DB)]
        PaymentDB[(Payment DB)]
        CustomerDB[(Customer DB)]
    end
    
    subgraph "Infrastructure Services"
        MessageBroker[RabbitMQ]
        CacheCluster[Redis Cluster]
        SearchEngine[Elasticsearch]
    end
    
    subgraph "Observability"
        Prometheus[Prometheus]
        Grafana[Grafana]
        Jaeger[Jaeger]
        ELK[ELK Stack]
    end
    
    CDN --> LoadBalancer
    LoadBalancer --> Gateway
    Gateway --> AuthService
    
    Gateway --> ServiceMesh
    
    ServiceMesh --> OrderService
    ServiceMesh --> InventoryService
    ServiceMesh --> PaymentService
    ServiceMesh --> CustomerService
    ServiceMesh --> NotificationService
    
    OrderService --> OrderDB
    InventoryService --> InventoryDB
    PaymentService --> PaymentDB
    CustomerService --> CustomerDB
    
    OrderService --> MessageBroker
    InventoryService --> MessageBroker
    PaymentService --> MessageBroker
    NotificationService --> MessageBroker
    
    OrderService --> CacheCluster
    CustomerService --> CacheCluster
    
    OrderService --> SearchEngine
    
    ServiceMesh --> Prometheus
    Prometheus --> Grafana
    ServiceMesh --> Jaeger
    ServiceMesh --> ELK
```

---

## 7. Data Flow Architecture

**Description:** Data flow from external sources through processing to storage.

```mermaid
graph LR
    subgraph "Data Sources"
        API[REST APIs]
        Files[File Uploads]
        Events[Event Streams]
        Webhooks[Webhooks]
    end
    
    subgraph "Ingestion Layer"
        Gateway[API Gateway]
        FileProcessor[File Processor]
        EventConsumer[Event Consumer]
    end
    
    subgraph "Processing Layer"
        Validation[Validation]
        Transformation[Transformation]
        Enrichment[Enrichment]
        BusinessLogic[Business Logic]
    end
    
    subgraph "Storage Layer"
        TransactionalDB[(Transactional DB<br/>PostgreSQL)]
        DocumentDB[(Document DB<br/>MongoDB)]
        BlobStorage[(Blob Storage<br/>Azure/S3)]
        TimeSeriesDB[(Time Series<br/>InfluxDB)]
    end
    
    subgraph "Analytics Layer"
        DataWarehouse[(Data Warehouse)]
        ReportingDB[(Reporting DB)]
        Analytics[Analytics Engine]
    end
    
    API --> Gateway
    Files --> FileProcessor
    Events --> EventConsumer
    Webhooks --> Gateway
    
    Gateway --> Validation
    FileProcessor --> Validation
    EventConsumer --> Validation
    
    Validation --> Transformation
    Transformation --> Enrichment
    Enrichment --> BusinessLogic
    
    BusinessLogic --> TransactionalDB
    BusinessLogic --> DocumentDB
    FileProcessor --> BlobStorage
    Events --> TimeSeriesDB
    
    TransactionalDB --> DataWarehouse
    DocumentDB --> DataWarehouse
    DataWarehouse --> ReportingDB
    ReportingDB --> Analytics
    
    style Validation fill:#ffe1e1
    style BusinessLogic fill:#e1ffe1
    style DataWarehouse fill:#e1e1ff
```

---

## 8. Deployment Architecture

**Description:** Cloud deployment on Azure with high availability.

```mermaid
graph TB
    subgraph "Azure Cloud"
        subgraph "Region: East US"
            subgraph "Virtual Network"
                subgraph "Application Gateway Subnet"
                    AppGateway[Application Gateway<br/>WAF Enabled]
                end
                
                subgraph "Web App Subnet"
                    AKS1[AKS Cluster<br/>3 Nodes]
                    subgraph "AKS Pods"
                        API1[API Pods x3]
                        Worker1[Worker Pods x2]
                    end
                end
                
                subgraph "Data Subnet"
                    PrimaryDB[(Azure SQL<br/>Primary)]
                    Redis1[Azure Redis<br/>Primary]
                end
            end
        end
        
        subgraph "Region: West US - DR"
            subgraph "Virtual Network DR"
                subgraph "Web App Subnet DR"
                    AKS2[AKS Cluster<br/>2 Nodes]
                end
                
                subgraph "Data Subnet DR"
                    SecondaryDB[(Azure SQL<br/>Read Replica)]
                    Redis2[Azure Redis<br/>Replica]
                end
            end
        end
        
        subgraph "Shared Services"
            TrafficManager[Traffic Manager<br/>Global Load Balancer]
            KeyVault[Azure Key Vault]
            Monitor[Application Insights]
            Storage[Blob Storage<br/>Geo-Redundant]
            ServiceBus[Azure Service Bus<br/>Premium]
            CDN[Azure CDN]
        end
    end
    
    subgraph "Users"
        Internet[Internet Users]
    end
    
    Internet --> CDN
    CDN --> TrafficManager
    TrafficManager --> AppGateway
    
    AppGateway --> AKS1
    AKS1 --> API1
    AKS1 --> Worker1
    
    API1 --> PrimaryDB
    API1 --> Redis1
    API1 --> Storage
    API1 --> ServiceBus
    Worker1 --> ServiceBus
    
    PrimaryDB -.->|Replication| SecondaryDB
    Redis1 -.->|Replication| Redis2
    
    API1 --> KeyVault
    API1 --> Monitor
    Worker1 --> Monitor
    
    TrafficManager -.->|Failover| AKS2
    AKS2 -.-> SecondaryDB
    
    style TrafficManager fill:#e1f5ff
    style PrimaryDB fill:#ffe1e1
    style ServiceBus fill:#e1ffe1
```

---

## Usage Notes

### C4 Model

These diagrams follow the **C4 Model** principles:
- **Context**: System in environment
- **Container**: High-level technology choices
- **Component**: Internal structure
- **Code**: Class-level details (sequence diagrams)

### Deployment Considerations

The deployment architecture shows:
- **High Availability**: Multi-region deployment
- **Disaster Recovery**: Read replicas and failover
- **Scalability**: Kubernetes for horizontal scaling
- **Security**: WAF, Key Vault, VNet isolation
- **Observability**: Centralized monitoring and logging

### Customization

Adapt diagrams for your infrastructure:
- Swap Azure for AWS/GCP services
- Adjust scaling parameters
- Modify network topology
- Add/remove microservices

---

**Document Version:** 1.0  
**Last Updated:** November 18, 2025  
**Maintained By:** Nalam360 Platform Team
