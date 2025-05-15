# E-Commerce API
## 📋 Overview
E-Commerce API is a RESTful API developed for a modern e-commerce platform, adhering to Clean Architecture and SOLID principles. This API supports core e-commerce functionalities such as product management, user registration and authentication, and payment processing.

## 🚀 Features
📦 Product management (create, list, update, delete)

🔐 User registration and authentication

💳 Payment Processing Integration – Iyzico payment service fully integrated

⚡ Redis-based caching

🔒 JWT-based security and authorization

🧾 Centralized logging infrastructure with Serilog

📜 API documentation with Swagger

🧪 Test suite using xUnit for ensuring code quality

## 🛠️ Technologies Used
| Technology | Description |
|-----------|----------|
| **.NET 9** | Core platform for the API |
| **FluentValidation** | Used for data validation |
| **Identity & JWT** | For authentication and security |
| **Redis** | Caching to improve performance |
| **Iyzico** | Secure and reliable payment gateway integration |
| **Swagger** | API documentation and testing interface |
| **Serilog** | Used for centralized logging infrastructure |
| **PostgreSQL** | For persistent data storage |
| **xUnit** | For unit testing the application |
| **Docker** | Containerization for environment consistency and deployment |

## 📐 Architecture
This project is developed using Clean Architecture. Business logic, data access, presentation and domain layers are separated. It is designed in accordance with SOLID, KISS, and DRY principles, making the code reusable and easy to maintain.
```
📁 Solution
  ├── 📁 API/                # Presentation Layer (Web API)
  │   ├── Controllers
  │   ├── Logs
  │   ├── API
  │   ├── DI Container
  │   └── Program.cs
  │
  ├── 📁 Application/        # Business Logic Layer
  │   ├── DTOs
  │   ├── Interfaces
  │   ├── Services
  │   ├── Utility
  │   └── Validations
  │
  ├── 📁 Domain/             # Domain and Entities
  │   ├── Entities
  │
  ├── 📁 Infrastructure/     # Infrastructure Layer
  │   ├── DB Context
  │   ├── Repositories
  │   ├── Migrations
  │
  └── 📁 ECommerce.Tests/    # Unit Tests
      ├── Services
```

## 🔧 Installation
```bash
# Clone the repository
git clone https://github.com/mehmetcanari/dotnet-ecommerce-demo.git
# Enter the directory
cd dotnet-ecommerce-demo

# Option 1: Run locally
dotnet restore
cd ECommerce.API
dotnet watch run 
```

## 🐳 Running with Docker Compose
Docker Compose simplifies the process of running the application and its dependencies (PostgreSQL and Redis) in isolated containers. Follow these steps to run the project using Docker Compose:

1. **Build and Start Containers**:
   ```bash
   docker compose up --build
   ```

2. **Access the API**:
   - The API will be available at [http://localhost:8080](http://localhost:8080).

3. **Stop Containers**:
   ```bash
   docker compose down
   ```

4. **View Logs**:
   ```bash
   docker compose logs ecommerce-api
   ```

5. **Rebuild Containers**:
   ```bash
   docker compose up --build
   ```

## 🔑 Environment Variables
Set the following environment variables before running the project:
```
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5076

JWT_SECRET=YourSecretKeyHere12345678901234567890
JWT_ISSUER=OnlineStoreWebAPI
JWT_AUDIENCE=OnlineStoreClient
JWT_ACCESS_TOKEN_EXPIRATION_MINUTES=30
JWT_REFRESH_TOKEN_EXPIRATION_DAYS=30

DB_CONNECTION_STRING=Server=localhost;Port=5432;Database=ECommerceDB;User Id=postgres;Password=your_password;

# Iyzico Payment Settings
IYZICO_API_KEY=your-sandbox-api-key-here
IYZICO_SECRET_KEY=your-sandbox-secret-key-here
IYZICO_BASE_URL=https://sandbox-api.iyzipay.com
```
## 🌟 Basic API Usage
### Authentication Endpoints
```http
### User Registration
POST {{baseUrl}}/{{route}}/create-user
Content-Type: application/json

{
    "name": "Test User Name",
    "surname": "Test User Surname",
    "email": "user@system.com",
    "identityNumber": "12345678901",
    "city": "Istanbul",
    "country": "TR",
    "zipCode": "34343",
    "address": "Istanbul, TR",
    "password": "UserPassword123!",
    "phoneNumber": "9876543210",
    "dateOfBirth": "1999-03-24"
}

### Login
POST {{baseUrl}}/{{route}}/login
Content-Type: application/json

{
  "email": "user@system.com",  
  "password": "UserPassword123!"
}

### Token Refresh
POST {{baseUrl}}/{{route}}/refresh-token
```

### Product Management (Admin)
```http
### Add New Product
POST {{baseUrl}}/api/admin/products/create
Content-Type: application/json
Authorization: Bearer {{adminToken}}

{
  "Name": "Playstation 5",
  "Description": "Playstation 5 is a gaming console that is the latest in the Playstation series",
  "Price": 499.99,
  "StockQuantity": 220,
  "DiscountRate": 25,
  "ImageUrl": "https://via.placeholder.com/150"
}

### List Products
GET {{baseUrl}}/api/admin/products
Authorization: Bearer {{adminToken}}
```

## 🚧 Project Status
**In Development**  
The project is being actively developed. The following features will be added in the near future:
- [x] Iyzico payment integration  
- [x] Centralized logging with Serilog  
- [x] Addition of unit tests  
- [x] Dockerization

## 📧 Contact
Project Owner - [bsn.mehmetcanari@gmail.com](mailto:bsn.mehmetcanari@gmail.com)

[![GitHub](https://img.shields.io/badge/github-%23121011.svg?style=for-the-badge&logo=github&logoColor=white)](https://github.com/mehmetcanari)

---

**Note:** Documentation will be updated as development progresses.
