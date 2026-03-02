-- =============================================
-- SecondBike — Full Database Script
-- Database: SecondBikeDb
-- Target: SQL Server / LocalDB
-- Generated from Domain Entities + EF Core Configurations
-- =============================================

-- 1. CREATE DATABASE
-- =============================================
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SecondBikeDb')
BEGIN
    CREATE DATABASE [SecondBikeDb];
END
GO

USE [SecondBikeDb];
GO

-- =============================================
-- 2. ASP.NET IDENTITY TABLES
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoles')
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id]               NVARCHAR(450) NOT NULL,
        [Name]             NVARCHAR(256) NULL,
        [NormalizedName]   NVARCHAR(256) NULL,
        [ConcurrencyStamp] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex]
        ON [AspNetRoles]([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUsers')
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id]                   NVARCHAR(450) NOT NULL,
        [UserName]             NVARCHAR(256) NULL,
        [NormalizedUserName]   NVARCHAR(256) NULL,
        [Email]                NVARCHAR(256) NULL,
        [NormalizedEmail]      NVARCHAR(256) NULL,
        [EmailConfirmed]       BIT           NOT NULL,
        [PasswordHash]         NVARCHAR(MAX) NULL,
        [SecurityStamp]        NVARCHAR(MAX) NULL,
        [ConcurrencyStamp]     NVARCHAR(MAX) NULL,
        [PhoneNumber]          NVARCHAR(MAX) NULL,
        [PhoneNumberConfirmed] BIT           NOT NULL,
        [TwoFactorEnabled]     BIT           NOT NULL,
        [LockoutEnd]           DATETIMEOFFSET NULL,
        [LockoutEnabled]       BIT           NOT NULL,
        [AccessFailedCount]    INT           NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
    CREATE NONCLUSTERED INDEX [EmailIndex]
        ON [AspNetUsers]([NormalizedEmail]);
    CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
        ON [AspNetUsers]([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoleClaims')
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id]         INT           IDENTITY(1,1) NOT NULL,
        [RoleId]     NVARCHAR(450) NOT NULL,
        [ClaimType]  NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles]([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserClaims')
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id]         INT           IDENTITY(1,1) NOT NULL,
        [UserId]     NVARCHAR(450) NOT NULL,
        [ClaimType]  NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserLogins')
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider]       NVARCHAR(128) NOT NULL,
        [ProviderKey]         NVARCHAR(128) NOT NULL,
        [ProviderDisplayName] NVARCHAR(MAX) NULL,
        [UserId]              NVARCHAR(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserRoles')
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] NVARCHAR(450) NOT NULL,
        [RoleId] NVARCHAR(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserTokens')
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId]        NVARCHAR(450) NOT NULL,
        [LoginProvider] NVARCHAR(128) NOT NULL,
        [Name]          NVARCHAR(128) NOT NULL,
        [Value]         NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
END
GO

-- =============================================
-- 3. APPLICATION TABLES
-- =============================================

-- 3.1 AppUsers
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AppUsers')
BEGIN
    CREATE TABLE [AppUsers] (
        [Id]                UNIQUEIDENTIFIER NOT NULL,
        [Email]             NVARCHAR(256)    NOT NULL,
        [FullName]          NVARCHAR(100)    NOT NULL,
        [PhoneNumber]       NVARCHAR(20)     NULL,
        [AvatarUrl]         NVARCHAR(500)    NULL,
        [Role]              INT              NOT NULL,  -- 1=Buyer,2=Seller,3=Inspector,4=Admin
        [Status]            INT              NOT NULL,  -- 1=Active,2=Suspended,3=Banned,4=Deleted
        [ShopName]          NVARCHAR(200)    NULL,
        [ShopDescription]   NVARCHAR(2000)   NULL,
        [IsVerifiedSeller]  BIT              NOT NULL DEFAULT 0,
        [SellerRating]      DECIMAL(3,2)     NOT NULL DEFAULT 0,
        [TotalRatingsCount] INT              NOT NULL DEFAULT 0,
        [IdentityUserId]    NVARCHAR(450)    NOT NULL,
        -- BaseEntity columns
        [CreatedAt]         DATETIME2        NOT NULL,
        [UpdatedAt]         DATETIME2        NULL,
        [IsDeleted]         BIT              NOT NULL DEFAULT 0,
        [CreatedBy]         NVARCHAR(MAX)    NULL,
        [UpdatedBy]         NVARCHAR(MAX)    NULL,
        CONSTRAINT [PK_AppUsers] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE NONCLUSTERED INDEX [IX_AppUsers_Email]          ON [AppUsers]([Email]);
    CREATE UNIQUE NONCLUSTERED INDEX [IX_AppUsers_IdentityUserId] ON [AppUsers]([IdentityUserId]);
END
GO

-- 3.2 BikePosts
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BikePosts')
BEGIN
    CREATE TABLE [BikePosts] (
        [Id]                  UNIQUEIDENTIFIER NOT NULL,
        [SellerId]            UNIQUEIDENTIFIER NOT NULL,
        [Title]               NVARCHAR(200)    NOT NULL,
        [Description]         NVARCHAR(MAX)    NOT NULL,
        [Price]               DECIMAL(18,2)    NOT NULL,
        [Status]              INT              NOT NULL,  -- PostStatus enum
        [Brand]               NVARCHAR(100)    NOT NULL,
        [Model]               NVARCHAR(100)    NOT NULL,
        [Year]                INT              NOT NULL,
        [Category]            INT              NOT NULL,  -- BikeCategory enum
        [Size]                INT              NOT NULL,  -- BikeSize enum
        [FrameMaterial]       NVARCHAR(50)     NULL,
        [Color]               NVARCHAR(50)     NULL,
        [Condition]           INT              NOT NULL,  -- BikeCondition enum
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
        -- BaseEntity
        [CreatedAt]           DATETIME2        NOT NULL,
        [UpdatedAt]           DATETIME2        NULL,
        [IsDeleted]           BIT              NOT NULL DEFAULT 0,
        [CreatedBy]           NVARCHAR(MAX)    NULL,
        [UpdatedBy]           NVARCHAR(MAX)    NULL,
        CONSTRAINT [PK_BikePosts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BikePosts_AppUsers_SellerId] FOREIGN KEY ([SellerId]) REFERENCES [AppUsers]([Id]) ON DELETE NO ACTION
    );
    CREATE NONCLUSTERED INDEX [IX_BikePosts_Status]          ON [BikePosts]([Status]);
    CREATE NONCLUSTERED INDEX [IX_BikePosts_Category_Status] ON [BikePosts]([Category], [Status]);
    CREATE NONCLUSTERED INDEX [IX_BikePosts_Brand_Status]    ON [BikePosts]([Brand], [Status]);
END
GO

-- 3.3 BikeImages
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BikeImages')
BEGIN
    CREATE TABLE [BikeImages] (
        [Id]           UNIQUEIDENTIFIER NOT NULL,
        [BikePostId]   UNIQUEIDENTIFIER NOT NULL,
        [ImageUrl]     NVARCHAR(500)    NOT NULL,
        [ThumbnailUrl] NVARCHAR(500)    NULL,
        [DisplayOrder] INT              NOT NULL DEFAULT 0,
        [IsPrimary]    BIT              NOT NULL DEFAULT 0,
        [Caption]      NVARCHAR(200)    NULL,
        -- BaseEntity
        [CreatedAt]    DATETIME2        NOT NULL,
        [UpdatedAt]    DATETIME2        NULL,
        [IsDeleted]    BIT              NOT NULL DEFAULT 0,
        [CreatedBy]    NVARCHAR(MAX)    NULL,
        [UpdatedBy]    NVARCHAR(MAX)    NULL,
        CONSTRAINT [PK_BikeImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BikeImages_BikePosts_BikePostId] FOREIGN KEY ([BikePostId]) REFERENCES [BikePosts]([Id]) ON DELETE CASCADE
    );
END
GO

-- 3.4 Orders
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
BEGIN
    CREATE TABLE [Orders] (
        [Id]                 UNIQUEIDENTIFIER NOT NULL,
        [OrderNumber]        NVARCHAR(50)     NOT NULL,
        [BuyerId]            UNIQUEIDENTIFIER NOT NULL,
        [SellerId]           UNIQUEIDENTIFIER NOT NULL,
        [BikePostId]         UNIQUEIDENTIFIER NOT NULL,
        [BikePrice]          DECIMAL(18,2)    NOT NULL,
        [DepositAmount]      DECIMAL(18,2)    NOT NULL,
        [DepositPercentage]  DECIMAL(5,2)     NOT NULL DEFAULT 15,
        [RemainingAmount]    DECIMAL(18,2)    NOT NULL,
        [ShippingFee]        DECIMAL(18,2)    NULL,
        [TotalAmount]        DECIMAL(18,2)    NOT NULL,
        [Status]             INT              NOT NULL,  -- OrderStatus enum
        [DepositPaidAt]      DATETIME2        NULL,
        [FullPaymentAt]      DATETIME2        NULL,
        [ShippedAt]          DATETIME2        NULL,
        [DeliveredAt]        DATETIME2        NULL,
        [CompletedAt]        DATETIME2        NULL,
        [CancelledAt]        DATETIME2        NULL,
        [CancellationReason] NVARCHAR(1000)   NULL,
        [ShippingAddress]    NVARCHAR(500)    NULL,
        [TrackingNumber]     NVARCHAR(100)    NULL,
        [ShippingProvider]   NVARCHAR(100)    NULL,
        [HasDispute]         BIT              NOT NULL DEFAULT 0,
        [DisputeReason]      NVARCHAR(2000)   NULL,
        [DisputeResolvedBy]  UNIQUEIDENTIFIER NULL,
        [DisputeResolvedAt]  DATETIME2        NULL,
        [DisputeResolution]  NVARCHAR(2000)   NULL,
        -- BaseEntity
        [CreatedAt]          DATETIME2        NOT NULL,
        [UpdatedAt]          DATETIME2        NULL,
        [IsDeleted]          BIT              NOT NULL DEFAULT 0,
        [CreatedBy]          NVARCHAR(MAX)    NULL,
        [UpdatedBy]          NVARCHAR(MAX)    NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Orders_AppUsers_BuyerId]    FOREIGN KEY ([BuyerId])    REFERENCES [AppUsers]([Id])  ON DELETE NO ACTION,
        CONSTRAINT [FK_Orders_AppUsers_SellerId]   FOREIGN KEY ([SellerId])   REFERENCES [AppUsers]([Id])  ON DELETE NO ACTION,
        CONSTRAINT [FK_Orders_BikePosts_BikePostId] FOREIGN KEY ([BikePostId]) REFERENCES [BikePosts]([Id]) ON DELETE NO ACTION
    );
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Orders_OrderNumber] ON [Orders]([OrderNumber]);
END
GO

-- 3.5 Payments
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Payments')
BEGIN
    CREATE TABLE [Payments] (
        [Id]                   UNIQUEIDENTIFIER NOT NULL,
        [OrderId]              UNIQUEIDENTIFIER NOT NULL,
        [TransactionId]        NVARCHAR(100)    NOT NULL,
        [Amount]               DECIMAL(18,2)    NOT NULL,
        [Type]                 INT              NOT NULL,  -- PaymentType
        [Method]               INT              NOT NULL,  -- PaymentMethod
        [Status]               INT              NOT NULL,  -- PaymentStatus
        [Gateway]              INT              NOT NULL,  -- PaymentGateway
        [GatewayTransactionId] NVARCHAR(200)    NULL,
        [GatewayResponse]      NVARCHAR(2000)   NULL,
        [ProcessedAt]          DATETIME2        NULL,
        [FailureReason]        NVARCHAR(1000)   NULL,
        [RefundAmount]         DECIMAL(18,2)    NULL,
        -- BaseEntity
        [CreatedAt]            DATETIME2        NOT NULL,
        [UpdatedAt]            DATETIME2        NULL,
        [IsDeleted]            BIT              NOT NULL DEFAULT 0,
        [CreatedBy]            NVARCHAR(MAX)    NULL,
        [UpdatedBy]            NVARCHAR(MAX)    NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]) ON DELETE NO ACTION
    );
END
GO

-- 3.6 Ratings
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Ratings')
BEGIN
    CREATE TABLE [Ratings] (
        [Id]                  UNIQUEIDENTIFIER NOT NULL,
        [OrderId]             UNIQUEIDENTIFIER NOT NULL,
        [FromUserId]          UNIQUEIDENTIFIER NOT NULL,
        [ToUserId]            UNIQUEIDENTIFIER NOT NULL,
        [Stars]               INT              NOT NULL,
        [Comment]             NVARCHAR(1000)   NULL,
        [CommunicationRating] INT              NULL,
        [AccuracyRating]      INT              NULL,
        [PackagingRating]     INT              NULL,
        [SpeedRating]         INT              NULL,
        [IsPublic]            BIT              NOT NULL DEFAULT 1,
        [SellerResponse]      NVARCHAR(1000)   NULL,
        [SellerRespondedAt]   DATETIME2        NULL,
        -- BaseEntity
        [CreatedAt]           DATETIME2        NOT NULL,
        [UpdatedAt]           DATETIME2        NULL,
        [IsDeleted]           BIT              NOT NULL DEFAULT 0,
        [CreatedBy]           NVARCHAR(MAX)    NULL,
        [UpdatedBy]           NVARCHAR(MAX)    NULL,
        CONSTRAINT [PK_Ratings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Ratings_Orders_OrderId]      FOREIGN KEY ([OrderId])    REFERENCES [Orders]([Id])   ON DELETE NO ACTION,
        CONSTRAINT [FK_Ratings_AppUsers_FromUserId] FOREIGN KEY ([FromUserId]) REFERENCES [AppUsers]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Ratings_AppUsers_ToUserId]   FOREIGN KEY ([ToUserId])   REFERENCES [AppUsers]([Id]) ON DELETE NO ACTION
    );
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Ratings_OrderId] ON [Ratings]([OrderId]);
END
GO

-- 3.7 Messages
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Messages')
BEGIN
    CREATE TABLE [Messages] (
        [Id]            UNIQUEIDENTIFIER NOT NULL,
        [SenderId]      UNIQUEIDENTIFIER NOT NULL,
        [ReceiverId]    UNIQUEIDENTIFIER NOT NULL,
        [BikePostId]    UNIQUEIDENTIFIER NULL,
        [Content]       NVARCHAR(2000)   NOT NULL,
        [Type]          INT              NOT NULL DEFAULT 1,  -- MessageType
        [AttachmentUrl] NVARCHAR(500)    NULL,
        [IsRead]        BIT              NOT NULL DEFAULT 0,
        [ReadAt]        DATETIME2        NULL,
        -- BaseEntity
        [CreatedAt]     DATETIME2        NOT NULL,
        [UpdatedAt]     DATETIME2        NULL,
        [IsDeleted]     BIT              NOT NULL DEFAULT 0,
        [CreatedBy]     NVARCHAR(MAX)    NULL,
        [UpdatedBy]     NVARCHAR(MAX)    NULL,
        CONSTRAINT [PK_Messages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Messages_AppUsers_SenderId]     FOREIGN KEY ([SenderId])   REFERENCES [AppUsers]([Id])  ON DELETE NO ACTION,
        CONSTRAINT [FK_Messages_AppUsers_ReceiverId]   FOREIGN KEY ([ReceiverId]) REFERENCES [AppUsers]([Id])  ON DELETE NO ACTION,
        CONSTRAINT [FK_Messages_BikePosts_BikePostId]  FOREIGN KEY ([BikePostId]) REFERENCES [BikePosts]([Id]) ON DELETE SET NULL
    );
    CREATE NONCLUSTERED INDEX [IX_Messages_SenderId_ReceiverId] ON [Messages]([SenderId], [ReceiverId]);
END
GO

-- 3.8 InspectionReports
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InspectionReports')
BEGIN
    CREATE TABLE [InspectionReports] (
        [Id]                    UNIQUEIDENTIFIER NOT NULL,
        [BikePostId]            UNIQUEIDENTIFIER NOT NULL,
        [InspectorId]           UNIQUEIDENTIFIER NOT NULL,
        [ReportNumber]          NVARCHAR(50)     NOT NULL,
        [Status]                INT              NOT NULL,  -- InspectionStatus
        [OverallCondition]      INT              NOT NULL,  -- OverallCondition
        [EstimatedValue]        DECIMAL(18,2)    NULL,
        [IsRecommended]         BIT              NOT NULL DEFAULT 0,
        [Summary]               NVARCHAR(3000)   NULL,
        [FrameScore]            INT              NOT NULL DEFAULT 5,
        [BrakesScore]           INT              NOT NULL DEFAULT 5,
        [GearsScore]            INT              NOT NULL DEFAULT 5,
        [WheelsScore]           INT              NOT NULL DEFAULT 5,
        [TiresScore]            INT              NOT NULL DEFAULT 5,
        [ChainScore]            INT              NOT NULL DEFAULT 5,
        [HasFrameDamage]        BIT              NOT NULL DEFAULT 0,
        [FrameNotes]            NVARCHAR(1000)   NULL,
        [HasRust]               BIT              NOT NULL DEFAULT 0,
        [HasCracks]             BIT              NOT NULL DEFAULT 0,
        [AllComponentsOriginal] BIT              NOT NULL DEFAULT 1,
        [ReplacedComponents]    NVARCHAR(1000)   NULL,
        [InspectedAt]           DATETIME2        NULL,
        -- BaseEntity
        [CreatedAt]             DATETIME2        NOT NULL,
        [UpdatedAt]             DATETIME2        NULL,
        [IsDeleted]             BIT              NOT NULL DEFAULT 0,
        [CreatedBy]             NVARCHAR(MAX)    NULL,
        [UpdatedBy]             NVARCHAR(MAX)    NULL,
        CONSTRAINT [PK_InspectionReports] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InspectionReports_BikePosts_BikePostId]   FOREIGN KEY ([BikePostId])  REFERENCES [BikePosts]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InspectionReports_AppUsers_InspectorId]   FOREIGN KEY ([InspectorId]) REFERENCES [AppUsers]([Id])  ON DELETE NO ACTION
    );
    CREATE UNIQUE NONCLUSTERED INDEX [IX_InspectionReports_ReportNumber] ON [InspectionReports]([ReportNumber]);
    CREATE UNIQUE NONCLUSTERED INDEX [IX_InspectionReports_BikePostId]   ON [InspectionReports]([BikePostId]);
END
GO

-- 3.9 Wishlists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Wishlists')
BEGIN
    CREATE TABLE [Wishlists] (
        [Id]         UNIQUEIDENTIFIER NOT NULL,
        [UserId]     UNIQUEIDENTIFIER NOT NULL,
        [BikePostId] UNIQUEIDENTIFIER NOT NULL,
        [Notes]      NVARCHAR(500)    NULL,
        -- BaseEntity
        [CreatedAt]  DATETIME2        NOT NULL,
        [UpdatedAt]  DATETIME2        NULL,
        [IsDeleted]  BIT              NOT NULL DEFAULT 0,
        [CreatedBy]  NVARCHAR(MAX)    NULL,
        [UpdatedBy]  NVARCHAR(MAX)    NULL,
        CONSTRAINT [PK_Wishlists] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Wishlists_AppUsers_UserId]      FOREIGN KEY ([UserId])     REFERENCES [AppUsers]([Id])  ON DELETE CASCADE,
        CONSTRAINT [FK_Wishlists_BikePosts_BikePostId] FOREIGN KEY ([BikePostId]) REFERENCES [BikePosts]([Id]) ON DELETE CASCADE
    );
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Wishlists_UserId_BikePostId] ON [Wishlists]([UserId], [BikePostId]);
END
GO

-- 3.10 Wallets
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Wallets')
BEGIN
    CREATE TABLE [Wallets] (
        [Id]        UNIQUEIDENTIFIER NOT NULL,
        [UserId]    UNIQUEIDENTIFIER NOT NULL,
        [Balance]   DECIMAL(18,2)    NOT NULL DEFAULT 0,
        [Currency]  NVARCHAR(10)     NOT NULL DEFAULT 'VND',
        [IsLocked]  BIT              NOT NULL DEFAULT 0,
        -- BaseEntity
        [CreatedAt] DATETIME2        NOT NULL,
        [UpdatedAt] DATETIME2        NULL,
        [IsDeleted] BIT              NOT NULL DEFAULT 0,
        [CreatedBy] NVARCHAR(MAX)    NULL,
        [UpdatedBy] NVARCHAR(MAX)    NULL,
        CONSTRAINT [PK_Wallets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Wallets_AppUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUsers]([Id]) ON DELETE NO ACTION
    );
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Wallets_UserId] ON [Wallets]([UserId]);
END
GO

-- 3.11 WalletTransactions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WalletTransactions')
BEGIN
    CREATE TABLE [WalletTransactions] (
        [Id]                UNIQUEIDENTIFIER NOT NULL,
        [WalletId]          UNIQUEIDENTIFIER NOT NULL,
        [TransactionNumber] NVARCHAR(50)     NOT NULL,
        [Type]              INT              NOT NULL,  -- WalletTransactionType
        [Amount]            DECIMAL(18,2)    NOT NULL,
        [BalanceBefore]     DECIMAL(18,2)    NOT NULL,
        [BalanceAfter]      DECIMAL(18,2)    NOT NULL,
        [Description]       NVARCHAR(500)    NOT NULL,
        [RelatedOrderId]    UNIQUEIDENTIFIER NULL,
        -- BaseEntity
        [CreatedAt]         DATETIME2        NOT NULL,
        [UpdatedAt]         DATETIME2        NULL,
        [IsDeleted]         BIT              NOT NULL DEFAULT 0,
        [CreatedBy]         NVARCHAR(MAX)    NULL,
        [UpdatedBy]         NVARCHAR(MAX)    NULL,
        CONSTRAINT [PK_WalletTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WalletTransactions_Wallets_WalletId] FOREIGN KEY ([WalletId]) REFERENCES [Wallets]([Id]) ON DELETE NO ACTION
    );
END
GO


-- =============================================
-- 4. SEED: IDENTITY ROLES
-- =============================================
IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = 'Buyer')
    INSERT INTO [AspNetRoles] ([Id],[Name],[NormalizedName],[ConcurrencyStamp])
    VALUES (NEWID(),'Buyer','BUYER',NEWID());

IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = 'Seller')
    INSERT INTO [AspNetRoles] ([Id],[Name],[NormalizedName],[ConcurrencyStamp])
    VALUES (NEWID(),'Seller','SELLER',NEWID());

IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = 'Inspector')
    INSERT INTO [AspNetRoles] ([Id],[Name],[NormalizedName],[ConcurrencyStamp])
    VALUES (NEWID(),'Inspector','INSPECTOR',NEWID());

IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = 'Admin')
    INSERT INTO [AspNetRoles] ([Id],[Name],[NormalizedName],[ConcurrencyStamp])
    VALUES (NEWID(),'Admin','ADMIN',NEWID());
GO


-- =============================================
-- 5. SEED: IDENTITY USERS (password = "Test@123456")
--    PasswordHash generated by ASP.NET Identity Hasher V3
-- =============================================
DECLARE @PasswordHash NVARCHAR(MAX) = N'AQAAAAIAAYagAAAAEDummyHashForTestingPurposes==';
-- NOTE: This is a placeholder hash. The real login will NOT work with this hash.
--       The app will use Identity's UserManager.CreateAsync to create real users.
--       Below we create Identity users so FK constraints are satisfied, then use
--       the app's /register endpoint for real logins.

DECLARE @IdBuyer1    NVARCHAR(450) = 'id-buyer-001';
DECLARE @IdBuyer2    NVARCHAR(450) = 'id-buyer-002';
DECLARE @IdSeller1   NVARCHAR(450) = 'id-seller-001';
DECLARE @IdSeller2   NVARCHAR(450) = 'id-seller-002';
DECLARE @IdInspector NVARCHAR(450) = 'id-inspector-001';
DECLARE @IdAdmin     NVARCHAR(450) = 'id-admin-001';

-- Buyer 1
IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = @IdBuyer1)
    INSERT INTO [AspNetUsers] ([Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],[PasswordHash],[SecurityStamp],[ConcurrencyStamp],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEnabled],[AccessFailedCount])
    VALUES (@IdBuyer1,'buyer1@secondbike.com','BUYER1@SECONDBIKE.COM','buyer1@secondbike.com','BUYER1@SECONDBIKE.COM',1,@PasswordHash,NEWID(),NEWID(),0,0,1,0);

-- Buyer 2
IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = @IdBuyer2)
    INSERT INTO [AspNetUsers] ([Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],[PasswordHash],[SecurityStamp],[ConcurrencyStamp],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEnabled],[AccessFailedCount])
    VALUES (@IdBuyer2,'buyer2@secondbike.com','BUYER2@SECONDBIKE.COM','buyer2@secondbike.com','BUYER2@SECONDBIKE.COM',1,@PasswordHash,NEWID(),NEWID(),0,0,1,0);

-- Seller 1
IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = @IdSeller1)
    INSERT INTO [AspNetUsers] ([Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],[PasswordHash],[SecurityStamp],[ConcurrencyStamp],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEnabled],[AccessFailedCount])
    VALUES (@IdSeller1,'seller1@secondbike.com','SELLER1@SECONDBIKE.COM','seller1@secondbike.com','SELLER1@SECONDBIKE.COM',1,@PasswordHash,NEWID(),NEWID(),0,0,1,0);

-- Seller 2
IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = @IdSeller2)
    INSERT INTO [AspNetUsers] ([Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],[PasswordHash],[SecurityStamp],[ConcurrencyStamp],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEnabled],[AccessFailedCount])
    VALUES (@IdSeller2,'seller2@secondbike.com','SELLER2@SECONDBIKE.COM','seller2@secondbike.com','SELLER2@SECONDBIKE.COM',1,@PasswordHash,NEWID(),NEWID(),0,0,1,0);

-- Inspector
IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = @IdInspector)
    INSERT INTO [AspNetUsers] ([Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],[PasswordHash],[SecurityStamp],[ConcurrencyStamp],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEnabled],[AccessFailedCount])
    VALUES (@IdInspector,'inspector@secondbike.com','INSPECTOR@SECONDBIKE.COM','inspector@secondbike.com','INSPECTOR@SECONDBIKE.COM',1,@PasswordHash,NEWID(),NEWID(),0,0,1,0);

-- Admin
IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = @IdAdmin)
    INSERT INTO [AspNetUsers] ([Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],[PasswordHash],[SecurityStamp],[ConcurrencyStamp],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEnabled],[AccessFailedCount])
    VALUES (@IdAdmin,'admin@secondbike.com','ADMIN@SECONDBIKE.COM','admin@secondbike.com','ADMIN@SECONDBIKE.COM',1,@PasswordHash,NEWID(),NEWID(),0,0,1,0);
GO


-- =============================================
-- 6. SEED: APP USERS
-- =============================================
DECLARE @Now DATETIME2 = GETUTCDATE();

DECLARE @AppBuyer1    UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @AppBuyer2    UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @AppSeller1   UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';
DECLARE @AppSeller2   UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444';
DECLARE @AppInspector UNIQUEIDENTIFIER = '55555555-5555-5555-5555-555555555555';
DECLARE @AppAdmin     UNIQUEIDENTIFIER = '66666666-6666-6666-6666-666666666666';

IF NOT EXISTS (SELECT 1 FROM [AppUsers] WHERE [Id] = @AppBuyer1)
    INSERT INTO [AppUsers] ([Id],[Email],[FullName],[PhoneNumber],[AvatarUrl],[Role],[Status],[ShopName],[ShopDescription],[IsVerifiedSeller],[SellerRating],[TotalRatingsCount],[IdentityUserId],[CreatedAt],[IsDeleted])
    VALUES (@AppBuyer1,'buyer1@secondbike.com',N'Nguy?n V?n An','0901234567',NULL,1,1,NULL,NULL,0,0,0,'id-buyer-001',@Now,0);

IF NOT EXISTS (SELECT 1 FROM [AppUsers] WHERE [Id] = @AppBuyer2)
    INSERT INTO [AppUsers] ([Id],[Email],[FullName],[PhoneNumber],[AvatarUrl],[Role],[Status],[ShopName],[ShopDescription],[IsVerifiedSeller],[SellerRating],[TotalRatingsCount],[IdentityUserId],[CreatedAt],[IsDeleted])
    VALUES (@AppBuyer2,'buyer2@secondbike.com',N'Tr?n Th? Bình','0912345678',NULL,1,1,NULL,NULL,0,0,0,'id-buyer-002',@Now,0);

IF NOT EXISTS (SELECT 1 FROM [AppUsers] WHERE [Id] = @AppSeller1)
    INSERT INTO [AppUsers] ([Id],[Email],[FullName],[PhoneNumber],[AvatarUrl],[Role],[Status],[ShopName],[ShopDescription],[IsVerifiedSeller],[SellerRating],[TotalRatingsCount],[IdentityUserId],[CreatedAt],[IsDeleted])
    VALUES (@AppSeller1,'seller1@secondbike.com',N'Lê Hoàng C??ng','0923456789',NULL,2,1,N'C??ng Bike Shop',N'Chuyên xe ??p th? thao ?ã qua s? d?ng',1,4.50,12,'id-seller-001',@Now,0);

IF NOT EXISTS (SELECT 1 FROM [AppUsers] WHERE [Id] = @AppSeller2)
    INSERT INTO [AppUsers] ([Id],[Email],[FullName],[PhoneNumber],[AvatarUrl],[Role],[Status],[ShopName],[ShopDescription],[IsVerifiedSeller],[SellerRating],[TotalRatingsCount],[IdentityUserId],[CreatedAt],[IsDeleted])
    VALUES (@AppSeller2,'seller2@secondbike.com',N'Ph?m Minh ??c','0934567890',NULL,2,1,N'??c Pro Cycling',N'Xe ??p road cao c?p, nh?p kh?u',1,4.80,8,'id-seller-002',@Now,0);

IF NOT EXISTS (SELECT 1 FROM [AppUsers] WHERE [Id] = @AppInspector)
    INSERT INTO [AppUsers] ([Id],[Email],[FullName],[PhoneNumber],[AvatarUrl],[Role],[Status],[ShopName],[ShopDescription],[IsVerifiedSeller],[SellerRating],[TotalRatingsCount],[IdentityUserId],[CreatedAt],[IsDeleted])
    VALUES (@AppInspector,'inspector@secondbike.com',N'V? Thanh H?i','0945678901',NULL,3,1,NULL,NULL,0,0,0,'id-inspector-001',@Now,0);

IF NOT EXISTS (SELECT 1 FROM [AppUsers] WHERE [Id] = @AppAdmin)
    INSERT INTO [AppUsers] ([Id],[Email],[FullName],[PhoneNumber],[AvatarUrl],[Role],[Status],[ShopName],[ShopDescription],[IsVerifiedSeller],[SellerRating],[TotalRatingsCount],[IdentityUserId],[CreatedAt],[IsDeleted])
    VALUES (@AppAdmin,'admin@secondbike.com',N'Admin SecondBike','0900000000',NULL,4,1,NULL,NULL,0,0,0,'id-admin-001',@Now,0);
GO


-- =============================================
-- 7. SEED: BIKE POSTS (6 bikes — various categories)
-- =============================================
DECLARE @Now2 DATETIME2 = GETUTCDATE();
DECLARE @Seller1 UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';
DECLARE @Seller2 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444';

DECLARE @Bike1 UNIQUEIDENTIFIER = 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA';
DECLARE @Bike2 UNIQUEIDENTIFIER = 'BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB';
DECLARE @Bike3 UNIQUEIDENTIFIER = 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC';
DECLARE @Bike4 UNIQUEIDENTIFIER = 'DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD';
DECLARE @Bike5 UNIQUEIDENTIFIER = 'EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE';
DECLARE @Bike6 UNIQUEIDENTIFIER = 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF';

-- Status: 1=Draft, 2=PendingModeration, 3=Active, 4=Sold
-- Category: 1=RoadBike, 2=MountainBike, 3=Hybrid, 4=Gravel, 5=CycloCross, 6=Touring, 7=BMX, 8=Electric
-- Size: 1=XS, 2=S, 3=M, 4=L, 5=XL
-- Condition: 1=New, 2=LikeNew, 3=Excellent, 4=Good, 5=Fair, 6=NeedsRepair

IF NOT EXISTS (SELECT 1 FROM [BikePosts] WHERE [Id] = @Bike1)
INSERT INTO [BikePosts] ([Id],[SellerId],[Title],[Description],[Price],[Status],[Brand],[Model],[Year],[Category],[Size],[FrameMaterial],[Color],[Condition],[WeightKg],[OdometerKm],[UsageHistory],[HasAccidents],[City],[District],[PublishedAt],[ViewCount],[WishlistCount],[CreatedAt],[IsDeleted])
VALUES (@Bike1, @Seller1,
    N'Giant TCR Advanced Pro 2023 — Carbon Road Bike',
    N'Xe ??p road carbon cao c?p, Shimano Ultegra Di2, bánh DT Swiss. ?ã ?i 2,500km, b?o d??ng ??nh k? t?i Giant Store. Lý do bán: nâng ??i lên Pinarello.',
    45000000, 3, 'Giant', 'TCR Advanced Pro', 2023, 1, 3, 'Carbon', 'Matte Black', 2, 7.20, 2500,
    N'Weekend rides only, always stored indoors', 0,
    N'Ho Chi Minh City', N'District 2', @Now2, 156, 12, @Now2, 0);

IF NOT EXISTS (SELECT 1 FROM [BikePosts] WHERE [Id] = @Bike2)
INSERT INTO [BikePosts] ([Id],[SellerId],[Title],[Description],[Price],[Status],[Brand],[Model],[Year],[Category],[Size],[FrameMaterial],[Color],[Condition],[WeightKg],[OdometerKm],[UsageHistory],[HasAccidents],[City],[District],[PublishedAt],[ViewCount],[WishlistCount],[CreatedAt],[IsDeleted])
VALUES (@Bike2, @Seller1,
    N'Trek Marlin 7 2022 — Mountain Bike 29er',
    N'MTB hardtail ch?t l??ng, Shimano Deore 1x10, phu?c RockShox Judy. Phù h?p trail riding và commute. ?ã thay l?p m?i Maxxis Ardent.',
    12500000, 3, 'Trek', 'Marlin 7', 2022, 2, 4, 'Aluminum', 'Viper Red', 4, 13.50, 5200,
    N'Trail riding weekends, daily commute', 0,
    N'Ha Noi', N'Cau Giay', @Now2, 89, 7, @Now2, 0);

IF NOT EXISTS (SELECT 1 FROM [BikePosts] WHERE [Id] = @Bike3)
INSERT INTO [BikePosts] ([Id],[SellerId],[Title],[Description],[Price],[Status],[Brand],[Model],[Year],[Category],[Size],[FrameMaterial],[Color],[Condition],[WeightKg],[OdometerKm],[UsageHistory],[HasAccidents],[City],[District],[PublishedAt],[ViewCount],[WishlistCount],[CreatedAt],[IsDeleted])
VALUES (@Bike3, @Seller2,
    N'Specialized Diverge 2024 — Gravel Adventure',
    N'Gravel bike m?i mua 3 tháng, Future Shock 2.0, SRAM Rival eTap AXS. Nh? m?i, còn b?o hành. Bán vì chuy?n sang ch?i MTB.',
    55000000, 3, 'Specialized', 'Diverge Expert', 2024, 4, 3, 'Carbon', 'Gloss Forest Green', 2, 8.90, 800,
    N'Light gravel rides only', 0,
    N'Ho Chi Minh City', N'Thu Duc', @Now2, 234, 18, @Now2, 0);

IF NOT EXISTS (SELECT 1 FROM [BikePosts] WHERE [Id] = @Bike4)
INSERT INTO [BikePosts] ([Id],[SellerId],[Title],[Description],[Price],[Status],[Brand],[Model],[Year],[Category],[Size],[FrameMaterial],[Color],[Condition],[WeightKg],[OdometerKm],[UsageHistory],[HasAccidents],[City],[District],[PublishedAt],[ViewCount],[WishlistCount],[CreatedAt],[IsDeleted])
VALUES (@Bike4, @Seller2,
    N'Cannondale CAAD13 Disc 2023 — Aluminum Road Racer',
    N'Nhôm nh? nh?t phân khúc, Shimano 105 R7000, phanh ??a. T?c ?? và s? tho?i mái. ?ã ?i 4,000km, service m?i nh?t tháng tr??c.',
    28000000, 3, 'Cannondale', 'CAAD13 Disc', 2023, 1, 4, 'Aluminum', 'Rally Red', 3, 8.50, 4000,
    N'Training and racing', 0,
    N'Da Nang', N'Hai Chau', @Now2, 67, 5, @Now2, 0);

IF NOT EXISTS (SELECT 1 FROM [BikePosts] WHERE [Id] = @Bike5)
INSERT INTO [BikePosts] ([Id],[SellerId],[Title],[Description],[Price],[Status],[Brand],[Model],[Year],[Category],[Size],[FrameMaterial],[Color],[Condition],[WeightKg],[OdometerKm],[UsageHistory],[HasAccidents],[City],[District],[PublishedAt],[ViewCount],[WishlistCount],[CreatedAt],[IsDeleted])
VALUES (@Bike5, @Seller1,
    N'VinFast DrgnFly E-Bike 2024 — Electric City Bike',
    N'Xe ??p ?i?n VinFast, pin 36V 13Ah, ?i ???c 60km/l?n s?c. Motor 250W, LCD display. M?i mua 2 tháng, còn b?o hành 22 tháng.',
    18000000, 2, 'VinFast', 'DrgnFly', 2024, 8, 3, 'Aluminum', 'Space Grey', 1, 22.00, 300,
    N'City commute only', 0,
    N'Ho Chi Minh City', N'District 7', NULL, 0, 0, @Now2, 0);

IF NOT EXISTS (SELECT 1 FROM [BikePosts] WHERE [Id] = @Bike6)
INSERT INTO [BikePosts] ([Id],[SellerId],[Title],[Description],[Price],[Status],[Brand],[Model],[Year],[Category],[Size],[FrameMaterial],[Color],[Condition],[WeightKg],[OdometerKm],[UsageHistory],[HasAccidents],[City],[District],[PublishedAt],[ViewCount],[WishlistCount],[CreatedAt],[IsDeleted])
VALUES (@Bike6, @Seller2,
    N'BMC Teammachine SLR 2022 — Pro-Level Carbon',
    N'Frameset BMC Teammachine, groupset Shimano Dura-Ace R9200, wheelset Roval CLX50. Xe ?ua chuyên nghi?p.',
    85000000, 3, 'BMC', 'Teammachine SLR01', 2022, 1, 2, 'Carbon', 'Team White/Red', 3, 6.80, 8000,
    N'Racing and training, professionally maintained', 0,
    N'Ho Chi Minh City', N'District 1', @Now2, 312, 25, @Now2, 0);
GO


-- =============================================
-- 8. SEED: BIKE IMAGES (2-3 per bike)
-- =============================================
DECLARE @Bike1Id UNIQUEIDENTIFIER = 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA';
DECLARE @Bike2Id UNIQUEIDENTIFIER = 'BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB';
DECLARE @Bike3Id UNIQUEIDENTIFIER = 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC';
DECLARE @Bike4Id UNIQUEIDENTIFIER = 'DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD';
DECLARE @Bike5Id UNIQUEIDENTIFIER = 'EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE';
DECLARE @Bike6Id UNIQUEIDENTIFIER = 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF';
DECLARE @Now3 DATETIME2 = GETUTCDATE();

IF NOT EXISTS (SELECT 1 FROM [BikeImages] WHERE [BikePostId] = @Bike1Id)
BEGIN
    INSERT INTO [BikeImages] ([Id],[BikePostId],[ImageUrl],[ThumbnailUrl],[DisplayOrder],[IsPrimary],[CreatedAt],[IsDeleted])
    VALUES (NEWID(), @Bike1Id, 'https://images.unsplash.com/photo-1485965120184-e220f721d03e?w=800', 'https://images.unsplash.com/photo-1485965120184-e220f721d03e?w=200', 0, 1, @Now3, 0),
           (NEWID(), @Bike1Id, 'https://images.unsplash.com/photo-1571068316344-75bc76f77890?w=800', 'https://images.unsplash.com/photo-1571068316344-75bc76f77890?w=200', 1, 0, @Now3, 0);
END

IF NOT EXISTS (SELECT 1 FROM [BikeImages] WHERE [BikePostId] = @Bike2Id)
BEGIN
    INSERT INTO [BikeImages] ([Id],[BikePostId],[ImageUrl],[ThumbnailUrl],[DisplayOrder],[IsPrimary],[CreatedAt],[IsDeleted])
    VALUES (NEWID(), @Bike2Id, 'https://images.unsplash.com/photo-1576435728678-68d0fbf94e91?w=800', 'https://images.unsplash.com/photo-1576435728678-68d0fbf94e91?w=200', 0, 1, @Now3, 0),
           (NEWID(), @Bike2Id, 'https://images.unsplash.com/photo-1544191696-102dbdaeeaa0?w=800', 'https://images.unsplash.com/photo-1544191696-102dbdaeeaa0?w=200', 1, 0, @Now3, 0);
END

IF NOT EXISTS (SELECT 1 FROM [BikeImages] WHERE [BikePostId] = @Bike3Id)
BEGIN
    INSERT INTO [BikeImages] ([Id],[BikePostId],[ImageUrl],[ThumbnailUrl],[DisplayOrder],[IsPrimary],[CreatedAt],[IsDeleted])
    VALUES (NEWID(), @Bike3Id, 'https://images.unsplash.com/photo-1532298229144-0ec0c57515c7?w=800', 'https://images.unsplash.com/photo-1532298229144-0ec0c57515c7?w=200', 0, 1, @Now3, 0),
           (NEWID(), @Bike3Id, 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=800', 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=200', 1, 0, @Now3, 0),
           (NEWID(), @Bike3Id, 'https://images.unsplash.com/photo-1558618666-fcd25c85f82e?w=800', 'https://images.unsplash.com/photo-1558618666-fcd25c85f82e?w=200', 2, 0, @Now3, 0);
END

IF NOT EXISTS (SELECT 1 FROM [BikeImages] WHERE [BikePostId] = @Bike4Id)
BEGIN
    INSERT INTO [BikeImages] ([Id],[BikePostId],[ImageUrl],[ThumbnailUrl],[DisplayOrder],[IsPrimary],[CreatedAt],[IsDeleted])
    VALUES (NEWID(), @Bike4Id, 'https://images.unsplash.com/photo-1505705694340-019e56e27e5a?w=800', 'https://images.unsplash.com/photo-1505705694340-019e56e27e5a?w=200', 0, 1, @Now3, 0),
           (NEWID(), @Bike4Id, 'https://images.unsplash.com/photo-1511994298241-608e28f14fde?w=800', 'https://images.unsplash.com/photo-1511994298241-608e28f14fde?w=200', 1, 0, @Now3, 0);
END

IF NOT EXISTS (SELECT 1 FROM [BikeImages] WHERE [BikePostId] = @Bike5Id)
BEGIN
    INSERT INTO [BikeImages] ([Id],[BikePostId],[ImageUrl],[ThumbnailUrl],[DisplayOrder],[IsPrimary],[CreatedAt],[IsDeleted])
    VALUES (NEWID(), @Bike5Id, 'https://images.unsplash.com/photo-1558618666-fcd25c85f82e?w=800', 'https://images.unsplash.com/photo-1558618666-fcd25c85f82e?w=200', 0, 1, @Now3, 0);
END

IF NOT EXISTS (SELECT 1 FROM [BikeImages] WHERE [BikePostId] = @Bike6Id)
BEGIN
    INSERT INTO [BikeImages] ([Id],[BikePostId],[ImageUrl],[ThumbnailUrl],[DisplayOrder],[IsPrimary],[CreatedAt],[IsDeleted])
    VALUES (NEWID(), @Bike6Id, 'https://images.unsplash.com/photo-1517649763962-0c623066013b?w=800', 'https://images.unsplash.com/photo-1517649763962-0c623066013b?w=200', 0, 1, @Now3, 0),
           (NEWID(), @Bike6Id, 'https://images.unsplash.com/photo-1571068316344-75bc76f77890?w=800', 'https://images.unsplash.com/photo-1571068316344-75bc76f77890?w=200', 1, 0, @Now3, 0),
           (NEWID(), @Bike6Id, 'https://images.unsplash.com/photo-1485965120184-e220f721d03e?w=800', 'https://images.unsplash.com/photo-1485965120184-e220f721d03e?w=200', 2, 0, @Now3, 0);
END
GO


-- =============================================
-- 9. SEED: ORDERS (2 orders — different statuses)
-- =============================================
DECLARE @Now4 DATETIME2 = GETUTCDATE();
DECLARE @Order1 UNIQUEIDENTIFIER = 'A1A1A1A1-A1A1-A1A1-A1A1-A1A1A1A1A1A1';
DECLARE @Order2 UNIQUEIDENTIFIER = 'B2B2B2B2-B2B2-B2B2-B2B2-B2B2B2B2B2B2';

-- Order 1: Buyer1 buys Bike4 from Seller2 — Completed
IF NOT EXISTS (SELECT 1 FROM [Orders] WHERE [Id] = @Order1)
INSERT INTO [Orders] ([Id],[OrderNumber],[BuyerId],[SellerId],[BikePostId],[BikePrice],[DepositAmount],[DepositPercentage],[RemainingAmount],[TotalAmount],[Status],[DepositPaidAt],[FullPaymentAt],[CompletedAt],[ShippingAddress],[TrackingNumber],[ShippingProvider],[HasDispute],[CreatedAt],[IsDeleted])
VALUES (@Order1, 'SB-20250101-ABC123',
    '11111111-1111-1111-1111-111111111111', -- Buyer1
    '44444444-4444-4444-4444-444444444444', -- Seller2
    'DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD', -- Bike4
    28000000, 4200000, 15, 23800000, 28000000,
    6, -- Completed
    DATEADD(DAY,-10,@Now4), DATEADD(DAY,-7,@Now4), DATEADD(DAY,-2,@Now4),
    N'123 Nguy?n Hu?, Qu?n 1, TP.HCM', 'VN123456789', 'GiaoHangNhanh',
    0, DATEADD(DAY,-12,@Now4), 0);

-- Order 2: Buyer2 buys Bike1 from Seller1 — DepositPaid, waiting delivery
IF NOT EXISTS (SELECT 1 FROM [Orders] WHERE [Id] = @Order2)
INSERT INTO [Orders] ([Id],[OrderNumber],[BuyerId],[SellerId],[BikePostId],[BikePrice],[DepositAmount],[DepositPercentage],[RemainingAmount],[TotalAmount],[Status],[DepositPaidAt],[ShippingAddress],[HasDispute],[CreatedAt],[IsDeleted])
VALUES (@Order2, 'SB-20250115-DEF456',
    '22222222-2222-2222-2222-222222222222', -- Buyer2
    '33333333-3333-3333-3333-333333333333', -- Seller1
    'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', -- Bike1
    45000000, 6750000, 15, 38250000, 45000000,
    2, -- DepositPaid
    DATEADD(DAY,-3,@Now4),
    N'456 Lê L?i, Qu?n 3, TP.HCM',
    0, DATEADD(DAY,-5,@Now4), 0);
GO


-- =============================================
-- 10. SEED: PAYMENTS
-- =============================================
DECLARE @Now5 DATETIME2 = GETUTCDATE();

-- Payment for Order1 deposit
IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [OrderId] = 'A1A1A1A1-A1A1-A1A1-A1A1-A1A1A1A1A1A1' AND [Type] = 1)
INSERT INTO [Payments] ([Id],[OrderId],[TransactionId],[Amount],[Type],[Method],[Status],[Gateway],[ProcessedAt],[CreatedAt],[IsDeleted])
VALUES (NEWID(), 'A1A1A1A1-A1A1-A1A1-A1A1-A1A1A1A1A1A1', 'PAY-DEPOSIT-001', 4200000, 1, 3, 3, 1, DATEADD(DAY,-10,@Now5), DATEADD(DAY,-10,@Now5), 0);

-- Payment for Order1 final
IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [OrderId] = 'A1A1A1A1-A1A1-A1A1-A1A1-A1A1A1A1A1A1' AND [Type] = 2)
INSERT INTO [Payments] ([Id],[OrderId],[TransactionId],[Amount],[Type],[Method],[Status],[Gateway],[ProcessedAt],[CreatedAt],[IsDeleted])
VALUES (NEWID(), 'A1A1A1A1-A1A1-A1A1-A1A1-A1A1A1A1A1A1', 'PAY-FINAL-001', 23800000, 2, 3, 3, 1, DATEADD(DAY,-7,@Now5), DATEADD(DAY,-7,@Now5), 0);

-- Payment for Order2 deposit
IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [OrderId] = 'B2B2B2B2-B2B2-B2B2-B2B2-B2B2B2B2B2B2' AND [Type] = 1)
INSERT INTO [Payments] ([Id],[OrderId],[TransactionId],[Amount],[Type],[Method],[Status],[Gateway],[ProcessedAt],[CreatedAt],[IsDeleted])
VALUES (NEWID(), 'B2B2B2B2-B2B2-B2B2-B2B2-B2B2B2B2B2B2', 'PAY-DEPOSIT-002', 6750000, 1, 4, 3, 2, DATEADD(DAY,-3,@Now5), DATEADD(DAY,-3,@Now5), 0);
GO


-- =============================================
-- 11. SEED: RATINGS (1 completed order rating)
-- =============================================
DECLARE @Now6 DATETIME2 = GETUTCDATE();

IF NOT EXISTS (SELECT 1 FROM [Ratings] WHERE [OrderId] = 'A1A1A1A1-A1A1-A1A1-A1A1-A1A1A1A1A1A1')
INSERT INTO [Ratings] ([Id],[OrderId],[FromUserId],[ToUserId],[Stars],[Comment],[CommunicationRating],[AccuracyRating],[PackagingRating],[SpeedRating],[IsPublic],[SellerResponse],[SellerRespondedAt],[CreatedAt],[IsDeleted])
VALUES (NEWID(),
    'A1A1A1A1-A1A1-A1A1-A1A1-A1A1A1A1A1A1',
    '11111111-1111-1111-1111-111111111111', -- from Buyer1
    '44444444-4444-4444-4444-444444444444', -- to Seller2
    5,
    N'Xe ??p nh? mô t?, giao hàng nhanh, ?óng gói c?n th?n. Seller r?t nhi?t tình t? v?n. Highly recommended!',
    5, 5, 5, 4,
    1,
    N'C?m ?n b?n ?ã tin t??ng mua hàng! Chúc b?n có nh?ng chuy?n ??p vui v? ??',
    DATEADD(DAY,-1,@Now6),
    DATEADD(DAY,-2,@Now6), 0);
GO


-- =============================================
-- 12. SEED: MESSAGES (conversation between Buyer2 & Seller1 about Bike1)
-- =============================================
DECLARE @Now7 DATETIME2 = GETUTCDATE();

IF NOT EXISTS (SELECT 1 FROM [Messages] WHERE [SenderId] = '22222222-2222-2222-2222-222222222222' AND [ReceiverId] = '33333333-3333-3333-3333-333333333333')
BEGIN
    INSERT INTO [Messages] ([Id],[SenderId],[ReceiverId],[BikePostId],[Content],[Type],[IsRead],[ReadAt],[CreatedAt],[IsDeleted])
    VALUES
    (NEWID(),'22222222-2222-2222-2222-222222222222','33333333-3333-3333-3333-333333333333','AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA',
     N'Anh ?i, xe Giant TCR này còn không ?? Em mu?n h?i thêm v? tình tr?ng xe.',1,1,DATEADD(HOUR,-5,@Now7),DATEADD(HOUR,-6,@Now7),0),
    (NEWID(),'33333333-3333-3333-3333-333333333333','22222222-2222-2222-2222-222222222222','AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA',
     N'Xe v?n còn em nhé! Xe m?i b?o d??ng tháng tr??c, groupset Ultegra Di2 ch?y r?t m??t. Em mu?n xem xe tr?c ti?p không?',1,1,DATEADD(HOUR,-4,@Now7),DATEADD(HOUR,-5,@Now7),0),
    (NEWID(),'22222222-2222-2222-2222-222222222222','33333333-3333-3333-3333-333333333333','AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA',
     N'D? em ??t c?c luôn r?i ?. Anh ship cho em qua GiaoHangNhanh ???c không? C?m ?n anh!',1,1,DATEADD(HOUR,-3,@Now7),DATEADD(HOUR,-4,@Now7),0),
    (NEWID(),'33333333-3333-3333-3333-333333333333','22222222-2222-2222-2222-222222222222','AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA',
     N'OK em. Anh th?y ??n c?c r?i, s? ?óng gói k? và g?i trong hôm nay nhé! ????',1,0,NULL,DATEADD(HOUR,-2,@Now7),0);
END
GO


-- =============================================
-- 13. SEED: INSPECTION REPORT (for Bike3 — Specialized Diverge)
-- =============================================
DECLARE @Now8 DATETIME2 = GETUTCDATE();

IF NOT EXISTS (SELECT 1 FROM [InspectionReports] WHERE [BikePostId] = 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC')
INSERT INTO [InspectionReports] ([Id],[BikePostId],[InspectorId],[ReportNumber],[Status],[OverallCondition],[EstimatedValue],[IsRecommended],[Summary],[FrameScore],[BrakesScore],[GearsScore],[WheelsScore],[TiresScore],[ChainScore],[HasFrameDamage],[HasRust],[HasCracks],[AllComponentsOriginal],[InspectedAt],[CreatedAt],[IsDeleted])
VALUES (NEWID(),
    'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC', -- Bike3
    '55555555-5555-5555-5555-555555555555', -- Inspector
    'INS-20250110-A1B2C3',
    3, -- Completed
    1, -- Excellent
    52000000, 1,
    N'Xe trong tình tr?ng g?n nh? m?i. Frame carbon không có v?t n?t hay tr?y x??c ?áng k?. T?t c? components nguyên b?n. Future Shock 2.0 ho?t ??ng t?t. Khuy?n ngh? mua.',
    9, 9, 10, 9, 8, 9,
    0, 0, 0, 1,
    DATEADD(DAY,-5,@Now8), DATEADD(DAY,-6,@Now8), 0);
GO


-- =============================================
-- 14. SEED: WISHLISTS
-- =============================================
DECLARE @Now9 DATETIME2 = GETUTCDATE();

IF NOT EXISTS (SELECT 1 FROM [Wishlists] WHERE [UserId] = '11111111-1111-1111-1111-111111111111' AND [BikePostId] = 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC')
INSERT INTO [Wishlists] ([Id],[UserId],[BikePostId],[CreatedAt],[IsDeleted])
VALUES (NEWID(),'11111111-1111-1111-1111-111111111111','CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC',@Now9,0);

IF NOT EXISTS (SELECT 1 FROM [Wishlists] WHERE [UserId] = '11111111-1111-1111-1111-111111111111' AND [BikePostId] = 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF')
INSERT INTO [Wishlists] ([Id],[UserId],[BikePostId],[CreatedAt],[IsDeleted])
VALUES (NEWID(),'11111111-1111-1111-1111-111111111111','FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF',@Now9,0);

IF NOT EXISTS (SELECT 1 FROM [Wishlists] WHERE [UserId] = '22222222-2222-2222-2222-222222222222' AND [BikePostId] = 'BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB')
INSERT INTO [Wishlists] ([Id],[UserId],[BikePostId],[CreatedAt],[IsDeleted])
VALUES (NEWID(),'22222222-2222-2222-2222-222222222222','BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB',@Now9,0);
GO


-- =============================================
-- 15. SEED: WALLETS
-- =============================================
DECLARE @Now10 DATETIME2 = GETUTCDATE();

IF NOT EXISTS (SELECT 1 FROM [Wallets] WHERE [UserId] = '11111111-1111-1111-1111-111111111111')
INSERT INTO [Wallets] ([Id],[UserId],[Balance],[Currency],[IsLocked],[CreatedAt],[IsDeleted])
VALUES (NEWID(),'11111111-1111-1111-1111-111111111111', 5000000,'VND',0,@Now10,0);

IF NOT EXISTS (SELECT 1 FROM [Wallets] WHERE [UserId] = '22222222-2222-2222-2222-222222222222')
INSERT INTO [Wallets] ([Id],[UserId],[Balance],[Currency],[IsLocked],[CreatedAt],[IsDeleted])
VALUES (NEWID(),'22222222-2222-2222-2222-222222222222', 2000000,'VND',0,@Now10,0);

IF NOT EXISTS (SELECT 1 FROM [Wallets] WHERE [UserId] = '33333333-3333-3333-3333-333333333333')
INSERT INTO [Wallets] ([Id],[UserId],[Balance],[Currency],[IsLocked],[CreatedAt],[IsDeleted])
VALUES (NEWID(),'33333333-3333-3333-3333-333333333333', 45000000,'VND',0,@Now10,0);

IF NOT EXISTS (SELECT 1 FROM [Wallets] WHERE [UserId] = '44444444-4444-4444-4444-444444444444')
INSERT INTO [Wallets] ([Id],[UserId],[Balance],[Currency],[IsLocked],[CreatedAt],[IsDeleted])
VALUES (NEWID(),'44444444-4444-4444-4444-444444444444', 28000000,'VND',0,@Now10,0);
GO


-- =============================================
-- 16. ASSIGN IDENTITY ROLES TO USERS
-- =============================================
DECLARE @BuyerRoleId    NVARCHAR(450) = (SELECT [Id] FROM [AspNetRoles] WHERE [NormalizedName] = 'BUYER');
DECLARE @SellerRoleId   NVARCHAR(450) = (SELECT [Id] FROM [AspNetRoles] WHERE [NormalizedName] = 'SELLER');
DECLARE @InspectorRoleId NVARCHAR(450) = (SELECT [Id] FROM [AspNetRoles] WHERE [NormalizedName] = 'INSPECTOR');
DECLARE @AdminRoleId    NVARCHAR(450) = (SELECT [Id] FROM [AspNetRoles] WHERE [NormalizedName] = 'ADMIN');

IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = 'id-buyer-001')
    INSERT INTO [AspNetUserRoles] ([UserId],[RoleId]) VALUES ('id-buyer-001', @BuyerRoleId);
IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = 'id-buyer-002')
    INSERT INTO [AspNetUserRoles] ([UserId],[RoleId]) VALUES ('id-buyer-002', @BuyerRoleId);
IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = 'id-seller-001')
    INSERT INTO [AspNetUserRoles] ([UserId],[RoleId]) VALUES ('id-seller-001', @SellerRoleId);
IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = 'id-seller-002')
    INSERT INTO [AspNetUserRoles] ([UserId],[RoleId]) VALUES ('id-seller-002', @SellerRoleId);
IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = 'id-inspector-001')
    INSERT INTO [AspNetUserRoles] ([UserId],[RoleId]) VALUES ('id-inspector-001', @InspectorRoleId);
IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = 'id-admin-001')
    INSERT INTO [AspNetUserRoles] ([UserId],[RoleId]) VALUES ('id-admin-001', @AdminRoleId);
GO


-- =============================================
-- DONE! Verification queries:
-- =============================================
SELECT 'AspNetUsers'       AS [Table], COUNT(*) AS [Count] FROM [AspNetUsers]
UNION ALL SELECT 'AspNetRoles',     COUNT(*) FROM [AspNetRoles]
UNION ALL SELECT 'AspNetUserRoles', COUNT(*) FROM [AspNetUserRoles]
UNION ALL SELECT 'AppUsers',        COUNT(*) FROM [AppUsers]
UNION ALL SELECT 'BikePosts',       COUNT(*) FROM [BikePosts]
UNION ALL SELECT 'BikeImages',      COUNT(*) FROM [BikeImages]
UNION ALL SELECT 'Orders',          COUNT(*) FROM [Orders]
UNION ALL SELECT 'Payments',        COUNT(*) FROM [Payments]
UNION ALL SELECT 'Ratings',         COUNT(*) FROM [Ratings]
UNION ALL SELECT 'Messages',        COUNT(*) FROM [Messages]
UNION ALL SELECT 'InspectionReports', COUNT(*) FROM [InspectionReports]
UNION ALL SELECT 'Wishlists',       COUNT(*) FROM [Wishlists]
UNION ALL SELECT 'Wallets',         COUNT(*) FROM [Wallets]
ORDER BY [Table];
GO

PRINT '========================================';
PRINT 'SecondBikeDb created successfully!';
PRINT '========================================';
PRINT '';
PRINT 'Connection String (appsettings.json):';
PRINT 'Server=(localdb)\MSSQLLocalDB;Database=SecondBikeDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True';
PRINT '';
PRINT 'Test Accounts:';
PRINT '  Buyer 1:    buyer1@secondbike.com    (AppUser ID: 11111111-...)';
PRINT '  Buyer 2:    buyer2@secondbike.com    (AppUser ID: 22222222-...)';
PRINT '  Seller 1:   seller1@secondbike.com   (AppUser ID: 33333333-...)';
PRINT '  Seller 2:   seller2@secondbike.com   (AppUser ID: 44444444-...)';
PRINT '  Inspector:  inspector@secondbike.com (AppUser ID: 55555555-...)';
PRINT '  Admin:      admin@secondbike.com     (AppUser ID: 66666666-...)';
PRINT '';
PRINT 'NOTE: Password hashes are placeholders. Use the app Register endpoint to create real logins.';
GO
