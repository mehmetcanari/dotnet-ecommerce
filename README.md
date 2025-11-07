# E-Commerce Project
## 📋 Overview
Modern, scalable e-commerce RESTful API built with Clean Architecture and SOLID principles. Supports product management, user authentication, secure payment processing, and other essential online store capabilities.

## 🛠️ Tech Stack
| Technology | Purpose |
|-----------|---------|
| **.NET 9** | Core framework |
| **PostgreSQL** | Primary database |
| **MongoDB** | NoSQL database for product data |
| **Redis** | Caching layer |
| **JWT & Identity** | Authentication/authorization |
| **AWS S3** | File storage and management |
| **Iyzico** | Payment gateway |
| **FluentValidation** | Input validation |
| **Serilog** | Structured logging |
| **Docker** | Containerization |
| **MediatR** | CQRS implementation |
| **Elasticsearch** | Advanced product search and filtering |
| **SignalR** | Store notifications |

## 🛡️ Technical Approaches & Best Practices

| Approach                     | Description | Implementation |
|------------------------------|-------------|----------------|
| **Global Exception Handling** | Centralized error handling for consistent responses | Custom middleware catches all exceptions, returns structured JSON responses |
| **API Versioning**           | Backward compatibility and smooth API evolution | URL-based versioning (`/api/v1/`, `/api/v2/`) with version-specific controllers |
| **Targeted Queries**         | Optimized database performance | Repository pattern with eager loading, projection queries for specific data needs |
| **Result Pattern**           | Standardized response structure | Generic `Result<T>` wrapper for consistent success/error handling across endpoints |
| **Rate Limiting**            | API abuse prevention | ASP.NET Core middleware |
| **Security Headers**         | Enhanced protection against common attacks | Middleware adds HSTS, X-Frame-Options, CSP, and other security headers |
| **CORS**                     | Cross-origin resource sharing for web clients | Policy-based configuration with environment-specific origins, credentials support for authenticated requests |
| **Polyglot Persistence**      | Multi-database architecture for scalability | PostgreSQL for relational data, MongoDB for non-relational product data storage optimized for read-heavy operations |
| **Transactions**   | Data consistency across operations | Unit of Work pattern with EF Core transactions for multi-repository operations |
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
  │   ├── API
  │   └── Configurations
  │
  ├── 📁 Application/        
  │   ├── DTO
  │   ├── Exceptions
  │   ├── Services
  │   ├── Queue
  │   ├── Commands
  │   ├── Events
  │   ├── Queries
  │   ├── Abstract/Services
  │   ├── Utility
  │   ├── Dependencies  #Service Dependencies
  │   └── Validations
  │
  ├── 📁 Domain/        
  │   ├── Abstract/Repository       
  │   └── Entities
  │
  ├── 📁 Infrastructure/     
  │   ├── Context
  │   ├── Repositories
  │   ├── Dependencies  #Infrastructure Dependencies  
  │   └── Migrations
  │
  └── 📁 ECommerce.Shared/    
      ├── Constants
```

## 🚀 Quick Start

### Prerequisites
- .NET 9 SDK
- Docker 

### Setup dependencies: Docker Compose
```bash
git clone https://github.com/mehmetcanari/dotnet-ecommerce-demo.git
cd dotnet-ecommerce-demo
docker compose up --build
```
API will be available at http://localhost:5076

## 📚 API Documentation

### Swagger/OpenAPI
- **Local Development**: [http://localhost:5076](http://localhost:5076)
  
## 📧 Contact
**Mehmet Can Arı** - [bsn.mehmetcanari@gmail.com](mailto:bsn.mehmetcanari@gmail.com)

[![GitHub](https://img.shields.io/badge/github-%23121011.svg?style=for-the-badge&logo=github&logoColor=white)](https://github.com/mehmetcanari)


---
