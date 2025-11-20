# Nalam360 Enterprise Platform - Integration Patterns Diagrams

This document contains diagrams for API integration patterns, message-driven integration, and inter-service communication.

**Format:** Mermaid  
**Date:** November 18, 2025

---

## Table of Contents

1. [REST API Gateway Pattern](#1-rest-api-gateway-pattern)
2. [GraphQL API Integration](#2-graphql-api-integration)
3. [Webhook Integration](#3-webhook-integration)
4. [Message Bus Integration](#4-message-bus-integration)
5. [Saga Pattern for Distributed Transactions](#5-saga-pattern-for-distributed-transactions)
6. [API Rate Limiting](#6-api-rate-limiting)
7. [API Versioning Strategies](#7-api-versioning-strategies)
8. [Service-to-Service Authentication](#8-service-to-service-authentication)

---

## 1. REST API Gateway Pattern

**Description:** Centralized API gateway for routing, authentication, and rate limiting.

```mermaid
sequenceDiagram
    participant Client
    participant Gateway as API Gateway
    participant AuthService as Auth Service
    participant Cache as Redis Cache
    participant OrderAPI as Order Service
    participant InventoryAPI as Inventory Service
    participant RateLimiter as Rate Limiter

    Client->>Gateway: POST /api/orders
    Note right of Client: Authorization: Bearer <token>

    Gateway->>RateLimiter: Check rate limit
    RateLimiter->>Cache: GET rate:client123
    Cache-->>RateLimiter: Request count: 45/100
    
    alt Rate limit exceeded
        RateLimiter-->>Gateway: 429 Too Many Requests
        Gateway-->>Client: 429 + Retry-After header
    end
    
    RateLimiter-->>Gateway: Allow request
    
    Gateway->>AuthService: Validate JWT
    AuthService->>AuthService: Verify signature
    AuthService->>AuthService: Check expiration
    AuthService->>Cache: Get cached user claims
    
    alt Invalid token
        AuthService-->>Gateway: 401 Unauthorized
        Gateway-->>Client: 401 Unauthorized
    end
    
    AuthService-->>Gateway: Claims + Permissions
    
    Gateway->>Gateway: Check permissions (orders.create)
    
    alt No permission
        Gateway-->>Client: 403 Forbidden
    end
    
    Gateway->>Gateway: Route to service
    Note right of Gateway: Route: /api/orders → OrderAPI
    
    Gateway->>Gateway: Add headers
    Note right of Gateway: X-User-Id, X-Tenant-Id, <br/>X-Correlation-Id
    
    Gateway->>OrderAPI: POST /orders (proxied)
    
    OrderAPI->>OrderAPI: Process request
    OrderAPI->>InventoryAPI: Check stock
    InventoryAPI-->>OrderAPI: Stock available
    OrderAPI-->>Gateway: 201 Created
    
    Gateway->>Gateway: Transform response
    Gateway->>Cache: Cache response (if GET)
    
    Gateway-->>Client: 201 Created + Location header
```

---

## 2. GraphQL API Integration

**Description:** GraphQL API with query batching and dataloader pattern.

```mermaid
sequenceDiagram
    participant Client
    participant GraphQL as GraphQL Server
    participant Resolver as Query Resolver
    participant DataLoader as DataLoader
    participant OrderAPI as Order Service
    participant CustomerAPI as Customer Service
    participant DB as Database

    Client->>GraphQL: POST /graphql
    Note right of Client: query { <br/>  order(id: "123") { <br/>    id, total <br/>    customer { name } <br/>    items { product { name } } <br/>  } <br/>}
    
    GraphQL->>GraphQL: Parse query
    GraphQL->>GraphQL: Validate schema
    GraphQL->>GraphQL: Build execution plan
    
    GraphQL->>Resolver: Resolve 'order' field
    Resolver->>DataLoader: Load order(123)
    DataLoader->>OrderAPI: GET /orders/123
    OrderAPI->>DB: SELECT * FROM Orders WHERE Id = 123
    DB-->>OrderAPI: Order data
    OrderAPI-->>DataLoader: Order
    DataLoader-->>Resolver: Order
    
    Resolver->>Resolver: Resolve 'customer' field
    Resolver->>DataLoader: Load customer(456)
    
    Note right of DataLoader: Batch customer requests
    DataLoader->>DataLoader: Collect customer IDs [456, ...]
    DataLoader->>DataLoader: Wait 10ms for batching
    
    DataLoader->>CustomerAPI: GET /customers?ids=456,789,...
    CustomerAPI->>DB: SELECT * FROM Customers WHERE Id IN (...)
    DB-->>CustomerAPI: Customer[]
    CustomerAPI-->>DataLoader: Customers
    
    DataLoader->>DataLoader: Cache results
    DataLoader-->>Resolver: Customer(456)
    
    Resolver->>Resolver: Resolve 'items' field
    Resolver->>DataLoader: Load order items
    DataLoader-->>Resolver: OrderItem[]
    
    loop For each item
        Resolver->>Resolver: Resolve 'product' field
        Resolver->>DataLoader: Load product (cached)
        DataLoader-->>Resolver: Product
    end
    
    Resolver-->>GraphQL: Complete order graph
    GraphQL-->>Client: JSON response
```

---

## 3. Webhook Integration

**Description:** Webhook registration, delivery, and retry mechanism.

```mermaid
sequenceDiagram
    participant System as Source System
    participant WebhookService as Webhook Service
    participant Queue as Message Queue
    participant Dispatcher as Webhook Dispatcher
    participant Subscriber as Subscriber API
    participant DB as Webhook DB

    Note over System,Subscriber: Phase 1: Webhook Registration
    
    Subscriber->>System: POST /webhooks/register
    Note right of Subscriber: { <br/>  url: "https://api.subscriber.com/webhook", <br/>  events: ["order.created", "order.shipped"], <br/>  secret: "abc123" <br/>}
    
    System->>DB: Save webhook subscription
    System->>System: Generate signing key
    System-->>Subscriber: 201 Created + webhookId
    
    Note over System,Subscriber: Phase 2: Event Occurs
    
    System->>System: Order created event
    System->>WebhookService: Trigger webhook
    
    WebhookService->>DB: Get matching subscriptions
    DB-->>WebhookService: Subscription[]
    
    loop For each subscription
        WebhookService->>WebhookService: Create webhook payload
        WebhookService->>WebhookService: Sign payload (HMAC-SHA256)
        Note right of WebhookService: signature = HMAC(secret, payload)
        
        WebhookService->>Queue: Enqueue webhook delivery
    end
    
    Note over System,Subscriber: Phase 3: Webhook Delivery
    
    Queue->>Dispatcher: Dequeue webhook
    
    Dispatcher->>Subscriber: POST /webhook
    Note right of Dispatcher: Headers: <br/>  X-Webhook-Signature <br/>  X-Webhook-Id <br/>  X-Webhook-Event
    
    alt Success
        Subscriber->>Subscriber: Verify signature
        Subscriber->>Subscriber: Process event
        Subscriber-->>Dispatcher: 200 OK
        
        Dispatcher->>DB: Mark as delivered
        Dispatcher-->>Queue: Ack message
    else Failure
        Subscriber-->>Dispatcher: 500 Server Error
        
        Dispatcher->>DB: Increment retry count
        
        alt Retry limit not reached
            Dispatcher->>Queue: Re-queue with backoff
            Note right of Dispatcher: Retry schedule: <br/>  1min, 5min, 15min, 1h, 6h
        else Max retries exceeded
            Dispatcher->>DB: Mark as failed
            Dispatcher->>System: Alert webhook failure
            Dispatcher-->>Queue: Ack message (discard)
        end
    end
```

---

## 4. Message Bus Integration

**Description:** Publish-subscribe messaging with topic routing.

```mermaid
graph TB
    subgraph "Publishers"
        OrderService[Order Service]
        PaymentService[Payment Service]
        InventoryService[Inventory Service]
    end
    
    subgraph "Message Broker"
        Exchange[Topic Exchange<br/>"events.exchange"]
        
        subgraph "Queues"
            OrderQueue[order.events]
            EmailQueue[email.notifications]
            AuditQueue[audit.logs]
            AnalyticsQueue[analytics.events]
        end
    end
    
    subgraph "Subscribers"
        EmailService[Email Service]
        AuditService[Audit Service]
        AnalyticsService[Analytics Service]
        WarehouseService[Warehouse Service]
    end
    
    OrderService -->|"order.created"| Exchange
    OrderService -->|"order.shipped"| Exchange
    PaymentService -->|"payment.completed"| Exchange
    InventoryService -->|"inventory.low"| Exchange
    
    Exchange -->|"order.*"| OrderQueue
    Exchange -->|"#"| EmailQueue
    Exchange -->|"#"| AuditQueue
    Exchange -->|"*.completed"| AnalyticsQueue
    
    OrderQueue --> WarehouseService
    EmailQueue --> EmailService
    AuditQueue --> AuditService
    AnalyticsQueue --> AnalyticsService
    
    style Exchange fill:#e1e1ff
    style OrderQueue fill:#ffe1e1
    style EmailQueue fill:#ffe1e1
    style AuditQueue fill:#ffe1e1
    style AnalyticsQueue fill:#ffe1e1
```

---

## 5. Saga Pattern for Distributed Transactions

**Description:** Orchestration saga for multi-service transactions with compensation.

```mermaid
sequenceDiagram
    participant Client
    participant Orchestrator as Saga Orchestrator
    participant OrderAPI as Order Service
    participant PaymentAPI as Payment Service
    participant InventoryAPI as Inventory Service
    participant ShippingAPI as Shipping Service
    participant SagaLog as Saga Log DB

    Client->>Orchestrator: Create order
    
    Orchestrator->>SagaLog: Start saga (orderId)
    SagaLog-->>Orchestrator: SagaId: saga-123
    
    Note over Orchestrator: Step 1: Create Order
    Orchestrator->>SagaLog: Log step: CreateOrder
    Orchestrator->>OrderAPI: Create order
    
    alt Order creation succeeds
        OrderAPI-->>Orchestrator: Order created
        Orchestrator->>SagaLog: Mark step complete
    else Order creation fails
        OrderAPI-->>Orchestrator: Error
        Orchestrator->>SagaLog: Mark saga failed
        Orchestrator-->>Client: 500 Error
    end
    
    Note over Orchestrator: Step 2: Reserve Inventory
    Orchestrator->>SagaLog: Log step: ReserveInventory
    Orchestrator->>InventoryAPI: Reserve items
    
    alt Inventory reserved
        InventoryAPI-->>Orchestrator: Reserved
        Orchestrator->>SagaLog: Mark step complete
    else Insufficient stock
        InventoryAPI-->>Orchestrator: Out of stock
        
        Note over Orchestrator: Begin compensation
        Orchestrator->>SagaLog: Start compensation
        
        Orchestrator->>OrderAPI: Cancel order (compensate)
        OrderAPI-->>Orchestrator: Cancelled
        
        Orchestrator->>SagaLog: Mark saga failed
        Orchestrator-->>Client: 400 Out of stock
    end
    
    Note over Orchestrator: Step 3: Process Payment
    Orchestrator->>SagaLog: Log step: ProcessPayment
    Orchestrator->>PaymentAPI: Charge payment
    
    alt Payment succeeds
        PaymentAPI-->>Orchestrator: Charged
        Orchestrator->>SagaLog: Mark step complete
    else Payment fails
        PaymentAPI-->>Orchestrator: Declined
        
        Note over Orchestrator: Compensate previous steps
        Orchestrator->>SagaLog: Start compensation
        
        Orchestrator->>InventoryAPI: Release reservation
        InventoryAPI-->>Orchestrator: Released
        
        Orchestrator->>OrderAPI: Cancel order
        OrderAPI-->>Orchestrator: Cancelled
        
        Orchestrator->>SagaLog: Mark saga failed
        Orchestrator-->>Client: 402 Payment failed
    end
    
    Note over Orchestrator: Step 4: Create Shipment
    Orchestrator->>SagaLog: Log step: CreateShipment
    Orchestrator->>ShippingAPI: Create shipment
    
    alt Shipment created
        ShippingAPI-->>Orchestrator: Shipment created
        Orchestrator->>SagaLog: Mark step complete
        Orchestrator->>SagaLog: Mark saga complete
        Orchestrator-->>Client: 201 Order created
    else Shipment fails
        ShippingAPI-->>Orchestrator: Error
        
        Orchestrator->>PaymentAPI: Refund payment
        Orchestrator->>InventoryAPI: Release reservation
        Orchestrator->>OrderAPI: Cancel order
        
        Orchestrator->>SagaLog: Mark saga failed
        Orchestrator-->>Client: 500 Error
    end
```

---

## 6. API Rate Limiting

**Description:** Token bucket rate limiting with Redis.

```mermaid
sequenceDiagram
    participant Client
    participant API as API Endpoint
    participant Limiter as Rate Limiter
    participant Redis
    participant Config as Rate Config

    Client->>API: GET /api/data
    
    API->>Limiter: CheckRateLimit(clientId, endpoint)
    
    Limiter->>Config: Get rate limit rules
    Config-->>Limiter: 100 requests / 1 minute
    
    Limiter->>Limiter: Build Redis key
    Note right of Limiter: rate:client123:/api/data
    
    Limiter->>Redis: MULTI (start transaction)
    Limiter->>Redis: GET key
    Redis-->>Limiter: current count or null
    
    alt First request (key doesn't exist)
        Limiter->>Redis: SET key 1
        Limiter->>Redis: EXPIRE key 60
        Limiter->>Redis: EXEC
        Redis-->>Limiter: Success
        
        Limiter-->>API: Allow (1/100)
        API->>API: Process request
        API-->>Client: 200 OK
        Note right of Client: Headers: <br/>  X-RateLimit-Limit: 100 <br/>  X-RateLimit-Remaining: 99 <br/>  X-RateLimit-Reset: 1637012345
    
    else Subsequent request
        Limiter->>Redis: INCR key
        Limiter->>Redis: TTL key
        Limiter->>Redis: EXEC
        Redis-->>Limiter: count: 45, ttl: 38
        
        alt Under limit
            Limiter-->>API: Allow (45/100)
            API->>API: Process request
            API-->>Client: 200 OK
            Note right of Client: X-RateLimit-Remaining: 55
        
        else Limit exceeded
            Limiter-->>API: Deny (101/100)
            API-->>Client: 429 Too Many Requests
            Note right of Client: Headers: <br/>  X-RateLimit-Limit: 100 <br/>  X-RateLimit-Remaining: 0 <br/>  X-RateLimit-Reset: 1637012345 <br/>  Retry-After: 38
        end
    end
```

---

## 7. API Versioning Strategies

**Description:** API versioning approaches for backward compatibility.

```mermaid
graph TB
    subgraph "Client Applications"
        OldClient[Legacy App<br/>v1 API]
        NewClient[Modern App<br/>v2 API]
        MobileApp[Mobile App<br/>v2 API]
    end
    
    subgraph "API Gateway"
        Gateway[API Gateway]
        Router[Version Router]
    end
    
    subgraph "API Versions"
        V1API[API v1<br/>/api/v1/orders]
        V2API[API v2<br/>/api/v2/orders]
        
        subgraph "v1 Implementation"
            V1Controller[Orders Controller v1]
            V1Service[Order Service v1]
            V1Adapter[V1 to V2 Adapter]
        end
        
        subgraph "v2 Implementation"
            V2Controller[Orders Controller v2]
            V2Service[Order Service v2]
        end
    end
    
    subgraph "Domain Layer"
        DomainService[Order Domain Service]
        Repository[Order Repository]
    end
    
    OldClient -->|"GET /api/v1/orders"| Gateway
    NewClient -->|"GET /api/v2/orders"| Gateway
    MobileApp -->|"Accept: application/vnd.api.v2+json"| Gateway
    
    Gateway --> Router
    
    Router -->|"URL: v1"| V1API
    Router -->|"URL: v2"| V2API
    Router -->|"Header: v2"| V2API
    
    V1API --> V1Controller
    V2API --> V2Controller
    
    V1Controller --> V1Service
    V1Service --> V1Adapter
    V1Adapter --> DomainService
    
    V2Controller --> V2Service
    V2Service --> DomainService
    
    DomainService --> Repository
    
    style V1API fill:#ffe1e1
    style V2API fill:#e1ffe1
    style DomainService fill:#e1e1ff
```

---

## 8. Service-to-Service Authentication

**Description:** OAuth2 client credentials flow for machine-to-machine auth.

```mermaid
sequenceDiagram
    participant ServiceA as Order Service
    participant Cache as Token Cache
    participant AuthServer as Auth Server
    participant ServiceB as Inventory Service

    Note over ServiceA: Need to call Inventory API
    
    ServiceA->>Cache: Get cached token (serviceA:inventory)
    
    alt Token exists and valid
        Cache-->>ServiceA: Access token
    else Token expired or missing
        Cache-->>ServiceA: null
        
        ServiceA->>AuthServer: POST /oauth/token
        Note right of ServiceA: grant_type: client_credentials <br/>client_id: order-service <br/>client_secret: ****** <br/>scope: inventory.read inventory.write
        
        AuthServer->>AuthServer: Validate client credentials
        AuthServer->>AuthServer: Check scope permissions
        
        alt Valid credentials
            AuthServer->>AuthServer: Generate JWT
            Note right of AuthServer: iss: auth-server <br/>sub: order-service <br/>aud: inventory-service <br/>scope: inventory.* <br/>exp: now + 1h
            
            AuthServer-->>ServiceA: {"access_token": "eyJ...", "expires_in": 3600}
            
            ServiceA->>Cache: Cache token (55min TTL)
            Cache-->>ServiceA: Cached
        else Invalid credentials
            AuthServer-->>ServiceA: 401 Unauthorized
        end
    end
    
    ServiceA->>ServiceB: GET /api/inventory/check
    Note right of ServiceA: Authorization: Bearer eyJ...
    
    ServiceB->>ServiceB: Extract JWT
    ServiceB->>ServiceB: Verify signature (public key)
    ServiceB->>ServiceB: Check expiration
    ServiceB->>ServiceB: Validate audience
    ServiceB->>ServiceB: Check scope (inventory.read)
    
    alt Valid token
        ServiceB->>ServiceB: Process request
        ServiceB-->>ServiceA: 200 OK + inventory data
    else Invalid token
        ServiceB-->>ServiceA: 401 Unauthorized
    else Insufficient scope
        ServiceB-->>ServiceA: 403 Forbidden
    end
```

---

## Integration Best Practices

### 1. REST API Design
- Use HTTP methods correctly (GET, POST, PUT, DELETE)
- Return appropriate status codes (2xx, 4xx, 5xx)
- Include pagination for collections
- Support filtering, sorting, field selection
- Version APIs from day one

### 2. GraphQL Design
- Use DataLoader to prevent N+1 queries
- Implement query complexity limits
- Set reasonable timeout and depth limits
- Provide clear error messages
- Use subscriptions for real-time updates

### 3. Webhooks
- Always sign payloads (HMAC-SHA256)
- Implement exponential backoff retry
- Set reasonable timeout (10-30s)
- Support webhook testing endpoints
- Log all delivery attempts

### 4. Messaging
- Use idempotency keys
- Implement dead letter queues
- Set appropriate message TTL
- Use message deduplication
- Monitor queue depth

### 5. Saga Pattern
- Log every saga step
- Design compensating transactions
- Handle partial failures gracefully
- Set step timeouts
- Implement saga recovery

### 6. Rate Limiting
- Use distributed rate limiting (Redis)
- Implement per-user and per-IP limits
- Return clear rate limit headers
- Provide upgrade paths for limits
- Monitor rate limit violations

### 7. Versioning
- Support multiple versions simultaneously
- Deprecate old versions gradually (6-12 months)
- Document breaking changes clearly
- Use semantic versioning
- Provide migration guides

### 8. Service Auth
- Use short-lived tokens (1 hour)
- Cache tokens appropriately
- Rotate client secrets regularly
- Use separate credentials per service
- Audit all service-to-service calls

---

**Document Version:** 1.0  
**Last Updated:** November 18, 2025  
**Maintained By:** Nalam360 Platform Team
