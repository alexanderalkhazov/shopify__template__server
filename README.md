# Backend Test API - Entity Framework Core Template

A clean and modern .NET 8 API template demonstrating Entity Framework Core with PostgreSQL, Repository Pattern, Unit of Work implementation, Redis caching, and Hangfire background jobs. This template provides a solid foundatio- `entities:category:{category}` - Entities by category
- `entities:status:{status}` - Entities by status
- `entities:search:{term}` - Search results
- `entities:priority:{min}-{max}` - Priority range results

## 🔄 Hangfire Background Jobs

### Job Service Interface
```csharp
public interface IJobService
{
    // Fire-and-forget jobs
    string EnqueueJob<T>(Expression<Func<T, Task>> methodCall);
    
    // Delayed jobs
    string ScheduleJob<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);
    
    // Recurring jobs
    void AddOrUpdateRecurringJob<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression);
    
    // Job management
    bool DeleteJob(string jobId);
    void RemoveRecurringJob(string jobId);
}
```

### Job Types and Examples

#### 1. Fire-and-forget Jobs
Execute immediately in the background:
```bash
# Cache warming
POST /api/v1/jobs/enqueue/cache-warm

# Data processing
POST /api/v1/jobs/enqueue/data-processing?batchSize=100
```

#### 2. Delayed Jobs
Execute after a specified delay:
```bash
# Schedule notification in 30 minutes
POST /api/v1/jobs/schedule/notification
Content-Type: application/json
{
  "message": "Hello World",
  "recipient": "user123", 
  "delayMinutes": 30
}
```

#### 3. Recurring Jobs
Execute on a schedule using cron expressions:
```bash
# Create recurring cache cleanup (every 6 hours)
POST /api/v1/jobs/recurring/create
Content-Type: application/json
{
  "jobId": "cache-cleanup-hourly",
  "cronExpression": "0 */6 * * *",
  "jobType": "cache-cleanup"
}
```

### Pre-configured Recurring Jobs
The template includes these recurring jobs by default:

1. **Cache Cleanup** (`cache-cleanup`)
   - **Schedule**: Every 6 hours (`0 */6 * * *`)
   - **Purpose**: Remove expired cache entries and temporary data

2. **System Health Check** (`system-health-check`)
   - **Schedule**: Every 5 minutes (`*/5 * * * *`)
   - **Purpose**: Monitor system health and cache reports

### Common Cron Expressions
```
*/5 * * * *     Every 5 minutes
0 * * * *       Every hour
0 */6 * * *     Every 6 hours
0 0 * * *       Daily at midnight
0 0 * * 0       Weekly (Sunday at midnight)
0 0 1 * *       Monthly (1st day at midnight)
```

### Hangfire Dashboard Features
Access the dashboard at `/hangfire` to:
- **Monitor Jobs**: View running, completed, and failed jobs
- **Job History**: See detailed execution logs and timings
- **Recurring Jobs**: Manage scheduled jobs and their next run times
- **Server Stats**: Monitor worker processes and server health
- **Real-time Updates**: Live updates of job status and progress

### Job Queues
The system uses multiple queues for job prioritization:
- **critical**: High-priority jobs (processed first)
- **default**: Standard jobs (normal processing)
- **background**: Low-priority jobs (processed when resources available)

### Error Handling and Retries
- **Automatic Retries**: Failed jobs are retried up to 10 times
- **Exponential Backoff**: Retry delays increase progressively
- **Dead Letter Queue**: Permanently failed jobs for manual review
- **Detailed Logging**: All job executions logged with timestamps and errors

## 🛒 Comprehensive Shopify App Template

Complete Shopify app template with OAuth authentication, webhook management, and API integration.

### Features
- **OAuth 2.0 Flow**: Secure app installation and authorization
- **Scope Management**: Configurable OAuth scopes with validation
- **Webhook Handling**: Automatic webhook creation and processing
- **Shop Management**: Store and manage multiple shop installations
- **API Integration**: Full Shopify REST API support
- **Background Jobs**: Webhook processing with Hangfire
- **Discord Notifications**: Real-time alerts for shop events

### Configuration
Configure your Shopify app in `appsettings.json`:
```json
{
  "Shopify": {
    "ClientId": "your-shopify-app-client-id",
    "ClientSecret": "your-shopify-app-client-secret",
    "Scopes": "read_products,write_products,read_orders,write_orders,read_customers",
    "WebhookSecret": "your-webhook-secret",
    "AppUrl": "https://your-app-domain.com",
    "RedirectUrl": "https://your-app-domain.com/auth/callback",
    "ApiVersion": "2024-10",
    "EnableWebhooks": true,
    "RequiredWebhooks": [
      "orders/create",
      "orders/updated", 
      "products/create",
      "app/uninstalled"
    ]
  }
}
```

### OAuth & Installation Flow

#### 1. Initiate Installation
```bash
# Generate OAuth URL for shop installation
GET /api/shopify/auth?shop=your-shop-name&state=optional-state

# Response:
{
  "authUrl": "https://your-shop.myshopify.com/admin/oauth/authorize?...",
  "shopDomain": "your-shop.myshopify.com",
  "message": "Redirect user to this URL to begin OAuth flow"
}
```

#### 2. Handle OAuth Callback
```bash
# OAuth callback (automatically handled)
GET /api/shopify/auth/callback?shop=your-shop.myshopify.com&code=auth-code

# Response:
{
  "success": true,
  "shop": {
    "domain": "your-shop.myshopify.com",
    "name": "Your Shop",
    "scopes": ["read_products", "write_products"],
    "webhooksConfigured": true
  }
}
```

### Shop Management

#### Get All Installed Shops
```bash
GET /api/shopify/shops

# Response:
{
  "shops": [
    {
      "id": 1,
      "domain": "shop1.myshopify.com",
      "name": "Shop 1",
      "email": "owner@shop1.com",
      "scopes": ["read_products", "write_products"],
      "webhooksConfigured": true,
      "installedAt": "2024-01-01T00:00:00Z",
      "lastActivity": "2024-01-02T10:30:00Z"
    }
  ],
  "count": 1
}
```

### Webhook Management

Webhooks are automatically created during installation. Supported webhook topics:

- **Order Events**: `orders/create`, `orders/updated`, `orders/paid`, `orders/cancelled`
- **Product Events**: `products/create`, `products/update`
- **App Events**: `app/uninstalled`

#### Manual Webhook Setup
```bash
# Setup webhooks for a specific shop
POST /api/shopify/shops/{shopDomain}/webhooks/setup

# Response:
{
  "success": true,
  "message": "Webhooks configured successfully"
}
```

#### Webhook Processing
Webhooks are automatically processed and trigger:
- Discord notifications
- Database updates
- Background job processing
- Shop activity tracking

### API Integration

#### Get Shop Products
```bash
GET /api/shopify/shops/{shopDomain}/products?limit=50

# Response:
{
  "products": [
    {
      "id": 123456789,
      "title": "Sample Product",
      "handle": "sample-product",
      "product_type": "Physical",
      "vendor": "Your Brand",
      "status": "active",
      "variants": [...]
    }
  ],
  "count": 1
}
```

#### Get Shop Orders
```bash
GET /api/shopify/shops/{shopDomain}/orders?limit=50

# Response:
{
  "orders": [
    {
      "id": 987654321,
      "order_number": 1001,
      "email": "customer@example.com",
      "total_price": "99.99",
      "currency": "USD",
      "financial_status": "paid"
    }
  ],
  "count": 1
}
```

### Scope Management

#### Check Shop Scopes
```bash
GET /api/shopify/scopes/check/{shopDomain}

# Response:
{
  "shopDomain": "shop.myshopify.com",
  "currentScopes": ["read_products", "write_products"],
  "requiredScopes": ["read_products", "write_products", "read_orders"],
  "hasRequiredScopes": false,
  "missingScopes": ["read_orders"]
}
```

### Analytics & Monitoring

#### App Analytics
```bash
GET /api/shopify/analytics/overview

# Response:
{
  "activeShops": 25,
  "recentInstalls": 5,
  "shopsWithoutWebhooks": 2,
  "recentInstallations": [
    {
      "domain": "new-shop.myshopify.com",
      "name": "New Shop",
      "installedAt": "2024-01-02T10:00:00Z"
    }
  ]
}
```

### Database Schema

The template includes two main Shopify tables:

#### `shopify_shops`
- Shop installation records
- OAuth tokens and scopes
- Shop metadata and activity tracking

#### `shopify_webhooks` 
- Webhook registration tracking
- Topic and endpoint mapping
- Active status monitoring

### Background Jobs Integration

Shopify webhooks trigger background jobs for:
- Order processing and notifications
- Product synchronization
- Inventory updates
- Customer data processing
- App analytics and reporting

### Development Setup

1. **Create Shopify App**: Set up your app in the Shopify Partner Dashboard
2. **Configure Endpoints**: Update `appsettings.json` with your app credentials
3. **Database Migration**: Run migrations to create Shopify tables
4. **Ngrok Tunnel**: Use ngrok for local webhook testing
5. **Test Installation**: Install your app on a development store

### Security Features

- **HMAC Verification**: All webhooks verified with HMAC signatures
- **OAuth State Parameter**: CSRF protection for OAuth flow
- **Scope Validation**: Automatic scope requirement checking
- **Token Storage**: Secure encrypted token storage
- **Request Signing**: All API requests properly authenticated

### Error Handling & Monitoring

- **Discord Alerts**: Real-time error notifications
- **Comprehensive Logging**: Detailed operation logging
- **Retry Logic**: Automatic retry for failed operations
- **Health Checks**: Shop connectivity monitoring
- **Analytics Dashboard**: Installation and usage metrics

### Production Deployment

1. **SSL Certificate**: Ensure HTTPS for all endpoints
2. **Environment Variables**: Move secrets to environment variables
3. **Database Security**: Use connection string encryption
4. **Rate Limiting**: Implement API rate limiting
5. **Monitoring**: Set up application monitoring and alerting

This template provides everything needed to build a production-ready Shopify app with proper OAuth flow, webhook management, and API integration.

## 🛠️ Development Commandsbuilding scalable web APIs with comprehensive job processing capabilities.

## 🚀 Features

- **Entity Framework Core 8.0** with PostgreSQL provider
- **Generic Repository Pattern** with specific entity implementations
- **Unit of Work Pattern** for transaction management
- **Redis Caching Layer** for improved performance
- **Hangfire Background Jobs** with PostgreSQL storage
- **Job Management API** with scheduling and monitoring endpoints
- **Clean Architecture** with organized endpoints
- **Docker Compose** setup for easy development
- **Comprehensive Entity Model** with various field types
- **Database seeding** with sample data
- **AutoMapper Integration** for object-to-object mapping with DTOs
- **Health checks** for database and Redis monitoring
- **Swagger UI** for API documentation

## 📋 Prerequisites

1. **.NET 8.0 SDK** or later
2. **Docker & Docker Compose**
3. **Git** (optional)

## 🐳 Quick Start with Docker

### 1. Clone and Setup
```bash
git clone <your-repo-url>
cd BackendTest
```

### 2. Start Services with Docker
```bash
# Start PostgreSQL, Redis, and pgAdmin
docker-compose up -d

# Verify containers are running
docker ps
```

This will start:
- **PostgreSQL**: `localhost:5432` (postgres/password)
- **Redis**: `localhost:6379`
- **pgAdmin**: `localhost:5050` (admin@admin.com/admin123)

### 3. Setup the API
```bash
cd API

# Restore dependencies
dotnet restore

# Create and run migrations
dotnet ef migrations add InitialEntityMigration
dotnet ef database update

# Run the application
dotnet run
```

### 4. Access the API
- **API Base URL**: `http://localhost:5121`
- **Swagger UI**: `http://localhost:5121/swagger`
- **Health Check**: `http://localhost:5121/health`
- **Redis Health Check**: `http://localhost:5121/health/redis`
- **Hangfire Dashboard**: `http://localhost:5121/hangfire`

## 🏗️ Project Structure

```
BackendTest/
├── docker-compose.yaml              # Docker services configuration
├── API/
│   ├── Models/
│   │   ├── Entity.cs                # Generic entity with comprehensive fields
│   │   ├── DTOs/
│   │   │   └── EntityDto.cs         # Data Transfer Objects for API contracts
│   │   └── ThirdParty/
│   │       └── Discord.cs           # Discord webhook models
│   ├── Mapping/
│   │   ├── MappingExtensions.cs     # AutoMapper helper extensions
│   │   └── Profiles/
│   │       └── EntityProfile.cs     # Entity mapping configurations
│   ├── Data/
│   │   └── ApplicationDbContext.cs  # EF DbContext with configurations
│   ├── Services/
│   │   ├── Interfaces/
│   │   │   ├── ICacheService.cs     # Caching service interface
│   │   │   └── IJobService.cs       # Background job service interface
│   │   └── Implementations/
│   │       ├── CacheService.cs      # Redis caching implementation
│   │       └── JobService.cs        # Hangfire job service implementation
│   ├── Jobs/
│   │   └── SampleBackgroundJobs.cs  # Sample background job implementations
│   ├── Repositories/
│   │   ├── Interfaces/
│   │   │   ├── IGenericRepository.cs    # Generic CRUD interface
│   │   │   ├── IEntityRepository.cs     # Entity-specific operations
│   │   │   └── IUnitOfWork.cs          # Transaction management
│   │   └── Implementations/
│   │       ├── GenericRepository.cs    # Base repository implementation
│   │       ├── EntityRepository.cs     # Entity repository with business logic
│   │       └── UnitOfWork.cs           # Transaction and repository coordination
│   ├── Endpoints/
│   │   ├── EntityEndpoints.cs          # Entity API endpoints with caching
│   │   ├── JobEndpoints.cs             # Background job management endpoints
│   │   ├── HealthEndpoints.cs          # Health check endpoints
│   │   └── EndpointExtensions.cs       # Endpoint registration
│   ├── Program.cs                      # Application startup and configuration
│   ├── appsettings.json               # Production configuration
│   └── appsettings.Development.json   # Development configuration
└── README.md
```

## 🎯 API Endpoints

### Health Checks
- `GET /health` - Database and Redis connection health check
- `GET /health/redis` - Redis-specific health check

### Entity Management (`/api/v1/entities`)
All endpoints now include Redis caching with automatic cache invalidation and AutoMapper DTOs:

- `GET /` - Get all entities as `EntitySummaryDto[]` (cached for 5 minutes)
- `GET /{id}` - Get entity by ID as `EntityDto` (cached for 10 minutes)
- `GET /code/{code}` - Get entity by unique code as `EntityDto` (cached for 10 minutes)
- `GET /active` - Get all active entities (cached for 3 minutes)
- `GET /featured` - Get featured entities (cached for 15 minutes)
- `GET /category/{category}` - Filter by category (cached for 8 minutes)
- `GET /status/{status}` - Filter by status (cached for 5 minutes)
- `GET /search?term={term}` - Search by name (cached for 2 minutes)
- `GET /priority?min={min}&max={max}` - Filter by priority range (cached for 5 minutes)
- `POST /` - Create new entity with `CreateEntityDto`
- `PUT /{id}` - Update entity with `UpdateEntityDto`
- `DELETE /{id}` - Delete entity by ID
- `DELETE /cache` - Clear all entity-related cache

## 🗺️ AutoMapper Integration

### Overview
AutoMapper provides clean object-to-object mapping between domain models and Data Transfer Objects (DTOs), ensuring separation of concerns and API contract management.

### Available DTOs

#### EntityDto
Full DTO representation of an Entity, used for detailed responses.

#### EntitySummaryDto  
Simplified DTO for list views with only essential fields for better performance.

#### CreateEntityDto
DTO for creating new entities with required fields and defaults.

#### UpdateEntityDto
DTO for updating entities with nullable fields (only provided fields are updated).

### Mapping Profiles

The `EntityProfile` handles all Entity-related mappings:
- **Entity → EntityDto**: Full mapping for detailed responses
- **Entity → EntitySummaryDto**: Simplified mapping for list views  
- **CreateEntityDto → Entity**: Maps creation requests to domain model
- **UpdateEntityDto → Entity**: Partial updates (only non-null values)
- **EntityDto → Entity**: Reverse mapping if needed

### Usage Examples

#### Creating Entities
```bash
POST /api/v1/entities
Content-Type: application/json

{
  "name": "New Product",
  "code": "PROD001",
  "description": "A sample product",
  "status": "Active",
  "priority": 5,
  "price": 99.99,
  "quantity": 100,
  "category": "Electronics",
  "isActive": true
}
```

#### Updating Entities
```bash
PUT /api/v1/entities/1
Content-Type: application/json

{
  "name": "Updated Product Name",
  "price": 109.99,
  "quantity": 150
}
# Only provided fields will be updated
```

### Benefits
1. **Separation of Concerns**: Domain models stay clean, DTOs handle API contracts
2. **Performance**: Summary DTOs reduce payload size for list operations
3. **Security**: Internal model fields are not exposed accidentally
4. **Flexibility**: Easy to add API-specific fields without changing domain models
5. **Validation**: DTOs can have different validation rules than domain models

### Background Jobs (`/api/v1/jobs`)
Comprehensive job management with Hangfire integration:

#### Fire-and-forget Jobs
- `POST /enqueue/cache-warm` - Trigger immediate cache warming
- `POST /enqueue/data-processing?batchSize={size}` - Process data batch

#### Scheduled Jobs  
- `POST /schedule/notification` - Schedule delayed notification
  - Body: `{ "message": "text", "recipient": "user", "delayMinutes": 30 }`
- `POST /schedule/cache-warm?delayMinutes={minutes}` - Schedule delayed cache warming

#### Recurring Jobs
- `POST /recurring/create` - Create/update recurring job
  - Body: `{ "jobId": "unique-id", "cronExpression": "0 */6 * * *", "jobType": "cache-cleanup" }`
- `DELETE /recurring/{jobId}` - Remove recurring job

#### Job Monitoring
- `GET /{jobId}/status` - Get job status and details
- `GET /recurring` - List all recurring jobs
- `DELETE /{jobId}` - Cancel pending/running job

#### Utilities
- `GET /examples/cron` - Get cron expression examples
- `GET /dashboard-url` - Get Hangfire dashboard URL

### Cache Response Format
All cached endpoints return data with cache information:
```json
{
  "data": [...],
  "fromCache": true|false
}
```

## 🗄️ Entity Model

The `Entity` model demonstrates various field types commonly used in business applications:

```csharp
public class Entity
{
    // Identifiers
    public int Id { get; set; }              // Primary key
    public string Code { get; set; }         // Unique business identifier
    
    // Basic Information
    public string Name { get; set; }         // Display name
    public string? Description { get; set; } // Detailed description
    
    // Classification
    public string Status { get; set; }       // Workflow status
    public string? Category { get; set; }    // Grouping category
    public int Priority { get; set; }        // Ordering/importance
    
    // Financial & Metrics
    public decimal? Price { get; set; }      // Monetary values
    public int Quantity { get; set; }        // Counts/quantities
    public decimal? Percentage { get; set; } // Rates/percentages
    
    // Flags & Features
    public bool IsActive { get; set; }       // Soft delete pattern
    public bool IsFeatured { get; set; }     // Highlighting
    
    // Flexible Data
    public string? Tags { get; set; }        // Comma-separated tags
    public string? Metadata { get; set; }    // JSON for custom properties
    public string? ExternalId { get; set; }  // Integration with external systems
    
    // Date Management
    public DateTime? StartDate { get; set; } // Period start
    public DateTime? EndDate { get; set; }   // Period end
    public DateTime? DueDate { get; set; }   // Deadline
    
    // Audit Trail
    public DateTime CreatedAt { get; set; }  // Creation timestamp
    public DateTime UpdatedAt { get; set; }  // Last modification
    public string? CreatedBy { get; set; }   // Creator identification
    public string? UpdatedBy { get; set; }   // Last modifier
}
```

## 🔧 Configuration

### Database Connection
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=flutter_builder;Username=postgres;Password=password",
    "Redis": "localhost:6379"
  }
}
```

### Cache Configuration
```json
{
  "Cache": {
    "DefaultExpiration": "00:30:00"
  }
}
```

### Hangfire Configuration
```json
{
  "Hangfire": {
    "DashboardTitle": "Backend Test API Jobs",
    "WorkerCount": 20,
    "Queues": ["default", "critical", "background"],
    "JobExpirationTimeout": "24:00:00",
    "DashboardPath": "/hangfire"
  }
}
```

### Docker Services
The `docker-compose.yaml` includes:
- **PostgreSQL 15** with persistent storage
- **Redis 7** with persistence enabled
- **pgAdmin 4** for database management

## � Redis Caching Features

### Cache Service Interface
```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task<bool> ExistsAsync(string key);
}
```

### Caching Strategy
- **Entity by ID**: 10 minutes TTL
- **Entity by Code**: 10 minutes TTL
- **All Entities**: 5 minutes TTL
- **Active Entities**: 3 minutes TTL
- **Featured Entities**: 15 minutes TTL
- **Search Results**: 2 minutes TTL
- **Category/Status Filters**: 5-8 minutes TTL

### Cache Keys Pattern
- `entity:id:{id}` - Individual entity by ID
- `entity:code:{code}` - Individual entity by code
- `entities:all` - All entities collection
- `entities:active` - Active entities collection
- `entities:featured` - Featured entities collection
- `entities:category:{category}` - Entities by category
- `entities:status:{status}` - Entities by status
- `entities:search:{term}` - Search results
- `entities:priority:{min}-{max}` - Priority range results

## �🛠️ Development Commands

### Entity Framework Migrations
```bash
# Create a new migration
dotnet ef migrations add [MigrationName]

# Apply migrations to database
dotnet ef database update

# Rollback to specific migration
dotnet ef database update [MigrationName]

# Remove last migration (if not applied)
dotnet ef migrations remove

# Drop database and recreate
dotnet ef database drop
dotnet ef migrations add InitialEntityMigration
dotnet ef database update
```

### Docker Commands
```bash
# Start all services (PostgreSQL, Redis, pgAdmin)
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs

# View specific service logs
docker-compose logs redis
docker-compose logs db

# Rebuild and start
docker-compose up -d --build
```

### Redis Commands (for debugging)
```bash
# Connect to Redis container
docker exec -it <redis-container-name> redis-cli

# Common Redis commands in CLI:
KEYS *                    # List all keys
GET entity:id:1          # Get specific cached entity
FLUSHALL                 # Clear all cache
TTL entity:id:1          # Check time to live
```

## 🏛️ Architecture Patterns

### Repository Pattern
- **Generic Repository**: Common CRUD operations for all entities
- **Specific Repository**: Business-specific operations for Entity
- **Unit of Work**: Manages transactions and coordinates repositories

### Caching Layer
- **Cache-Aside Pattern**: Application manages cache explicitly
- **Write-Through**: Updates go to both cache and database
- **TTL-based Expiration**: Automatic cache invalidation
- **Pattern-based Clearing**: Clear multiple related cache entries

### Benefits
1. **Performance** - Reduced database load with intelligent caching
2. **Scalability** - Redis handles high-throughput scenarios
3. **Separation of Concerns** - Data access isolated from business logic
4. **Testability** - Easy to mock for unit testing
5. **Consistency** - Standardized data access patterns
6. **Maintainability** - Changes isolated to specific layers

## 🚦 Getting Started for Development

### 1. Fork/Clone the Repository
```bash
git clone <repository-url>
cd BackendTest
```

### 2. Start Development Environment
```bash
# Start database and Redis
docker-compose up -d

# Navigate to API project
cd API

# Install dependencies
dotnet restore

# Apply migrations
dotnet ef database update

# Start development server
dotnet watch run
```

### 3. Access Development Tools
- **API**: `http://localhost:5121`
- **Swagger**: `http://localhost:5121/swagger`
- **pgAdmin**: `http://localhost:5050`

### 4. Testing Cache Functionality
```bash
# Call an endpoint twice to see caching in action
curl http://localhost:5121/api/v1/entities
# First call: "fromCache": false
# Second call: "fromCache": true

# Clear cache
curl -X DELETE http://localhost:5121/api/v1/entities/cache
```

### 5. Testing Background Jobs
```bash
# Fire-and-forget job
curl -X POST http://localhost:5121/api/v1/jobs/enqueue/cache-warm
# Response: {"jobId": "abc123", "message": "Cache warming job enqueued"}

# Schedule a delayed job
curl -X POST "http://localhost:5121/api/v1/jobs/schedule/cache-warm?delayMinutes=2"
# Response: {"jobId": "def456", "scheduledFor": "2025-10-02T10:02:00Z"}

# Create recurring job
curl -X POST http://localhost:5121/api/v1/jobs/recurring/create \
  -H "Content-Type: application/json" \
  -d '{"jobId": "test-cleanup", "cronExpression": "*/2 * * * *", "jobType": "cache-cleanup"}'

# Check job status
curl http://localhost:5121/api/v1/jobs/{jobId}/status

# View Hangfire Dashboard
open http://localhost:5121/hangfire
```

## 🧪 Sample Data

The database is automatically seeded with sample entities demonstrating different field types and use cases:

1. **Sample Organization** - Basic entity with standard fields
2. **Sample Department** - Department-type entity  
3. **Sample Project** - Project entity with dates, pricing, and progress tracking

## 📚 Next Steps for Your Project

This template provides a solid foundation. Consider adding:

1. **Authentication & Authorization** (JWT, Identity)
2. **Validation** with FluentValidation
3. **Logging** with Serilog
4. **Exception Handling** middleware
5. **API Versioning**
9. **Advanced Job Patterns** (continuation jobs, batch jobs)
10. **Job Security** (authorization for job endpoints)
11. **Integration Tests** (including job testing)
12. **Distributed Caching** strategies
13. **Job Monitoring** and alerting
14. **Custom Job Queues** and priorities
15. **Job Data Persistence** and audit trails
12. **Cache Warming** procedures

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is provided as a template for educational and development purposes.

---

**Happy Coding! 🚀**