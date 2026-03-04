# SecondBike - Use Case Full Guide (All 7 Members)

> This document details every Use Case (UC) step-by-step, organized by team member responsibility.  
> **Implementation Status: ALL UCs are fully implemented in both BE and FE.**

---

## Member 1: Seller Core (Profile & Post Management)

### UC 1.1: Manage Profile

**Actors:** Buyer, Seller, Inspector, Admin (any authenticated user)

**Precondition:** User is logged in.

**Main Flow:**
1. User navigates to Profile page (`/profile`).
2. System displays current profile info (full name, phone, avatar, email — read-only).
3. User clicks "Edit Profile" → redirected to `/profile/edit`.
4. User modifies fields (fullName, phoneNumber).
5. User clicks "Save" → `PUT /api/profile`.
6. System validates and updates profile → returns updated data.
7. System displays success message.

**Alternative Flows:**

| Sub-flow | Steps |
|----------|-------|
| Upload Avatar | User clicks "Upload Avatar" → selects image file → `POST /api/profile/avatar` (multipart/form-data) → avatar URL returned |
| Remove Avatar | User clicks "Remove Avatar" → `DELETE /api/profile/avatar` → avatar reset to default |
| Change Password | User navigates to `/profile/change-password` → enters current + new password → `PUT /api/profile/change-password` |

**API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/profile` | Get current user profile |
| `PUT` | `/api/profile` | Update profile (fullName, phoneNumber) |
| `POST` | `/api/profile/avatar` | Upload avatar image |
| `DELETE` | `/api/profile/avatar` | Remove avatar |
| `PUT` | `/api/profile/change-password` | Change password |

**FE Pages:** `ProfilePage.tsx`, `EditProfilePage.tsx`, `ChangePasswordPage.tsx`

---

### UC 1.2: Manage Post (CRUD Bike Listings)

**Actors:** Seller

**Precondition:** User is logged in with Seller role.

**Main Flow — Create Post:**
1. Seller navigates to `/seller/posts/create`.
2. System displays create form (title, description, price, condition, brand, type, specs, images).
3. Seller fills in required fields and uploads images.
4. Seller clicks "Submit" → `POST /api/bikes` (multipart/form-data).
5. System validates data, creates listing with `ListingStatus = 2 (Pending)`.
6. System returns created listing data.
7. Seller sees success message; post appears in "My Posts" with Pending status.

**Alternative Flow — Edit Post:**
1. Seller navigates to `/seller/posts` → sees list of own posts.
2. Seller clicks "Edit" on a listing → redirected to `/seller/posts/:id/edit`.
3. System loads existing data into form → `GET /api/bikes/:id`.
4. Seller modifies fields.
5. Seller clicks "Save" → `PUT /api/bikes`.
6. System validates and updates listing.

**Alternative Flow — Delete Post:**
1. Seller clicks "Delete" on a listing in My Posts page.
2. System shows confirmation dialog.
3. Seller confirms → `DELETE /api/bikes/:id`.
4. System soft-deletes listing.

**Alternative Flow — Toggle Visibility (Hide/Show):**
1. Seller clicks "Hide" or "Show" toggle on a listing.
2. System calls `PATCH /api/bikes/:id/visibility`.
3. Listing visibility is toggled.

**API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/bikes` | Create new listing (multipart) |
| `PUT` | `/api/bikes` | Update existing listing |
| `DELETE` | `/api/bikes/:id` | Delete listing |
| `PATCH` | `/api/bikes/:id/visibility` | Toggle visibility |
| `GET` | `/api/bikes/my-posts` | Get seller's own listings |

**FE Pages:** `CreatePostPage.tsx`, `EditPostPage.tsx`, `MyPostsPage.tsx`

---

## Member 2: Transaction Hub (Orders & Payment)

### UC 2.1: Place Order & Deposit

**Actors:** Buyer

**Precondition:** User is logged in, listing is Active (status=1), item is in stock.

**Main Flow — Buy Now:**
1. Buyer views Bike Detail page (`/bikes/:id`).
2. Buyer clicks "Buy Now".
3. System calls `POST /api/orders` with `{ items: [{ listingId, quantity: 1 }] }`.
4. BE validates stock, creates Order with `Status = 1 (Pending)`, returns OrderDto.
5. Buyer is redirected to Order Detail page (`/orders/:id`).
6. System displays order info with payment options.

**Alternative Flow — Buy from Cart:**
1. Buyer navigates to Cart page (`/cart`).
2. Buyer reviews cart items.
3. Buyer clicks "Place Order" → `POST /api/orders` with multiple items.
4. BE creates order for all cart items.

**Main Flow — View Orders:**
1. Buyer navigates to `/orders` → `GET /api/orders/my-purchases`.
2. System displays list of all orders with status.
3. Buyer clicks an order → `/orders/:id` → `GET /api/orders/:id`.

**Alternative Flow — Cancel Order:**
1. Buyer views order with Pending or Paid status.
2. Buyer clicks "Cancel" → confirms dialog.
3. System calls `POST /api/orders/:id/cancel`.
4. Order status changes to `5 (Cancelled)`.

**Alternative Flow — Confirm Delivery:**
1. Buyer receives order (Shipping status).
2. Buyer clicks "Confirm Delivery" → `POST /api/orders/:id/confirm-delivery`.
3. Order status changes to `4 (Completed)`.

**API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/orders` | Create order |
| `GET` | `/api/orders/:id` | Get order detail |
| `GET` | `/api/orders/my-purchases` | List buyer's orders |
| `POST` | `/api/orders/:id/cancel` | Cancel order |
| `POST` | `/api/orders/:id/confirm-delivery` | Confirm delivery |

**Order Status Enum:** `1=Pending, 2=Paid, 3=Shipping, 4=Completed, 5=Cancelled, 6=Disputed`

**FE Pages:** `MyOrdersPage.tsx`, `OrderDetailPage.tsx`

---

### UC 2.2: Process Payment (VNPay Integration)

**Actors:** Buyer

**Precondition:** Order exists with Pending status.

**Main Flow:**
1. Buyer views Order Detail (`/orders/:id`) with Pending status.
2. Buyer selects payment type: `deposit` (partial) or `full` (complete payment).
3. Buyer clicks "Pay Now" → `POST /api/orders/create-payment-url` with `{ orderId, paymentType }`.
4. BE generates VNPay payment URL → returns `{ paymentUrl }`.
5. FE redirects browser to VNPay URL via `window.location.href` (external redirect).
6. Buyer completes payment on VNPay portal.
7. VNPay redirects back to FE return URL → `GET /api/orders/vnpay-return`.
8. BE verifies payment signature, updates order status to `2 (Paid)`.
9. VNPay sends IPN callback → `GET /api/orders/vnpay-ipn`.
10. System confirms payment recorded.

**API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/orders/create-payment-url` | Generate VNPay URL |
| `GET` | `/api/orders/vnpay-return` | Handle VNPay redirect callback |
| `GET` | `/api/orders/vnpay-ipn` | Handle VNPay IPN notification |

**FE Pages:** `OrderDetailPage.tsx` (payment section)

---

## Member 3: Interaction (Chat & Ratings)

### UC 3.1: Chat / Messaging (Real-time with SignalR)

**Actors:** Buyer, Seller (any authenticated user)

**Precondition:** User is logged in.

**Main Flow:**
1. Buyer views Bike Detail page → clicks "Chat with Seller".
2. System navigates to `/messages/:sellerId`.
3. FE loads conversation history → `GET /api/messages/conversations/:otherUserId`.
4. User types message and clicks Send.
5. FE sends message via SignalR Hub (real-time) AND `POST /api/messages`.
6. Other user receives message in real-time via SignalR `ChatHub`.
7. System updates unread count.

**Alternative Flow — View Conversations:**
1. User navigates to `/messages` → `GET /api/messages/conversations`.
2. System displays list of all conversations with last message preview and unread count.
3. User clicks a conversation → opens chat room.

**Alternative Flow — Mark as Read:**
1. User opens a conversation.
2. System automatically calls `PATCH /api/messages/conversations/:otherUserId/read`.
3. Unread count resets for that conversation.

**API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/messages` | Send message |
| `GET` | `/api/messages/conversations` | List all conversations |
| `GET` | `/api/messages/conversations/:otherUserId` | Get conversation history |
| `PATCH` | `/api/messages/conversations/:otherUserId/read` | Mark conversation as read |
| `GET` | `/api/messages/unread-count` | Get total unread count |

**Real-time:** `SignalR ChatHub` for instant message delivery

**FE Pages:** `ConversationsPage.tsx`, `ChatRoomPage.tsx`

---

### UC 3.2: Rate Seller

**Actors:** Buyer

**Precondition:** Order is Completed (status=4). Buyer has not yet rated this order.

**Main Flow:**
1. Buyer views completed order detail (`/orders/:id`).
2. System checks if rating exists → `GET /api/ratings/order/:orderId/check`.
3. If not rated, Buyer sees rating form.
4. Buyer selects star rating (1-5) and writes comment.
5. Buyer clicks "Submit" → `POST /api/ratings` with `{ orderId, score, comment }`.
6. System validates (order must be completed, buyer must be the buyer of order).
7. Rating is created and linked to seller.

**Alternative Flow — View Seller Reputation:**
1. Any user views Bike Detail page.
2. System displays seller reputation section → `GET /api/ratings/seller/:sellerId/stats`.
3. System shows average rating, total reviews.
4. User can expand to see individual reviews → `GET /api/ratings/seller/:sellerId`.

**API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/ratings` | Submit rating |
| `GET` | `/api/ratings/seller/:sellerId` | Get seller reviews |
| `GET` | `/api/ratings/seller/:sellerId/stats` | Get seller rating stats |
| `GET` | `/api/ratings/order/:orderId/check` | Check if order is rated |

**FE Components:** `SellerReputation.tsx` (on BikeDetailPage), rating form (on OrderDetailPage)

---

## Member 4: Buyer Experience (Search, Detail, Wishlist, Cart)

### UC 4.1: Research Bikes & Filter

**Actors:** Any user (public)

**Precondition:** None.

**Main Flow:**
1. User navigates to `/bikes` or `/store`.
2. System loads bike listings → `GET /api/bikes` with default params.
3. System displays grid of bike cards (image, title, price, brand, condition).
4. User applies filters (brand, type, price range, condition, search keyword).
5. System reloads → `GET /api/bikes?brandId=X&typeId=Y&minPrice=Z&maxPrice=W&search=keyword`.
6. Results update dynamically.

**Filter Parameters:**

| Param | Type | Description |
|-------|------|-------------|
| `search` | string | Keyword search (title, description) |
| `brandId` | number | Filter by brand |
| `typeId` | number | Filter by category/type |
| `minPrice` | number | Minimum price |
| `maxPrice` | number | Maximum price |
| `condition` | string | Bike condition |

**Supporting Endpoints:**
- `GET /api/bikes/brands` — List all brands for filter dropdown
- `GET /api/bikes/types` — List all types for filter dropdown

**API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/bikes` | List/search/filter bikes |
| `GET` | `/api/bikes/brands` | Get brand list for filters |
| `GET` | `/api/bikes/types` | Get type list for filters |

**FE Pages:** `BikeList.tsx`

---

### UC 4.2: View Bike Detail & Wishlist / Cart

**Actors:** Any user (public for viewing; auth for wishlist/cart)

**Main Flow — View Detail:**
1. User clicks a bike card → navigates to `/bikes/:id`.
2. System loads listing detail → `GET /api/bikes/:id`.
3. System displays: image gallery, title, price, condition, brand, type, specs (model, color, frame size, material, wheel size, brake type, weight, transmission, serial number), description, seller info, seller reputation.

**Alternative Flow — Add to Wishlist:**
1. Authenticated user clicks "Wishlist" toggle on Bike Detail.
2. If not in wishlist → `POST /api/wishlist/:listingId` → heart icon fills.
3. If in wishlist → `DELETE /api/wishlist/:listingId` → heart icon empties.
4. User can view all wishlist items at `/wishlist` → `GET /api/wishlist`.

**Alternative Flow — Add to Cart:**
1. Authenticated user clicks "Add to Cart" on Bike Detail.
2. System calls `POST /api/cart/:listingId` → button changes to "In Cart".
3. If already in cart → `DELETE /api/cart/:listingId` → removed.
4. User views cart at `/cart` → `GET /api/cart`.
5. Cart displays items with total price.

**API Endpoints (Wishlist):**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/wishlist` | Get wishlist items |
| `POST` | `/api/wishlist/:listingId` | Add to wishlist |
| `DELETE` | `/api/wishlist/:listingId` | Remove from wishlist |
| `GET` | `/api/wishlist/:listingId/check` | Check if in wishlist |

**API Endpoints (Cart):**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/cart` | Get cart items |
| `POST` | `/api/cart/:listingId` | Add to cart |
| `DELETE` | `/api/cart/:listingId` | Remove from cart |
| `DELETE` | `/api/cart` | Clear entire cart |
| `GET` | `/api/cart/:listingId/check` | Check if in cart |
| `GET` | `/api/cart/count` | Get cart item count |

**FE Pages:** `BikeDetailPage.tsx`, `WishlistPage.tsx`, `CartPage.tsx`

---

## Member 5: Quality & Auth (Register/Login & Inspection)

### UC 5.1: Register / Login

**Actors:** Guest (unauthenticated user)

**Main Flow — Email Registration:**
1. Guest navigates to `/register`.
2. System displays multi-step registration form.
3. **Step 1 (Email):** Guest enters email → system sends OTP to email.
4. **Step 2 (OTP):** Guest enters OTP code → `POST /api/auth/confirm-email`.
5. **Step 3 (Password):** Guest enters username, password, confirm password, selects role (Buyer/Seller/Inspector).
6. Guest clicks "Register" → `POST /api/auth/register`.
7. System creates user account and returns JWT token.
8. User is redirected to home page, logged in.

**Alternative Flow — Google Login:**
1. Guest clicks "Login with Google" on `/login`.
2. Google OAuth popup appears → user selects Google account.
3. FE receives Google credential → `POST /api/auth/google` with `{ credential }`.
4. BE verifies Google token, creates/finds user, returns JWT.
5. User is redirected to home page.

**Alternative Flow — Email Login:**
1. Guest navigates to `/login`.
2. Enters email/username and password.
3. Clicks "Login" → `POST /api/auth/login`.
4. System validates credentials, returns JWT token.
5. FE stores token in localStorage/context.

**Alternative Flow — Resend Confirmation:**
1. If email not confirmed → `POST /api/auth/resend-confirmation`.

**Alternative Flow — Logout:**
1. User clicks "Logout" in user dropdown.
2. FE calls `POST /api/auth/logout`.
3. FE clears token from storage, redirects to login.

**API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/auth/register` | Register new user |
| `POST` | `/api/auth/login` | Login with email/password |
| `POST` | `/api/auth/google` | Login with Google credential |
| `POST` | `/api/auth/confirm-email` | Confirm email with OTP |
| `POST` | `/api/auth/resend-confirmation` | Resend OTP email |
| `POST` | `/api/auth/logout` | Logout |

**FE Pages:** `Login.tsx`, `Register.tsx`, `VerifyEmail.tsx`

---

### UC 5.2: Inspect Vehicle & Upload Report

**Actors:** Seller (request), Inspector (accept & report)

**Main Flow — Seller Requests Inspection:**
1. Seller navigates to `/seller/inspections`.
2. System shows list of seller's inspection requests → `GET /api/inspections/requests/my`.
3. Seller clicks "Request Inspection" → form with `{ listingId, notes }`.
4. Seller submits → `POST /api/inspections/requests`.
5. System creates request with `Status = Pending`.

**Alternative Flow — Cancel Request:**
1. Seller clicks "Cancel" on a Pending request.
2. System calls `DELETE /api/inspections/requests/:requestId`.

**Main Flow — Inspector Accepts Request:**
1. Inspector navigates to `/inspector/pending`.
2. System loads pending requests → `GET /api/inspections/requests/pending`.
3. Inspector reviews request details.
4. Inspector clicks "Accept" → `PATCH /api/inspections/requests/:requestId/accept`.
5. Request status changes to `Assigned`, linked to inspector.

**Main Flow — Inspector Uploads Report:**
1. Inspector navigates to `/inspector/assigned`.
2. System loads assigned requests → `GET /api/inspections/requests/assigned`.
3. Inspector clicks "Upload Report" → navigates to `/inspector/upload-report/:requestId`.
4. Inspector fills report form (overallCondition, frameCondition, wheelsCondition, brakesCondition, drivetrainCondition, comments, score, images).
5. Inspector submits → `POST /api/inspections/reports`.
6. System creates inspection report, request status changes to `Completed`.

**Alternative Flow — View Reports:**
1. Inspector views own reports → `/inspector/reports` → `GET /api/inspections/reports/my`.
2. Public: view inspection for a listing → `GET /api/inspections/listing/:listingId`.
3. Public: view report by request → `GET /api/inspections/reports/:requestId`.

**API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/inspections/requests` | Create inspection request (Seller) |
| `GET` | `/api/inspections/requests/my` | Get my requests (Seller) |
| `DELETE` | `/api/inspections/requests/:requestId` | Cancel request (Seller) |
| `GET` | `/api/inspections/requests/pending` | Get pending requests (Inspector) |
| `GET` | `/api/inspections/requests/assigned` | Get assigned requests (Inspector) |
| `PATCH` | `/api/inspections/requests/:requestId/accept` | Accept request (Inspector) |
| `POST` | `/api/inspections/reports` | Upload report (Inspector) |
| `GET` | `/api/inspections/reports/my` | Get my reports (Inspector) |
| `GET` | `/api/inspections/listing/:listingId` | Get report by listing (Public) |
| `GET` | `/api/inspections/reports/:requestId` | Get report by request (Public) |

**FE Pages:** `MyInspectionRequestsPage.tsx` (Seller), `PendingInspectionsPage.tsx`, `AssignedInspectionsPage.tsx`, `UploadReportPage.tsx`, `MyInspectionReportsPage.tsx` (Inspector), `InspectionReportPage.tsx` (Public)

---

## Member 6: Admin Dashboard (Moderation, Disputes, Abuse)

### UC 6.1: Moderate Posts

**Actors:** Admin

**Precondition:** Admin is logged in.

**Main Flow:**
1. Admin navigates to `/admin/moderation`.
2. System loads pending posts → `GET /api/admin/posts/pending`.
3. System displays list of pending listings (title, seller, price, brand, type, image, date).
4. Admin reviews a post.
5. **Approve:** Admin clicks "Approve" → `POST /api/admin/posts/moderate` with `{ listingId, approve: true }`.
6. Listing status changes to `1 (Active)` → visible to public.

**Alternative Flow — Reject Post:**
1. Admin clicks "Reject" on a post.
2. System prompts for rejection reason.
3. Admin enters reason → `POST /api/admin/posts/moderate` with `{ listingId, approve: false, rejectionReason }`.
4. Listing status changes to `4 (Rejected)`.

**API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/admin/posts/pending` | Get pending posts |
| `POST` | `/api/admin/posts/moderate` | Approve/reject post |

**FE Pages:** `PostModerationPage.tsx`

---

### UC 6.2: Resolve Disputes & User Management + Abuse Reports

**Actors:** Admin

**Sub-UC 6.2a — User Management:**
1. Admin navigates to `/admin/users`.
2. System loads users → `GET /api/admin/users?roleId=X` (optional filter).
3. System displays user table (ID, username, email, role, status, listings count, orders count).
4. Admin clicks "Ban" on a user → confirms → `PATCH /api/admin/users/:userId/status` with `{ status: 0 }`.
5. User status changes to `0 (Banned)`.
6. Admin can "Unban" → `PATCH /api/admin/users/:userId/status` with `{ status: 1 }`.

**Sub-UC 6.2b — Abuse Reports:**
1. Admin navigates to `/admin/abuse`.
2. **Pending Tab:** System loads pending requests → `GET /api/admin/abuse/pending` → `AbuseRequestDto[]`.
3. Admin clicks "Process" on a report → resolve modal opens.
4. Admin enters resolution text, optional checkboxes: "Ban reported user", "Hide target listing".
5. **Resolve:** `POST /api/admin/abuse/resolve` with `{ requestAbuseId, resolution, status: 2, banTargetUser, hideTargetListing }`.
6. **Reject:** Same endpoint with `status: 3`.
7. **Resolved Tab:** Shows all processed reports → `GET /api/admin/abuse/reports` → `AbuseReportDto[]`.

**Sub-UC 6.2c — Dispute Resolution:**
1. Admin resolves order dispute → `POST /api/admin/disputes/resolve` with `{ orderId, resolution, refundBuyer, banSeller }`.
2. If `refundBuyer: true` → Order status set to `6 (Disputed)`.
3. If `banSeller: true` → Seller status set to `0 (Banned)`.

**Sub-UC 6.2d — Dashboard:**
1. Admin navigates to `/admin/dashboard` → `GET /api/admin/dashboard`.
2. System displays stats: totalUsers, totalActiveListings, pendingModerations, pendingAbuseReports, totalOrders, totalRevenue.
3. Quick links to moderation, user management, abuse reports.

**API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/admin/dashboard` | Dashboard stats |
| `GET` | `/api/admin/users` | List users (filter by role) |
| `PATCH` | `/api/admin/users/:userId/status` | Ban/unban user |
| `GET` | `/api/admin/abuse/pending` | Pending abuse requests |
| `GET` | `/api/admin/abuse/reports` | Resolved abuse reports |
| `POST` | `/api/admin/abuse/resolve` | Resolve abuse request |
| `POST` | `/api/admin/disputes/resolve` | Resolve order dispute |

**FE Pages:** `AdminDashboard.tsx`, `UserManagementPage.tsx`, `AbuseManagementPage.tsx`

**Buyer-side (Request Abuse):**
1. Buyer views Bike Detail → clicks "Report Abuse".
2. Selects reason, submits → `POST /api/abuse` with `{ targetListingId, targetUserId, reason }`.
3. Buyer views own reports → `/my-reports` → `GET /api/abuse/my-reports`.

**FE Pages/Components:** `ReportAbuseButton.tsx` (on BikeDetailPage), `MyAbuseReportsPage.tsx`

---

## Member 7: UserManager, ManageCategory / Brands (Admin CRUD)

### UC 7.1: UserManager (Admin CRUD Users)

**Actors:** Admin

**Main Flow — List/Create/Update/Delete Users:**
1. Admin navigates to `/admin/users` (UserManagementPage).
2. System loads all users → `GET /api/usermanager`.
3. Admin can search/filter users.
4. **Create:** Admin clicks "Create User" → fills form (username, email, password, roleId, fullName, phone) → `POST /api/usermanager`.
5. **View Detail:** `GET /api/usermanager/:userId`.
6. **Update:** Admin clicks "Edit" → modifies fields → `PUT /api/usermanager`.
7. **Delete:** Admin clicks "Delete" → confirms → `DELETE /api/usermanager/:userId`.
8. **Reset Password:** Admin clicks "Reset Password" → `POST /api/usermanager/:userId/reset-password`.

**API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/usermanager` | List all users |
| `GET` | `/api/usermanager/:userId` | Get user detail |
| `POST` | `/api/usermanager` | Create user |
| `PUT` | `/api/usermanager` | Update user |
| `DELETE` | `/api/usermanager/:userId` | Delete user |
| `POST` | `/api/usermanager/:userId/reset-password` | Reset user password |

**FE Pages:** `UserManagementPage.tsx`

---

### UC 7.2: Manage Categories (Bike Types)

**Actors:** Admin

**Main Flow:**
1. Admin navigates to `/admin/categories`.
2. System loads categories → `GET /api/categories`.
3. **Create:** Admin clicks "Add Category" → enters name, description → `POST /api/categories`.
4. **Update:** Admin clicks "Edit" → modifies fields → `PUT /api/categories`.
5. **Delete:** Admin clicks "Delete" → confirms → `DELETE /api/categories/:typeId`.

**API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/categories` | List all categories |
| `GET` | `/api/categories/:typeId` | Get category detail |
| `POST` | `/api/categories` | Create category |
| `PUT` | `/api/categories` | Update category |
| `DELETE` | `/api/categories/:typeId` | Delete category |

**FE Pages:** `CategoryManagementPage.tsx`

---

### UC 7.3: Manage Brands

**Actors:** Admin

**Main Flow:**
1. Admin navigates to `/admin/brands`.
2. System loads brands → `GET /api/brands`.
3. **Create:** Admin clicks "Add Brand" → enters name, description, logoUrl → `POST /api/brands`.
4. **Update:** Admin clicks "Edit" → modifies fields → `PUT /api/brands`.
5. **Delete:** Admin clicks "Delete" → confirms → `DELETE /api/brands/:brandId`.

**API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/brands` | List all brands |
| `GET` | `/api/brands/:brandId` | Get brand detail |
| `POST` | `/api/brands` | Create brand |
| `PUT` | `/api/brands` | Update brand |
| `DELETE` | `/api/brands/:brandId` | Delete brand |

**FE Pages:** `BrandManagementPage.tsx`

---

## Summary: All Members & UCs

| # | Member | UC | BE Controller | FE Pages | Status |
|---|--------|----|----|----------|--------|
| 1 | Seller Core | Manage Profile | `ProfileController` | Profile, EditProfile, ChangePassword | ✅ |
| 1 | Seller Core | Manage Post | `BikesController` | CreatePost, EditPost, MyPosts | ✅ |
| 2 | Transaction | Place Order | `OrdersController` | MyOrders, OrderDetail | ✅ |
| 2 | Transaction | Process Payment (VNPay) | `OrdersController` | OrderDetail (payment) | ✅ |
| 3 | Interaction | Chat / Messaging | `MessagesController` + SignalR `ChatHub` | Conversations, ChatRoom | ✅ |
| 3 | Interaction | Rate Seller | `RatingsController` | SellerReputation, OrderDetail | ✅ |
| 4 | Buyer Exp. | Research & Filter | `BikesController` | BikeList | ✅ |
| 4 | Buyer Exp. | Detail & Wishlist/Cart | `BikesController`, `WishlistController`, `CartController` | BikeDetail, Wishlist, Cart | ✅ |
| 5 | Quality & Auth | Register/Login | `AuthController` | Login, Register, VerifyEmail | ✅ |
| 5 | Quality & Auth | Inspect Vehicle | `InspectionsController` | MyInspections, Pending, Assigned, UploadReport, Reports | ✅ |
| 6 | Admin | Moderate Posts | `AdminController` | PostModeration | ✅ |
| 6 | Admin | Disputes & Abuse | `AdminController`, `AbuseController` | AbuseManagement, MyAbuseReports | ✅ |
| 7 | Admin CRUD | UserManager | `UserManagerController` | UserManagement | ✅ |
| 7 | Admin CRUD | Categories | `CategoriesController` | CategoryManagement | ✅ |
| 7 | Admin CRUD | Brands | `BrandsController` | BrandManagement | ✅ |
