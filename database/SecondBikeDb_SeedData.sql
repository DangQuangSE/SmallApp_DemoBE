-- =============================================
-- SecondBikeDb — Seed Data (5 rows per table, NO NULL values)
-- Run this script AFTER SecondBikeDb_CreateAndSeed.sql
-- All data in English
-- =============================================

USE SecondBikeDb;
GO

-- =============================================
-- 1. UserRole (4 rows already seeded from original script, add 1 more)
-- =============================================
INSERT INTO UserRole (RoleName) VALUES
('Moderator');

-- Result: RoleID 1=Admin, 2=Buyer, 3=Seller, 4=Inspector, 5=Moderator

-- =============================================
-- 2. [User] — 5 users (1 Admin, 2 Seller, 2 Buyer)
-- PasswordHash = BCrypt hash of "Password123!"
-- =============================================
INSERT INTO [User] (RoleID, Username, Email, PasswordHash, IsVerified, Status, CreatedAt) VALUES
(1, 'admin_bike',     'admin@secondbike.com',     '12345', 1, 1, '2025-01-01 08:00:00'),
(3, 'seller_marcus',  'marcus.seller@gmail.com',  '12345', 1, 1, '2025-01-05 09:30:00'),
(3, 'seller_emily',   'emily.seller@gmail.com',   '12345', 1, 1, '2025-01-10 10:00:00'),
(2, 'buyer_alex',     'alex.buyer@gmail.com',     '12345', 1, 1, '2025-02-01 11:00:00'),
(2, 'buyer_sarah',    'sarah.buyer@gmail.com',    '12345', 1, 1, '2025-02-15 14:00:00');

-- Result: UserID 1=admin, 2=seller_marcus, 3=seller_emily, 4=buyer_alex, 5=buyer_sarah

-- =============================================
-- 3. UserProfile — 5 profiles (1 per user)
-- =============================================
INSERT INTO UserProfile (UserID, FullName, PhoneNumber, Address, AvatarUrl) VALUES
(1, N'John Admin',       '0901000001', N'District 1, Ho Chi Minh City',     'https://res.cloudinary.com/demo/image/upload/avatars/admin.jpg'),
(2, N'Marcus Johnson',   '0902000002', N'District 3, Ho Chi Minh City',     'https://res.cloudinary.com/demo/image/upload/avatars/marcus.jpg'),
(3, N'Emily Nguyen',     '0903000003', N'District 7, Ho Chi Minh City',     'https://res.cloudinary.com/demo/image/upload/avatars/emily.jpg'),
(4, N'Alex Thompson',    '0904000004', N'Binh Thanh District, HCMC',        'https://res.cloudinary.com/demo/image/upload/avatars/alex.jpg'),
(5, N'Sarah Williams',   '0905000005', N'Thu Duc City, HCMC',               'https://res.cloudinary.com/demo/image/upload/avatars/sarah.jpg');

-- =============================================
-- 4. Brand — 5 brands
-- =============================================
INSERT INTO Brand (BrandName, Country) VALUES
(N'Giant',        N'Taiwan'),
(N'Trek',         N'USA'),
(N'Specialized',  N'USA'),
(N'Merida',       N'Taiwan'),
(N'Java',         N'China');

-- Result: BrandID 1=Giant, 2=Trek, 3=Specialized, 4=Merida, 5=Java

-- =============================================
-- 5. BikeType — 5 bike types
-- =============================================
INSERT INTO BikeType (TypeName) VALUES
(N'Mountain'),
(N'Road'),
(N'City'),
(N'Touring'),
(N'Folding');

-- Result: TypeID 1=Mountain, 2=Road, 3=City, 4=Touring, 5=Folding

-- =============================================
-- 6. Bicycle — 5 bikes
-- =============================================
INSERT INTO Bicycle (BrandID, TypeID, ModelName, SerialNumber, Color, Condition) VALUES
(1, 3, N'Escape 3',       'GNT2024001', N'Black',    N'Used'),
(2, 2, N'Domane AL 2',    'TRK2024002', N'Red',      N'Like New'),
(3, 1, N'Rockhopper',     'SPZ2024003', N'Blue',     N'Used'),
(4, 4, N'Silex 400',      'MRD2024004', N'Silver',   N'New'),
(5, 5, N'Fit 3',          'JAV2024005', N'White',    N'Like New');

-- Result: BikeID 1-5

-- =============================================
-- 7. BicycleDetail — 5 technical details (1 per bike)
-- =============================================
INSERT INTO BicycleDetail (BikeID, FrameSize, FrameMaterial, WheelSize, BrakeType, Weight, Transmission) VALUES
(1, 'M',  N'Aluminum',       '700c',  N'V-Brake',        11.50, N'Shimano Altus 3x8'),
(2, 'S',  N'Aluminum',       '700c',  N'Disc Brake',     9.80,  N'Shimano Claris 2x8'),
(3, 'L',  N'Aluminum',       '29',    N'Disc Brake',     13.20, N'Shimano Deore 1x12'),
(4, 'M',  N'Carbon',         '700c',  N'Hydraulic Disc', 9.50,  N'Shimano GRX 2x11'),
(5, 'S',  N'Chromoly Steel', '20',    N'V-Brake',        12.00, N'Shimano Tourney 1x7');

-- =============================================
-- 8. BicycleListing — 5 listings (seller_marcus: 3, seller_emily: 2)
-- =============================================
INSERT INTO BicycleListing (SellerID, BikeID, Title, Description, Price, Quantity, ListingStatus, Address, PostedDate) VALUES
(2, 1, N'Giant Escape 3 — 95% condition, barely used',
       N'Purchased in 2023, ridden only about 500km. Lightweight aluminum frame, 700c wheels, perfect for city commuting and fitness riding. Regularly maintained, no scratches.',
       5500000, 2, 1, N'District 3, Ho Chi Minh City', '2025-06-01 08:00:00'),

(2, 2, N'Trek Domane AL 2 — USA imported road bike',
       N'Authentic Trek road bike with aluminum Domane frame, Shimano Claris 2x8 groupset. Like new condition, only 200km ridden. Great for beginners getting into road cycling.',
       12000000, 1, 1, N'District 1, Ho Chi Minh City', '2025-06-05 10:30:00'),

(2, 3, N'Specialized Rockhopper 29 — Trail-ready MTB',
       N'Specialized Rockhopper mountain bike, 29-inch wheels, air fork with lockout, Shimano Deore 1x12. Taken on trails in Da Lat and Bao Loc. Bike runs perfectly.',
       8500000, 1, 1, N'Binh Thanh District, HCMC', '2025-06-10 14:00:00'),

(3, 4, N'Merida Silex 400 — Brand new gravel bike',
       N'Merida Silex 400 gravel bike, carbon frame, Shimano GRX 2x11, hydraulic disc brakes. Brand new sealed, tags not cut. Full box with all accessories.',
       25000000, 1, 1, N'District 7, Ho Chi Minh City', '2025-06-12 09:00:00'),

(3, 5, N'Java Fit 3 — Compact folding commuter',
       N'Java Fit 3 folding bike, Chromoly steel frame, 20-inch wheels, 7-speed. Folds in 15 seconds, easy to carry on bus or in car. Like new condition.',
       4500000, 3, 1, N'Thu Duc City, HCMC', '2025-06-15 16:00:00');

-- Result: ListingID 1-5

-- =============================================
-- 9. ListingMedia — 5 images (1 thumbnail per listing)
-- =============================================
INSERT INTO ListingMedia (ListingID, MediaUrl, MediaType, IsThumbnail) VALUES
(1, 'https://res.cloudinary.com/demo/image/upload/listings/giant-escape-3-main.jpg',         'image', 1),
(2, 'https://res.cloudinary.com/demo/image/upload/listings/trek-domane-al2-main.jpg',        'image', 1),
(3, 'https://res.cloudinary.com/demo/image/upload/listings/specialized-rockhopper-main.jpg', 'image', 1),
(4, 'https://res.cloudinary.com/demo/image/upload/listings/merida-silex-400-main.jpg',       'image', 1),
(5, 'https://res.cloudinary.com/demo/image/upload/listings/java-fit-3-main.jpg',             'image', 1);

-- =============================================
-- 10. [Order] — 5 orders (buyer_alex: 3, buyer_sarah: 2)
-- Status: 1=Pending, 2=Paid(Deposit), 3=Shipping, 4=Completed, 5=Cancelled
-- =============================================
INSERT INTO [Order] (BuyerID, TotalAmount, OrderStatus, OrderDate) VALUES
(4, 5500000,  4, '2025-06-20 10:00:00'),   -- OrderID 1: Completed  (buyer_alex buys Giant)
(4, 12000000, 3, '2025-06-22 11:30:00'),   -- OrderID 2: Shipping   (buyer_alex buys Trek)
(4, 8500000,  2, '2025-06-24 09:00:00'),   -- OrderID 3: Deposit Paid (buyer_alex buys Rockhopper)
(5, 25000000, 1, '2025-06-25 14:00:00'),   -- OrderID 4: Pending    (buyer_sarah buys Merida)
(5, 9000000,  5, '2025-06-18 08:00:00');   -- OrderID 5: Cancelled  (buyer_sarah buys 2 Java Fit)

-- =============================================
-- 10b. OrderDetail — 5 rows (1 row per order, order 5 has quantity=2)
-- =============================================
INSERT INTO OrderDetail (OrderID, ListingID, Quantity, UnitPrice) VALUES
(1, 1, 1, 5500000),    -- Order 1: 1x Giant Escape 3
(2, 2, 1, 12000000),   -- Order 2: 1x Trek Domane AL 2
(3, 3, 1, 8500000),    -- Order 3: 1x Specialized Rockhopper
(4, 4, 1, 25000000),   -- Order 4: 1x Merida Silex 400
(5, 5, 2, 4500000);    -- Order 5: 2x Java Fit 3 = 9,000,000

-- =============================================
-- 11. Deposit — 5 deposits (1 per order)
-- Status: 1=Pending, 2=Confirmed, 3=Cancelled
-- =============================================
INSERT INTO Deposit (OrderID, Amount, Status, DepositDate) VALUES
(1, 1100000,  2, '2025-06-20 10:05:00'),   -- Confirmed (order completed)
(2, 2400000,  2, '2025-06-22 11:35:00'),   -- Confirmed (order shipping)
(3, 1700000,  2, '2025-06-24 09:05:00'),   -- Confirmed (deposit paid)
(4, 5000000,  1, '2025-06-25 14:00:00'),   -- Pending   (order pending)
(5, 1800000,  3, '2025-06-18 08:00:00');   -- Cancelled (order cancelled)

-- =============================================
-- 12. Payment — 5 VNPay transactions
-- Order 1: fully paid (deposit + remaining) = 2 payments
-- Order 2: fully paid (deposit + remaining) = 2 payments
-- Order 3: deposit only = 1 payment
-- =============================================
INSERT INTO Payment (OrderID, Amount, PaymentMethod, TransactionRef, PaymentDate) VALUES
(1, 1100000,  N'VNPay', '1-20250620100530',  '2025-06-20 10:05:30'),   -- Order 1: deposit
(1, 4400000,  N'VNPay', '1-20250620110000',  '2025-06-20 11:00:00'),   -- Order 1: full remaining
(2, 2400000,  N'VNPay', '2-20250622113500',  '2025-06-22 11:35:00'),   -- Order 2: deposit
(2, 9600000,  N'VNPay', '2-20250622140000',  '2025-06-22 14:00:00'),   -- Order 2: full remaining
(3, 1700000,  N'VNPay', '3-20250624090500',  '2025-06-24 09:05:00');   -- Order 3: deposit only

-- =============================================
-- 13. Payout — 5 seller payouts
-- =============================================
INSERT INTO Payout (OrderID, AmountToSeller, Status, PayoutDate) VALUES
(1, 4950000,  2, '2025-06-21 10:00:00'),   -- Transferred (order 1 completed)
(2, 10800000, 1, '2025-06-23 10:00:00'),   -- Pending     (order 2 shipping)
(3, 7650000,  1, '2025-06-25 10:00:00'),   -- Pending     (order 3 deposit paid)
(4, 22500000, 1, '2025-06-26 10:00:00'),   -- Pending     (order 4 pending)
(5, 8100000,  1, '2025-06-19 10:00:00');   -- Pending     (order 5 cancelled — will not transfer)

-- =============================================
-- 14. ServiceFee — 5 service fees (10% per order)
-- =============================================
INSERT INTO ServiceFee (OrderID, FeeAmount, Description) VALUES
(1, 550000,  N'10% service fee for order #1'),
(2, 1200000, N'10% service fee for order #2'),
(3, 850000,  N'10% service fee for order #3'),
(4, 2500000, N'10% service fee for order #4'),
(5, 900000,  N'10% service fee for order #5');

-- =============================================
-- 15. Feedback — 5 ratings
-- =============================================
INSERT INTO Feedback (UserID, TargetUserID, OrderID, Rating, Comment, CreatedAt) VALUES
(4, 2, 1, 5, N'Bike is in great shape, exactly as described. Seller shipped quickly and packed carefully. Very satisfied!',                '2025-06-21 15:00:00'),
(4, 2, 2, 4, N'Good quality bike, but delivery was one day late. Overall still a good experience.',                                       '2025-06-23 16:00:00'),
(4, 2, 3, 5, N'Rockhopper is amazing on trails, fork works buttery smooth. Seller was very helpful with advice.',                         '2025-06-25 10:00:00'),
(5, 3, 4, 4, N'Brand new bike as advertised. Price is a bit high but worth every penny for the quality.',                                  '2025-06-26 11:00:00'),
(2, 4, 1, 5, N'Buyer paid promptly and picked up on time. Smooth transaction all around.',                                                '2025-06-21 16:00:00');

-- =============================================
-- 16. Wishlist — 5 wishlist items
-- =============================================
INSERT INTO Wishlist (UserID, ListingID, AddedDate) VALUES
(4, 4, '2025-06-20 08:00:00'),   -- buyer_alex likes Merida Silex
(4, 5, '2025-06-20 08:05:00'),   -- buyer_alex likes Java Fit
(5, 1, '2025-06-19 09:00:00'),   -- buyer_sarah likes Giant Escape
(5, 2, '2025-06-19 09:10:00'),   -- buyer_sarah likes Trek Domane
(5, 3, '2025-06-19 09:15:00');   -- buyer_sarah likes Rockhopper

-- =============================================
-- 17. ShoppingCart — 5 cart items
-- =============================================
INSERT INTO ShoppingCart (UserID, ListingID, AddedDate) VALUES
(4, 4, '2025-06-24 20:00:00'),   -- buyer_alex adds Merida to cart
(4, 5, '2025-06-24 20:05:00'),   -- buyer_alex adds Java Fit to cart
(5, 1, '2025-06-25 10:00:00'),   -- buyer_sarah adds Giant Escape
(5, 3, '2025-06-25 10:05:00'),   -- buyer_sarah adds Rockhopper
(5, 5, '2025-06-25 10:10:00');   -- buyer_sarah adds Java Fit

-- =============================================
-- 18. ChatSession — 5 chat sessions (buyer <-> seller)
-- =============================================
INSERT INTO ChatSession (BuyerID, SellerID, ListingID, StartedAt) VALUES
(4, 2, 1, '2025-06-19 10:00:00'),   -- buyer_alex asks seller_marcus about Giant Escape
(4, 2, 2, '2025-06-21 11:00:00'),   -- buyer_alex asks seller_marcus about Trek Domane
(4, 2, 3, '2025-06-23 09:00:00'),   -- buyer_alex asks seller_marcus about Rockhopper
(5, 3, 4, '2025-06-24 14:00:00'),   -- buyer_sarah asks seller_emily about Merida Silex
(5, 3, 5, '2025-06-17 08:00:00');   -- buyer_sarah asks seller_emily about Java Fit

-- =============================================
-- 19. ChatMessage — 5 messages
-- =============================================
INSERT INTO ChatMessage (SessionID, SenderID, Content, SentAt, IsRead) VALUES
(1, 4, N'Hi, is the Giant Escape 3 still available? Can I come see it in person?',                    '2025-06-19 10:01:00', 1),
(1, 2, N'Yes, still available! Feel free to come by District 3 anytime to check it out.',             '2025-06-19 10:05:00', 1),
(2, 4, N'How long have you been riding the Trek Domane? Any scratches on the frame?',                 '2025-06-21 11:01:00', 1),
(4, 5, N'Hi, is the price negotiable for the Merida Silex? Could you offer a small discount?',        '2025-06-24 14:01:00', 1),
(5, 5, N'If I buy 2 Java Fit bikes, can you give me a bundle discount?',                              '2025-06-17 08:01:00', 1);

-- =============================================
-- 20. InspectionRequest — 5 inspection requests
-- RequestStatus: 1=Pending, 2=In Progress, 3=Completed
-- =============================================
INSERT INTO InspectionRequest (ListingID, InspectorID, RequestStatus, RequestDate) VALUES
(1, 1, 3, '2025-06-02 09:00:00'),   -- Giant Escape: Completed
(2, 1, 3, '2025-06-06 10:00:00'),   -- Trek Domane: Completed
(3, 1, 2, '2025-06-11 11:00:00'),   -- Rockhopper: In Progress
(4, 1, 1, '2025-06-13 12:00:00'),   -- Merida Silex: Pending
(5, 1, 3, '2025-06-16 08:00:00');   -- Java Fit: Completed

-- =============================================
-- 21. InspectionReport — 5 reports
-- FinalVerdict: 1=Pass, 0=Fail
-- =============================================
INSERT INTO InspectionReport (RequestID, FrameCheck, BrakeCheck, TransmissionCheck, InspectorNote, FinalVerdict, ReportUrl, CompletedAt) VALUES
(1, N'Original aluminum frame, no cracks or bending. Weld joints are clean and uniform.',
    N'V-Brake functioning well, brake pads at 70% life remaining. Cables are rust-free.',
    N'Shimano Altus 3x8 shifts smoothly, no chain skipping. Cassette is still fresh.',
    N'Bike is in good condition, matches Used 95% description. Recommended for purchase.',
    1, 'https://res.cloudinary.com/demo/raw/upload/reports/inspection-1.pdf', '2025-06-03 14:00:00'),

(2, N'Aluminum Domane series frame, fully original. Paint is glossy with no scratches.',
    N'Disc brakes working perfectly, rotors still thick. Braking performance at 95%.',
    N'Shimano Claris 2x8 operating stable. Cables recently replaced.',
    N'Bike is truly like new as described. Excellent quality for this price range.',
    1, 'https://res.cloudinary.com/demo/raw/upload/reports/inspection-2.pdf', '2025-06-07 15:00:00'),

(3, N'Aluminum trail frame with a few minor scratches near the head tube. No structural impact.',
    N'Hydraulic disc brakes working well. Brake fluid service needed within 3 months.',
    N'Shimano Deore 1x12 shifts precisely. Cassette at approximately 60% life.',
    N'Bike has been used on trails, in fair condition. Cassette replacement recommended soon.',
    1, 'https://res.cloudinary.com/demo/raw/upload/reports/inspection-3.pdf', '2025-06-12 16:00:00'),

(4, N'Carbon frame still factory sealed, unassembled. No cracks detected.',
    N'Shimano GRX hydraulic disc brakes, unused. 100% new condition.',
    N'Shimano GRX 2x11, unused. 100% new condition.',
    N'Brand new in box, never used. All genuine accessories included.',
    1, 'https://res.cloudinary.com/demo/raw/upload/reports/inspection-4.pdf', '2025-06-14 10:00:00'),

(5, N'Chromoly steel frame, folding hinge operates smoothly. Safety lock is secure.',
    N'V-Brake functioning well. Brake pads at 80% life remaining.',
    N'Shimano Tourney 1x7, shifts lightly. Suitable for city riding.',
    N'Folding bike in good condition, folds and unfolds quickly. Ideal for daily commuting.',
    1, 'https://res.cloudinary.com/demo/raw/upload/reports/inspection-5.pdf', '2025-06-17 09:00:00');

-- =============================================
-- 22. RequestAbuse — 5 abuse reports
-- =============================================
INSERT INTO RequestAbuse (ReporterID, TargetListingID, TargetUserID, Reason, CreatedAt) VALUES
(4, 1, 2, N'Listing photos do not match reality, bike has more scratches than described.',                        '2025-06-22 10:00:00'),
(5, 2, 2, N'Seller did not respond to messages after deposit was made, suspected fraud.',                         '2025-06-23 11:00:00'),
(4, 3, 2, N'Price is significantly higher than market value, suspected price gouging.',                            '2025-06-24 12:00:00'),
(2, 4, 5, N'Buyer repeatedly cancels orders, causing inconvenience to sellers.',                                   '2025-06-25 13:00:00'),
(3, 5, 4, N'Buyer requested delivery then refused to accept the package, causing shipping costs.',                 '2025-06-26 14:00:00');

-- =============================================
-- 23. ReportAbuse — 5 admin resolutions
-- Status: 1=Resolved, 2=Dismissed
-- =============================================
INSERT INTO ReportAbuse (RequestAbuseID, AdminID, Resolution, Status, ResolvedAt) VALUES
(1, 1, N'Investigated and verified photos match reality. Buyer assessment was inaccurate. Dismissed.',             2, '2025-06-22 15:00:00'),
(2, 1, N'Seller responded within 24 hours. Warning issued to seller regarding response time.',                     1, '2025-06-24 10:00:00'),
(3, 1, N'Pricing is set by the seller and does not violate platform policies. Dismissed.',                         2, '2025-06-24 16:00:00'),
(4, 1, N'Warning issued to buyer. Account will be suspended if behavior continues.',                               1, '2025-06-26 09:00:00'),
(5, 1, N'Verified that buyer had a legitimate reason for refusing delivery. Dismissed.',                            2, '2025-06-27 10:00:00');

-- =============================================
-- 24. AuditLogSystem — 5 system logs
-- =============================================
INSERT INTO AuditLogSystem (UserID, Action, IPAddress, Details, LogDate) VALUES
(1, N'Admin Login',          '192.168.1.1',   N'Admin logged in successfully from Chrome on Windows.',              '2025-06-20 08:00:00'),
(2, N'Create Listing',       '192.168.1.10',  N'Seller created listing #1: Giant Escape 3.',                        '2025-06-01 08:00:00'),
(4, N'Place Order',          '192.168.1.20',  N'Buyer placed order #1: 1x Giant Escape 3.',                         '2025-06-20 10:00:00'),
(4, N'VNPay Payment',        '192.168.1.20',  N'Buyer paid deposit for order #1 via VNPay: 1,100,000 VND.',         '2025-06-20 10:05:30'),
(1, N'Resolve Abuse Report', '192.168.1.1',   N'Admin resolved abuse report #1: Dismissed.',                        '2025-06-22 15:00:00');

-- =============================================
-- 25. ConfigurationSystem — 5 system configurations
-- =============================================
INSERT INTO ConfigurationSystem (ConfigKey, ConfigValue, Description) VALUES
(N'DepositRate',         N'0.20',  N'Deposit rate: 20% of total order amount'),
(N'ServiceFeeRate',      N'0.10',  N'Service fee: 10% of total order amount'),
(N'MaxImagesPerListing', N'10',    N'Maximum number of images per listing'),
(N'MaxImageSizeMB',      N'5',     N'Maximum image file size in MB'),
(N'VnPaySandboxMode',    N'true',  N'VNPay sandbox mode enabled (true/false)');

-- =============================================
-- 26. RefreshToken — 5 tokens (all revoked for safety)
-- =============================================
INSERT INTO RefreshToken (UserID, Token, ExpiresAt, CreatedAt, RevokedAt) VALUES
(1, N'rt_admin_token_abc123def456ghi789jkl012mno345pqr678stu901',    '2025-07-20 08:00:00', '2025-06-20 08:00:00', '2025-06-20 12:00:00'),
(2, N'rt_seller_marcus_token_xyz789abc012def345ghi678jkl901mno234',  '2025-07-01 09:30:00', '2025-06-01 09:30:00', '2025-06-15 10:00:00'),
(3, N'rt_seller_emily_token_pqr456stu789vwx012yza345bcd678efg901',   '2025-07-10 10:00:00', '2025-06-10 10:00:00', '2025-06-20 11:00:00'),
(4, N'rt_buyer_alex_token_hij234klm567nop890qrs123tuv456wxy789',     '2025-07-20 11:00:00', '2025-06-20 11:00:00', '2025-06-25 09:00:00'),
(5, N'rt_buyer_sarah_token_zab567cde890fgh123ijk456lmn789opq012',    '2025-07-15 14:00:00', '2025-06-15 14:00:00', '2025-06-25 15:00:00');

