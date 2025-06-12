# E-Commerce API
## 📋 Overview
Modern e-commerce RESTful API built with Clean Architecture and SOLID principles. Supports product management, user authentication, secure payment processing, and cloud file storage with AWS S3.

[![Coverage](https://img.shields.io/badge/Coverage-78.9%25-green?style=for-the-badge)](./coveragereport/Summary.md)

> **Current critical component test coverage:** 78.95% (1185/1501 lines) - Total 142 Test Cases
> 
> Coverage includes:
> - Application Layer Services 
> - Infrastructure Layer Repositories

## 🚀 Features

### Core Features
- 📦 **Product Management** - Full CRUD operations with inventory tracking
- 🔐 **Authentication & Authorization** - JWT-based security with role management
- 💳 **Payment Processing** - Integrated Iyzico payment gateway
- ☁️ **Cloud Integration** - Secure and scalable file storage with AWS S3

### Additional Features
- ⚡ **Performance** - Redis caching and optimized database queries
- 🧾 **Observability** - Centralized logging with Serilog
- 📜 **Documentation** - Interactive Swagger API docs
- 🧪 **Testing** - Comprehensive test suite with xUnit
- 🔄 **CQRS** - Command Query Responsibility Segregation with MediatR

## 🛠️ Tech Stack
| Technology | Purpose |
|-----------|---------|
| **.NET 9** | Core framework |
| **PostgreSQL** | Primary database |
| **Redis** | Caching layer |
| **JWT & Identity** | Authentication/authorization |
| **AWS S3** | File storage and management |
| **Iyzico** | Payment gateway |
| **FluentValidation** | Input validation |
| **Serilog** | Structured logging |
| **Docker** | Containerization |
| **xUnit** | Unit testing |
| **MediatR** | CQRS implementation |

## 🛡️ Technical Approaches & Best Practices

| Approach                     | Description | Implementation |
|------------------------------|-------------|----------------|
| **Global Exception Handling** | Centralized error handling for consistent responses | Custom middleware catches all exceptions, returns structured JSON responses |
| **API Versioning**           | Backward compatibility and smooth API evolution | URL-based versioning (`/api/v1/`, `/api/v2/`) with version-specific controllers |
| **Targeted Queries**         | Optimized database performance | Repository pattern with eager loading, projection queries for specific data needs |
| **Result Pattern**           | Standardized response structure | Generic `Result<T>` wrapper for consistent success/error handling across endpoints |
| **Rate Limiting**            | API abuse prevention | ASP.NET Core middleware |
| **Security Headers**         | Enhanced protection against common attacks | Middleware adds HSTS, X-Frame-Options, CSP, and other security headers |
| **BaseValidator**            | Service operations and validation | Base class with DTO validation |
| **Transaction Management**   | Data consistency across operations | Unit of Work pattern with EF Core transactions for multi-repository operations |
| **Background Jobs**          | Automated system maintenance | `BackgroundService` for token cleanup, cache refresh, and scheduled tasks |
| **Cloud Storage**            | Secure and scalable file management | AWS S3 integration for file upload |
| **Pagination**              | Efficient data retrieval and performance | Repository pattern with Skip/Take implementation, default page size of 50 items |
| **CQRS Pattern**            | Separation of read and write operations | MediatR implementation with Commands and Queries for better scalability and maintainability |

## 📐 Architecture
Clean Architecture implementation with clear separation of concerns:

```
📁 Solution
  ├── 📁 Presentation/                
  │   ├── Controllers
  │   ├── Logs
  │   ├── API
  │   ├── DI Container
  │   └── Program.cs
  │
  ├── 📁 Application/        
  │   ├── DTO
  │   ├── Exceptions
  │   ├── Services
  │   ├── Commands
  │   ├── Queries
  │   ├── Abstract/Services
  │   ├── Utility
  │   ├── Depdendencies  #Service Dependencies
  │   └── Validations
  │
  ├── 📁 Domain/        
  │   ├── Abstract/Repository       
  │   ├── Entities
  │
  ├── 📁 Infrastructure/     
  │   ├── Context
  │   ├── Repositories
  │   ├── Dependencies  #Infrastructure Dependencies  
  │   ├── Migrations
  │
  └── 📁 ECommerce.Tests/    
      ├── Services
```

## 🚀 Quick Start

### Prerequisites
- .NET 9 SDK
- Docker & Docker Compose
- PostgreSQL (if running locally)
- Redis (if running locally)
- AWS S3 account and bucket (for file storage)

### Option 1: Docker Compose (Recommended)
```bash
git clone https://github.com/mehmetcanari/dotnet-ecommerce-demo.git
cd dotnet-ecommerce-demo
docker compose up --build
```
API will be available at http://localhost:5076

### Option 2: Local Development
```bash
git clone https://github.com/mehmetcanari/dotnet-ecommerce-demo.git
cd dotnet-ecommerce-demo
dotnet restore
cd ECommerce.Presentation
dotnet watch run
```

## ⚙️ Configuration

### Required Environment Variables
```env
# Application
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5076

# JWT Configuration
JWT_SECRET=YourSecretKeyHere12345678901234567890
JWT_ISSUER=OnlineStoreWebAPI
JWT_AUDIENCE=OnlineStoreClient
JWT_ACCESS_TOKEN_EXPIRATION_MINUTES=30
JWT_REFRESH_TOKEN_EXPIRATION_DAYS=30

# Database
DB_CONNECTION_STRING=Server=localhost;Port=5432;Database=ECommerceDB;User Id=postgres;Password=your_password;

# AWS S3
AWS_ACCESS_KEY=your-aws-access-key
AWS_SECRET_KEY=your-aws-secret-key
AWS_REGION=eu-central-1
AWS_BUCKET_NAME=your-bucket-name

# Payment Gateway (Iyzipay Sandbox)
IYZICO_API_KEY=your-sandbox-api-key-here
IYZICO_SECRET_KEY=your-sandbox-secret-key-here
IYZICO_BASE_URL=https://sandbox-api.iyzipay.com

# Tokens - For testing purpose
USER_TOKEN=
ADMIN_TOKEN=
```

## ☁️ AWS S3 Integration
- All product images and file uploads are securely stored in AWS S3.
- S3 credentials and bucket info are managed via environment variables for security and flexibility.

## 📖 API Documentation

### Authentication
```http
# Register New User
POST /api/v1/auth/register
Content-Type: application/json

{
    "name": "John",
    "surname": "Doe",
    "email": "john.doe@example.com",
    "password": "SecurePassword123!",
    "phoneNumber": "1234567890",
    "identityNumber": "12345678901",
    "address": "123 Main St",
    "city": "Istanbul",
    "country": "TR",
    "zipCode": "34000",
    "dateOfBirth": "1990-01-01"
}

# Login
POST /api/v1/auth/login
Content-Type: application/json

{
    "email": "john.doe@example.com",
    "password": "SecurePassword123!"
}

# Refresh Token
POST /api/v1/auth/refresh-token
```

### Product Management
```http
# Create Product (Admin)
POST /api/v1/admin/products
Authorization: Bearer {admin_token}
Content-Type: application/json

{
    "name": "PlayStation 5",
    "description": "Next-gen gaming console",
    "price": 499.99,
    "stockQuantity": 100,
    "discountRate": 10,
    "imageUrl": "https://example.com/ps5.jpg"
}

# Get Products
GET /api/v1/products?page=1&size=10
```

## 🧪 Testing
```bash
# Run all tests
dotnet test
```

## 📧 Contact
**Mehmet Can Arı** - [bsn.mehmetcanari@gmail.com](mailto:bsn.mehmetcanari@gmail.com)

[![GitHub](https://img.shields.io/badge/github-%23121011.svg?style=for-the-badge&logo=github&logoColor=white)](https://github.com/mehmetcanari)


---
