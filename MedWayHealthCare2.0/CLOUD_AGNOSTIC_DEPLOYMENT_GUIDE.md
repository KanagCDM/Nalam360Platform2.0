# MedWayHealthCare 2.0 - Cloud-Agnostic Deployment Guide

**Last Updated:** November 23, 2025  
**Architecture:** ✅ **100% Cloud-Agnostic** - Zero Hard Dependencies on Any Cloud Provider

---

## 🎯 Deployment Philosophy

MedWayHealthCare 2.0 is designed with **deployment flexibility** as a core principle:

- ✅ **On-Premise:** Run as monolith on your own servers
- ✅ **Azure App Service:** Deploy to Azure with zero code changes
- ✅ **Azure Container Services (AKS):** Run in Kubernetes on Azure
- ✅ **Any Cloud Provider:** AWS, GCP, DigitalOcean, etc.
- ✅ **Hybrid:** Mix on-premise and cloud components

**Key Principle:** Cloud migration is **plug-and-play** based on configuration, not code changes.

---

## 🏗️ Cloud-Agnostic Architecture

### Infrastructure Components

| Component | On-Premise | Azure | AWS | GCP | Notes |
|-----------|------------|-------|-----|-----|-------|
| **Database** | PostgreSQL 12+ | Azure PostgreSQL | RDS PostgreSQL | Cloud SQL | Same connection string pattern |
| **Caching** | Redis 6+ | Azure Cache for Redis | ElastiCache | Memorystore | Same Redis protocol |
| **Messaging** | Kafka / Confluent | Azure Event Hubs (Kafka API) | MSK (Kafka) | Kafka on GKE | Kafka protocol everywhere |
| **App Server** | Kestrel on Linux/Windows | Azure App Service | Elastic Beanstalk | App Engine | ASP.NET Core runs anywhere |
| **Container** | Docker + Docker Compose | Azure Container Instances/AKS | ECS/EKS | GKE | Standard Docker containers |
| **File Storage** | Local/NFS | Azure Blob Storage | S3 | Cloud Storage | Abstracted via interfaces |
| **Identity** | Local IdentityServer | Azure AD (optional) | Cognito (optional) | Identity Platform (optional) | OAuth2/OIDC standard |

---

## 📦 Technology Stack (100% Cloud-Agnostic)

### Primary Technologies

```yaml
Runtime: .NET 10.0 (cross-platform: Linux, Windows, macOS)
Database: PostgreSQL 12+ (open-source, cloud-agnostic)
Caching: Redis 6+ (open-source, cloud-agnostic)
Messaging: Apache Kafka 3.x+ (open-source, cloud-agnostic)
Web Server: Kestrel (built-in ASP.NET Core)
Containerization: Docker (industry standard)
Orchestration: Docker Compose (on-premise) / Kubernetes (cloud)
```

### Why These Choices?

1. **PostgreSQL** - Open-source, enterprise-grade, runs everywhere (on-premise, all clouds)
2. **Redis** - Industry standard caching, same protocol everywhere
3. **Kafka** - De facto standard for event streaming, cloud-agnostic
4. **.NET 10** - Cross-platform, high-performance, free, runs on Linux/Windows
5. **Docker** - Standard containerization, works on-premise and all clouds

---

## 🚀 Deployment Scenarios

### Scenario 1: On-Premise Monolith (Full Control)

**Use Case:** Hospital wants full control, data sovereignty, existing infrastructure

**Architecture:**
```
┌─────────────────────────────────────────────┐
│  On-Premise Data Center                     │
│                                              │
│  ┌──────────────────────────────────────┐  │
│  │ MedWayHealthCare 2.0 Monolith        │  │
│  │ (.NET 10 on Linux/Windows Server)    │  │
│  │                                       │  │
│  │  - Web API (Kestrel)                 │  │
│  │  - Blazor Server UI                  │  │
│  │  - Background Jobs                   │  │
│  └──────────────────────────────────────┘  │
│                    │                         │
│  ┌─────────────────┼─────────────────────┐  │
│  │                 │                     │  │
│  ▼                 ▼                     ▼  │
│  PostgreSQL     Redis Cache          Kafka  │
│  (Primary DB)   (Distributed)      (Events) │
└─────────────────────────────────────────────┘
```

**Deployment:**
```bash
# Option 1: Docker Compose (Recommended)
cd MedWayHealthCare2.0/deployment/on-premise
docker-compose up -d

# Option 2: Native Installation
dotnet publish -c Release
./deploy-onpremise.sh
```

**Configuration:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=medway;Username=medway_app;Password=***"
  },
  "Caching": {
    "Redis": {
      "ConnectionString": "localhost:6379"
    }
  },
  "Messaging": {
    "Provider": "Kafka",
    "Kafka": {
      "BootstrapServers": "localhost:9092",
      "GroupId": "medway-healthcare"
    }
  }
}
```

**Pros:**
- ✅ Full control over infrastructure
- ✅ Data stays on-premise
- ✅ No cloud costs
- ✅ Predictable performance

**Cons:**
- ⚠️ Requires IT team for maintenance
- ⚠️ Manual scaling
- ⚠️ Upfront hardware costs

---

### Scenario 2: Azure App Service (Managed PaaS)

**Use Case:** Hospital wants cloud benefits without managing infrastructure

**Architecture:**
```
┌─────────────────────────────────────────────┐
│  Microsoft Azure                             │
│                                              │
│  ┌──────────────────────────────────────┐  │
│  │ Azure App Service (Linux)            │  │
│  │ - MedWayHealthCare 2.0               │  │
│  │ - Auto-scaling enabled               │  │
│  └──────────────────────────────────────┘  │
│                    │                         │
│  ┌─────────────────┼─────────────────────┐  │
│  │                 │                     │  │
│  ▼                 ▼                     ▼  │
│  Azure Database  Azure Cache        Azure   │
│  for PostgreSQL  for Redis      Event Hubs  │
│  (Managed)      (Managed)      (Kafka API)  │
└─────────────────────────────────────────────┘
```

**Deployment:**
```bash
# Azure CLI deployment
az login
az webapp create --resource-group medway-rg \
  --plan medway-plan \
  --name medway-healthcare \
  --runtime "DOTNETCORE:10.0"

# Deploy code
az webapp deployment source config-zip \
  --resource-group medway-rg \
  --name medway-healthcare \
  --src medway-release.zip
```

**Configuration:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=medway-db.postgres.database.azure.com;Database=medway;Username=***;Password=***"
  },
  "Caching": {
    "Redis": {
      "ConnectionString": "medway-cache.redis.cache.windows.net:6380,ssl=True,password=***"
    }
  },
  "Messaging": {
    "Provider": "Kafka",
    "Kafka": {
      "BootstrapServers": "medway-eventhub.servicebus.windows.net:9093",
      "SaslMechanism": "Plain",
      "SecurityProtocol": "SaslSsl",
      "SaslUsername": "$ConnectionString",
      "SaslPassword": "Endpoint=sb://***"
    }
  }
}
```

**Note:** Azure Event Hubs supports Kafka protocol - **no code changes required**, only connection string!

**Pros:**
- ✅ Zero infrastructure management
- ✅ Auto-scaling
- ✅ Built-in monitoring
- ✅ High availability

**Cons:**
- ⚠️ Cloud costs (pay-as-you-go)
- ⚠️ Data in cloud

---

### Scenario 3: Azure Container Services (Kubernetes)

**Use Case:** Large hospital network, microservices, multi-region

**Architecture:**
```
┌──────────────────────────────────────────────────┐
│  Azure Kubernetes Service (AKS)                  │
│                                                   │
│  ┌──────────────────────────────────────────┐   │
│  │ Kubernetes Cluster                        │   │
│  │                                           │   │
│  │  ┌───────────┐  ┌───────────┐           │   │
│  │  │ Web API   │  │ Blazor UI │           │   │
│  │  │ (3 pods)  │  │ (2 pods)  │           │   │
│  │  └───────────┘  └───────────┘           │   │
│  │                                           │   │
│  │  ┌───────────┐  ┌───────────┐           │   │
│  │  │ Background│  │ Event     │           │   │
│  │  │ Jobs      │  │ Consumers │           │   │
│  │  └───────────┘  └───────────┘           │   │
│  └──────────────────────────────────────────┘   │
│                      │                           │
│  ┌───────────────────┼───────────────────────┐  │
│  │                   │                       │  │
│  ▼                   ▼                       ▼  │
│  Azure Database   Azure Cache        Confluent  │
│  for PostgreSQL   for Redis         Kafka Cloud│
│  (Managed)       (Managed)          (Managed)   │
└──────────────────────────────────────────────────┘
```

**Deployment:**
```bash
# Create AKS cluster
az aks create --resource-group medway-rg \
  --name medway-aks \
  --node-count 3 \
  --enable-addons monitoring

# Deploy using Helm
helm install medway-healthcare ./charts/medway \
  --values values-production.yaml
```

**Configuration:**
```yaml
# Kubernetes ConfigMap
apiVersion: v1
kind: ConfigMap
metadata:
  name: medway-config
data:
  ConnectionStrings__DefaultConnection: "Host=medway-db.postgres.database.azure.com;..."
  Caching__Redis__ConnectionString: "medway-cache.redis.cache.windows.net:6380,ssl=True"
  Messaging__Provider: "Kafka"
  Messaging__Kafka__BootstrapServers: "pkc-xxxxx.centralus.azure.confluent.cloud:9092"
```

**Pros:**
- ✅ Microservices-ready
- ✅ Auto-scaling at pod level
- ✅ Zero-downtime deployments
- ✅ Multi-region support

**Cons:**
- ⚠️ Higher complexity
- ⚠️ Requires Kubernetes expertise
- ⚠️ Higher costs

---

### Scenario 4: Hybrid (On-Premise + Cloud)

**Use Case:** Sensitive data on-premise, processing in cloud

**Architecture:**
```
┌─────────────────────────┐    ┌─────────────────────────┐
│  On-Premise             │    │  Microsoft Azure        │
│                         │    │                         │
│  ┌──────────────────┐  │    │  ┌──────────────────┐  │
│  │ PostgreSQL       │  │    │  │ Azure App Service│  │
│  │ (PHI/PII Data)   │  │◄───┼──│ MedWayHealthCare │  │
│  └──────────────────┘  │    │  └──────────────────┘  │
│                         │    │           │            │
│  ┌──────────────────┐  │    │  ┌────────▼────────┐  │
│  │ Kafka            │◄─┼────┼──│ Azure Event Hubs│  │
│  │ (Internal Events)│  │    │  │ (External Events)│  │
│  └──────────────────┘  │    │  └─────────────────┘  │
└─────────────────────────┘    └─────────────────────────┘
         VPN Tunnel / ExpressRoute
```

**Configuration:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=onprem-db.hospital.local;...",
    "ReportingConnection": "Host=reporting-db.postgres.database.azure.com;..."
  },
  "Messaging": {
    "Provider": "Kafka",
    "Kafka": {
      "BootstrapServers": "onprem-kafka.hospital.local:9092,azure-eventhub.servicebus.windows.net:9093"
    }
  }
}
```

---

## 📝 Configuration Guide

### Application Settings Structure

```json
{
  "DeploymentMode": "OnPremise | AzureAppService | Kubernetes",
  
  "ConnectionStrings": {
    "DefaultConnection": "PostgreSQL connection string"
  },
  
  "Caching": {
    "Provider": "Redis",
    "Redis": {
      "ConnectionString": "localhost:6379 | azure-cache.redis.cache.windows.net:6380,ssl=True"
    }
  },
  
  "Messaging": {
    "Provider": "Kafka",
    "Kafka": {
      "BootstrapServers": "localhost:9092",
      "GroupId": "medway-healthcare",
      "EnableAutoCommit": false,
      "AutoOffsetReset": "Earliest",
      
      // Optional: For cloud Kafka (Azure Event Hubs, Confluent Cloud, AWS MSK)
      "SaslMechanism": "Plain",
      "SecurityProtocol": "SaslSsl",
      "SaslUsername": "$ConnectionString",
      "SaslPassword": "***"
    }
  },
  
  "Storage": {
    "Provider": "Local | AzureBlob | S3",
    "Local": {
      "RootPath": "/var/medway/files"
    },
    "AzureBlob": {
      "ConnectionString": "***",
      "ContainerName": "medway-files"
    },
    "S3": {
      "AccessKey": "***",
      "SecretKey": "***",
      "BucketName": "medway-files",
      "Region": "us-east-1"
    }
  }
}
```

---

## 🔄 Migration Path: On-Premise → Cloud

### Step 1: Run On-Premise (Current State)
- PostgreSQL on local server
- Redis on local server
- Kafka on local server

### Step 2: Hybrid (Database to Cloud)
- Move database to Azure PostgreSQL (or keep on-premise)
- Keep Redis and Kafka on-premise
- **Configuration change only - no code changes**

### Step 3: Hybrid (Cache to Cloud)
- Move Redis to Azure Cache for Redis
- Keep Kafka on-premise or switch to Azure Event Hubs (Kafka protocol)
- **Configuration change only - no code changes**

### Step 4: Full Cloud
- Application on Azure App Service / AKS
- PostgreSQL on Azure
- Redis on Azure
- Kafka on Azure Event Hubs or Confluent Cloud
- **Configuration change only - no code changes**

---

## 🐳 Docker Deployment

### On-Premise Docker Compose

```yaml
# docker-compose.yml
version: '3.8'

services:
  # Application
  medway-api:
    image: medway/healthcare-api:latest
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=medway;Username=medway;Password=***
      - Caching__Redis__ConnectionString=redis:6379
      - Messaging__Kafka__BootstrapServers=kafka:9092
    depends_on:
      - postgres
      - redis
      - kafka
  
  # Database
  postgres:
    image: postgres:16
    volumes:
      - postgres_data:/var/lib/postgresql/data
    environment:
      - POSTGRES_DB=medway
      - POSTGRES_USER=medway
      - POSTGRES_PASSWORD=***
    ports:
      - "5432:5432"
  
  # Cache
  redis:
    image: redis:7-alpine
    volumes:
      - redis_data:/data
    ports:
      - "6379:6379"
  
  # Messaging
  zookeeper:
    image: confluentinc/cp-zookeeper:7.5.0
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181
      ZOOKEEPER_TICK_TIME: 2000
  
  kafka:
    image: confluentinc/cp-kafka:7.5.0
    depends_on:
      - zookeeper
    ports:
      - "9092:9092"
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:9092
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1

volumes:
  postgres_data:
  redis_data:
```

**Deployment:**
```bash
docker-compose up -d
```

---

## ☁️ Cloud Provider Equivalents

### Messaging: Kafka Everywhere

| Environment | Implementation | Connection String |
|-------------|---------------|-------------------|
| **On-Premise** | Apache Kafka 3.x | `localhost:9092` |
| **Azure** | Azure Event Hubs (Kafka API) | `namespace.servicebus.windows.net:9093` |
| **AWS** | Amazon MSK | `b-1.cluster.kafka.us-east-1.amazonaws.com:9092` |
| **GCP** | Confluent Cloud on GCP | `pkc-xxxxx.us-central1.gcp.confluent.cloud:9092` |
| **Confluent Cloud** | Managed Kafka | `pkc-xxxxx.centralus.azure.confluent.cloud:9092` |

**All use the same Kafka protocol - only connection string changes!**

---

## 🔧 Infrastructure Setup

### On-Premise Setup

```bash
# Install PostgreSQL 16
sudo apt install postgresql-16

# Install Redis 7
sudo apt install redis-server

# Install Kafka (via Docker or native)
docker run -d --name kafka \
  -p 9092:9092 \
  confluentinc/cp-kafka:7.5.0

# Deploy application
dotnet publish -c Release -o /var/www/medway
sudo systemctl start medway
```

### Azure Setup

```bash
# Create resource group
az group create --name medway-rg --location centralus

# Create PostgreSQL
az postgres flexible-server create \
  --resource-group medway-rg \
  --name medway-db \
  --location centralus \
  --admin-user medwayadmin \
  --admin-password *** \
  --sku-name Standard_D2ds_v4

# Create Redis
az redis create \
  --resource-group medway-rg \
  --name medway-cache \
  --location centralus \
  --sku Standard \
  --vm-size c1

# Create Event Hubs (Kafka compatible)
az eventhubs namespace create \
  --resource-group medway-rg \
  --name medway-events \
  --location centralus \
  --sku Standard \
  --enable-kafka true

# Create App Service
az webapp create \
  --resource-group medway-rg \
  --plan medway-plan \
  --name medway-healthcare \
  --runtime "DOTNETCORE:10.0"
```

---

## 📊 Cost Comparison

### On-Premise (5 Years)

| Component | Cost |
|-----------|------|
| Servers (3x) | $15,000 |
| PostgreSQL License | $0 (open-source) |
| Redis License | $0 (open-source) |
| Kafka License | $0 (open-source) |
| Maintenance (5 years) | $50,000 |
| **Total** | **$65,000** |

### Azure (5 Years)

| Component | Monthly | 5 Years |
|-----------|---------|---------|
| App Service (Standard S1) | $75 | $4,500 |
| PostgreSQL (General Purpose 2 vCores) | $120 | $7,200 |
| Redis (Standard C1) | $75 | $4,500 |
| Event Hubs (Standard) | $100 | $6,000 |
| **Total** | **$370/mo** | **$22,200** |

**Result:** Cloud is cheaper for smaller deployments, on-premise for larger ones.

---

## ✅ Verification Checklist

### Cloud-Agnostic Checklist

- [x] **No Azure-specific SDKs** in core application code
- [x] **Standard protocols only** (PostgreSQL, Redis, Kafka)
- [x] **Configuration-based switching** between providers
- [x] **Docker containers** for portability
- [x] **Standard .NET** (no Azure-specific runtime dependencies)
- [x] **Open-source stack** (PostgreSQL, Redis, Kafka)
- [x] **Plug-and-play migration** via configuration files only

### Deployment Readiness

- [ ] Test on-premise deployment with Docker Compose
- [ ] Test Azure App Service deployment
- [ ] Test Azure Container deployment (AKS)
- [ ] Verify connection strings work across environments
- [ ] Load test on each platform
- [ ] Document migration runbook

---

## 📞 Support

For deployment assistance:
- On-Premise: See `docs/deployment/on-premise/README.md`
- Azure: See `docs/deployment/azure/README.md`
- Kubernetes: See `docs/deployment/kubernetes/README.md`
- Docker: See `docker-compose.yml` examples

---

**Remember:** Cloud migration should be a **configuration change**, not a **code rewrite**. MedWayHealthCare 2.0 is designed for this flexibility from day one.
