# ProductAdminPortal - Quick Start Guide 🚀

This guide will get you up and running with ProductAdminPortal in 5 minutes using Docker Compose.

---

## Prerequisites

- **Docker Desktop** installed ([Download](https://www.docker.com/products/docker-desktop/))
- **Git** installed
- **8GB RAM** available
- **10GB disk space** available

---

## Step 1: Clone Repository

```bash
git clone <repository-url>
cd ProductAdminPortal
```

---

## Step 2: Configure Environment

```bash
# Copy environment template
cp .env.template .env

# Quick configuration (for development)
# Edit .env and set these minimum values:
POSTGRES_PASSWORD=devpassword123
JWT_SECRET=your-super-secret-development-key-min-32-chars
```

**macOS/Linux:**
```bash
# Auto-generate secure JWT secret
export JWT_SECRET=$(openssl rand -base64 32)
echo "JWT_SECRET=$JWT_SECRET" >> .env
echo "POSTGRES_PASSWORD=devpassword123" >> .env
```

**Windows (PowerShell):**
```powershell
# Generate random JWT secret
$jwt = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | % {[char]$_})
Add-Content .env "JWT_SECRET=$jwt"
Add-Content .env "POSTGRES_PASSWORD=devpassword123"
```

---

## Step 3: Start Services

```bash
# Start all services (backend, database, cache)
docker-compose up -d

# View startup logs
docker-compose logs -f

# Wait for "Application started" message
# Press Ctrl+C to exit logs
```

---

## Step 4: Verify Services

```bash
# Check all services are running
docker-compose ps

# Should show:
# - productadmin-postgres (healthy)
# - productadmin-redis (healthy)
# - productadmin-backend (healthy)
```

---

## Step 5: Access Application

Open your browser:

- **Backend API**: http://localhost:5000
- **API Documentation (Swagger)**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health

Test with curl:
```bash
# Health check
curl http://localhost:5000/health

# API endpoints (once backend is implemented)
curl http://localhost:5000/api/v1/products
```

---

## Optional: Development Tools

Start with database management UI:

```bash
# Start with pgAdmin and Redis Commander
docker-compose --profile tools up -d

# Access tools:
# - pgAdmin: http://localhost:5050
# - Redis Commander: http://localhost:8081
```

**pgAdmin Login:**
- Email: `admin@productadmin.com`
- Password: `admin123`

**Connect to PostgreSQL in pgAdmin:**
- Host: `postgres`
- Port: `5432`
- Database: `productadmin`
- Username: `postgres`
- Password: (value from .env POSTGRES_PASSWORD)

---

## Database Setup

The database is automatically initialized with:
1. ✅ Complete schema (19 tables)
2. ✅ Sample data (products, subscriptions, customers)
3. ✅ Test data for pricing calculations

**View sample data:**
```bash
# Connect to PostgreSQL
docker-compose exec postgres psql -U postgres -d productadmin

# Run queries:
SELECT * FROM products;
SELECT * FROM subscription_plans;
SELECT * FROM customers;

# Exit psql
\q
```

---

## Common Commands

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f postgres
```

### Restart Services
```bash
# Restart all
docker-compose restart

# Restart specific service
docker-compose restart backend
```

### Stop Services
```bash
# Stop (keeps data)
docker-compose stop

# Stop and remove containers (keeps volumes/data)
docker-compose down

# Stop and remove everything including data (⚠️ WARNING)
docker-compose down -v
```

### Database Operations
```bash
# Access PostgreSQL shell
docker-compose exec postgres psql -U postgres -d productadmin

# Run SQL file
docker-compose exec -T postgres psql -U postgres -d productadmin < your-script.sql

# Backup database
docker-compose exec postgres pg_dump -U postgres productadmin > backup.sql

# Restore database
docker-compose exec -T postgres psql -U postgres -d productadmin < backup.sql
```

### Redis Operations
```bash
# Access Redis CLI
docker-compose exec redis redis-cli

# Check Redis info
docker-compose exec redis redis-cli INFO

# Monitor Redis commands
docker-compose exec redis redis-cli MONITOR
```

---

## Troubleshooting

### Port Already in Use

If you see "port is already allocated" error:

```bash
# Check what's using the port
lsof -i :5000    # macOS/Linux
netstat -ano | findstr :5000  # Windows

# Option 1: Stop the conflicting service
# Option 2: Change port in docker-compose.yml
ports:
  - "5001:5000"  # Change 5000 to 5001
```

### Services Not Starting

```bash
# View detailed logs
docker-compose logs backend

# Check Docker resources
docker system df

# Clean up old containers/images
docker system prune -a
```

### Database Connection Errors

```bash
# Ensure PostgreSQL is healthy
docker-compose ps

# Check PostgreSQL logs
docker-compose logs postgres

# Verify connection string in backend
docker-compose exec backend env | grep Connection
```

### Out of Memory

```bash
# Check Docker memory limit
docker stats

# Increase Docker memory:
# Docker Desktop > Settings > Resources > Memory
# Set to at least 4GB
```

---

## Development Workflow

### 1. Make Code Changes

Edit files in your IDE (VS Code, Rider, etc.)

### 2. Rebuild Backend

```bash
# Rebuild and restart backend
docker-compose up -d --build backend

# View logs
docker-compose logs -f backend
```

### 3. Reset Database (if needed)

```bash
# Stop services
docker-compose down

# Remove database volume
docker volume rm productadminportal_postgres_data

# Start fresh
docker-compose up -d
```

### 4. Run Tests

```bash
# Backend tests (once test project is created)
docker-compose exec backend dotnet test

# Database tests
docker-compose exec postgres psql -U postgres -d productadmin -c "SELECT COUNT(*) FROM products;"
```

---

## Next Steps

### Backend Development
1. Review database schema: `Database/schema.sql`
2. Review seed data: `Database/seed.sql`
3. Create C# models matching schema
4. Implement services (ProductService, PricingService)
5. Create API controllers

### Frontend Development
1. Set up React app in `frontend/` directory
2. Configure API base URL: `http://localhost:5000/api/v1`
3. Build UI components
4. Integrate with backend

### Testing
1. Create unit tests: `tests/ProductAdminPortal.Tests/`
2. Create integration tests: `tests/ProductAdminPortal.IntegrationTests/`
3. Run tests in Docker: `docker-compose exec backend dotnet test`

---

## Useful Resources

- **README.md** - Complete project documentation
- **DEPLOYMENT_COMPLETE.md** - Production deployment guide
- **Documentation/API_SPECIFICATION.yaml** - Complete API reference
- **Documentation/PRICING_CALCULATION_COMPLETE.md** - Pricing engine guide
- **Database/schema.sql** - Complete database schema
- **SeedData/** - Sample data files

---

## Getting Help

### Check Logs
```bash
docker-compose logs -f
```

### Check Service Health
```bash
curl http://localhost:5000/health
docker-compose ps
```

### Review Configuration
```bash
cat .env
cat docker-compose.yml
```

### Common Issues
1. **Port conflicts**: Change ports in docker-compose.yml
2. **Out of memory**: Increase Docker memory limit
3. **Database errors**: Check postgres logs
4. **Redis errors**: Check redis logs

---

## Clean Restart

If you encounter issues, perform a clean restart:

```bash
# Stop everything
docker-compose down -v

# Remove images (optional)
docker-compose down --rmi all

# Clean Docker system
docker system prune -a

# Recreate .env
cp .env.template .env
# Edit .env with your settings

# Start fresh
docker-compose up -d

# Verify
docker-compose ps
docker-compose logs -f
```

---

**🎉 You're all set! The application is running and ready for development.**

Access Swagger UI at http://localhost:5000/swagger to explore the API (once backend is implemented).
