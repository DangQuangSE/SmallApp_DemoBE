-- =============================================
-- SecondBikeDb — Full Create Script
-- Updated: Order ? BicycleListing = Many-to-Many via OrderDetail
-- Added: Quantity to BicycleListing
-- =============================================

-- T?o Database
CREATE DATABASE SecondBikeDb;
GO
USE SecondBikeDb;
GO

-- 1. B?ng UserRole
CREATE TABLE UserRole (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL -- Admin, Seller, Buyer, Inspector
);

-- 2. B?ng User
CREATE TABLE [User] (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    RoleID INT NOT NULL,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    IsVerified BIT DEFAULT 0,
    Status TINYINT DEFAULT 1, -- 1: Active, 0: Banned
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoleID) REFERENCES UserRole(RoleID)
);

-- 3. B?ng UserProfile
CREATE TABLE UserProfile (
    ProfileID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL UNIQUE,
    FullName NVARCHAR(100),
    PhoneNumber VARCHAR(20),
    Address NVARCHAR(255),
    AvatarUrl NVARCHAR(MAX),
    FOREIGN KEY (UserID) REFERENCES [User](UserID)
);

-- 4. B?ng Brand (Hãng xe)
CREATE TABLE Brand (
    BrandID INT PRIMARY KEY IDENTITY(1,1),
    BrandName NVARCHAR(100) NOT NULL,
    Country NVARCHAR(50)
);

-- 5. B?ng BikeType (Lo?i xe)
CREATE TABLE BikeType (
    TypeID INT PRIMARY KEY IDENTITY(1,1),
    TypeName NVARCHAR(50) NOT NULL
);

-- 6. B?ng Bicycle (Thông tin xe c?t lõi)
CREATE TABLE Bicycle (
    BikeID INT PRIMARY KEY IDENTITY(1,1),
    BrandID INT,
    TypeID INT,
    ModelName NVARCHAR(100),
    SerialNumber VARCHAR(50),
    Color NVARCHAR(50),
    Condition NVARCHAR(50),
    FOREIGN KEY (BrandID) REFERENCES Brand(BrandID),
    FOREIGN KEY (TypeID) REFERENCES BikeType(TypeID)
);

-- 7. B?ng BicycleDetail (Chi ti?t k? thu?t)
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

-- 8. B?ng BicycleListing (Bài ??ng bán) — THÊM Quantity
CREATE TABLE BicycleListing (
    ListingID INT PRIMARY KEY IDENTITY(1,1),
    SellerID INT NOT NULL,
    BikeID INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18, 2) NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,  -- ? M?I: s? l??ng xe cùng lo?i
    ListingStatus TINYINT DEFAULT 1,  -- 0: Hidden, 1: Active, 2: Pending, 3: Sold, 4: Rejected
    Address NVARCHAR(255),
    PostedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (SellerID) REFERENCES [User](UserID),
    FOREIGN KEY (BikeID) REFERENCES Bicycle(BikeID)
);

-- 9. B?ng ListingMedia (?nh/Video)
CREATE TABLE ListingMedia (
    MediaID INT PRIMARY KEY IDENTITY(1,1),
    ListingID INT NOT NULL,
    MediaUrl NVARCHAR(MAX) NOT NULL,
    MediaType NVARCHAR(20),
    IsThumbnail BIT DEFAULT 0,
    FOREIGN KEY (ListingID) REFERENCES BicycleListing(ListingID)
);

CREATE TABLE [Order] (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    BuyerID INT NOT NULL,
    TotalAmount DECIMAL(18, 2),
    OrderStatus TINYINT DEFAULT 1,  -- 1: Pending, 2: Paid(Deposit), 3: Shipping, 4: Completed, 5: Cancelled, 6: Refunded
    OrderDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (BuyerID) REFERENCES [User](UserID)
);

-- 10b. ? B?NG M?I: OrderDetail — B?ng trung gian N-N gi?a Order và BicycleListing
CREATE TABLE OrderDetail (
    OrderDetailID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    ListingID INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(18, 2) NOT NULL,
    CONSTRAINT FK__OrderDetail__OrderID FOREIGN KEY (OrderID) REFERENCES [Order](OrderID),
    CONSTRAINT FK__OrderDetail__ListingID FOREIGN KEY (ListingID) REFERENCES BicycleListing(ListingID)
);

-- 11. Các b?ng tài chính liên quan Order
CREATE TABLE Deposit (
    DepositID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    Amount DECIMAL(18, 2),
    Status TINYINT DEFAULT 1, -- 1: Pending, 2: Confirmed, 3: Cancelled
    DepositDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (OrderID) REFERENCES [Order](OrderID)
);

CREATE TABLE Payment (
    PaymentID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    Amount DECIMAL(18, 2),
    PaymentMethod NVARCHAR(50),
    TransactionRef VARCHAR(100),
    PaymentDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (OrderID) REFERENCES [Order](OrderID)
);

CREATE TABLE Payout (
    PayoutID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    AmountToSeller DECIMAL(18, 2),
    Status TINYINT DEFAULT 1, -- 1: Pending, 2: Transferred
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

-- 12. B?ng T??ng tác (Feedback, Wishlist, Cart)
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

-- 13. H? th?ng Chat
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

-- 14. H? th?ng Ki?m ??nh (Inspection)
CREATE TABLE InspectionRequest (
    RequestID INT PRIMARY KEY IDENTITY(1,1),
    ListingID INT NOT NULL,
    InspectorID INT,
    RequestStatus TINYINT DEFAULT 1, -- 1: Pending, 2: In Progress, 3: Completed
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
    FinalVerdict TINYINT, -- 1: Pass, 0: Fail
    ReportUrl NVARCHAR(MAX),
    CompletedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RequestID) REFERENCES InspectionRequest(RequestID)
);

-- 15. H? th?ng Báo cáo & C?u hình
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
    Status TINYINT, -- 1: Resolved, 2: Dismissed
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

-- 16. B?ng RefreshToken (Authentication)
CREATE TABLE RefreshToken (
    RefreshTokenID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    Token NVARCHAR(MAX) NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    RevokedAt DATETIME NULL,
    FOREIGN KEY (UserID) REFERENCES [User](UserID)
);

-- =============================================
-- SEED DATA
-- =============================================

-- Seed UserRole
INSERT INTO UserRole (RoleName) VALUES
('Admin'),
('Buyer'),
('Seller'),
('Inspector');



