-- Tạo Database
CREATE DATABASE SecondBikeDb;
GO
USE SecondBikeDb;
GO

-- 1. Bảng UserRole
CREATE TABLE UserRole (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL -- Admin, Seller, Buyer, Inspector
);

-- 2. Bảng User
CREATE TABLE [User] (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    RoleID INT NOT NULL,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    IsVerified BIT DEFAULT 0,
    -- Chuyển Status sang TINYINT. Mapping VD: 1: Active, 0: Banned
    Status TINYINT DEFAULT 1, 
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoleID) REFERENCES UserRole(RoleID)
);

-- 3. Bảng UserProfile
CREATE TABLE UserProfile (
    ProfileID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL UNIQUE,
    FullName NVARCHAR(100),
    PhoneNumber VARCHAR(20),
    Address NVARCHAR(255),
    AvatarUrl NVARCHAR(MAX),
    FOREIGN KEY (UserID) REFERENCES [User](UserID)
);

-- 4. Bảng Brand (Hãng xe)
CREATE TABLE Brand (
    BrandID INT PRIMARY KEY IDENTITY(1,1),
    BrandName NVARCHAR(100) NOT NULL,
    Country NVARCHAR(50)
);

-- 5. Bảng BikeType (Loại xe)
CREATE TABLE BikeType (
    TypeID INT PRIMARY KEY IDENTITY(1,1),
    TypeName NVARCHAR(50) NOT NULL -- Địa hình, Đua, Đường phố...
);

-- 6. Bảng Bicycle (Thông tin xe cốt lõi)
CREATE TABLE Bicycle (
    BikeID INT PRIMARY KEY IDENTITY(1,1),
    BrandID INT,
    TypeID INT,
    ModelName NVARCHAR(100),
    SerialNumber VARCHAR(50), 
    Color NVARCHAR(50),
    Condition NVARCHAR(50), -- Giữ nguyên NVARCHAR vì đây là mô tả (Mới, 99%...), trừ khi muốn strict enum
    FOREIGN KEY (BrandID) REFERENCES Brand(BrandID),
    FOREIGN KEY (TypeID) REFERENCES BikeType(TypeID)
);

-- 7. Bảng BicycleDetail (Chi tiết kỹ thuật)
CREATE TABLE BicycleDetail (
    DetailID INT PRIMARY KEY IDENTITY(1,1),
    BikeID INT NOT NULL UNIQUE,
    FrameSize NVARCHAR(20),
    FrameMaterial NVARCHAR(50),
    WheelSize NVARCHAR(20),
    BrakeType NVARCHAR(50), 
    Weight DECIMAL(5,2), 
    Transmission NVARCHAR(100), 
    FOREIGN KEY (BikeID) REFERENCES Bicycle(BikeID)
);

-- 8. Bảng BicycleListing (Bài đăng bán)
CREATE TABLE BicycleListing (
    ListingID INT PRIMARY KEY IDENTITY(1,1),
    SellerID INT NOT NULL,
    BikeID INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18, 2) NOT NULL,
    -- Chuyển ListingStatus sang TINYINT. Mapping VD: 1: Active, 2: Sold, 3: Hidden, 0: PendingApproval
    ListingStatus TINYINT DEFAULT 1, 
    Address NVARCHAR(255), 
    PostedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (SellerID) REFERENCES [User](UserID),
    FOREIGN KEY (BikeID) REFERENCES Bicycle(BikeID)
);

-- 9. Bảng ListingMedia (Ảnh/Video)
CREATE TABLE ListingMedia (
    MediaID INT PRIMARY KEY IDENTITY(1,1),
    ListingID INT NOT NULL,
    MediaUrl NVARCHAR(MAX) NOT NULL,
    MediaType NVARCHAR(20), -- Image, Video (Có thể giữ String hoặc chuyển Enum tùy logic)
    IsThumbnail BIT DEFAULT 0,
    FOREIGN KEY (ListingID) REFERENCES BicycleListing(ListingID)
);

-- 10. Bảng Order (Đơn hàng)
CREATE TABLE [Order] (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    BuyerID INT NOT NULL,
    ListingID INT NOT NULL,
    TotalAmount DECIMAL(18, 2),
    -- Chuyển OrderStatus sang TINYINT. Mapping VD: 1: Pending, 2: Deposited, 3: Paid, 4: Completed, 0: Cancelled
    OrderStatus TINYINT DEFAULT 1, 
    OrderDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (BuyerID) REFERENCES [User](UserID),
    FOREIGN KEY (ListingID) REFERENCES BicycleListing(ListingID)
);

-- 11. Các bảng tài chính liên quan Order
CREATE TABLE Deposit (
    DepositID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    Amount DECIMAL(18, 2),
    -- Chuyển Status sang TINYINT. Mapping VD: 1: Pending, 2: Success
    Status TINYINT DEFAULT 1, 
    DepositDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (OrderID) REFERENCES [Order](OrderID)
);

CREATE TABLE Payment (
    PaymentID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    Amount DECIMAL(18, 2),
    PaymentMethod NVARCHAR(50), -- Banking, Momo...
    TransactionRef VARCHAR(100),
    PaymentDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (OrderID) REFERENCES [Order](OrderID)
);

CREATE TABLE Payout (
    PayoutID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    AmountToSeller DECIMAL(18, 2),
    -- Chuyển Status sang TINYINT. Mapping VD: 1: Pending, 2: Transferred
    Status TINYINT DEFAULT 1, 
    PayoutDate DATETIME,
    FOREIGN KEY (OrderID) REFERENCES [Order](OrderID)
);

CREATE TABLE ServiceFee (
    FeeID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    FeeAmount DECIMAL(18, 2),
    Description NVARCHAR(200),
    FOREIGN KEY (OrderID) REFERENCES [Order](OrderID)
);

-- 12. Bảng Tương tác (Feedback, Wishlist, Cart)
CREATE TABLE Feedback (
    FeedbackID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL, 
    TargetUserID INT NOT NULL, 
    OrderID INT NOT NULL,
    Rating INT CHECK (Rating >= 1 AND Rating <= 5),
    Comment NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES [User](UserID),
    FOREIGN KEY (TargetUserID) REFERENCES [User](UserID),
    FOREIGN KEY (OrderID) REFERENCES [Order](OrderID)
);

CREATE TABLE Wishlist (
    WishlistID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    ListingID INT NOT NULL,
    AddedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES [User](UserID),
    FOREIGN KEY (ListingID) REFERENCES BicycleListing(ListingID)
);

CREATE TABLE ShoppingCart (
    CartID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    ListingID INT NOT NULL,
    AddedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES [User](UserID),
    FOREIGN KEY (ListingID) REFERENCES BicycleListing(ListingID)
);

-- 13. Hệ thống Chat
CREATE TABLE ChatSession (
    SessionID INT PRIMARY KEY IDENTITY(1,1),
    BuyerID INT NOT NULL,
    SellerID INT NOT NULL,
    ListingID INT, 
    StartedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (BuyerID) REFERENCES [User](UserID),
    FOREIGN KEY (SellerID) REFERENCES [User](UserID),
    FOREIGN KEY (ListingID) REFERENCES BicycleListing(ListingID)
);

CREATE TABLE ChatMessage (
    MessageID INT PRIMARY KEY IDENTITY(1,1),
    SessionID INT NOT NULL,
    SenderID INT NOT NULL,
    Content NVARCHAR(MAX),
    SentAt DATETIME DEFAULT GETDATE(),
    IsRead BIT DEFAULT 0,
    FOREIGN KEY (SessionID) REFERENCES ChatSession(SessionID),
    FOREIGN KEY (SenderID) REFERENCES [User](UserID)
);

-- 14. Hệ thống Kiểm định (Inspection)
CREATE TABLE InspectionRequest (
    RequestID INT PRIMARY KEY IDENTITY(1,1),
    ListingID INT NOT NULL,
    InspectorID INT, 
    -- Chuyển RequestStatus sang TINYINT. Mapping VD: 1: Pending, 2: Assigned, 3: Completed
    RequestStatus TINYINT DEFAULT 1, 
    RequestDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ListingID) REFERENCES BicycleListing(ListingID),
    FOREIGN KEY (InspectorID) REFERENCES [User](UserID)
);

CREATE TABLE InspectionReport (
    ReportID INT PRIMARY KEY IDENTITY(1,1),
    RequestID INT NOT NULL UNIQUE,
    FrameCheck NVARCHAR(MAX), 
    BrakeCheck NVARCHAR(MAX), 
    TransmissionCheck NVARCHAR(MAX), 
    InspectorNote NVARCHAR(MAX),
    -- Chuyển FinalVerdict sang TINYINT. Mapping VD: 1: Pass, 0: Fail
    FinalVerdict TINYINT, 
    ReportUrl NVARCHAR(MAX), 
    CompletedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RequestID) REFERENCES InspectionRequest(RequestID)
);

-- 15. Hệ thống Báo cáo & Cấu hình
CREATE TABLE RequestAbuse (
    RequestAbuseID INT PRIMARY KEY IDENTITY(1,1),
    ReporterID INT NOT NULL,
    TargetListingID INT, 
    TargetUserID INT, 
    Reason NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ReporterID) REFERENCES [User](UserID),
    FOREIGN KEY (TargetListingID) REFERENCES BicycleListing(ListingID),
    FOREIGN KEY (TargetUserID) REFERENCES [User](UserID)
);

CREATE TABLE ReportAbuse (
    ReportAbuseID INT PRIMARY KEY IDENTITY(1,1),
    RequestAbuseID INT NOT NULL UNIQUE,
    AdminID INT NOT NULL,
    Resolution NVARCHAR(MAX), 
    -- Chuyển Status sang TINYINT. Mapping VD: 1: Resolved, 2: Dismissed
    Status TINYINT, 
    ResolvedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RequestAbuseID) REFERENCES RequestAbuse(RequestAbuseID),
    FOREIGN KEY (AdminID) REFERENCES [User](UserID)
);

CREATE TABLE AuditLogSystem (
    LogID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    Action NVARCHAR(100),
    IPAddress VARCHAR(50),
    Details NVARCHAR(MAX),
    LogDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES [User](UserID)
);

CREATE TABLE ConfigurationSystem (
    ConfigID INT PRIMARY KEY IDENTITY(1,1),
    ConfigKey NVARCHAR(50) NOT NULL UNIQUE,
    ConfigValue NVARCHAR(MAX),
    Description NVARCHAR(200)
);

-- 16. Bảng RefreshToken (Authentication)
CREATE TABLE RefreshToken (
    RefreshTokenID INT PRIMARY KEY IDENTITY(1,1), -- Đổi Id thành RefreshTokenID cho đồng bộ
    UserID INT NOT NULL,
    Token NVARCHAR(MAX) NOT NULL, -- Đổi tên HashedToken thành Token cho ngắn gọn (vẫn lưu hash đc), dùng MAX cho an toàn
    ExpiresAt DATETIME NOT NULL, -- Dùng DATETIME cho đồng bộ với hệ thống
    CreatedAt DATETIME DEFAULT GETDATE(), -- Thêm ngày tạo
    RevokedAt DATETIME NULL, -- Thêm cột này để xử lý Logout (nếu NULL là còn hiệu lực)
    FOREIGN KEY (UserID) REFERENCES [User](UserID)
);