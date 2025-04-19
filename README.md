# E-Commerce API
## 📋 Sunum
E-Commerce API, modern bir e-ticaret platformu için geliştirilmiş, temiz mimari (Clean Architecture) ve SOLID prensiplerine uygun bir RESTful API'dir. Bu API, ürün yönetimi, kullanıcı kayıt ve giriş işlemleri, ödeme işlemleri gibi temel e-ticaret işlevlerini destekler.

## 🚀 Özellikler
📦 Ürün yönetimi (ekleme, listeleme, güncelleme, silme)

🔐 Kullanıcı kayıt ve kimlik doğrulama

💳 Ödeme işlemleri entegrasyonu (Iyzico entegre edilecek. Devam etmekte.)

⚡ Redis tabanlı önbellekleme

🔒 JWT tabanlı güvenlik ve yetkilendirme

🧾 Serilog ile merkezi loglama altyapısı

📚 Kapsamlı API dokümantasyonu

## 🛠️ Kullanılan Teknolojiler
| Teknoloji | Açıklama |
|-----------|----------|
| **.NET 9** | API'nin temel platformu |
| **FluentValidation** | Veri doğrulama için kullanılır |
| **Identity & JWT** | Kimlik doğrulama ve güvenlik için |
| **Redis** | Performansı artırmak için cacheleme |
| **Swagger** | API dokümantasyonu ve test arayüzü |
| **Serilog** | Merkezi loglama altyapısı için kullanılır |
| **PostgreSQL** | Kalıcı veri depolama için |

## 📐 Mimari Yapı
Bu proje N-Tier Mimari ve Clean Architecture kullanılarak geliştirilmiştir. İş mantığı, veri erişimi ve sunum katmanları ayrılmıştır. SOLID, KISS ve DRY prensiplerine uygun şekilde tasarlanmış olup, kodun yeniden kullanılabilirliği ve bakımı kolaydır.
```
📁 Solution
  ├── 📁 API/                # Sunum Katmanı (Web API)
  │   ├── Controllers
  │   ├── API
  │   ├── DI Container
  │   └── Program.cs
  │
  ├── 📁 Application/        # İş Mantığı Katmanı
  │   ├── DTOs
  │   ├── Interfaces
  │   ├── Services
  │   └── Validations
  │
  ├── 📁 Domain/             # Domain ve Entitiler
  │   ├── Entities
  │
  └── 📁 Infrastructure/     # Altyapı Katmanı
      ├── DB Context
      ├── Repositories
      ├── Migrations
```

## 🔧 Kurulum
```bash
# Repoyu klonlayın
git clone https://github.com/mehmetcanari/dotnet-ecommerce-demo.git
# Klasöre girin
cd dotnet-ecommerce-demo
# Bağımlılıkları yükleyin
dotnet restore
# Uygulamayı çalıştırın
cd ECommerce.API
dotnet run 
```

## 🔑 Ortam Değişkenleri
Projeyi çalıştırmadan önce aşağıdaki ortam değişkenlerini ayarlayın:
```
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5076
JWT_SECRET=YourSecretKeyHere
JWT_ISSUER=OnlineStoreWebAPI
JWT_AUDIENCE=OnlineStoreClient
JWT_ACCESS_TOKEN_EXPIRATION_MINUTES=30
JWT_REFRESH_TOKEN_EXPIRATION_DAYS=30
DB_CONNECTION_STRING=Server=your_server;Database=ecommerce;Username=your_username;Password=your_password
REDIS_CONNECTION=localhost:6379
# Kimlik doğrulama sonrası alınan tokenlar
ADMIN_TOKEN=
USER_TOKEN=
```
> **Not:** Auth endpoint'lerinden admin veya kullanıcı için token aldığınızda, bu token değerlerini `ADMIN_TOKEN` ve `USER_TOKEN` değişkenlerine yapıştırın.

## 🌟 Temel API Kullanımı
### Kimlik Doğrulama Endpoint'leri
```http
### Kullanıcı Kaydı
POST {{baseUrl}}/api/auth/create-user
Content-Type: application/json

{
  "fullName": "John Doe",
  "email": "user@example.com",
  "password": "P@ssw0rd123",
  "address": "İstanbul",
  "phoneNumber": "5551234567",
  "dateOfBirth": "1990-01-01"
}

### Giriş Yap
POST {{baseUrl}}/api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "P@ssw0rd123"
}

### Token Yenileme
POST {{baseUrl}}/{{route}}/refresh-token
```

### Ürün Yönetimi (Admin)
```http
### Yeni Ürün Ekleme
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

### Ürün Listeleme
GET {{baseUrl}}/api/admin/products
Authorization: Bearer {{adminToken}}
```

## 🚧 Proje Durumu
**Geliştirme Aşamasında**  
Proje aktif olarak geliştirilmeye devam etmektedir. Aşağıdaki özellikler yakın gelecekte eklenecektir:
- [ ] Iyzico ödeme entegrasyonu
- [x] Serilog ile merkezi logging
- [ ] Unit test eklenmesi
- [ ] Dockerize ve CI/CD pipeline

## 🤝 Katkıda Bulunma
1. Projeyi fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add some amazing feature'`)
4. Branch'inize push edin (`git push origin feature/amazing-feature`)
5. Pull request açın

## 📧 İletişim
Proje Sahibi - [bsn.mehmetcanari@gmail.com](mailto:bsn.mehmetcanari@gmail.com)

[![GitHub](https://img.shields.io/badge/github-%23121011.svg?style=for-the-badge&logo=github&logoColor=white)](https://github.com/mehmetcanari)

---

**Not:** Geliştirmeler devam ettikçe dokümantasyon güncellenecektir.
