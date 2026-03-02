# 📘 SecondBike — Backend API Documentation

> **Mục đích:** Tài liệu tổng hợp kiến trúc, công nghệ, phân công và luồng hoạt động của từng chức năng.
> Phục vụ cho việc **đọc hiểu code nhanh** và **present cho lecture**.

---

## 📑 Mục lục

1. [Tổng quan kiến trúc](#1-tổng-quan-kiến-trúc)
2. [Công nghệ sử dụng](#2-công-nghệ-sử-dụng)
3. [Cấu trúc thư mục dự án](#3-cấu-trúc-thư-mục-dự-án)
4. [Design Patterns & Nguyên tắc SOLID](#4-design-patterns--nguyên-tắc-solid)
5. [Thành viên 1 — Seller Core (Người bán & Sản phẩm)](#5-thành-viên-1--seller-core-người-bán--sản-phẩm)
6. [Thành viên 2 — Transaction Hub (Giao dịch & Thanh toán)](#6-thành-viên-2--transaction-hub-giao-dịch--thanh-toán)
7. [Thành viên 3 — Interaction (Tương tác & Cộng đồng)](#7-thành-viên-3--interaction-tương-tác--cộng-đồng)
8. [Thành viên 4 — Buyer Experience (Người mua & Tìm kiếm)](#8-thành-viên-4--buyer-experience-người-mua--tìm-kiếm)
9. [Thành viên 5 — Quality & Auth (Kiểm định & Xác thực)](#9-thành-viên-5--quality--auth-kiểm-định--xác-thực)
10. [Thành viên 6 — Admin Dashboard (Quản trị hệ thống)](#10-thành-viên-6--admin-dashboard-quản-trị-hệ-thống)
11. [Bảng tổng hợp API Endpoints](#11-bảng-tổng-hợp-api-endpoints)
12. [Database Schema tóm tắt](#12-database-schema-tóm-tắt)

---

## 1. Tổng quan kiến trúc

Dự án áp dụng **Clean Architecture** (hay còn gọi Onion Architecture), chia thành 4 layer:

```
┌─────────────────────────────────────────────────────┐
│                  SecondBike.Api                      │  ← Presentation Layer (Controllers, Program.cs)
│                   (.NET 9)                           │
├─────────────────────────────────────────────────────┤
│              SecondBike.Application                  │  ← Application Layer (DTOs, Interfaces, Validators)
│                   (.NET 8)                           │
├─────────────────────────────────────────────────────┤
│             SecondBike.Infrastructure                │  ← Infrastructure Layer (EF Core, Services, Repos)
│                   (.NET 8)                           │
├─────────────────────────────────────────────────────┤
│                SecondBike.Domain                     │  ← Domain Layer (Entities, Enums)
│                   (.NET 8)                           │
└─────────────────────────────────────────────────────┘
```

**Quy tắc phụ thuộc (Dependency Rule):**
- `Domain` → **không phụ thuộc ai** (lõi trong cùng).
- `Application` → phụ thuộc `Domain`.
- `Infrastructure` → phụ thuộc `Domain` + `Application`.
- `Api` → phụ thuộc `Application` + `Infrastructure`.

> Controller **không bao giờ** gọi trực tiếp Repository hay DbContext. Luồng luôn là:
> `Controller → Service Interface → Service Implementation → Repository → DbContext`.

---

## 2. Công nghệ sử dụng

| Công nghệ | Phiên bản | Vai trò |
|---|---|---|
| **ASP.NET Core Web API** | .NET 9 | Framework chính cho RESTful API |
| **Entity Framework Core** | 8.x | ORM — truy vấn CSDL |
| **SQL Server** | — | Cơ sở dữ liệu quan hệ |
| **ASP.NET Core Identity** | 8.x | Quản lý user, password hashing, role |
| **JWT Bearer Authentication** | 9.x | Xác thực bằng token (stateless) |
| **SignalR** | Built-in | Chat real-time (WebSocket) |
| **FluentValidation** | 12.x | Validate dữ liệu đầu vào (DTOs) |
| **Swashbuckle (Swagger)** | 10.x | Tự sinh API docs / test trên trình duyệt |
| **Newtonsoft / System.Text.Json** | Built-in | Serialize enum thành string |

---

## 3. Cấu trúc thư mục dự án

```
src/
├── SecondBike.Domain/                  ← DOMAIN LAYER
│   ├── Common/
│   │   └── BaseEntity.cs              # Entity gốc (Id, CreatedAt, UpdatedAt, IsDeleted, audit fields)
│   ├── Entities/
│   │   ├── AppUser.cs                 # Người dùng (Buyer/Seller/Inspector/Admin)
│   │   ├── BikePost.cs               # Tin đăng bán xe
│   │   ├── BikeImage.cs              # Ảnh của tin đăng
│   │   ├── Order.cs                  # Đơn hàng (deposit, full-payment)
│   │   ├── Payment.cs               # Giao dịch thanh toán
│   │   ├── Message.cs               # Tin nhắn chat
│   │   ├── Rating.cs                # Đánh giá người bán
│   │   ├── Wishlist.cs              # Danh sách yêu thích
│   │   ├── Wallet.cs                # Ví tiền nội bộ
│   │   ├── WalletTransaction.cs     # Lịch sử giao dịch ví
│   │   └── InspectionReport.cs      # Báo cáo kiểm định xe
│   └── Enums/
│       ├── UserRole.cs              # Buyer, Seller, Inspector, Admin
│       ├── UserStatus.cs            # Active, Suspended, Banned, Deleted
│       ├── PostStatus.cs            # Draft → PendingModeration → Active → Sold/Hidden/Rejected
│       ├── OrderStatus.cs           # Pending → DepositPaid → FullyPaid → Shipping → Completed
│       ├── PaymentType.cs           # Deposit, FinalPayment, FullPayment, Refund
│       ├── PaymentMethod.cs         # CreditCard, BankTransfer, EWallet, Cash
│       ├── PaymentGateway.cs        # VNPay, Momo, ZaloPay, Stripe, PayPal
│       ├── PaymentStatus.cs         # Pending, Processing, Completed, Failed, Refunded
│       ├── BikeCategory.cs          # RoadBike, MountainBike, Hybrid, Electric...
│       ├── BikeSize.cs              # XS, S, M, L, XL
│       ├── BikeCondition.cs         # New, LikeNew, Excellent, Good, Fair, NeedsRepair
│       ├── MessageType.cs           # Text, Image, File, System
│       ├── InspectionStatus.cs      # Pending, InProgress, Completed, Rejected
│       ├── OverallCondition.cs      # Excellent → Poor (đánh giá tổng thể khi kiểm định)
│       ├── DisputeStatus.cs         # Open, UnderReview, Resolved, Closed
│       └── WalletTransactionType.cs # Deposit, Withdrawal, Refund, Payment, Commission
│
├── SecondBike.Application/            ← APPLICATION LAYER
│   ├── Common/
│   │   ├── Result.cs                 # Result<T> pattern — tránh throw exception cho lỗi nghiệp vụ
│   │   └── PagedResult.cs            # Kết quả phân trang (Items, TotalCount, Page, PageSize)
│   ├── DTOs/
│   │   ├── Bikes/                    # CreateBikePostDto, UpdateBikePostDto, BikePostDto, BikeFilterDto
│   │   ├── Orders/                   # CreateOrderDto, OrderDto, ProcessPaymentDto
│   │   ├── Chat/                     # SendMessageDto, MessageDto, ConversationDto
│   │   ├── Ratings/                  # CreateRatingDto, RatingDto
│   │   ├── Users/                    # RegisterDto, LoginDto, AuthResultDto, UserProfileDto, UpdateProfileDto
│   │   ├── Inspections/              # CreateInspectionDto, InspectionReportDto
│   │   └── Admin/                    # ModeratePostDto, ResolveDisputeDto, AdminUserDto, DashboardStatsDto
│   ├── Interfaces/
│   │   ├── IRepository.cs            # Generic CRUD (GetById, Find, Add, Update, Delete, Any, Count)
│   │   ├── IUnitOfWork.cs            # SaveChangesAsync — commit transaction
│   │   └── Services/
│   │       ├── IBikePostService.cs   # Seller Core
│   │       ├── IBikeSearchService.cs # Buyer Experience
│   │       ├── IOrderService.cs      # Transaction Hub
│   │       ├── IMessageService.cs    # Interaction (Chat)
│   │       ├── IRatingService.cs     # Interaction (Rating)
│   │       ├── IWishlistService.cs   # Buyer Experience
│   │       ├── IAuthService.cs       # Quality & Auth
│   │       ├── IJwtService.cs        # JWT token generation
│   │       ├── IInspectionService.cs # Quality & Auth (Inspector)
│   │       └── IAdminService.cs      # Admin Dashboard
│   ├── Validators/
│   │   ├── CreateBikePostValidator.cs  # Validate tin đăng (Title, Price, Brand, Images...)
│   │   ├── RegisterValidator.cs        # Validate đăng ký (Email, Password policy)
│   │   └── CreateRatingValidator.cs    # Validate đánh giá (Stars 1-5)
│   └── DependencyInjection.cs         # AddApplication() — đăng ký FluentValidation
│
├── SecondBike.Infrastructure/         ← INFRASTRUCTURE LAYER
│   ├── Data/
│   │   ├── AppDbContext.cs            # IdentityDbContext + audit fields + soft-delete filter
│   │   └── Configurations/           # EF Core Fluent API (table mapping, index, FK, precision)
│   ├── Repositories/
│   │   ├── Repository.cs             # Generic Repository<T> — dùng DbSet<T>
│   │   └── UnitOfWork.cs             # Wrap AppDbContext.SaveChangesAsync()
│   ├── Services/                     # Tất cả service implementation
│   │   ├── BikePostService.cs
│   │   ├── BikeSearchService.cs
│   │   ├── OrderService.cs
│   │   ├── MessageService.cs
│   │   ├── RatingService.cs
│   │   ├── WishlistService.cs
│   │   ├── AuthService.cs
│   │   ├── JwtService.cs
│   │   ├── InspectionService.cs
│   │   └── AdminService.cs
│   ├── Hubs/
│   │   └── ChatHub.cs                # SignalR Hub — real-time messaging
│   └── DependencyInjection.cs        # AddInfrastructure() — đăng ký tất cả DI
│
└── SecondBike.Api/                    ← PRESENTATION LAYER
    ├── Controllers/
    │   ├── BaseApiController.cs       # Helper: GetCurrentUserId(), ToResponse()
    │   ├── AuthController.cs          # /api/auth/*
    │   ├── BikesController.cs         # /api/bikes/*
    │   ├── OrdersController.cs        # /api/orders/*
    │   ├── MessagesController.cs      # /api/messages/*
    │   ├── RatingsController.cs       # /api/ratings/*
    │   ├── WishlistController.cs      # /api/wishlist/*
    │   ├── InspectionsController.cs   # /api/inspections/*
    │   └── AdminController.cs         # /api/admin/*
    ├── Program.cs                     # Pipeline: CORS → JWT → SignalR → Swagger → Seed roles
    └── appsettings.json               # Connection string + JWT config
```

---

## 4. Design Patterns & Nguyên tắc SOLID

### 4.1. Design Patterns được áp dụng

| Pattern | File(s) | Mô tả |
|---|---|---|
| **Repository Pattern** | `IRepository<T>`, `Repository<T>` | Tách logic truy vấn DB khỏi business logic. Controller/Service không biết EF Core tồn tại. |
| **Unit of Work** | `IUnitOfWork`, `UnitOfWork` | Gom nhiều thay đổi (nhiều repo) vào 1 transaction duy nhất, gọi `SaveChangesAsync()` một lần. |
| **Result Pattern** | `Result<T>`, `Result` | Trả về kết quả thành công/thất bại thay vì throw exception. FE nhận được lỗi có cấu trúc rõ ràng. |
| **DTO Pattern** | Thư mục `DTOs/` | Tách biệt Entity (DB) với dữ liệu trả về/nhận vào API. Không bao giờ expose Entity trực tiếp. |
| **Dependency Injection** | `DependencyInjection.cs` (2 file) | Đăng ký tất cả service, repo qua DI container. Không `new` trực tiếp. |
| **Soft Delete** | `BaseEntity.IsDeleted`, `AppDbContext` | Không xóa vật lý. Đánh cờ `IsDeleted = true` + Global Query Filter tự động loại trừ. |

### 4.2. Nguyên tắc SOLID

| Nguyên tắc | Áp dụng trong dự án |
|---|---|
| **S — Single Responsibility** | Mỗi Service chỉ xử lý 1 nhóm nghiệp vụ (BikePostService chỉ lo CRUD bài đăng, OrderService chỉ lo đơn hàng). Mỗi Controller chỉ gọi 1-2 service liên quan. |
| **O — Open/Closed** | Thêm service mới (VD: `IPaymentGatewayService`) chỉ cần tạo interface + implementation, đăng ký DI. Không sửa code cũ. |
| **L — Liskov Substitution** | Tất cả entity kế thừa `BaseEntity`. `Repository<T>` hoạt động đúng cho mọi entity con. |
| **I — Interface Segregation** | Mỗi service có interface riêng (`IBikePostService`, `IOrderService`...). Controller chỉ inject interface cần dùng, không phụ thuộc method thừa. |
| **D — Dependency Inversion** | Controller phụ thuộc interface (`IBikePostService`), KHÔNG phụ thuộc class cụ thể (`BikePostService`). Có thể swap implementation bất kỳ lúc nào. |

---

## 5. Thành viên 1 — Seller Core (Người bán & Sản phẩm)

### 📋 Chức năng
| UC | Mô tả |
|---|---|
| **Post New Bike** | Seller tạo tin đăng bán xe mới (nhập thông tin kỹ thuật + upload ảnh) |
| **Manage Post** | Sửa / Xóa / Ẩn / Gửi duyệt tin đăng |

### 🔧 Công nghệ áp dụng từng function

| Function | Công nghệ | Giải thích |
|---|---|---|
| `CreateAsync()` | EF Core (Add), Repository Pattern, Unit of Work | Tạo `BikePost` + tạo nhiều `BikeImage` trong cùng 1 transaction |
| `UpdateAsync()` | EF Core (Update), Owner check | So sánh `SellerId` để đảm bảo chỉ chủ sở hữu mới sửa được |
| `DeleteAsync()` | Soft Delete | Gọi `Repository.Delete()` → `AppDbContext` tự chuyển thành `IsDeleted = true` |
| `ToggleVisibilityAsync()` | Enum state toggle | Chuyển đổi `PostStatus.Active ↔ PostStatus.Hidden` |
| `SubmitForModerationAsync()` | Enum state change | Đổi status thành `PendingModeration` để Admin duyệt |
| `GetBySellerAsync()` | EF Core Include | `FindWithIncludesAsync` load kèm navigation `Images` |
| `FluentValidation` | `CreateBikePostValidator` | Validate: Title ≤ 200 ký tự, Price > 0, Year hợp lệ, ít nhất 1 ảnh, tối đa 10 ảnh |

### 🔄 Luồng hoạt động: Đăng tin xe mới

```
React FE                    API                         Service                      Database
   │                         │                            │                             │
   │  POST /api/bikes        │                            │                             │
   │  { title, price, ... }  │                            │                             │
   │ ─────────────────────►  │                            │                             │
   │                         │  [JWT Authorize]           │                             │
   │                         │  GetCurrentUserId()        │                             │
   │                         │  ──────────────────────►   │                             │
   │                         │                            │  _userRepo.GetByIdAsync()   │
   │                         │                            │  ─────────────────────────► │
   │                         │                            │  ◄───────── seller found ── │
   │                         │                            │                             │
   │                         │                            │  new BikePost { ... }       │
   │                         │                            │  _postRepo.AddAsync()       │
   │                         │                            │  ─────────────────────────► │
   │                         │                            │                             │
   │                         │                            │  foreach ImageUrl:          │
   │                         │                            │    _imageRepo.AddAsync()    │
   │                         │                            │  ─────────────────────────► │
   │                         │                            │                             │
   │                         │                            │  _uow.SaveChangesAsync()   │
   │                         │                            │  ─────────────────────────► │
   │                         │                            │  ◄──── committed ────────── │
   │                         │                            │                             │
   │                         │  ◄── Result<BikePostDto> ──│                             │
   │  ◄── 200 OK { data } ──│                            │                             │
```

### 📁 Các file liên quan

| Layer | File |
|---|---|
| Controller | `BikesController.cs` — endpoint `POST`, `PUT`, `DELETE`, `PATCH .../visibility`, `PATCH .../submit`, `GET my-posts` |
| Interface | `IBikePostService.cs` |
| Implementation | `BikePostService.cs` |
| DTOs | `CreateBikePostDto.cs`, `UpdateBikePostDto.cs`, `BikePostDto.cs` |
| Validator | `CreateBikePostValidator.cs` |
| Entities | `BikePost.cs`, `BikeImage.cs` |
| Config | `BikePostConfiguration.cs`, `BikeImageConfiguration.cs` |

---

## 6. Thành viên 2 — Transaction Hub (Giao dịch & Thanh toán)

### 📋 Chức năng
| UC | Mô tả |
|---|---|
| **Place Order & Deposit** | Buyer đặt hàng, hệ thống tính tiền cọc (10-30% giá xe) |
| **Process Payment** | Xử lý thanh toán cọc hoặc thanh toán còn lại |

### 🔧 Công nghệ áp dụng từng function

| Function | Công nghệ | Giải thích |
|---|---|---|
| `PlaceOrderAsync()` | Business rule validation, `Math.Clamp()` | Kiểm tra: xe Active, không mua xe mình, tính depositAmount = price × clamp(pct, 10, 30) |
| `ProcessPaymentAsync()` | EF Core transaction, State machine | Tạo record `Payment`, cập nhật `Order.Status` (Pending → DepositPaid → FullyPaid) |
| `ConfirmDeliveryAsync()` | Cross-entity update | Đổi Order → Completed + BikePost → Sold trong cùng transaction |
| `CancelOrderAsync()` | Authorization check | Kiểm tra user là Buyer hoặc Seller mới được cancel |
| `OpenDisputeAsync()` | State change + flag | Set `HasDispute = true`, status → `Disputed` |
| Order Number generation | String format | `$"SB-{DateTime:yyyyMMdd}-{Guid[..6]}"` — mã đơn hàng duy nhất |

### 🔄 Luồng hoạt động: Đặt hàng & Thanh toán

```
                              ┌──────────────┐
                              │   Pending    │ ← PlaceOrderAsync()
                              └──────┬───────┘
                                     │ ProcessPaymentAsync(Deposit)
                              ┌──────▼───────┐
                              │ DepositPaid  │
                              └──────┬───────┘
                                     │ ProcessPaymentAsync(FinalPayment)
                              ┌──────▼───────┐
                              │  FullyPaid   │
                              └──────┬───────┘
                                     │ (Seller ships)
                              ┌──────▼───────┐
                              │   Shipping   │
                              └──────┬───────┘
                                     │ ConfirmDeliveryAsync()
                              ┌──────▼───────┐
                              │  Completed   │ → BikePost.Status = Sold
                              └──────────────┘

        Nhánh lỗi:
        ─── CancelOrderAsync()  ──► Cancelled
        ─── OpenDisputeAsync()  ──► Disputed ──► Admin ResolveDispute ──► Refunded / Completed
```

### 📁 Các file liên quan

| Layer | File |
|---|---|
| Controller | `OrdersController.cs` — tất cả endpoint đều `[Authorize]` |
| Interface | `IOrderService.cs` |
| Implementation | `OrderService.cs` |
| DTOs | `CreateOrderDto.cs`, `OrderDto.cs`, `ProcessPaymentDto.cs` |
| Entities | `Order.cs`, `Payment.cs` |
| Config | `OrderConfiguration.cs`, `PaymentConfiguration.cs` |
| Enums | `OrderStatus.cs`, `PaymentType.cs`, `PaymentMethod.cs`, `PaymentGateway.cs`, `PaymentStatus.cs` |

---

## 7. Thành viên 3 — Interaction (Tương tác & Cộng đồng)

### 📋 Chức năng
| UC | Mô tả |
|---|---|
| **Chat / Messaging** | Nhắn tin real-time giữa Buyer và Seller (SignalR WebSocket) |
| **Rate Seller** | Buyer đánh giá Seller sau khi hoàn thành đơn hàng (1-5 sao) |

### 🔧 Công nghệ áp dụng từng function

#### Chat / Messaging

| Function | Công nghệ | Giải thích |
|---|---|---|
| `SendAsync()` (REST) | EF Core, Repository | Lưu message vào DB, trả về `MessageDto` |
| `ChatHub.SendMessage()` (SignalR) | **SignalR WebSocket** | Gửi real-time: `Clients.Group(receiverId).SendAsync("ReceiveMessage", data)` |
| `ChatHub.JoinChat()` | SignalR Groups | User join group = userId → nhận tin nhắn riêng |
| `ChatHub.MarkAsRead()` | SignalR + DB | Đánh dấu đã đọc + thông báo bên kia `"MessagesRead"` |
| `GetConversationsAsync()` | LINQ GroupBy | Gom messages theo cặp user, lấy tin cuối + đếm unread |
| JWT qua query string | `JwtBearerEvents.OnMessageReceived` | SignalR gửi token qua `?access_token=xxx` thay vì header |

#### Rate Seller

| Function | Công nghệ | Giải thích |
|---|---|---|
| `CreateAsync()` | Business validation | Chỉ Buyer mới rate, Order phải Completed, mỗi Order chỉ rate 1 lần |
| Average rating update | Aggregate calculation | Tính lại `seller.SellerRating = sum(stars) / count` mỗi khi có rating mới |
| `RespondAsync()` | Owner check | Chỉ Seller (ToUserId) mới được phản hồi rating của mình |
| `FluentValidation` | `CreateRatingValidator` | Stars: 1-5, sub-ratings: 1-5 (optional), Comment ≤ 1000 ký tự |

### 🔄 Luồng hoạt động: Chat Real-time

```
Buyer (React)                 SignalR Hub                    Database
    │                             │                             │
    │  connection.start()         │                             │
    │  ──────────────────────►    │                             │
    │                             │                             │
    │  JoinChat(buyerId)          │                             │
    │  ──────────────────────►    │                             │
    │                     Groups.Add(connectionId, buyerId)     │
    │                             │                             │
    │  SendMessage(senderId, dto) │                             │
    │  ──────────────────────►    │                             │
    │                             │  _messageService.SendAsync()│
    │                             │  ─────────────────────────► │
    │                             │  ◄──── MessageDto ───────── │
    │                             │                             │
    │                             │  Clients.Group(receiverId)  │
    │                             │    .SendAsync("ReceiveMessage", data)
    │                             │  ──────────────────────────────────► Seller (React)
    │                             │                             │
    │                             │  Clients.Group(senderId)    │
    │  ◄── "ReceiveMessage" ──────│    .SendAsync (multi-device sync)
```

### 📁 Các file liên quan

| Layer | File |
|---|---|
| Controller | `MessagesController.cs` (REST), `RatingsController.cs` |
| Hub | `ChatHub.cs` (SignalR real-time) |
| Interface | `IMessageService.cs`, `IRatingService.cs` |
| Implementation | `MessageService.cs`, `RatingService.cs` |
| DTOs | `SendMessageDto.cs`, `MessageDto.cs`, `ConversationDto.cs`, `CreateRatingDto.cs`, `RatingDto.cs` |
| Validator | `CreateRatingValidator.cs` |
| Entities | `Message.cs`, `Rating.cs` |
| Config | `MessageConfiguration.cs`, `RatingConfiguration.cs` |

---

## 8. Thành viên 4 — Buyer Experience (Người mua & Tìm kiếm)

### 📋 Chức năng
| UC | Mô tả |
|---|---|
| **Research Bikes & Filter** | Tìm kiếm, lọc xe theo nhiều tiêu chí + phân trang |
| **View Detail & Wishlist** | Xem chi tiết xe + Lưu vào danh sách yêu thích |

### 🔧 Công nghệ áp dụng từng function

| Function | Công nghệ | Giải thích |
|---|---|---|
| `SearchAsync()` | **EF Core IQueryable chaining** | Build query động: Where → Where → OrderBy → Skip/Take. Chỉ 1 câu SQL duy nhất gửi đến DB |
| Filter by SearchTerm | `string.Contains()` → SQL `LIKE` | Tìm trong Title, Brand, Model, Description |
| Sort options | Switch expression | `"price_asc"`, `"price_desc"`, `"oldest"`, `"popular"`, `"newest"` (default) |
| Pagination | `PagedResult<T>` | Trả về `Items`, `TotalCount`, `Page`, `PageSize`, `TotalPages`, `HasPrevious`, `HasNext` |
| `GetDetailAsync()` | EF Core Include + ViewCount++ | Load BikePost + Images, tăng ViewCount mỗi lần xem |
| `GetBrandsAsync()` | LINQ Distinct | Lấy danh sách brand unique từ các bài Active (cho dropdown filter) |
| `GetCitiesAsync()` | LINQ Distinct | Lấy danh sách city unique (cho dropdown filter) |
| `AddAsync()` (Wishlist) | Unique check + counter | Kiểm tra trùng, tạo Wishlist record, tăng `BikePost.WishlistCount` |
| `RemoveAsync()` (Wishlist) | Delete + counter | Xóa Wishlist record, giảm `BikePost.WishlistCount` |
| `IsInWishlistAsync()` | `AnyAsync()` | Kiểm tra nhanh xe có trong wishlist không (cho nút ❤️ trên FE) |

### 🔄 Luồng hoạt động: Tìm kiếm & Lọc

```
React FE                          API                          BikeSearchService                    Database
   │                               │                                │                                │
   │  GET /api/bikes?              │                                │                                │
   │    category=RoadBike          │                                │                                │
   │    &minPrice=5000000          │                                │                                │
   │    &maxPrice=20000000         │                                │                                │
   │    &city=HCM                  │                                │                                │
   │    &sortBy=price_asc          │                                │                                │
   │    &page=1&pageSize=12        │                                │                                │
   │  ────────────────────────►    │                                │                                │
   │                               │  SearchAsync(filter)           │                                │
   │                               │  ────────────────────────────► │                                │
   │                               │                                │  IQueryable<BikePost> query    │
   │                               │                                │    .Where(Active, !Deleted)    │
   │                               │                                │    .Where(Category == Road)    │
   │                               │                                │    .Where(Price >= 5M)         │
   │                               │                                │    .Where(Price <= 20M)        │
   │                               │                                │    .Where(City == "HCM")       │
   │                               │                                │    .OrderBy(Price)             │
   │                               │                                │    .CountAsync()               │
   │                               │                                │    .Skip(0).Take(12)           │
   │                               │                                │  ──────────────────────────►   │
   │                               │                                │  ◄──── rows + count ─────────  │
   │                               │                                │                                │
   │                               │  ◄── PagedResult<BikePostDto> ─│                                │
   │  ◄── 200 OK { items, total,   │                                │                                │
   │       page, pageSize,         │                                │                                │
   │       totalPages, hasNext }   │                                │                                │
```

### 📁 Các file liên quan

| Layer | File |
|---|---|
| Controller | `BikesController.cs` — `GET /`, `GET /{id}`, `GET /brands`, `GET /cities` (public) |
| Controller | `WishlistController.cs` — `GET`, `POST /{id}`, `DELETE /{id}`, `GET /{id}/check` |
| Interface | `IBikeSearchService.cs`, `IWishlistService.cs` |
| Implementation | `BikeSearchService.cs`, `WishlistService.cs` |
| DTOs | `BikeFilterDto.cs`, `BikePostDto.cs`, `BikeImageDto.cs` |
| Common | `PagedResult.cs` |
| Entities | `BikePost.cs`, `BikeImage.cs`, `Wishlist.cs` |

---

## 9. Thành viên 5 — Quality & Auth (Kiểm định & Xác thực)

### 📋 Chức năng
| UC | Mô tả |
|---|---|
| **Register / Login / Profile** | Đăng ký, đăng nhập (JWT), quản lý hồ sơ cá nhân |
| **Inspect Vehicle & Report** | Inspector kiểm tra xe, tạo báo cáo kiểm định |

### 🔧 Công nghệ áp dụng từng function

#### Authentication

| Function | Công nghệ | Giải thích |
|---|---|---|
| `RegisterAsync()` | **ASP.NET Core Identity** + JWT | `UserManager.CreateAsync()` tạo IdentityUser → hash password (PBKDF2) → tạo AppUser → tạo Wallet → generate JWT |
| `LoginAsync()` | **SignInManager** + JWT | `PasswordSignInAsync()` verify password → lockout 5 lần → map IdentityUser → AppUser → generate JWT |
| `JwtService.GenerateToken()` | **System.IdentityModel.Tokens.Jwt** | Claims: NameIdentifier (UserId), Email, Name, Role. Expire: 24h. Algorithm: HMAC-SHA256 |
| Password policy | Identity Options | Tối thiểu 8 ký tự, uppercase + lowercase + digit + special character |
| Role seeding | `Program.cs` startup | Tự động tạo 4 role: Buyer, Seller, Inspector, Admin khi app khởi động |
| `FluentValidation` | `RegisterValidator` | Email format, password policy, FullName ≤ 100 |

#### Inspection

| Function | Công nghệ | Giải thích |
|---|---|---|
| `CreateAsync()` | Role check + unique check | Chỉ `UserRole.Inspector` mới tạo được. Mỗi BikePost chỉ có 1 report. |
| `CompleteAsync()` | State change + timestamp | Status → Completed, set `InspectedAt = UtcNow` |
| Component scores | Domain model | 6 điểm thành phần (Frame, Brakes, Gears, Wheels, Tires, Chain) từ 1-10 |
| Report number | String format | `$"INS-{DateTime:yyyyMMdd}-{Guid[..6]}"` |

### 🔄 Luồng hoạt động: Đăng ký & Đăng nhập

```
React FE                       AuthController               AuthService                    Identity DB
   │                               │                            │                              │
   │  POST /api/auth/register      │                            │                              │
   │  { email, password,           │                            │                              │
   │    fullName, role }           │                            │                              │
   │  ────────────────────────►    │                            │                              │
   │                               │  RegisterAsync(dto)        │                              │
   │                               │  ────────────────────────► │                              │
   │                               │                            │  UserManager.CreateAsync()   │
   │                               │                            │  (hash password PBKDF2)      │
   │                               │                            │  ─────────────────────────►  │
   │                               │                            │  ◄──── IdentityUser ───────  │
   │                               │                            │                              │
   │                               │                            │  AddToRoleAsync("Buyer")     │
   │                               │                            │  ─────────────────────────►  │
   │                               │                            │                              │
   │                               │                            │  Create AppUser + Wallet     │
   │                               │                            │  _uow.SaveChangesAsync()     │
   │                               │                            │  ─────────────────────────►  │
   │                               │                            │                              │
   │                               │                            │  _jwtService.GenerateToken() │
   │                               │                            │  → JWT (24h, HMAC-SHA256)    │
   │                               │                            │                              │
   │  ◄── 200 { token, user } ────│  ◄── AuthResultDto ────────│                              │
   │                               │                            │                              │
   │  (FE lưu token vào localStorage, gắn vào header Authorization: Bearer xxx)                │
```

### 📁 Các file liên quan

| Layer | File |
|---|---|
| Controller | `AuthController.cs` — `POST register`, `POST login`, `GET profile`, `PUT profile`, `POST logout` |
| Controller | `InspectionsController.cs` — `[Authorize(Roles = "Inspector")]` |
| Interface | `IAuthService.cs`, `IJwtService.cs`, `IInspectionService.cs` |
| Implementation | `AuthService.cs`, `JwtService.cs`, `InspectionService.cs` |
| DTOs | `RegisterDto`, `LoginDto`, `AuthResultDto`, `UserProfileDto`, `UpdateProfileDto`, `CreateInspectionDto`, `InspectionReportDto` |
| Validator | `RegisterValidator.cs` |
| Entities | `AppUser.cs`, `Wallet.cs`, `InspectionReport.cs` |
| Config | `AppUserConfiguration.cs`, `WalletConfiguration.cs`, `InspectionReportConfiguration.cs` |
| Startup | `Program.cs` — JWT config, Role seeding |

---

## 10. Thành viên 6 — Admin Dashboard (Quản trị hệ thống)

### 📋 Chức năng
| UC | Mô tả |
|---|---|
| **Moderate Posts** | Duyệt / Từ chối tin đăng của Seller |
| **Resolve Disputes & User Management** | Xử lý tranh chấp, khóa tài khoản, hoàn tiền, quản lý user |

### 🔧 Công nghệ áp dụng từng function

| Function | Công nghệ | Giải thích |
|---|---|---|
| `GetDashboardStatsAsync()` | Aggregate queries | `CountAsync()` cho TotalUsers, ActivePosts, PendingModerations, OpenDisputes + `Sum()` cho revenue |
| `GetPendingPostsAsync()` | Filter + Join | Lọc `PostStatus.PendingModeration`, join Seller để lấy tên |
| `ModeratePostAsync()` | State machine | Approve → `Active` + set `PublishedAt` / Reject → `Rejected` + set `RejectionReason` |
| `GetUsersAsync()` | Optional filter | Lọc theo Role (hoặc lấy tất cả), đếm TotalPosts + TotalOrders cho mỗi user |
| `UpdateUserStatusAsync()` | Enum update | Đổi `UserStatus` (Active / Suspended / Banned) |
| `ResolveDisputeAsync()` | Cross-entity + conditional | Set resolution, nếu `RefundBuyer` → Order = Refunded, nếu `BanSeller` → Seller.Status = Banned |
| `GetDisputedOrdersAsync()` | Filter | Lọc `HasDispute = true` và `DisputeResolvedAt == null` |
| Authorization | `[Authorize(Roles = "Admin")]` | Toàn bộ controller chỉ Admin mới truy cập được |

### 🔄 Luồng hoạt động: Duyệt tin đăng

```
Admin (React)                AdminController              AdminService                  Database
   │                             │                            │                            │
   │  GET /api/admin/posts/      │                            │                            │
   │      pending                │                            │                            │
   │  ──────────────────────►    │                            │                            │
   │                             │  [Authorize(Roles="Admin")]│                            │
   │                             │  GetPendingPostsAsync()    │                            │
   │                             │  ────────────────────────► │                            │
   │                             │                            │  FindAsync(PendingMod)     │
   │                             │                            │  ─────────────────────►    │
   │                             │                            │  ◄──── posts ────────────  │
   │  ◄── List<PendingPostDto> ──│  ◄──────────────────────── │                            │
   │                             │                            │                            │
   │  POST /api/admin/posts/     │                            │                            │
   │       moderate              │                            │                            │
   │  { bikePostId, approve:     │                            │                            │
   │    true, notes: "OK" }      │                            │                            │
   │  ──────────────────────►    │                            │                            │
   │                             │  ModeratePostAsync()       │                            │
   │                             │  ────────────────────────► │                            │
   │                             │                            │  post.Status = Active      │
   │                             │                            │  post.PublishedAt = UtcNow │
   │                             │                            │  post.ModeratedBy = adminId│
   │                             │                            │  _uow.SaveChangesAsync()   │
   │                             │                            │  ─────────────────────►    │
   │  ◄── 200 { "Success" } ────│  ◄──────────────────────── │                            │

(Tin đăng giờ đã Active, Buyer có thể thấy khi tìm kiếm)
```

### 📁 Các file liên quan

| Layer | File |
|---|---|
| Controller | `AdminController.cs` — `[Authorize(Roles = "Admin")]` cho toàn bộ |
| Interface | `IAdminService.cs` |
| Implementation | `AdminService.cs` |
| DTOs | `ModeratePostDto`, `ResolveDisputeDto`, `AdminUserDto`, `PendingPostDto`, `DashboardStatsDto` |
| Entities | `BikePost.cs` (moderation fields), `Order.cs` (dispute fields), `AppUser.cs` (status) |

---

## 11. Bảng tổng hợp API Endpoints

### 🔓 Public (Không cần đăng nhập)

| Method | URL | Mô tả | Controller |
|---|---|---|---|
| `POST` | `/api/auth/register` | Đăng ký tài khoản | AuthController |
| `POST` | `/api/auth/login` | Đăng nhập, nhận JWT | AuthController |
| `GET` | `/api/bikes` | Tìm kiếm & lọc xe (phân trang) | BikesController |
| `GET` | `/api/bikes/{id}` | Xem chi tiết xe | BikesController |
| `GET` | `/api/bikes/brands` | Danh sách hãng xe | BikesController |
| `GET` | `/api/bikes/cities` | Danh sách thành phố | BikesController |
| `GET` | `/api/ratings/seller/{sellerId}` | Xem đánh giá của seller | RatingsController |
| `GET` | `/api/inspections/bike/{bikePostId}` | Xem báo cáo kiểm định | InspectionsController |

### 🔐 Authenticated (Cần JWT token)

| Method | URL | Mô tả | Controller |
|---|---|---|---|
| `GET` | `/api/auth/profile` | Xem profile cá nhân | AuthController |
| `PUT` | `/api/auth/profile` | Cập nhật profile | AuthController |
| `POST` | `/api/auth/logout` | Đăng xuất | AuthController |
| `POST` | `/api/bikes` | Đăng tin xe mới | BikesController |
| `PUT` | `/api/bikes` | Sửa tin đăng | BikesController |
| `DELETE` | `/api/bikes/{id}` | Xóa tin đăng | BikesController |
| `PATCH` | `/api/bikes/{id}/visibility` | Ẩn/Hiện tin | BikesController |
| `PATCH` | `/api/bikes/{id}/submit` | Gửi duyệt | BikesController |
| `GET` | `/api/bikes/my-posts` | Xem tin đăng của mình | BikesController |
| `POST` | `/api/orders` | Đặt hàng | OrdersController |
| `GET` | `/api/orders/{id}` | Xem đơn hàng | OrdersController |
| `GET` | `/api/orders/my-purchases` | Đơn đã mua | OrdersController |
| `GET` | `/api/orders/my-sales` | Đơn đã bán | OrdersController |
| `POST` | `/api/orders/{id}/cancel` | Hủy đơn | OrdersController |
| `POST` | `/api/orders/{id}/confirm-delivery` | Xác nhận nhận hàng | OrdersController |
| `POST` | `/api/orders/payment` | Thanh toán | OrdersController |
| `POST` | `/api/orders/{id}/dispute` | Mở tranh chấp | OrdersController |
| `POST` | `/api/messages` | Gửi tin nhắn (REST) | MessagesController |
| `GET` | `/api/messages/conversations` | Danh sách hội thoại | MessagesController |
| `GET` | `/api/messages/conversations/{userId}` | Xem hội thoại cụ thể | MessagesController |
| `PATCH` | `/api/messages/conversations/{userId}/read` | Đánh dấu đã đọc | MessagesController |
| `GET` | `/api/messages/unread-count` | Đếm tin chưa đọc | MessagesController |
| `POST` | `/api/ratings` | Đánh giá seller | RatingsController |
| `POST` | `/api/ratings/{id}/respond` | Seller phản hồi | RatingsController |
| `GET` | `/api/wishlist` | Xem wishlist | WishlistController |
| `POST` | `/api/wishlist/{bikePostId}` | Thêm vào wishlist | WishlistController |
| `DELETE` | `/api/wishlist/{bikePostId}` | Xóa khỏi wishlist | WishlistController |
| `GET` | `/api/wishlist/{bikePostId}/check` | Kiểm tra trong wishlist | WishlistController |

### 🔑 Role-based (Cần role cụ thể)

| Method | URL | Role | Mô tả | Controller |
|---|---|---|---|---|
| `POST` | `/api/inspections` | Inspector | Tạo báo cáo kiểm định | InspectionsController |
| `GET` | `/api/inspections/my-reports` | Inspector | Xem reports của mình | InspectionsController |
| `PATCH` | `/api/inspections/{id}/complete` | Inspector | Hoàn thành kiểm định | InspectionsController |
| `GET` | `/api/admin/dashboard` | Admin | Thống kê tổng quan | AdminController |
| `GET` | `/api/admin/posts/pending` | Admin | Tin chờ duyệt | AdminController |
| `POST` | `/api/admin/posts/moderate` | Admin | Duyệt/Từ chối tin | AdminController |
| `GET` | `/api/admin/users` | Admin | Danh sách user | AdminController |
| `PATCH` | `/api/admin/users/{id}/status` | Admin | Khóa/Mở tài khoản | AdminController |
| `GET` | `/api/admin/disputes` | Admin | Đơn đang tranh chấp | AdminController |
| `POST` | `/api/admin/disputes/resolve` | Admin | Giải quyết tranh chấp | AdminController |

### 📡 SignalR Hub

| Hub URL | Method | Mô tả |
|---|---|---|
| `/chathub` | `JoinChat(userId)` | Tham gia group cá nhân để nhận tin nhắn |
| `/chathub` | `SendMessage(senderId, dto)` | Gửi tin nhắn real-time |
| `/chathub` | `MarkAsRead(userId, otherUserId)` | Đánh dấu đã đọc |
| — | Event: `ReceiveMessage` | FE lắng nghe để nhận tin nhắn mới |
| — | Event: `MessagesRead` | FE lắng nghe để cập nhật trạng thái đã đọc |

---

## 12. Database Schema tóm tắt

```
┌──────────────┐     1:N     ┌──────────────┐     1:N     ┌──────────────┐
│   AppUsers   │────────────►│  BikePosts   │────────────►│  BikeImages  │
│              │             │              │             │              │
│  Id (PK)     │             │  Id (PK)     │             │  Id (PK)     │
│  Email       │             │  SellerId(FK)│             │  BikePostId  │
│  FullName    │             │  Title       │             │  ImageUrl    │
│  Role        │             │  Price       │             │  IsPrimary   │
│  Status      │             │  Status      │             │  DisplayOrder│
│  SellerRating│             │  Brand/Model │             └──────────────┘
│  IdentityId  │             │  Category    │
└──────┬───────┘             │  Condition   │     1:1     ┌──────────────────┐
       │                     │  City        │────────────►│InspectionReports │
       │                     └──────┬───────┘             │  InspectorId(FK) │
       │                            │                     │  Scores (6 items)│
       │                            │ 1:N                 │  OverallCondition│
       │                     ┌──────▼───────┐             └──────────────────┘
       │                     │   Orders     │
       │   BuyerId(FK) ──────│              │
       │   SellerId(FK) ─────│  OrderNumber │     1:N     ┌──────────────┐
       │                     │  Status      │────────────►│   Payments   │
       │                     │  DepositAmt  │             │  Amount      │
       │                     │  TotalAmount │             │  Type        │
       │                     │  HasDispute  │             │  Gateway     │
       │                     └──────┬───────┘             └──────────────┘
       │                            │ 1:1
       │                     ┌──────▼───────┐
       │                     │   Ratings    │
       │   FromUserId(FK) ───│  Stars (1-5) │
       │   ToUserId(FK) ─────│  Comment     │
       │                     │  SubRatings  │
       │                     └──────────────┘
       │
       │   1:N               ┌──────────────┐
       ├────────────────────►│   Messages   │
       │  SenderId/ReceiverId│  Content     │
       │                     │  IsRead      │
       │                     │  BikePostId? │
       │                     └──────────────┘
       │
       │   1:N               ┌──────────────┐
       ├────────────────────►│  Wishlists   │
       │                     │  BikePostId  │
       │                     └──────────────┘
       │
       │   1:1               ┌──────────────┐     1:N     ┌────────────────────┐
       └────────────────────►│   Wallets    │────────────►│ WalletTransactions │
                             │  Balance     │             │  Amount            │
                             │  Currency    │             │  Type              │
                             └──────────────┘             │  BalanceBefore/After│
                                                          └────────────────────┘

Tất cả bảng đều kế thừa BaseEntity:
  - Id (Guid, PK)
  - CreatedAt (auto-set)
  - UpdatedAt (auto-set)
  - IsDeleted (Soft Delete, Global Query Filter)
  - CreatedBy / UpdatedBy
```

---

## 🚀 Cách chạy dự án

```bash
# 1. Clone repo
git clone https://github.com/DangQuangSE/SmallApp_Demo.git

# 2. Update connection string trong appsettings.json

# 3. Chạy migration
cd src/SecondBike.Api
dotnet ef migrations add InitialCreate --project ../SecondBike.Infrastructure
dotnet ef database update --project ../SecondBike.Infrastructure

# 4. Chạy API
dotnet run

# 5. Mở Swagger
# https://localhost:{port}/swagger
```

---

## 📌 Ghi chú cho FE (React)

1. **Login** → nhận `token` → lưu vào `localStorage`.
2. **Mọi request authenticated** → gắn header: `Authorization: Bearer <token>`.
3. **SignalR** → kết nối: `/chathub?access_token=<token>`.
4. **Enum** được trả về dạng **string** (nhờ `JsonStringEnumConverter`), VD: `"Active"`, `"RoadBike"`.
5. **Null fields** sẽ **không xuất hiện** trong response (nhờ `JsonIgnoreCondition.WhenWritingNull`).
6. **CORS** đã mở cho `localhost:3000` (CRA) và `localhost:5173` (Vite).
