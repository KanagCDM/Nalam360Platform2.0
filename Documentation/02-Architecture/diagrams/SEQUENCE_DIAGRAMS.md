# Nalam360 Enterprise Platform - Architecture Sequence Diagrams

This document contains comprehensive sequence diagrams illustrating the key architectural flows and patterns used in the Nalam360 Enterprise Platform.

**Total Diagrams:** 15  
**Format:** Mermaid  
**Date:** November 18, 2025

---

## Table of Contents

1. [CQRS Command Flow](#1-cqrs-command-flow)
2. [CQRS Query Flow](#2-cqrs-query-flow)
3. [Domain Event Dispatch](#3-domain-event-dispatch)
4. [Repository Pattern](#4-repository-pattern)
5. [Unit of Work Pattern](#5-unit-of-work-pattern)
6. [Messaging with Outbox Pattern](#6-messaging-with-outbox-pattern)
7. [RabbitMQ Event Publishing](#7-rabbitmq-event-publishing)
8. [Circuit Breaker Pattern](#8-circuit-breaker-pattern)
9. [Feature Flag Evaluation](#9-feature-flag-evaluation)
10. [RBAC Authorization Flow](#10-rbac-authorization-flow)
11. [Database Migration Execution](#11-database-migration-execution)
12. [Validation Pipeline](#12-validation-pipeline)
13. [Cloud Storage Upload](#13-cloud-storage-upload)
14. [Distributed Tracing](#14-distributed-tracing)
15. [Multi-Tenancy Resolution](#15-multi-tenancy-resolution)

---

## 1. CQRS Command Flow

**Description:** Shows how a command flows through the mediator pipeline with validation and logging behaviors before reaching the handler.

```mermaid
sequenceDiagram
    participant Client
    participant API as API Controller
    participant Mediator
    participant LogBehavior as Logging Behavior
    participant ValBehavior as Validation Behavior
    participant Handler as Command Handler
    participant Repository
    participant UoW as Unit of Work
    participant DB as Database

    Client->>API: POST /api/orders
    API->>Mediator: Send(CreateOrderCommand)
    
    Mediator->>LogBehavior: Handle(command)
    LogBehavior->>LogBehavior: Log command details
    
    LogBehavior->>ValBehavior: next()
    ValBehavior->>ValBehavior: Validate command
    alt Validation Fails
        ValBehavior-->>API: Result.Failure(errors)
        API-->>Client: 400 Bad Request
    end
    
    ValBehavior->>Handler: next()
    Handler->>Repository: GetByIdAsync(customerId)
    Repository->>DB: SELECT * FROM Customers
    DB-->>Repository: Customer data
    Repository-->>Handler: Customer
    
    Handler->>Handler: Create Order aggregate
    Handler->>Handler: order.AddDomainEvent()
    
    Handler->>Repository: AddAsync(order)
    Repository->>UoW: Track entity
    
    Handler->>UoW: SaveChangesAsync()
    UoW->>DB: BEGIN TRANSACTION
    UoW->>DB: INSERT INTO Orders
    UoW->>UoW: Dispatch domain events
    UoW->>DB: COMMIT
    DB-->>UoW: Success
    
    UoW-->>Handler: Success
    Handler-->>ValBehavior: Result.Success(orderId)
    ValBehavior-->>LogBehavior: Result
    LogBehavior->>LogBehavior: Log success
    LogBehavior-->>Mediator: Result
    Mediator-->>API: Result.Success
    API-->>Client: 201 Created
```

---

## 2. CQRS Query Flow

**Description:** Demonstrates query execution with optional caching and specifications.

```mermaid
sequenceDiagram
    participant Client
    participant API as API Controller
    participant Mediator
    participant LogBehavior as Logging Behavior
    participant Handler as Query Handler
    participant Cache as Cache Service
    participant Repository
    participant Spec as Specification
    participant DB as Database

    Client->>API: GET /api/orders?status=pending
    API->>Mediator: Send(GetOrdersQuery)
    
    Mediator->>LogBehavior: Handle(query)
    LogBehavior->>Handler: next()
    
    Handler->>Cache: GetAsync("orders:pending")
    Cache-->>Handler: null (cache miss)
    
    Handler->>Handler: Create specification
    Note right of Handler: new OrdersByStatusSpec("pending")
    
    Handler->>Repository: GetAllAsync(spec)
    Repository->>Spec: ToExpression()
    Spec-->>Repository: Expression<Func<Order, bool>>
    
    Repository->>DB: SELECT * FROM Orders WHERE Status = 'Pending'
    DB-->>Repository: Order[]
    Repository-->>Handler: IReadOnlyList<Order>
    
    Handler->>Handler: Map to DTOs
    Handler->>Cache: SetAsync("orders:pending", dtos, 5min)
    Cache-->>Handler: Cached
    
    Handler-->>LogBehavior: Result.Success(dtos)
    LogBehavior-->>Mediator: Result
    Mediator-->>API: Result.Success
    API-->>Client: 200 OK + JSON
```

---

## 3. Domain Event Dispatch

**Description:** Shows how domain events are collected from aggregates and dispatched after persistence.

```mermaid
sequenceDiagram
    participant Handler as Command Handler
    participant Aggregate as Order Aggregate
    participant UoW as Unit of Work
    participant Dispatcher as Event Dispatcher
    participant Handler1 as OrderCreatedHandler
    participant Handler2 as SendEmailHandler
    participant Email as Email Service

    Handler->>Aggregate: Create(customerId, items)
    Aggregate->>Aggregate: Validate business rules
    Aggregate->>Aggregate: AddDomainEvent(OrderCreatedEvent)
    
    Handler->>UoW: SaveChangesAsync()
    UoW->>UoW: SaveChanges to DB
    
    UoW->>Aggregate: GetDomainEvents()
    Aggregate-->>UoW: [OrderCreatedEvent]
    
    loop For each event
        UoW->>Dispatcher: DispatchAsync(event)
        
        par Parallel Handler Execution
            Dispatcher->>Handler1: Handle(OrderCreatedEvent)
            Handler1->>Handler1: Update inventory
            Handler1-->>Dispatcher: Success
        and
            Dispatcher->>Handler2: Handle(OrderCreatedEvent)
            Handler2->>Email: SendConfirmationAsync()
            Email-->>Handler2: Email sent
            Handler2-->>Dispatcher: Success
        end
    end
    
    UoW->>Aggregate: ClearDomainEvents()
    Aggregate-->>UoW: Events cleared
    
    UoW-->>Handler: Success
```

---

## 4. Repository Pattern

**Description:** Generic repository with specification pattern for complex queries.

```mermaid
sequenceDiagram
    participant Service
    participant Repo as Repository<Order>
    participant Spec as Specification<Order>
    participant DbContext
    participant DbSet as DbSet<Order>
    participant DB as Database

    Service->>Repo: GetAllAsync(specification)
    
    alt No specification
        Repo->>DbSet: ToListAsync()
        DbSet->>DB: SELECT * FROM Orders
    else With specification
        Repo->>Spec: ToExpression()
        Spec-->>Repo: Expression<Func<Order, bool>>
        Repo->>DbSet: Where(expression).ToListAsync()
        DbSet->>DB: SELECT * FROM Orders WHERE ...
    end
    
    DB-->>DbSet: Order[]
    DbSet-->>Repo: List<Order>
    Repo-->>Service: IReadOnlyList<Order>
    
    Note right of Service: Alternatively: GetByIdAsync
    Service->>Repo: GetByIdAsync(id)
    Repo->>DbSet: FindAsync(id)
    DbSet->>DB: SELECT * WHERE Id = @id
    DB-->>DbSet: Order or null
    DbSet-->>Repo: Order?
    Repo-->>Service: Order?
```

---

## 5. Unit of Work Pattern

**Description:** Transactional coordination with automatic entity tracking and domain event dispatch.

```mermaid
sequenceDiagram
    participant Service
    participant UoW as Unit of Work
    participant Repo1 as OrderRepository
    participant Repo2 as CustomerRepository
    participant DbContext
    participant Dispatcher as Event Dispatcher
    participant DB as Database

    Service->>Repo1: AddAsync(order)
    Repo1->>DbContext: Add(order)
    DbContext->>DbContext: Track entity (Added)
    
    Service->>Repo2: UpdateAsync(customer)
    Repo2->>DbContext: Update(customer)
    DbContext->>DbContext: Track entity (Modified)
    
    Service->>UoW: SaveChangesAsync()
    
    UoW->>DbContext: Database.BeginTransactionAsync()
    DbContext->>DB: BEGIN TRANSACTION
    
    UoW->>DbContext: SaveChangesAsync()
    DbContext->>DB: INSERT INTO Orders
    DbContext->>DB: UPDATE Customers
    DB-->>DbContext: Rows affected
    
    UoW->>UoW: Collect domain events
    loop For each aggregate with events
        UoW->>Dispatcher: DispatchAsync(events)
        Dispatcher->>Dispatcher: Execute handlers
        Dispatcher-->>UoW: Success
    end
    
    UoW->>DbContext: CommitTransactionAsync()
    DbContext->>DB: COMMIT
    
    alt Success
        DB-->>DbContext: Committed
        DbContext-->>UoW: Success
        UoW-->>Service: Success (rows affected)
    else Failure
        DbContext->>DB: ROLLBACK
        DB-->>DbContext: Rolled back
        DbContext-->>UoW: Exception
        UoW-->>Service: Exception
    end
```

---

## 6. Messaging with Outbox Pattern

**Description:** Transactional outbox pattern ensuring reliable message delivery.

```mermaid
sequenceDiagram
    participant Service
    participant UoW as Unit of Work
    participant Outbox as Outbox Repository
    participant DB as Database
    participant Processor as Outbox Processor
    participant EventBus as Event Bus
    participant RabbitMQ

    Note over Service,RabbitMQ: Phase 1: Save to Outbox
    Service->>UoW: BeginTransactionAsync()
    Service->>Service: Create business entity
    Service->>UoW: AddAsync(order)
    
    Service->>Service: Create integration event
    Note right of Service: OrderCreatedIntegrationEvent
    
    Service->>Outbox: SaveAsync(event)
    Outbox->>DB: INSERT INTO OutboxMessages
    Note right of DB: EventId, Type, Payload, Status=Pending
    
    Service->>UoW: CommitAsync()
    UoW->>DB: COMMIT TRANSACTION
    DB-->>Service: Success
    
    Note over Service,RabbitMQ: Phase 2: Background Processing
    Processor->>Processor: Timer tick (every 10s)
    Processor->>Outbox: GetPendingAsync(batchSize: 100)
    Outbox->>DB: SELECT TOP 100 WHERE Status = 'Pending'
    DB-->>Outbox: OutboxMessage[]
    Outbox-->>Processor: Messages
    
    loop For each message
        Processor->>Processor: Deserialize event
        Processor->>EventBus: PublishAsync(event)
        EventBus->>RabbitMQ: basic.publish(exchange, routingKey, body)
        RabbitMQ-->>EventBus: Confirmation
        
        alt Publish succeeded
            EventBus-->>Processor: Success
            Processor->>Outbox: MarkAsProcessedAsync(messageId)
            Outbox->>DB: UPDATE Status = 'Processed', ProcessedAt = NOW()
        else Publish failed
            EventBus-->>Processor: Exception
            Processor->>Outbox: IncrementRetryCount(messageId)
            Outbox->>DB: UPDATE RetryCount++, LastError
        end
    end
```

---

## 7. RabbitMQ Event Publishing

**Description:** Publishing events to RabbitMQ with retry logic and confirmations.

```mermaid
sequenceDiagram
    participant Service
    participant EventBus as RabbitMQ Event Bus
    participant Policy as Retry Policy
    participant Connection as RabbitMQ Connection
    participant Channel as RabbitMQ Channel
    participant Exchange
    participant Queue

    Service->>EventBus: PublishAsync(OrderCreatedEvent)
    
    EventBus->>EventBus: Serialize event to JSON
    EventBus->>EventBus: Generate correlation ID
    
    EventBus->>Policy: ExecuteAsync(publishAction)
    Policy->>Connection: EnsureConnected()
    
    alt Not connected
        Connection->>Connection: CreateConnection()
        Connection->>Connection: factory.CreateConnection()
        Note right of Connection: Connection params from config
    end
    
    Policy->>Channel: CreateModel()
    Connection-->>Channel: IModel
    
    Channel->>Channel: ConfirmSelect() (publisher confirms)
    
    Policy->>Channel: ExchangeDeclare("orders.events", "topic")
    Channel->>Exchange: Declare exchange
    Exchange-->>Channel: OK
    
    Policy->>Channel: BasicPublish(exchange, routingKey, props, body)
    Note right of Channel: routingKey = "order.created"
    Channel->>Exchange: Publish message
    Exchange->>Queue: Route by pattern
    Queue-->>Exchange: Stored
    
    Channel->>Channel: WaitForConfirms(timeout: 5s)
    
    alt Confirmed
        Channel-->>Policy: true
        Policy-->>EventBus: Success
        EventBus-->>Service: Task completed
    else Not confirmed
        Channel-->>Policy: false
        Policy->>Policy: Retry (exponential backoff)
        Note right of Policy: Retry 3 times with jitter
        Policy->>Channel: BasicPublish() [Retry]
    else Timeout or error
        Channel-->>Policy: Exception
        Policy-->>EventBus: Exception
        EventBus-->>Service: PublishException
    end
```

---

## 8. Circuit Breaker Pattern

**Description:** Polly circuit breaker protecting external service calls.

```mermaid
sequenceDiagram
    participant Service
    participant Circuit as Circuit Breaker
    participant Policy as Resilience Policy
    participant External as External API
    participant Cache as Fallback Cache

    Note over Circuit: State: Closed (Normal)
    
    Service->>Circuit: ExecuteAsync(httpCall)
    Circuit->>Policy: Check state
    Policy-->>Circuit: Closed - Allow
    
    Circuit->>External: GET /api/data
    
    alt Success
        External-->>Circuit: 200 OK + Data
        Circuit->>Circuit: Reset failure count
        Circuit-->>Service: Result.Success(data)
    else Failure
        External-->>Circuit: 500 Server Error
        Circuit->>Circuit: Increment failure count (3/5)
        Circuit-->>Service: Result.Failure(error)
    end
    
    Note over Circuit: More failures...
    
    Service->>Circuit: ExecuteAsync(httpCall)
    Circuit->>External: GET /api/data
    External-->>Circuit: Timeout
    Circuit->>Circuit: Failure count (5/5) - THRESHOLD REACHED
    Circuit->>Circuit: Open circuit
    
    Note over Circuit: State: Open (Blocking)
    
    Service->>Circuit: ExecuteAsync(httpCall)
    Circuit->>Policy: Check state
    Policy-->>Circuit: Open - Block call
    
    Circuit->>Cache: Try get cached data
    Cache-->>Circuit: Cached data (if available)
    Circuit-->>Service: Result.Failure("Circuit open") or cached data
    
    Note over Circuit: Wait 30 seconds (break duration)
    
    Note over Circuit: State: Half-Open (Testing)
    
    Service->>Circuit: ExecuteAsync(httpCall)
    Circuit->>Policy: Check state
    Policy-->>Circuit: Half-Open - Allow test
    
    Circuit->>External: GET /api/data (test call)
    
    alt Success
        External-->>Circuit: 200 OK
        Circuit->>Circuit: Close circuit
        Note over Circuit: State: Closed
        Circuit-->>Service: Result.Success(data)
    else Failure
        External-->>Circuit: Error
        Circuit->>Circuit: Re-open circuit
        Note over Circuit: State: Open (another 30s)
        Circuit-->>Service: Result.Failure
    end
```

---

## 9. Feature Flag Evaluation

**Description:** Feature flag evaluation with percentage rollout and user targeting.

```mermaid
sequenceDiagram
    participant User
    participant App as Application
    participant FFService as Feature Flag Service
    participant Strategy as Rollout Strategy
    participant Provider as Remote Provider
    participant Cache as Feature Cache
    participant API as LaunchDarkly API

    User->>App: Access feature
    App->>FFService: IsEnabledAsync("new-checkout", context)
    
    FFService->>Cache: GetAsync("flag:new-checkout")
    
    alt Cache hit
        Cache-->>FFService: FeatureFlag
    else Cache miss
        FFService->>Provider: GetFeatureFlagAsync("new-checkout")
        Provider->>API: GET /flags/new-checkout
        API-->>Provider: FeatureFlag JSON
        Provider->>Provider: Deserialize
        Provider->>Cache: SetAsync(flag, TTL: 5min)
        Provider-->>FFService: FeatureFlag
    end
    
    FFService->>FFService: Get strategy type
    Note right of FFService: Strategy: PercentageRollout
    
    FFService->>Strategy: EvaluateAsync(context)
    
    alt Percentage Rollout
        Strategy->>Strategy: Hash user ID with SHA256
        Strategy->>Strategy: Convert to percentage (0-100)
        Note right of Strategy: hash(userId) % 100
        Strategy->>Strategy: Compare with rollout %
        alt User in rollout
            Strategy-->>FFService: true
        else User not in rollout
            Strategy-->>FFService: false
        end
    else User Targeting
        Strategy->>Strategy: Check if userId in targetedUsers
        alt User targeted
            Strategy-->>FFService: true
        else Not targeted
            Strategy-->>FFService: false
        end
    else Time Window
        Strategy->>Strategy: Check current time
        alt Within window
            Strategy-->>FFService: true
        else Outside window
            Strategy-->>FFService: false
        end
    end
    
    FFService-->>App: IsEnabled result
    
    alt Feature enabled
        App->>App: Execute new code path
        App-->>User: New feature UI
    else Feature disabled
        App->>App: Execute old code path
        App-->>User: Legacy UI
    end
```

---

## 10. RBAC Authorization Flow

**Description:** Role-based access control with permission inheritance and claim transformation.

```mermaid
sequenceDiagram
    participant User
    participant Middleware as Auth Middleware
    participant Transform as Claims Transformation
    participant AuthService as Authorization Service
    participant UserStore as User Principal Store
    participant RoleStore as Role Store
    participant PermStore as Permission Store
    participant Cache as Auth Cache

    User->>Middleware: HTTP Request + JWT
    Middleware->>Middleware: Validate JWT
    Middleware->>Middleware: Extract ClaimsPrincipal
    
    Middleware->>Transform: TransformAsync(principal)
    
    Transform->>UserStore: GetUserPrincipalAsync(userId)
    UserStore->>Cache: Get cached principal
    
    alt Cache miss
        UserStore->>UserStore: Query from DB
        UserStore->>Cache: Cache principal (5min)
    end
    
    UserStore-->>Transform: UserPrincipal
    
    Transform->>Transform: Get user roles
    Note right of Transform: roles: ["Admin", "Manager"]
    
    loop For each role
        Transform->>RoleStore: GetRolePermissionsAsync(role)
        RoleStore->>RoleStore: Get role + inherited roles
        Note right of RoleStore: Admin inherits from Manager
        RoleStore->>PermStore: Get permissions
        PermStore-->>RoleStore: Permission[]
        RoleStore-->>Transform: Permission[]
    end
    
    Transform->>Transform: Add permission claims
    Transform->>Transform: Add role claims
    Transform->>Transform: Remove denied permissions
    
    Transform-->>Middleware: Enhanced ClaimsPrincipal
    Middleware->>Middleware: Set HttpContext.User
    
    Note over Middleware: Request reaches controller
    
    Middleware->>AuthService: HasPermissionAsync("orders.create")
    
    AuthService->>AuthService: Get user claims
    AuthService->>AuthService: Check for permission claim
    
    alt Has permission claim
        AuthService->>AuthService: Check if explicitly denied
        alt Not denied
            AuthService-->>Middleware: AuthorizationResult.Success()
        else Explicitly denied
            AuthService-->>Middleware: AuthorizationResult.Failure("Denied")
        end
    else No permission claim
        AuthService-->>Middleware: AuthorizationResult.MissingPermission()
    end
    
    alt Authorized
        Middleware->>Middleware: Execute controller action
        Middleware-->>User: 200 OK + Response
    else Not authorized
        Middleware-->>User: 403 Forbidden
    end
```

---

## 11. Database Migration Execution

**Description:** Database migration with transaction support and rollback capability.

```mermaid
sequenceDiagram
    participant App as Application
    participant MigService as Migration Service
    participant Repo as Migration Repository
    participant Generator as Migration Generator
    participant DbContext
    participant DB as Database

    App->>MigService: ApplyAllPendingAsync()
    
    MigService->>Repo: GetAllMigrationsAsync()
    Repo->>Repo: Read .sql files from disk
    Repo-->>MigService: Migration[]
    
    MigService->>MigService: GetAppliedMigrationsAsync()
    MigService->>DB: SELECT * FROM __MigrationHistory
    DB-->>MigService: MigrationHistory[]
    
    MigService->>MigService: Filter pending migrations
    Note right of MigService: pending = all - applied
    
    alt No pending migrations
        MigService-->>App: Result.Success("Up to date")
    end
    
    loop For each pending migration
        MigService->>DB: BEGIN TRANSACTION
        
        MigService->>MigService: Split UpScript by "GO"
        
        loop For each SQL statement
            MigService->>DB: ExecuteNonQueryAsync(statement)
            
            alt Success
                DB-->>MigService: Rows affected
            else Error
                DB-->>MigService: SqlException
                MigService->>DB: ROLLBACK
                MigService-->>App: Result.Failure(error)
                Note right of MigService: Stop execution
            end
        end
        
        MigService->>MigService: Calculate checksum (SHA256)
        MigService->>MigService: Record in history
        
        MigService->>DB: INSERT INTO __MigrationHistory
        Note right of DB: MigrationId, Name, AppliedAt, <br/>Checksum, ExecutionTimeMs
        
        MigService->>DB: COMMIT
        DB-->>MigService: Success
    end
    
    MigService-->>App: Result.Success("Applied X migrations")
    
    Note over App,DB: Rollback Scenario
    
    App->>MigService: RollbackLastMigrationAsync()
    
    MigService->>DB: SELECT TOP 1 FROM __MigrationHistory ORDER BY AppliedAt DESC
    DB-->>MigService: Latest migration
    
    MigService->>Repo: GetMigrationByIdAsync(id)
    Repo-->>MigService: Migration with DownScript
    
    MigService->>DB: BEGIN TRANSACTION
    
    MigService->>MigService: Split DownScript by "GO"
    
    loop For each SQL statement
        MigService->>DB: ExecuteNonQueryAsync(statement)
        DB-->>MigService: Success
    end
    
    MigService->>DB: DELETE FROM __MigrationHistory WHERE Id = @id
    
    MigService->>DB: COMMIT
    DB-->>MigService: Success
    
    MigService-->>App: Result.Success("Rolled back")
```

---

## 12. Validation Pipeline

**Description:** FluentValidation and attribute validation in the CQRS pipeline.

```mermaid
sequenceDiagram
    participant Client
    participant API as API Controller
    participant Mediator
    participant ValBehavior as Validation Behavior
    participant FluentVal as FluentValidation
    participant AttrVal as Attribute Validator
    participant Handler as Command Handler

    Client->>API: POST /api/users {email, password}
    API->>API: Model binding
    
    opt Attribute Validation (ModelState)
        API->>AttrVal: ValidateModel(dto)
        AttrVal->>AttrVal: Check [Required], [EmailAddress], etc.
        alt Invalid
            AttrVal-->>API: ModelState errors
            API-->>Client: 400 Bad Request
            Note right of Client: Early return
        end
    end
    
    API->>Mediator: Send(CreateUserCommand)
    
    Mediator->>ValBehavior: Handle(command)
    
    ValBehavior->>ValBehavior: Get IValidator<CreateUserCommand>
    
    alt No validator registered
        ValBehavior->>Handler: next() (skip validation)
    else Validator exists
        ValBehavior->>FluentVal: ValidateAsync(command)
        
        FluentVal->>FluentVal: RuleFor(x => x.Email).EmailAddress()
        FluentVal->>FluentVal: RuleFor(x => x.Password).MinimumLength(8)
        FluentVal->>FluentVal: RuleFor(x => x.Email).MustAsync(BeUniqueEmail)
        
        par Async validation rules
            FluentVal->>FluentVal: Check email format
        and
            FluentVal->>FluentVal: Check password strength
        and
            FluentVal->>FluentVal: Query DB for email uniqueness
        end
        
        FluentVal->>FluentVal: Collect validation failures
        
        alt Validation failed
            FluentVal-->>ValBehavior: ValidationResult (Errors)
            ValBehavior->>ValBehavior: Convert to Result.Failure
            ValBehavior-->>Mediator: Result.Failure(validationErrors)
            Mediator-->>API: Result with errors
            API-->>Client: 400 Bad Request + error details
        else Validation passed
            FluentVal-->>ValBehavior: ValidationResult (IsValid)
            ValBehavior->>Handler: next()
            Handler->>Handler: Execute business logic
            Handler-->>ValBehavior: Result.Success
            ValBehavior-->>Mediator: Result
            Mediator-->>API: Result
            API-->>Client: 201 Created
        end
    end
```

---

## 13. Cloud Storage Upload

**Description:** File upload to Azure Blob Storage with SAS URL generation.

```mermaid
sequenceDiagram
    participant Client
    participant API as API Controller
    participant Storage as Azure Storage Service
    participant BlobClient as BlobServiceClient
    participant Container as BlobContainerClient
    participant Blob as BlobClient
    participant Azure as Azure Blob Storage

    Client->>API: POST /api/files/upload (multipart/form-data)
    API->>API: Read IFormFile
    
    API->>Storage: UploadAsync(stream, filename, contentType)
    
    Storage->>Storage: Generate blob name
    Note right of Storage: {guid}_{filename}
    
    Storage->>BlobClient: GetBlobContainerClient("uploads")
    BlobClient-->>Storage: ContainerClient
    
    Storage->>Container: CreateIfNotExistsAsync()
    Container->>Azure: Create container (if new)
    Azure-->>Container: Created or exists
    
    Storage->>Container: GetBlobClient(blobName)
    Container-->>Storage: BlobClient
    
    Storage->>Storage: Create upload options
    Note right of Storage: ContentType, Metadata, <br/>AccessTier, etc.
    
    Storage->>Blob: UploadAsync(stream, options)
    
    Blob->>Azure: Upload blob data (streaming)
    Note right of Azure: Automatic chunking for large files
    
    alt Upload successful
        Azure-->>Blob: BlobContentInfo
        Blob-->>Storage: Response
        
        Storage->>Storage: Generate SAS URL
        Storage->>Storage: Set permissions (Read)
        Storage->>Storage: Set expiry (1 hour)
        
        Storage->>Blob: GenerateSasUri(permissions, expiry)
        Blob-->>Storage: SAS URI
        
        Storage-->>API: Result.Success(url, blobName, size)
        API-->>Client: 200 OK {url, name, size, expiresAt}
    else Upload failed
        Azure-->>Blob: RequestFailedException
        Blob-->>Storage: Exception
        Storage-->>API: Result.Failure(error)
        API-->>Client: 500 Internal Server Error
    end
    
    Note over Client,Azure: Download Scenario
    
    Client->>Client: Click download link (SAS URL)
    Client->>Azure: GET https://.../blob?sas_token
    
    Azure->>Azure: Validate SAS token
    alt Valid token
        Azure->>Azure: Check permissions
        Azure->>Azure: Check expiry
        Azure-->>Client: 200 OK + blob content (stream)
    else Invalid/expired token
        Azure-->>Client: 403 Forbidden
    end
```

---

## 14. Distributed Tracing

**Description:** OpenTelemetry distributed tracing across microservices.

```mermaid
sequenceDiagram
    participant Client
    participant API1 as Order API
    participant Tracer1 as Activity Source
    participant API2 as Inventory API
    participant Tracer2 as Activity Source
    participant API3 as Payment API
    participant Tracer3 as Activity Source
    participant Collector as OTLP Collector
    participant Jaeger

    Client->>API1: POST /api/orders
    Note right of Client: traceparent header generated
    
    API1->>Tracer1: StartActivity("POST /api/orders")
    Tracer1->>Tracer1: Create root span
    Tracer1->>Tracer1: Extract trace context from headers
    Note right of Tracer1: TraceId: abc123<br/>SpanId: span001
    
    API1->>API1: Process order
    
    API1->>API2: POST /api/inventory/reserve
    Note right of API1: Inject traceparent header<br/>traceparent: 00-abc123-span002-01
    
    API2->>Tracer2: StartActivity("Reserve inventory")
    Tracer2->>Tracer2: Create child span
    Tracer2->>Tracer2: Extract parent context
    Note right of Tracer2: TraceId: abc123 (same)<br/>SpanId: span002<br/>ParentId: span001
    
    API2->>API2: Check stock
    API2->>API2: Reserve items
    
    API2->>Tracer2: StopActivity()
    Tracer2->>Tracer2: Set span attributes
    Note right of Tracer2: http.method: POST<br/>http.status_code: 200<br/>inventory.items: 3
    
    Tracer2->>Collector: Export span (OTLP)
    Collector->>Jaeger: Store span
    
    API2-->>API1: 200 OK {reserved: true}
    
    API1->>API3: POST /api/payment/charge
    Note right of API1: Propagate trace context
    
    API3->>Tracer3: StartActivity("Process payment")
    Tracer3->>Tracer3: Create child span
    Note right of Tracer3: TraceId: abc123<br/>SpanId: span003<br/>ParentId: span001
    
    API3->>API3: Call payment gateway
    
    alt Payment succeeded
        API3->>Tracer3: SetTag("payment.status", "success")
        API3->>Tracer3: SetTag("payment.amount", 99.99)
    else Payment failed
        API3->>Tracer3: SetTag("payment.status", "failed")
        API3->>Tracer3: SetStatus(StatusCode.Error)
        API3->>Tracer3: RecordException(exception)
    end
    
    API3->>Tracer3: StopActivity()
    Tracer3->>Collector: Export span (OTLP)
    Collector->>Jaeger: Store span
    
    API3-->>API1: 200 OK {charged: true}
    
    API1->>Tracer1: StopActivity()
    Tracer1->>Tracer1: Set span attributes
    Note right of Tracer1: http.method: POST<br/>http.route: /api/orders<br/>http.status_code: 201
    
    Tracer1->>Collector: Export span (OTLP)
    Collector->>Jaeger: Store span
    
    API1-->>Client: 201 Created
    
    Note over Collector,Jaeger: Trace visualization in Jaeger UI
    
    Jaeger->>Jaeger: Build trace tree
    Note right of Jaeger: POST /api/orders (2.5s)<br/>  └─ Reserve inventory (0.8s)<br/>  └─ Process payment (1.3s)
```

---

## 15. Multi-Tenancy Resolution

**Description:** Tenant resolution and data isolation in a multi-tenant application.

```mermaid
sequenceDiagram
    participant Client
    participant API as API Controller
    participant Middleware as Tenant Middleware
    participant Resolver as Tenant Resolver
    participant Context as Tenant Context
    participant DbContext
    participant QueryFilter as Global Query Filter
    participant DB as Database

    Client->>API: GET /api/orders
    Note right of Client: Header: X-Tenant-Id: tenant-abc
    
    API->>Middleware: Invoke(HttpContext)
    
    Middleware->>Resolver: ResolveAsync(HttpContext)
    
    Resolver->>Resolver: Try resolve from header
    Note right of Resolver: X-Tenant-Id header
    
    alt Tenant in header
        Resolver->>Resolver: Validate tenant
        Resolver-->>Middleware: TenantInfo
    else No tenant in header
        Resolver->>Resolver: Try resolve from subdomain
        Note right of Resolver: tenant-abc.example.com
        
        alt Tenant in subdomain
            Resolver-->>Middleware: TenantInfo
        else Try JWT claim
            Resolver->>Resolver: Extract from JWT claims
            Resolver-->>Middleware: TenantInfo or null
        end
    end
    
    alt Tenant resolved
        Middleware->>Context: SetCurrentTenant(tenantInfo)
        Context->>Context: Store in AsyncLocal
        
        Middleware->>Middleware: Continue pipeline
        
        Note over API,DB: Request processing with tenant context
        
        API->>DbContext: Query orders
        
        DbContext->>Context: GetCurrentTenantId()
        Context-->>DbContext: "tenant-abc"
        
        DbContext->>QueryFilter: Apply global filter
        Note right of QueryFilter: modelBuilder.Entity<Order>()<br/>.HasQueryFilter(o => <br/>  o.TenantId == currentTenantId)
        
        QueryFilter->>DB: SELECT * FROM Orders WHERE TenantId = 'tenant-abc'
        DB-->>QueryFilter: Order[] (filtered)
        QueryFilter-->>DbContext: Order[]
        DbContext-->>API: Order[]
        
        API-->>Client: 200 OK + orders
    else No tenant resolved
        Middleware-->>Client: 400 Bad Request "Missing tenant"
    end
    
    Note over Context: Tenant isolation guarantees
    
    Context->>Context: Tenant data isolated
    Note right of Context: - Queries auto-filtered<br/>- Inserts auto-set TenantId<br/>- Cross-tenant access blocked<br/>- Audit logs per tenant
```

---

## Diagram Usage Guide

### Rendering Diagrams

These diagrams use **Mermaid** syntax and can be rendered in:

1. **GitHub/GitLab** - Native support in markdown files
2. **VS Code** - Install "Markdown Preview Mermaid Support" extension
3. **Online** - Copy to https://mermaid.live/
4. **Documentation Sites** - Docusaurus, MkDocs, etc.

### Customization

Modify diagrams by:
- Adding/removing participants
- Changing interaction flows
- Adding notes for clarity
- Adjusting alternative (alt) and parallel (par) blocks

### Best Practices

1. **Keep it focused** - One main flow per diagram
2. **Add notes** - Explain complex logic inline
3. **Use alternatives** - Show success/failure paths
4. **Highlight key decisions** - Use colored notes or boxes
5. **Keep participants minimal** - Only essential actors

---

**Document Version:** 1.0  
**Last Updated:** November 18, 2025  
**Maintained By:** Nalam360 Platform Team
