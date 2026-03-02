-- =============================================
-- SecondBike Database Creation Script
-- SQL Server
-- Generated from Entity + Configuration code
-- =============================================

-- 1. Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SecondBikeDb')
BEGIN
    CREATE DATABASE SecondBikeDb;
END
GO

USE SecondBikeDb;
GO

-- =============================================
-- 2. Drop existing tables (reverse FK order)
-- =============================================
DROP TABLE IF EXISTS [WalletTransactions];
DROP TABLE IF EXISTS [Wallets];
DROP TABLE IF EXISTS [Wishlists];
DROP TABLE IF EXISTS [InspectionReports];
DROP TABLE IF EXISTS [Ratings];
DROP TABLE IF EXISTS [Payments];
DROP TABLE IF EXISTS [Messages];
DROP TABLE IF EXISTS [BikeImages];
DROP TABLE IF EXISTS [Orders];
DROP TABLE IF EXISTS [BikePosts];
DROP TABLE IF EXISTS [AppUsers];

-- Identity tables
DROP TABLE IF EXISTS [AspNetUserTokens];
DROP TABLE IF EXISTS [AspNetUserRoles];
DROP TABLE IF EXISTS [AspNetUserLogins];
DROP TABLE IF EXISTS [AspNetUserClaims];
DROP TABLE IF EXISTS [AspNetRoleClaims];
DROP TABLE IF EXISTS [AspNetUsers];
DROP TABLE IF EXISTS [AspNetRoles];
GO

-- =============================================
-- 3. ASP.NET Core Identity Tables
-- =============================================

CREATE TABLE [AspNetRoles] (
    [Id]               NVARCHAR(450)  NOT NULL,
    [Name]             NVARCHAR(256)  NULL,
    [NormalizedName]   NVARCHAR(256)  NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id]                   NVARCHAR(450)  NOT NULL,
    [UserName]             NVARCHAR(256)  NULL,
    [NormalizedUserName]   NVARCHAR(256)  NULL,
    [Email]                NVARCHAR(256)  NULL,
    [NormalizedEmail]      NVARCHAR(256)  NULL,
    [EmailConfirmed]       BIT            NOT NULL,
    [PasswordHash]         NVARCHAR(MAX)  NULL,
    [SecurityStamp]        NVARCHAR(MAX)  NULL,
    [ConcurrencyStamp]     NVARCHAR(MAX)  NULL,
    [PhoneNumber]          NVARCHAR(MAX)  NULL,
    [PhoneNumberConfirmed] BIT            NOT NULL,
    [TwoFactorEnabled]     BIT            NOT NULL,
    [LockoutEnd]           DATETIMEOFFSET NULL,
    [LockoutEnabled]       BIT            NOT NULL,
    [AccessFailedCount]    INT            NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id]         INT            IDENTITY(1,1) NOT NULL,
    [RoleId]     NVARCHAR(450)  NOT NULL,
    [ClaimType]  NVARCHAR(MAX)  NULL,
    [ClaimValue] NVARCHAR(MAX)  NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId])
        REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id]         INT            IDENTITY(1,1) NOT NULL,
    [UserId]     NVARCHAR(450)  NOT NULL,
    [ClaimType]  NVARCHAR(MAX)  NULL,
    [ClaimValue] NVARCHAR(MAX)  NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId])
        REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider]       NVARCHAR(128) NOT NULL,
    [ProviderKey]         NVARCHAR(128) NOT NULL,
    [ProviderDisplayName] NVARCHAR(MAX) NULL,
    [UserId]              NVARCHAR(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId])
        REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] NVARCHAR(450) NOT NULL,
    [RoleId] NVARCHAR(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId])
        REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId])
        REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId]        NVARCHAR(450) NOT NULL,
    [LoginProvider] NVARCHAR(128) NOT NULL,
    [Name]          NVARCHAR(128) NOT NULL,
    [Value]         NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId])
        REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

-- Identity Indexes
CREATE UNIQUE INDEX [RoleNameIndex]
    ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
CREATE INDEX [IX_AspNetRoleClaims_RoleId]       ON [AspNetRoleClaims] ([RoleId]);
CREATE INDEX [IX_AspNetUserClaims_UserId]       ON [AspNetUserClaims] ([UserId]);
CREATE INDEX [IX_AspNetUserLogins_UserId]       ON [AspNetUserLogins] ([UserId]);
CREATE INDEX [IX_AspNetUserRoles_RoleId]        ON [AspNetUserRoles] ([RoleId]);
CREATE INDEX [EmailIndex]                       ON [AspNetUsers] ([NormalizedEmail]);
CREATE UNIQUE INDEX [UserNameIndex]             ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

-- =============================================
-- 4. Application Tables
-- =============================================

-- 4.1 AppUsers
CREATE TABLE [AppUsers] (
    [Id]                UNIQUEIDENTIFIER NOT NULL,
    [Email]             NVARCHAR(256)    NOT NULL,
    [FullName]          NVARCHAR(100)    NOT NULL,
    [PhoneNumber]       NVARCHAR(20)     NULL,
    [AvatarUrl]         NVARCHAR(500)    NULL,
    [Role]              INT              NOT NULL DEFAULT 1,       -- UserRole: 1=Buyer,2=Seller,3=Inspector,4=Admin
    [Status]            INT              NOT NULL DEFAULT 1,       -- UserStatus: 1=Active,2=Suspended,3=Banned,4=Deleted
    [ShopName]          NVARCHAR(200)    NULL,
    [ShopDescription]   NVARCHAR(2000)   NULL,
    [IsVerifiedSeller]  BIT              NOT NULL DEFAULT 0,
    [SellerRating]      DECIMAL(3,2)     NOT NULL DEFAULT 0,
    [TotalRatingsCount] INT              NOT NULL DEFAULT 0,
    [IdentityUserId]    NVARCHAR(450)    NOT NULL,
    -- BaseEntity audit
    [CreatedAt]         DATETIME2        NOT NULL,
    [UpdatedAt]         DATETIME2        NULL,
    [IsDeleted]         BIT              NOT NULL DEFAULT 0,
    [CreatedBy]         NVARCHAR(MAX)    NULL,
    [UpdatedBy]         NVARCHAR(MAX)    NULL,
    CONSTRAINT [PK_AppUsers] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_AppUsers_Email]          ON [AppUsers] ([Email]);
CREATE UNIQUE INDEX [IX_AppUsers_IdentityUserId] ON [AppUsers] ([IdentityUserId]);
GO

-- 4.2 BikePosts
CREATE TABLE [BikePosts] (
    [Id]                  UNIQUEIDENTIFIER NOT NULL,
    [SellerId]            UNIQUEIDENTIFIER NOT NULL,
    [Title]               NVARCHAR(200)    NOT NULL,
    [Description]         NVARCHAR(5000)   NOT NULL,
    [Price]               DECIMAL(18,2)    NOT NULL,
    [Status]              INT              NOT NULL DEFAULT 1,  -- PostStatus: 1=Draft,...7=Expired
    [Brand]               NVARCHAR(100)    NOT NULL,
    [Model]               NVARCHAR(100)    NOT NULL,
    [Year]                INT              NOT NULL,
    [Category]            INT              NOT NULL,            -- BikeCategory: 1=Road,...8=Electric
    [Size]                INT              NOT NULL,            -- BikeSize: 1=XS,...5=XL
    [FrameMaterial]       NVARCHAR(50)     NULL,
    [Color]               NVARCHAR(50)     NULL,
    [Condition]           INT              NOT NULL,            -- BikeCondition: 1=New,...6=NeedsRepair
    [WeightKg]            DECIMAL(5,2)     NOT NULL DEFAULT 0,
    [OdometerKm]          INT              NULL,
    [UsageHistory]        NVARCHAR(2000)   NULL,
    [HasAccidents]        BIT              NOT NULL DEFAULT 0,
    [AccidentDescription] NVARCHAR(2000)   NULL,
    [City]                NVARCHAR(100)    NULL,
    [District]            NVARCHAR(100)    NULL,
    [PublishedAt]         DATETIME2        NULL,
    [ModeratedBy]         UNIQUEIDENTIFIER NULL,
    [ModerationNotes]     NVARCHAR(1000)   NULL,
    [RejectionReason]     NVARCHAR(1000)   NULL,
    [ViewCount]           INT              NOT NULL DEFAULT 0,
    [WishlistCount]       INT              NOT NULL DEFAULT 0,
    -- BaseEntity audit
    [CreatedAt]           DATETIME2        NOT NULL,
    [UpdatedAt]           DATETIME2        NULL,
    [IsDeleted]           BIT              NOT NULL DEFAULT 0,
    [CreatedBy]           NVARCHAR(MAX)    NULL,
    [UpdatedBy]           NVARCHAR(MAX)    NULL,
    CONSTRAINT [PK_BikePosts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BikePosts_AppUsers_SellerId] FOREIGN KEY ([SellerId])
        REFERENCES [AppUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_BikePosts_Status]          ON [BikePosts] ([Status]);
CREATE INDEX [IX_BikePosts_Category_Status] ON [BikePosts] ([Category], [Status]);
CREATE INDEX [IX_BikePosts_Brand_Status]    ON [BikePosts] ([Brand], [Status]);
CREATE INDEX [IX_BikePosts_SellerId]        ON [BikePosts] ([SellerId]);
GO

-- 4.3 BikeImages
CREATE TABLE [BikeImages] (
    [Id]           UNIQUEIDENTIFIER NOT NULL,
    [BikePostId]   UNIQUEIDENTIFIER NOT NULL,
    [ImageUrl]     NVARCHAR(500)    NOT NULL,
    [ThumbnailUrl] NVARCHAR(500)    NULL,
    [DisplayOrder] INT              NOT NULL DEFAULT 0,
    [IsPrimary]    BIT              NOT NULL DEFAULT 0,
    [Caption]      NVARCHAR(200)    NULL,
    -- BaseEntity audit
    [CreatedAt]    DATETIME2        NOT NULL,
    [UpdatedAt]    DATETIME2        NULL,
    [IsDeleted]    BIT              NOT NULL DEFAULT 0,
    [CreatedBy]    NVARCHAR(MAX)    NULL,
    [UpdatedBy]    NVARCHAR(MAX)    NULL,
    CONSTRAINT [PK_BikeImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BikeImages_BikePosts_BikePostId] FOREIGN KEY ([BikePostId])
        REFERENCES [BikePosts] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_BikeImages_BikePostId] ON [BikeImages] ([BikePostId]);
GO

-- 4.4 Orders
CREATE TABLE [Orders] (
    [Id]                  UNIQUEIDENTIFIER NOT NULL,
    [OrderNumber]         NVARCHAR(50)     NOT NULL,
    [BuyerId]             UNIQUEIDENTIFIER NOT NULL,
    [SellerId]            UNIQUEIDENTIFIER NOT NULL,
    [BikePostId]          UNIQUEIDENTIFIER NOT NULL,
    [BikePrice]           DECIMAL(18,2)    NOT NULL,
    [DepositAmount]       DECIMAL(18,2)    NOT NULL DEFAULT 0,
    [DepositPercentage]   DECIMAL(5,2)     NOT NULL DEFAULT 15,
    [RemainingAmount]     DECIMAL(18,2)    NOT NULL DEFAULT 0,
    [ShippingFee]         DECIMAL(18,2)    NULL,
    [TotalAmount]         DECIMAL(18,2)    NOT NULL DEFAULT 0,
    [Status]              INT              NOT NULL DEFAULT 1,  -- OrderStatus: 1=Pending,...9=Refunded
    [DepositPaidAt]       DATETIME2        NULL,
    [FullPaymentAt]       DATETIME2        NULL,
    [ShippedAt]           DATETIME2        NULL,
    [DeliveredAt]         DATETIME2        NULL,
    [CompletedAt]         DATETIME2        NULL,
    [CancelledAt]         DATETIME2        NULL,
    [CancellationReason]  NVARCHAR(1000)   NULL,
    [ShippingAddress]     NVARCHAR(500)    NOT NULL DEFAULT N'',
    [TrackingNumber]      NVARCHAR(100)    NULL,
    [ShippingProvider]    NVARCHAR(100)    NULL,
    [HasDispute]          BIT              NOT NULL DEFAULT 0,
    [DisputeReason]       NVARCHAR(2000)   NULL,
    [DisputeResolvedBy]   UNIQUEIDENTIFIER NULL,
    [DisputeResolvedAt]   DATETIME2        NULL,
    [DisputeResolution]   NVARCHAR(2000)   NULL,
    -- BaseEntity audit
    [CreatedAt]           DATETIME2        NOT NULL,
    [UpdatedAt]           DATETIME2        NULL,
    [IsDeleted]           BIT              NOT NULL DEFAULT 0,
    [CreatedBy]           NVARCHAR(MAX)    NULL,
    [UpdatedBy]           NVARCHAR(MAX)    NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Orders_AppUsers_BuyerId] FOREIGN KEY ([BuyerId])
        REFERENCES [AppUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Orders_AppUsers_SellerId] FOREIGN KEY ([SellerId])
        REFERENCES [AppUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Orders_BikePosts_BikePostId] FOREIGN KEY ([BikePostId])
        REFERENCES [BikePosts] ([Id]) ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX [IX_Orders_OrderNumber] ON [Orders] ([OrderNumber]);
CREATE INDEX [IX_Orders_BuyerId]            ON [Orders] ([BuyerId]);
CREATE INDEX [IX_Orders_SellerId]           ON [Orders] ([SellerId]);
CREATE INDEX [IX_Orders_BikePostId]         ON [Orders] ([BikePostId]);
GO

-- 4.5 Payments
CREATE TABLE [Payments] (
    [Id]                   UNIQUEIDENTIFIER NOT NULL,
    [OrderId]              UNIQUEIDENTIFIER NOT NULL,
    [TransactionId]        NVARCHAR(100)    NOT NULL,
    [Amount]               DECIMAL(18,2)    NOT NULL,
    [Type]                 INT              NOT NULL,            -- PaymentType: 1=Deposit,...4=Refund
    [Method]               INT              NOT NULL,            -- PaymentMethod: 1=CreditCard,...5=Cash
    [Status]               INT              NOT NULL DEFAULT 1,  -- PaymentStatus: 1=Pending,...6=Refunded
    [Gateway]              INT              NOT NULL,            -- PaymentGateway: 1=VNPay,...5=PayPal
    [GatewayTransactionId] NVARCHAR(200)    NULL,
    [GatewayResponse]      NVARCHAR(2000)   NULL,
    [ProcessedAt]          DATETIME2        NULL,
    [FailureReason]        NVARCHAR(1000)   NULL,
    [RefundAmount]         DECIMAL(18,2)    NULL,
    -- BaseEntity audit
    [CreatedAt]            DATETIME2        NOT NULL,
    [UpdatedAt]            DATETIME2        NULL,
    [IsDeleted]            BIT              NOT NULL DEFAULT 0,
    [CreatedBy]            NVARCHAR(MAX)    NULL,
    [UpdatedBy]            NVARCHAR(MAX)    NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payments_Orders_OrderId] FOREIGN KEY ([OrderId])
        REFERENCES [Orders] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_Payments_OrderId] ON [Payments] ([OrderId]);
GO

-- 4.6 Messages
CREATE TABLE [Messages] (
    [Id]            UNIQUEIDENTIFIER NOT NULL,
    [SenderId]      UNIQUEIDENTIFIER NOT NULL,
    [ReceiverId]    UNIQUEIDENTIFIER NOT NULL,
    [BikePostId]    UNIQUEIDENTIFIER NULL,
    [Content]       NVARCHAR(2000)   NOT NULL,
    [Type]          INT              NOT NULL DEFAULT 1,  -- MessageType: 1=Text,...4=System
    [AttachmentUrl] NVARCHAR(500)    NULL,
    [IsRead]        BIT              NOT NULL DEFAULT 0,
    [ReadAt]        DATETIME2        NULL,
    -- BaseEntity audit
    [CreatedAt]     DATETIME2        NOT NULL,
    [UpdatedAt]     DATETIME2        NULL,
    [IsDeleted]     BIT              NOT NULL DEFAULT 0,
    [CreatedBy]     NVARCHAR(MAX)    NULL,
    [UpdatedBy]     NVARCHAR(MAX)    NULL,
    CONSTRAINT [PK_Messages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Messages_AppUsers_SenderId] FOREIGN KEY ([SenderId])
        REFERENCES [AppUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Messages_AppUsers_ReceiverId] FOREIGN KEY ([ReceiverId])
        REFERENCES [AppUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Messages_BikePosts_BikePostId] FOREIGN KEY ([BikePostId])
        REFERENCES [BikePosts] ([Id]) ON DELETE SET NULL
);
GO

CREATE INDEX [IX_Messages_SenderId_ReceiverId] ON [Messages] ([SenderId], [ReceiverId]);
CREATE INDEX [IX_Messages_BikePostId]          ON [Messages] ([BikePostId]);
GO

-- 4.7 Ratings
CREATE TABLE [Ratings] (
    [Id]                  UNIQUEIDENTIFIER NOT NULL,
    [OrderId]             UNIQUEIDENTIFIER NOT NULL,
    [FromUserId]          UNIQUEIDENTIFIER NOT NULL,
    [ToUserId]            UNIQUEIDENTIFIER NOT NULL,
    [Stars]               INT              NOT NULL,            -- 1 to 5
    [Comment]             NVARCHAR(1000)   NULL,
    [CommunicationRating] INT              NULL,                -- 1 to 5 (optional)
    [AccuracyRating]      INT              NULL,
    [PackagingRating]     INT              NULL,
    [SpeedRating]         INT              NULL,
    [IsPublic]            BIT              NOT NULL DEFAULT 1,
    [SellerResponse]      NVARCHAR(1000)   NULL,
    [SellerRespondedAt]   DATETIME2        NULL,
    -- BaseEntity audit
    [CreatedAt]           DATETIME2        NOT NULL,
    [UpdatedAt]           DATETIME2        NULL,
    [IsDeleted]           BIT              NOT NULL DEFAULT 0,
    [CreatedBy]           NVARCHAR(MAX)    NULL,
    [UpdatedBy]           NVARCHAR(MAX)    NULL,
    CONSTRAINT [PK_Ratings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Ratings_Orders_OrderId] FOREIGN KEY ([OrderId])
        REFERENCES [Orders] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Ratings_AppUsers_FromUserId] FOREIGN KEY ([FromUserId])
        REFERENCES [AppUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Ratings_AppUsers_ToUserId] FOREIGN KEY ([ToUserId])
        REFERENCES [AppUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX [IX_Ratings_OrderId]    ON [Ratings] ([OrderId]);
CREATE INDEX [IX_Ratings_FromUserId]        ON [Ratings] ([FromUserId]);
CREATE INDEX [IX_Ratings_ToUserId]          ON [Ratings] ([ToUserId]);
GO

-- 4.8 InspectionReports
CREATE TABLE [InspectionReports] (
    [Id]                    UNIQUEIDENTIFIER NOT NULL,
    [BikePostId]            UNIQUEIDENTIFIER NOT NULL,
    [InspectorId]           UNIQUEIDENTIFIER NOT NULL,
    [ReportNumber]          NVARCHAR(50)     NOT NULL,
    [Status]                INT              NOT NULL DEFAULT 1, -- InspectionStatus: 1=Pending,...4=Rejected
    [OverallCondition]      INT              NOT NULL,           -- OverallCondition: 1=Excellent,...5=Poor
    [EstimatedValue]        DECIMAL(18,2)    NULL,
    [IsRecommended]         BIT              NOT NULL DEFAULT 0,
    [Summary]               NVARCHAR(3000)   NULL,
    [FrameScore]            INT              NOT NULL DEFAULT 0, -- 1-10
    [BrakesScore]           INT              NOT NULL DEFAULT 0,
    [GearsScore]            INT              NOT NULL DEFAULT 0,
    [WheelsScore]           INT              NOT NULL DEFAULT 0,
    [TiresScore]            INT              NOT NULL DEFAULT 0,
    [ChainScore]            INT              NOT NULL DEFAULT 0,
    [HasFrameDamage]        BIT              NOT NULL DEFAULT 0,
    [FrameNotes]            NVARCHAR(1000)   NULL,
    [HasRust]               BIT              NOT NULL DEFAULT 0,
    [HasCracks]             BIT              NOT NULL DEFAULT 0,
    [AllComponentsOriginal] BIT              NOT NULL DEFAULT 0,
    [ReplacedComponents]    NVARCHAR(1000)   NULL,
    [InspectedAt]           DATETIME2        NULL,
    -- BaseEntity audit
    [CreatedAt]             DATETIME2        NOT NULL,
    [UpdatedAt]             DATETIME2        NULL,
    [IsDeleted]             BIT              NOT NULL DEFAULT 0,
    [CreatedBy]             NVARCHAR(MAX)    NULL,
    [UpdatedBy]             NVARCHAR(MAX)    NULL,
    CONSTRAINT [PK_InspectionReports] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InspectionReports_BikePosts_BikePostId] FOREIGN KEY ([BikePostId])
        REFERENCES [BikePosts] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_InspectionReports_AppUsers_InspectorId] FOREIGN KEY ([InspectorId])
        REFERENCES [AppUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX [IX_InspectionReports_ReportNumber] ON [InspectionReports] ([ReportNumber]);
CREATE UNIQUE INDEX [IX_InspectionReports_BikePostId]   ON [InspectionReports] ([BikePostId]);
CREATE INDEX [IX_InspectionReports_InspectorId]         ON [InspectionReports] ([InspectorId]);
GO

-- 4.9 Wishlists
CREATE TABLE [Wishlists] (
    [Id]         UNIQUEIDENTIFIER NOT NULL,
    [UserId]     UNIQUEIDENTIFIER NOT NULL,
    [BikePostId] UNIQUEIDENTIFIER NOT NULL,
    [Notes]      NVARCHAR(500)    NULL,
    -- BaseEntity audit
    [CreatedAt]  DATETIME2        NOT NULL,
    [UpdatedAt]  DATETIME2        NULL,
    [IsDeleted]  BIT              NOT NULL DEFAULT 0,
    [CreatedBy]  NVARCHAR(MAX)    NULL,
    [UpdatedBy]  NVARCHAR(MAX)    NULL,
    CONSTRAINT [PK_Wishlists] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Wishlists_AppUsers_UserId] FOREIGN KEY ([UserId])
        REFERENCES [AppUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Wishlists_BikePosts_BikePostId] FOREIGN KEY ([BikePostId])
        REFERENCES [BikePosts] ([Id]) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [IX_Wishlists_UserId_BikePostId] ON [Wishlists] ([UserId], [BikePostId]);
GO

-- 4.10 Wallets
CREATE TABLE [Wallets] (
    [Id]        UNIQUEIDENTIFIER NOT NULL,
    [UserId]    UNIQUEIDENTIFIER NOT NULL,
    [Balance]   DECIMAL(18,2)    NOT NULL DEFAULT 0,
    [Currency]  NVARCHAR(10)     NOT NULL DEFAULT N'VND',
    [IsLocked]  BIT              NOT NULL DEFAULT 0,
    -- BaseEntity audit
    [CreatedAt] DATETIME2        NOT NULL,
    [UpdatedAt] DATETIME2        NULL,
    [IsDeleted] BIT              NOT NULL DEFAULT 0,
    [CreatedBy] NVARCHAR(MAX)    NULL,
    [UpdatedBy] NVARCHAR(MAX)    NULL,
    CONSTRAINT [PK_Wallets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Wallets_AppUsers_UserId] FOREIGN KEY ([UserId])
        REFERENCES [AppUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX [IX_Wallets_UserId] ON [Wallets] ([UserId]);
GO

-- 4.11 WalletTransactions
CREATE TABLE [WalletTransactions] (
    [Id]                UNIQUEIDENTIFIER NOT NULL,
    [WalletId]          UNIQUEIDENTIFIER NOT NULL,
    [TransactionNumber] NVARCHAR(50)     NOT NULL,
    [Type]              INT              NOT NULL,          -- WalletTransactionType: 1=Deposit,...5=Commission
    [Amount]            DECIMAL(18,2)    NOT NULL,
    [BalanceBefore]     DECIMAL(18,2)    NOT NULL,
    [BalanceAfter]      DECIMAL(18,2)    NOT NULL,
    [Description]       NVARCHAR(500)    NOT NULL DEFAULT N'',
    [RelatedOrderId]    UNIQUEIDENTIFIER NULL,
    -- BaseEntity audit
    [CreatedAt]         DATETIME2        NOT NULL,
    [UpdatedAt]         DATETIME2        NULL,
    [IsDeleted]         BIT              NOT NULL DEFAULT 0,
    [CreatedBy]         NVARCHAR(MAX)    NULL,
    [UpdatedBy]         NVARCHAR(MAX)    NULL,
    CONSTRAINT [PK_WalletTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WalletTransactions_Wallets_WalletId] FOREIGN KEY ([WalletId])
        REFERENCES [Wallets] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_WalletTransactions_WalletId] ON [WalletTransactions] ([WalletId]);
GO

-- =============================================
-- 5. Seed Identity Roles
-- =============================================
INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
VALUES
    (NEWID(), N'Buyer',     N'BUYER',     NEWID()),
    (NEWID(), N'Seller',    N'SELLER',    NEWID()),
    (NEWID(), N'Inspector', N'INSPECTOR', NEWID()),
    (NEWID(), N'Admin',     N'ADMIN',     NEWID());
GO

-- =============================================
-- 6. Seed Admin Account (optional)
-- Password: Admin@123456
-- PasswordHash below is for ASP.NET Identity v3 using PBKDF2
-- You may need to register via API instead
-- =============================================

/*
-- Uncomment and run if you want a pre-seeded admin:

DECLARE @AdminIdentityId NVARCHAR(450) = NEWID();
DECLARE @AdminAppUserId  UNIQUEIDENTIFIER = NEWID();

INSERT INTO [AspNetUsers] ([Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],
    [EmailConfirmed],[PasswordHash],[SecurityStamp],[ConcurrencyStamp],
    [PhoneNumber],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEnd],[LockoutEnabled],[AccessFailedCount])
VALUES (
    @AdminIdentityId,
    N'admin@secondbike.com',
    N'ADMIN@SECONDBIKE.COM',
    N'admin@secondbike.com',
    N'ADMIN@SECONDBIKE.COM',
    1, NULL, NEWID(), NEWID(),
    NULL, 0, 0, NULL, 1, 0
);

-- Link admin to Admin role
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
SELECT @AdminIdentityId, [Id] FROM [AspNetRoles] WHERE [NormalizedName] = N'ADMIN';

-- Create AppUser profile
INSERT INTO [AppUsers] ([Id],[Email],[FullName],[Role],[Status],[IdentityUserId],[IsVerifiedSeller],
    [SellerRating],[TotalRatingsCount],[CreatedAt],[IsDeleted])
VALUES (
    @AdminAppUserId,
    N'admin@secondbike.com',
    N'System Admin',
    4,  -- Admin
    1,  -- Active
    @AdminIdentityId,
    0, 0, 0,
    GETUTCDATE(),
    0
);

-- Create wallet for admin
INSERT INTO [Wallets] ([Id],[UserId],[Balance],[Currency],[IsLocked],[CreatedAt],[IsDeleted])
VALUES (NEWID(), @AdminAppUserId, 0, N'VND', 0, GETUTCDATE(), 0);
*/

PRINT N'=============================================';
PRINT N'SecondBike database created successfully!';
PRINT N'Tables: 18 (7 Identity + 11 Application)';
PRINT N'Roles seeded: Buyer, Seller, Inspector, Admin';
PRINT N'=============================================';
GO
