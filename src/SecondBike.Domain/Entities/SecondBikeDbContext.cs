using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class SecondBikeDbContext : DbContext
{
    public SecondBikeDbContext()
    {
    }

    public SecondBikeDbContext(DbContextOptions<SecondBikeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLogSystem> AuditLogSystems { get; set; }

    public virtual DbSet<Bicycle> Bicycles { get; set; }

    public virtual DbSet<BicycleDetail> BicycleDetails { get; set; }

    public virtual DbSet<BicycleListing> BicycleListings { get; set; }

    public virtual DbSet<BikeType> BikeTypes { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ChatSession> ChatSessions { get; set; }

    public virtual DbSet<ConfigurationSystem> ConfigurationSystems { get; set; }

    public virtual DbSet<Deposit> Deposits { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<InspectionReport> InspectionReports { get; set; }

    public virtual DbSet<InspectionRequest> InspectionRequests { get; set; }

    public virtual DbSet<ListingMedium> ListingMedia { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Payout> Payouts { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<ReportAbuse> ReportAbuses { get; set; }

    public virtual DbSet<RequestAbuse> RequestAbuses { get; set; }

    public virtual DbSet<ServiceFee> ServiceFees { get; set; }

    public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }
    private string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
             .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();
        var strConn = config["ConnectionStrings:DefaultConnection"];

        return strConn;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer(GetConnectionString());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogSystem>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__AuditLog__5E5499A80BCD9FAB");

            entity.ToTable("AuditLogSystem");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("IPAddress");
            entity.Property(e => e.LogDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogSystems)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__AuditLogS__UserI__2180FB33");
        });

        modelBuilder.Entity<Bicycle>(entity =>
        {
            entity.HasKey(e => e.BikeId).HasName("PK__Bicycle__7DC817C1FB1CF9B1");

            entity.ToTable("Bicycle");

            entity.Property(e => e.BikeId).HasColumnName("BikeID");
            entity.Property(e => e.BrandId).HasColumnName("BrandID");
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Condition).HasMaxLength(50);
            entity.Property(e => e.ModelName).HasMaxLength(100);
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TypeId).HasColumnName("TypeID");

            entity.HasOne(d => d.Brand).WithMany(p => p.Bicycles)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK__Bicycle__BrandID__48CFD27E");

            entity.HasOne(d => d.Type).WithMany(p => p.Bicycles)
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("FK__Bicycle__TypeID__49C3F6B7");
        });

        modelBuilder.Entity<BicycleDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PK__BicycleD__135C314DE38A14E1");

            entity.ToTable("BicycleDetail");

            entity.HasIndex(e => e.BikeId, "UQ__BicycleD__7DC817C0E12F0E2C").IsUnique();

            entity.Property(e => e.DetailId).HasColumnName("DetailID");
            entity.Property(e => e.BikeId).HasColumnName("BikeID");
            entity.Property(e => e.BrakeType).HasMaxLength(50);
            entity.Property(e => e.FrameMaterial).HasMaxLength(50);
            entity.Property(e => e.FrameSize).HasMaxLength(20);
            entity.Property(e => e.Transmission).HasMaxLength(100);
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.WheelSize).HasMaxLength(20);

            entity.HasOne(d => d.Bike).WithOne(p => p.BicycleDetail)
                .HasForeignKey<BicycleDetail>(d => d.BikeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BicycleDe__BikeI__4D94879B");
        });

        modelBuilder.Entity<BicycleListing>(entity =>
        {
            entity.HasKey(e => e.ListingId).HasName("PK__BicycleL__BF3EBEF01310B51A");

            entity.ToTable("BicycleListing");

            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.BikeId).HasColumnName("BikeID");
            entity.Property(e => e.ListingStatus).HasDefaultValue((byte)1);
            entity.Property(e => e.PostedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.SellerId).HasColumnName("SellerID");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Bike).WithMany(p => p.BicycleListings)
                .HasForeignKey(d => d.BikeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BicycleLi__BikeI__534D60F1");

            entity.HasOne(d => d.Seller).WithMany(p => p.BicycleListings)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BicycleLi__Selle__52593CB8");
        });

        modelBuilder.Entity<BikeType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__BikeType__516F03954353BB8C");

            entity.ToTable("BikeType");

            entity.Property(e => e.TypeId).HasColumnName("TypeID");
            entity.Property(e => e.TypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__Brand__DAD4F3BEA2CF6CBE");

            entity.ToTable("Brand");

            entity.Property(e => e.BrandId).HasColumnName("BrandID");
            entity.Property(e => e.BrandName).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(50);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__ChatMess__C87C037CBF673560");

            entity.ToTable("ChatMessage");

            entity.Property(e => e.MessageId).HasColumnName("MessageID");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.SenderId).HasColumnName("SenderID");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SessionId).HasColumnName("SessionID");

            entity.HasOne(d => d.Sender).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChatMessa__Sende__07C12930");

            entity.HasOne(d => d.Session).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChatMessa__Sessi__06CD04F7");
        });

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__ChatSess__C9F492708F0EDFAE");

            entity.ToTable("ChatSession");

            entity.Property(e => e.SessionId).HasColumnName("SessionID");
            entity.Property(e => e.BuyerId).HasColumnName("BuyerID");
            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.SellerId).HasColumnName("SellerID");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Buyer).WithMany(p => p.ChatSessionBuyers)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChatSessi__Buyer__00200768");

            entity.HasOne(d => d.Listing).WithMany(p => p.ChatSessions)
                .HasForeignKey(d => d.ListingId)
                .HasConstraintName("FK__ChatSessi__Listi__02084FDA");

            entity.HasOne(d => d.Seller).WithMany(p => p.ChatSessionSellers)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChatSessi__Selle__01142BA1");
        });

        modelBuilder.Entity<ConfigurationSystem>(entity =>
        {
            entity.HasKey(e => e.ConfigId).HasName("PK__Configur__C3BC333C3E73D227");

            entity.ToTable("ConfigurationSystem");

            entity.HasIndex(e => e.ConfigKey, "UQ__Configur__4A3067846DE706AA").IsUnique();

            entity.Property(e => e.ConfigId).HasColumnName("ConfigID");
            entity.Property(e => e.ConfigKey).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
        });

        modelBuilder.Entity<Deposit>(entity =>
        {
            entity.HasKey(e => e.DepositId).HasName("PK__Deposit__AB60DF5110A9231C");

            entity.ToTable("Deposit");

            entity.Property(e => e.DepositId).HasColumnName("DepositID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DepositDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Status).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Order).WithMany(p => p.Deposits)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Deposit__OrderID__619B8048");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDF6F363B3E5");

            entity.ToTable("Feedback");

            entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.TargetUserId).HasColumnName("TargetUserID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Order).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__OrderI__72C60C4A");

            entity.HasOne(d => d.TargetUser).WithMany(p => p.FeedbackTargetUsers)
                .HasForeignKey(d => d.TargetUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__Target__71D1E811");

            entity.HasOne(d => d.User).WithMany(p => p.FeedbackUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__UserID__70DDC3D8");
        });

        modelBuilder.Entity<InspectionReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Inspecti__D5BD48E5016CD4EA");

            entity.ToTable("InspectionReport");

            entity.HasIndex(e => e.RequestId, "UQ__Inspecti__33A8519BEF745F61").IsUnique();

            entity.Property(e => e.ReportId).HasColumnName("ReportID");
            entity.Property(e => e.CompletedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RequestId).HasColumnName("RequestID");

            entity.HasOne(d => d.Request).WithOne(p => p.InspectionReport)
                .HasForeignKey<InspectionReport>(d => d.RequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inspectio__Reque__123EB7A3");
        });

        modelBuilder.Entity<InspectionRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__Inspecti__33A8519A32B273F5");

            entity.ToTable("InspectionRequest");

            entity.Property(e => e.RequestId).HasColumnName("RequestID");
            entity.Property(e => e.InspectorId).HasColumnName("InspectorID");
            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.RequestDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RequestStatus).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Inspector).WithMany(p => p.InspectionRequests)
                .HasForeignKey(d => d.InspectorId)
                .HasConstraintName("FK__Inspectio__Inspe__0D7A0286");

            entity.HasOne(d => d.Listing).WithMany(p => p.InspectionRequests)
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inspectio__Listi__0C85DE4D");
        });

        modelBuilder.Entity<ListingMedium>(entity =>
        {
            entity.HasKey(e => e.MediaId).HasName("PK__ListingM__B2C2B5AF94070963");

            entity.Property(e => e.MediaId).HasColumnName("MediaID");
            entity.Property(e => e.IsThumbnail).HasDefaultValue(false);
            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.MediaType).HasMaxLength(20);

            entity.HasOne(d => d.Listing).WithMany(p => p.ListingMedia)
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ListingMe__Listi__571DF1D5");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__C3905BAF255936DF");

            entity.ToTable("Order");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.BuyerId).HasColumnName("BuyerID");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderStatus).HasDefaultValue((byte)1);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Buyer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__BuyerID__5BE2A6F2");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDetail");

            entity.ToTable("OrderDetail");

            entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDetail__OrderID");

            entity.HasOne(d => d.Listing).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDetail__ListingID");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A58F2EAFF51");

            entity.ToTable("Payment");

            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.TransactionRef)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__OrderID__656C112C");
        });

        modelBuilder.Entity<Payout>(entity =>
        {
            entity.HasKey(e => e.PayoutId).HasName("PK__Payout__35C3DFAE95B087D5");

            entity.ToTable("Payout");

            entity.Property(e => e.PayoutId).HasColumnName("PayoutID");
            entity.Property(e => e.AmountToSeller).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PayoutDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Order).WithMany(p => p.Payouts)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payout__OrderID__693CA210");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("PK__RefreshT__F5845E59E05D807B");

            entity.ToTable("RefreshToken");

            entity.Property(e => e.RefreshTokenId).HasColumnName("RefreshTokenID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.RevokedAt).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefreshTo__UserI__282DF8C2");
        });

        modelBuilder.Entity<ReportAbuse>(entity =>
        {
            entity.HasKey(e => e.ReportAbuseId).HasName("PK__ReportAb__A7AEDB9582C6F303");

            entity.ToTable("ReportAbuse");

            entity.HasIndex(e => e.RequestAbuseId, "UQ__ReportAb__F18246D976494F13").IsUnique();

            entity.Property(e => e.ReportAbuseId).HasColumnName("ReportAbuseID");
            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.RequestAbuseId).HasColumnName("RequestAbuseID");
            entity.Property(e => e.ResolvedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Admin).WithMany(p => p.ReportAbuses)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReportAbu__Admin__1DB06A4F");

            entity.HasOne(d => d.RequestAbuse).WithOne(p => p.ReportAbuse)
                .HasForeignKey<ReportAbuse>(d => d.RequestAbuseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReportAbu__Reque__1CBC4616");
        });

        modelBuilder.Entity<RequestAbuse>(entity =>
        {
            entity.HasKey(e => e.RequestAbuseId).HasName("PK__RequestA__F18246D8774A2997");

            entity.ToTable("RequestAbuse");

            entity.Property(e => e.RequestAbuseId).HasColumnName("RequestAbuseID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReporterId).HasColumnName("ReporterID");
            entity.Property(e => e.TargetListingId).HasColumnName("TargetListingID");
            entity.Property(e => e.TargetUserId).HasColumnName("TargetUserID");

            entity.HasOne(d => d.Reporter).WithMany(p => p.RequestAbuseReporters)
                .HasForeignKey(d => d.ReporterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RequestAb__Repor__160F4887");

            entity.HasOne(d => d.TargetListing).WithMany(p => p.RequestAbuses)
                .HasForeignKey(d => d.TargetListingId)
                .HasConstraintName("FK__RequestAb__Targe__17036CC0");

            entity.HasOne(d => d.TargetUser).WithMany(p => p.RequestAbuseTargetUsers)
                .HasForeignKey(d => d.TargetUserId)
                .HasConstraintName("FK__RequestAb__Targe__17F790F9");
        });

        modelBuilder.Entity<ServiceFee>(entity =>
        {
            entity.HasKey(e => e.FeeId).HasName("PK__ServiceF__B387B2094070D932");

            entity.ToTable("ServiceFee");

            entity.Property(e => e.FeeId).HasColumnName("FeeID");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.FeeAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");

            entity.HasOne(d => d.Order).WithMany(p => p.ServiceFees)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServiceFe__Order__6C190EBB");
        });

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Shopping__51BCD79708AB7B45");

            entity.ToTable("ShoppingCart");

            entity.Property(e => e.CartId).HasColumnName("CartID");
            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Listing).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShoppingC__Listi__7C4F7684");

            entity.HasOne(d => d.User).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShoppingC__UserI__7B5B524B");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC3E2D21F6");

            entity.ToTable("User");

            entity.HasIndex(e => e.Username, "UQ__User__536C85E46F5FFE00").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__User__A9D105349D33E8B4").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Status).HasDefaultValue((byte)1);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User__RoleID__3E52440B");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.ProfileId).HasName("PK__UserProf__290C8884657A7063");

            entity.ToTable("UserProfile");

            entity.HasIndex(e => e.UserId, "UQ__UserProf__1788CCAD6F5625AE").IsUnique();

            entity.Property(e => e.ProfileId).HasColumnName("ProfileID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithOne(p => p.UserProfile)
                .HasForeignKey<UserProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserProfi__UserI__4222D4EF");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__UserRole__8AFACE3A6519910B");

            entity.ToTable("UserRole");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(e => e.WishlistId).HasName("PK__Wishlist__233189CB4E91D466");

            entity.ToTable("Wishlist");

            entity.Property(e => e.WishlistId).HasColumnName("WishlistID");
            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Listing).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Wishlist__Listin__778AC167");

            entity.HasOne(d => d.User).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Wishlist__UserID__76969D2E");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
