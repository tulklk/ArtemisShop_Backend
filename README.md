# Artemis Shop Backend API

Backend API cho nền tảng thương mại điện tử đồng hồ thông minh GPS Bracelet - Artemis Shop. Dự án được xây dựng bằng .NET 8.0 với kiến trúc Clean Architecture.

## 📋 Mục lục

- [Giới thiệu](#giới-thiệu)
- [Tính năng](#tính-năng)
- [Công nghệ sử dụng](#công-nghệ-sử-dụng)
- [Kiến trúc dự án](#kiến-trúc-dự-án)
- [Cài đặt và Chạy dự án](#cài-đặt-và-chạy-dự-án)
- [Cấu hình](#cấu-hình)
- [API Documentation](#api-documentation)
- [Deployment](#deployment)
- [Cấu trúc dự án](#cấu-trúc-dự-án)

## 🎯 Giới thiệu

Artemis Shop là một nền tảng thương mại điện tử chuyên bán đồng hồ thông minh GPS Bracelet. Backend API này cung cấp đầy đủ các tính năng cần thiết cho một hệ thống e-commerce hiện đại, bao gồm quản lý sản phẩm, đơn hàng, thanh toán, giỏ hàng, đánh giá, và tích hợp AI chat hỗ trợ khách hàng.

## ✨ Tính năng

### 🔐 Xác thực và Phân quyền
- Đăng ký, đăng nhập người dùng
- JWT Token Authentication (Access Token + Refresh Token)
- Xác thực email
- OAuth đăng nhập (Google, Facebook)
- Phân quyền Admin/User

### 🛍️ Quản lý Sản phẩm
- CRUD sản phẩm với nhiều biến thể (variants)
- Quản lý danh mục sản phẩm
- Upload và quản lý hình ảnh sản phẩm
- Upload mô hình 3D (GLB/GLTF) cho sản phẩm
- Quản lý tồn kho (inventory)
- Thông số kỹ thuật sản phẩm (specifications)
- Tìm kiếm và lọc sản phẩm

### 🛒 Giỏ hàng và Đơn hàng
- Quản lý giỏ hàng (thêm, sửa, xóa)
- Tạo đơn hàng
- Đặt hàng cho khách (không cần đăng nhập)
- Theo dõi trạng thái đơn hàng
- Quản lý đơn hàng cho Admin

### 💳 Thanh toán
- Tích hợp PayOS để thanh toán trực tuyến
- Webhook xử lý kết quả thanh toán
- Hỗ trợ thanh toán COD

### ⭐ Đánh giá và Bình luận
- Đánh giá sản phẩm (rating)
- Bình luận sản phẩm
- Quản lý đánh giá/bình luận

### 🎁 Voucher
- Tạo và quản lý mã giảm giá
- Áp dụng voucher cho đơn hàng
- Theo dõi việc sử dụng voucher

### 💬 Chat AI
- Tích hợp Google Gemini AI
- Chat hỗ trợ khách hàng
- Gợi ý sản phẩm thông minh

### 📰 Tin tức
- Quản lý tin tức/blog
- Hiển thị tin tức cho người dùng

### ⚡ Tính năng khác
- Yêu thích sản phẩm (Wishlist)
- Health check endpoint
- Global exception handling
- CORS configuration
- File upload với static file serving

## 🛠️ Công nghệ sử dụng

### Framework & Runtime
- **.NET 8.0** - Framework chính
- **ASP.NET Core Web API** - Web API framework
- **Entity Framework Core 8.0** - ORM
- **PostgreSQL** - Database

### Libraries & Packages
- **MediatR** - Mediator pattern cho CQRS
- **AutoMapper** - Object mapping
- **FluentValidation** - Validation
- **JWT Bearer Authentication** - Authentication
- **Swagger/OpenAPI** - API documentation
- **PayOS SDK** - Payment integration
- **Google Gemini AI** - AI Chat integration
- **Npgsql** - PostgreSQL provider

### DevOps & Deployment
- **Docker** - Containerization
- **Fly.io** - Cloud deployment platform

## 🏗️ Kiến trúc dự án

Dự án sử dụng **Clean Architecture** với 4 lớp chính:

```
┌─────────────────────────────────────────┐
│         AtermisShop_API                 │  ← Presentation Layer
│      (Controllers, Middleware)          │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      AtermisShop.Application            │  ← Application Layer
│   (Commands, Queries, DTOs, Interfaces) │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│     AtermisShop.Infrastructure          │  ← Infrastructure Layer
│  (DbContext, Services, Repositories)    │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│       AtermisShop.Domain                │  ← Domain Layer
│      (Entities, Value Objects)          │
└─────────────────────────────────────────┘
```

### Nguyên tắc
- **Separation of Concerns**: Mỗi lớp có trách nhiệm riêng biệt
- **Dependency Inversion**: Lớp trên không phụ thuộc vào lớp dưới
- **CQRS Pattern**: Tách biệt Commands và Queries
- **Mediator Pattern**: Sử dụng MediatR để giảm coupling

## 🚀 Cài đặt và Chạy dự án

### Yêu cầu hệ thống
- .NET 8.0 SDK
- PostgreSQL (phiên bản 12 trở lên)
- Docker (tùy chọn, cho deployment)
- Git

### Các bước cài đặt

1. **Clone repository**
```bash
git clone <repository-url>
cd ArtemisShop_Backend
```

2. **Restore dependencies**
```bash
cd AtermisShop
dotnet restore
```

3. **Cấu hình database**
   - Tạo database PostgreSQL
   - Cập nhật connection string trong `appsettings.json` hoặc `appsettings.Development.json`

4. **Chạy migrations**
```bash
cd AtermisShop_API
dotnet ef database update --project ../AtermisShop.Infrastructure
```

5. **Chạy dự án**
```bash
dotnet run --project AtermisShop_API
```

API sẽ chạy tại: `https://localhost:5001` hoặc `http://localhost:5000`

6. **Truy cập Swagger UI**
```
https://localhost:5001/swagger
```

## ⚙️ Cấu hình

### appsettings.json

Các cấu hình cần thiết trong `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "User Id=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;Server=YOUR_DB_SERVER;Port=5432;Database=YOUR_DB_NAME;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Jwt": {
    "Issuer": "AtermisShop",
    "Audience": "AtermisShopFrontend",
    "Secret": "YOUR_JWT_SECRET_KEY_MIN_32_CHARACTERS_LONG",
    "AccessTokenMinutes": 60,
    "RefreshTokenDays": 30
  },
  "GoogleOAuth": {
    "ClientId": "YOUR_GOOGLE_OAUTH_CLIENT_ID"
  },
  "FacebookOAuth": {
    "AppId": "YOUR_FACEBOOK_APP_ID",
    "AppSecret": "YOUR_FACEBOOK_APP_SECRET"
  },
  "PayOS": {
    "ClientId": "YOUR_PAYOS_CLIENT_ID",
    "ApiKey": "YOUR_PAYOS_API_KEY",
    "ChecksumKey": "YOUR_PAYOS_CHECKSUM_KEY"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUsername": "YOUR_EMAIL@gmail.com",
    "SmtpPassword": "YOUR_EMAIL_APP_PASSWORD",
    "FromEmail": "YOUR_EMAIL@gmail.com",
    "FromName": "ARTEMIS Shop"
  },
  "FrontendUrl": "YOUR_FRONTEND_URL",
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY",
    "ModelName": "gemini-2.5-flash"
  }
}
```

### Environment Variables (cho Production)

Khi deploy, nên sử dụng environment variables thay vì hardcode trong file:
- `ConnectionStrings__DefaultConnection`
- `Jwt__Secret`
- `PayOS__ApiKey`
- `Gemini__ApiKey`
- etc.

## 📚 API Documentation

### Swagger UI

Sau khi chạy dự án, truy cập Swagger UI tại:
```
/swagger
```

Swagger cung cấp:
- Danh sách đầy đủ các endpoints
- Schema của request/response
- Khả năng test API trực tiếp
- Authentication với JWT Bearer token

### Các API Endpoints chính

#### Authentication
- `POST /api/auth/register` - Đăng ký
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/refresh-token` - Làm mới token
- `POST /api/auth/verify-email` - Xác thực email

#### Products
- `GET /api/products` - Lấy danh sách sản phẩm
- `GET /api/products/{id}` - Lấy chi tiết sản phẩm
- `POST /api/admin/products` - Tạo sản phẩm (Admin)
- `PUT /api/admin/products/{id}` - Cập nhật sản phẩm (Admin)
- `DELETE /api/admin/products/{id}` - Xóa sản phẩm (Admin)

#### Orders
- `GET /api/orders` - Lấy danh sách đơn hàng của user
- `POST /api/orders` - Tạo đơn hàng
- `GET /api/orders/{id}` - Lấy chi tiết đơn hàng

#### Cart
- `GET /api/cart` - Lấy giỏ hàng
- `POST /api/cart/items` - Thêm sản phẩm vào giỏ
- `PUT /api/cart/items/{id}` - Cập nhật số lượng
- `DELETE /api/cart/items/{id}` - Xóa sản phẩm khỏi giỏ

#### Payments
- `POST /api/payments/create` - Tạo thanh toán PayOS
- `POST /api/payments/webhook` - Webhook từ PayOS
- `POST /api/payments/return` - Return URL từ PayOS

#### Chat
- `POST /api/chat/message` - Gửi tin nhắn cho AI

Xem thêm chi tiết tại Swagger UI.

## 🚢 Deployment

### Docker

1. **Build Docker image**
```bash
docker build -t artemis-shop-api .
```

2. **Run container**
```bash
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="YOUR_CONNECTION_STRING" \
  -e Jwt__Secret="YOUR_JWT_SECRET" \
  artemis-shop-api
```

### Fly.io

Dự án đã được cấu hình sẵn để deploy lên Fly.io:

1. **Cài đặt Fly CLI**
```bash
# Windows (PowerShell)
iwr https://fly.io/install.ps1 -useb | iex
```

2. **Login**
```bash
fly auth login
```

3. **Deploy**
```bash
fly deploy
```

Cấu hình Fly.io trong `fly.toml`:
- App name: `customerbraceletwithgpswebsite-backend`
- Region: `sin` (Singapore)
- Memory: 1GB
- Port: 8080

### Environment Variables trên Fly.io

Set các biến môi trường:
```bash
fly secrets set ConnectionStrings__DefaultConnection="YOUR_CONNECTION_STRING"
fly secrets set Jwt__Secret="YOUR_JWT_SECRET"
fly secrets set PayOS__ApiKey="YOUR_PAYOS_API_KEY"
# ... các biến khác
```

## 📁 Cấu trúc dự án

```
ArtemisShop_Backend/
├── AtermisShop/
│   ├── AtermisShop_API/              # Presentation Layer
│   │   ├── Controllers/              # API Controllers
│   │   │   ├── Admin/               # Admin controllers
│   │   │   ├── AuthController.cs
│   │   │   ├── ProductsController.cs
│   │   │   ├── OrdersController.cs
│   │   │   └── ...
│   │   ├── Middleware/              # Custom middleware
│   │   ├── Swagger/                 # Swagger configuration
│   │   ├── Program.cs               # Application entry point
│   │   └── appsettings.json         # Configuration
│   │
│   ├── AtermisShop.Application/      # Application Layer
│   │   ├── Auth/                    # Authentication use cases
│   │   ├── Products/                # Product use cases
│   │   ├── Orders/                  # Order use cases
│   │   ├── Cart/                    # Cart use cases
│   │   ├── Payments/                # Payment use cases
│   │   ├── Chat/                    # Chat use cases
│   │   └── Common/                  # Shared interfaces
│   │
│   ├── AtermisShop.Domain/           # Domain Layer
│   │   ├── Products/                # Product entities
│   │   ├── Orders/                  # Order entities
│   │   ├── Users/                   # User entities
│   │   └── Common/                  # Base entities
│   │
│   └── AtermisShop.Infrastructure/   # Infrastructure Layer
│       ├── Persistence/             # DbContext, Repositories
│       ├── Auth/                    # JWT, Password hashing
│       ├── Payments/                # PayOS integration
│       ├── Services/                # Email, Gemini AI
│       └── Migrations/              # EF Core migrations
│
├── Dockerfile                        # Docker configuration
├── entrypoint.sh                     # Docker entrypoint script
├── fly.toml                          # Fly.io configuration
└── README.md                         # This file
```

## 🔒 Bảo mật

- JWT Authentication với Access Token và Refresh Token
- Password hashing với bcrypt
- CORS được cấu hình chỉ cho phép frontend domains cụ thể
- HTTPS enforcement
- Input validation với FluentValidation
- Global exception handling
- SQL injection protection với EF Core parameterized queries

## 🧪 Testing

Để test API, bạn có thể:
1. Sử dụng Swagger UI để test trực tiếp
2. Sử dụng Postman/Insomnia với file `.http` trong project
3. Viết unit tests và integration tests (có thể thêm sau)

## 📝 Ghi chú

- Database migrations sẽ tự động chạy khi ứng dụng khởi động
- Admin user sẽ được tự động tạo khi lần đầu chạy (seed data)
- File uploads được lưu tại `/data/uploads` trên Fly.io (sử dụng volume)

## 👥 Đóng góp

Mọi đóng góp đều được chào đón! Vui lòng tạo issue hoặc pull request.

## 📄 License

[Thêm license của bạn vào đây]

---

**Developed with ❤️ for Artemis Shop**

