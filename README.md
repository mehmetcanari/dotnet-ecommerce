# E-Commerce

![Development Status](https://img.shields.io/badge/status-in%20development-yellow)
![.NET Version](https://img.shields.io/badge/.NET-9.0-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)

A full-stack e-commerce solution built with .NET 9, featuring a RESTful API backend and MVC frontend. Designed with Clean Architecture principles for maintainability and scalability.
<img width="3423" height="1303" alt="homepage" src="https://github.com/user-attachments/assets/e1c257cf-7eba-4f61-ac70-5ff4a599a05d" />

## Tech Stack

**Backend API**
- .NET 9 Web API
- PostgreSQL & MongoDB (polyglot persistence)
- Redis (caching)
- Elasticsearch (product search)
- JWT authentication
- MediatR (CQRS)
- SignalR (notifications)

**Frontend**
- ASP.NET Core MVC
- Bootstrap 5
- jQuery

**Infrastructure**
- Docker & Docker Compose
- AWS S3 (file storage)
- Iyzico (payment processing)

## Key Features

- **Product Management**: Full CRUD operations with image uploads and category management
- **User Authentication**: Secure JWT-based authentication with role-based authorization
- **Shopping Cart & Orders**: Session-based cart with order processing and tracking
- **Payment Integration**: Secure payment processing via Iyzico gateway
- **Search & Filter**: Product search and filtering capabilities
- **Real-time Updates**: SignalR integration for live notifications

## Architecture

```mermaid
graph TD
    subgraph Client ["Presentation Layer"]
        MVC[ASP.NET Core MVC]
        style MVC fill:#2d3436,stroke:#fff,color:#fff
    end

    subgraph API ["API Layer"]
        WebAPI[Web API Controller]
        Auth[JWT Auth]
        style WebAPI fill:#512bd4,stroke:#fff,color:#fff
    end

    subgraph App ["Application Layer"]
        MediatR[MediatR Pipeline]
        CQRS_C[Commands / Writes]
        CQRS_Q[Queries / Reads]
        style MediatR fill:#0984e3,stroke:#fff,color:#fff
    end

    subgraph Infra ["Infrastructure & Persistence"]
        EF[EF Core / PostgreSQL]
        MongoDriver[MongoDB Driver]
        Redis[Redis Cache]
        Elastic[Elasticsearch]
        style Infra fill:#00b894,stroke:#fff,color:#fff
    end

    %% Flows
    MVC -->|HTTP/REST| WebAPI
    WebAPI -->|Authorize| Auth
    WebAPI -->|Send Request| MediatR
    
    MediatR -->|Create/Update| CQRS_C
    MediatR -->|Get Data| CQRS_Q
    
    %% WRITE OPERATIONS (Satır atlatarak daraltıldı)
    CQRS_C -->|User, Order,<br/>Basket, Category| EF
    CQRS_C -->|Product<br/>Data| MongoDriver
    
    %% READ OPERATIONS & CACHING
    CQRS_Q -->|1. Try Get| Redis
    CQRS_Q -->|2. Search| Elastic
    
    %% CACHE MISS FLOWS
    Redis -.->|Cache<br/>Miss| EF
    Redis -.->|Cache<br/>Miss| MongoDriver

    %% Physical Databases
    EF -.-> DB1[(PostgreSQL)]
    MongoDriver -.-> DB2[(MongoDB)]
    Redis -.-> DB3[(Redis)]
    Elastic -.-> DB4[(Elasticsearch)]
```

The project follows Clean Architecture with clear layer separation:

```
├── ECommerce.API/              # API presentation layer
├── ECommerce.MVC/              # MVC presentation layer
├── ECommerce.Application/      # Business logic & use cases
├── ECommerce.Domain/           # Core entities & interfaces
├── ECommerce.Infrastructure/   # Data access 
└── ECommerce.Shared/           # Common utilities & constants
```

**Design Patterns & Practices**
- Repository & Unit of Work patterns
- CQRS with MediatR
- Result pattern for error handling
- Global exception handling
- API versioning
- Rate limiting

## Getting Started

### Prerequisites
- .NET 9 SDK
- Docker Desktop

### Installation

1. Clone the repository
```bash
git clone https://github.com/mehmetcanari/dotnet-ecommerce-demo.git
cd dotnet-ecommerce-demo
```

2. Start services with Docker Compose
```bash
docker compose up --build
```

3. Configure environment variables

Create a `.env` file in the project root:
```bash
# Application
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5076

# JWT Configuration
JWT_SECRET=your_secret_key
JWT_ISSUER=OnlineStoreWebAPI
JWT_AUDIENCE=OnlineStoreClient
JWT_ACCESS_TOKEN_EXPIRATION_MINUTES=120
JWT_REFRESH_TOKEN_EXPIRATION_DAYS=180

# Database
DB_CONNECTION_STRING=Server=localhost;Port=5432;Database=ECommerceDB;User Id=postgres;Password=your_password;
MONGODB_CONNECTION_STRING=mongodb://localhost:27017
MONGODB_DATABASE_NAME=ecommerce-products

# Redis Cache
REDIS_CONNECTION_STRING=localhost:6380

# RabbitMQ
RABBITMQ_CONNECTION=amqp://guest:guest@localhost:5672/

# AWS S3
AWS_ACCESS_KEY=your_access_key
AWS_SECRET_KEY=your_secret_key
AWS_REGION=eu-central-1
AWS_BUCKET_NAME=your_bucket_name

# Payment Gateway (Iyzico)
IYZICO_API_KEY=your_api_key
IYZICO_SECRET_KEY=your_secret_key
IYZICO_BASE_URL=https://sandbox-api.iyzipay.com

# Admin Credentials
ADMIN_EMAIL=admin@ecommerce.dev
ADMIN_PASSWORD=Admin12345!
```

4. Access the application
- **MVC Frontend**: https://localhost:5077
- **API**: http://localhost:5076
- **Swagger Documentation**: http://localhost:5076/swagger

## Roadmap

- [ ] **Admin Panel**: Comprehensive dashboard for store management
- [ ] **Shipping Integration**: Cargo/shipping provider integration
- [ ] **OAuth Authentication**: Social login (Google, Facebook)
- [ ] **Cloudflare R2**: Migration from AWS S3 to Cloudflare R2
- [ ] **Favorite Products**: Wishlist functionality
- [ ] **Stripe Integration**: Additional payment gateway option

## Contact

**Mehmet Can Arı**  
📧 bsn.mehmetcanari@gmail.com  
🔗 [GitHub](https://github.com/mehmetcanari)

---
