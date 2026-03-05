# FE Guide: Post Moderation, User Management & Abuse Report Management

> **UC 1 - Moderate Posts:** Admin approves/rejects seller bike listings, ensuring content quality.
>
> **UC 2 - Resolve Disputes & User Management:** Admin handles order disputes, manages user status (ban/unban). Buyer submits abuse reports (Request Abuse) when detecting fraud.
>
> **Base URL:** `http://localhost:5xxx` (check BE port)
>
> **FE Stack:** React + Vite + TypeScript + Axios

---

## API Endpoint Summary

### Buyer - Abuse Reports (Auth Required)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/abuse` | Submit abuse report (report listing/user) |
| `GET` | `/api/abuse/my-reports` | View list of submitted reports |

### Admin - Dashboard & Moderation (Admin only)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/admin/dashboard` | Overview statistics |
| `GET` | `/api/admin/posts/pending` | Pending posts awaiting moderation |
| `POST` | `/api/admin/posts/moderate` | Approve/reject a post |
| `GET` | `/api/admin/users` | User list (filter by role) |
| `PATCH` | `/api/admin/users/{userId}/status` | Update user status (ban/unban) |
| `POST` | `/api/admin/disputes/resolve` | Resolve order dispute |
| `GET` | `/api/admin/abuse/pending` | Pending abuse requests (not yet resolved) |
| `GET` | `/api/admin/abuse/reports` | All resolved abuse reports |
| `POST` | `/api/admin/abuse/resolve` | Resolve an abuse request |

---

## BUYER: SUBMIT ABUSE REPORT

### Submit Report - `POST /api/abuse` (Auth Required)

> Buyer/Seller can report a listing or another user.
> Must specify at least `targetListingId` or `targetUserId`.

**Request Body:**
```json
{
  "targetListingId": 15,
  "targetUserId": 5,
  "reason": "Fraudulent listing, images do not match reality"
}
```

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `targetListingId` | number | *partial | ID of reported listing |
| `targetUserId` | number | *partial | ID of reported user |
| `reason` | string | yes | Reason for report (max 1000 chars) |

> At least one of `targetListingId` or `targetUserId` must be provided.

**Response `200 OK`:**
```json
{
  "requestAbuseId": 3,
  "reporterId": 2,
  "reporterName": "buyer_user",
  "targetListingId": 15,
  "targetListingTitle": "Giant Bicycle 2024",
  "targetUserId": 5,
  "targetUserName": "seller_john",
  "reason": "Fraudulent listing, images do not match reality",
  "createdAt": "2025-06-25T10:00:00Z",
  "isResolved": false
}
```

**Response `400`:**

| Error | Cause |
|-------|-------|
| `"Must specify either a listing or a user to report"` | No target specified |
| `"Reason is required"` | Reason is empty |
| `"You cannot report yourself"` | Self-report attempt |
| `"You cannot report your own listing"` | Reporting own listing |
| `"Target listing not found"` | Invalid listing ID |
| `"Target user not found"` | Invalid user ID |
| `"You have already submitted a pending report for this target"` | Duplicate pending report |

### Get My Reports - `GET /api/abuse/my-reports` (Auth Required)

**Response `200 OK`:** Array of `AbuseRequestDto`
```json
[
  {
    "requestAbuseId": 3,
    "reporterId": 2,
    "reporterName": "buyer_user",
    "targetListingId": 15,
    "targetListingTitle": "Giant Bicycle 2024",
    "targetUserId": 5,
    "targetUserName": "seller_john",
    "reason": "Fraudulent listing",
    "createdAt": "2025-06-25T10:00:00Z",
    "isResolved": false
  }
]
```

---

## ADMIN: ABUSE MANAGEMENT

### Get Pending Abuse - `GET /api/admin/abuse/pending` (Admin only)

**Response `200 OK`:** Array of `AbuseRequestDto` (same structure as above, where `isResolved = false`)

### Get All Reports - `GET /api/admin/abuse/reports` (Admin only)

**Response `200 OK`:** Array of `AbuseReportDto` (resolved reports with nested request)
```json
[
  {
    "reportAbuseId": 1,
    "requestAbuseId": 3,
    "adminName": "admin_user",
    "resolution": "User banned for fraudulent activity",
    "status": 2,
    "resolvedAt": "2025-06-26T10:00:00Z",
    "request": {
      "requestAbuseId": 3,
      "reporterId": 2,
      "reporterName": "buyer_user",
      "targetListingId": 15,
      "targetListingTitle": "Giant Bicycle 2024",
      "targetUserId": 5,
      "targetUserName": "seller_john",
      "reason": "Fraudulent listing",
      "createdAt": "2025-06-25T10:00:00Z",
      "isResolved": true
    }
  }
]
```

### Resolve Abuse - `POST /api/admin/abuse/resolve` (Admin only)

**Request Body:**
```json
{
  "requestAbuseId": 3,
  "resolution": "User banned, listing hidden",
  "status": 2,
  "banTargetUser": true,
  "hideTargetListing": true
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `requestAbuseId` | number | yes | ID of abuse request to resolve |
| `resolution` | string | yes | Resolution description |
| `status` | number | yes | `2` = Resolved, `3` = Rejected |
| `banTargetUser` | boolean | yes | Whether to ban the reported user (sets User.Status = 0) |
| `hideTargetListing` | boolean | yes | Whether to hide the target listing (sets ListingStatus = 0) |

**Response `200 OK`:** `AbuseReportDto` (same structure as above)

---

## React Router Setup

```tsx
// Admin routes (protected, role = Admin)
<Route path="/admin/dashboard" element={<AdminDashboard />} />
<Route path="/admin/moderation" element={<PostModerationPage />} />
<Route path="/admin/users" element={<UserManagementPage />} />
<Route path="/admin/abuse" element={<AbuseManagementPage />} />

// Buyer routes (protected, authenticated)
<Route path="/my-reports" element={<MyAbuseReportsPage />} />
```

---

## IMPORTANT NOTES

### 1. Authorization

| Role | Permissions |
|------|-------------|
| **Buyer/Seller** | Submit report (`POST /api/abuse`), view own reports |
| **Admin** | All `/api/admin/*` APIs - dashboard, moderation, user management, abuse resolve |

> FE should check `user.roleName === 'Admin'` before showing Admin sidebar/routes.
> If a non-Admin calls admin APIs, BE returns `403 Forbidden`.

### 2. Abuse Report Flow

```
Buyer detects violation
  |
  +-- Clicks "Report Abuse" on Bike Detail page
  |       --> POST /api/abuse (targetListingId + targetUserId + reason)
  |
  +-- Or reports a User directly
  |       --> POST /api/abuse (targetUserId + reason)
  |
  v
RequestAbuse record (isResolved: false)
  |
  v
Admin sees in GET /api/admin/abuse/pending
  |
  +-- Resolve: POST /api/admin/abuse/resolve
  |       --> banTargetUser: true --> User.Status = 0 (Banned)
  |       --> hideTargetListing: true --> Listing.ListingStatus = 0 (Hidden)
  |       --> Creates ReportAbuse record
  |
  +-- Reject: status = 3 (Rejected)
```

### 3. Post Moderation Flow

```
Seller posts listing --> ListingStatus = 2 (Pending)
  |
  v
Admin views GET /api/admin/posts/pending
  |
  +-- Approve: approve = true --> ListingStatus = 1 (Active)
  |
  +-- Reject: approve = false --> ListingStatus = 4 (Rejected)
```

### 4. Duplicate Report Prevention

- BE checks: if user has already submitted a pending report for the same target (listing+user), returns `"You have already submitted a pending report for this target"`.
- FE should handle this error and display "Already reported" state.

---

## FE Checklist

### Buyer - Abuse Reporting
- [x] "Report Abuse" button on Bike Detail page
- [x] Reason selection form (required, max 1000 chars)
- [x] Send `POST /api/abuse` with `targetListingId` + `targetUserId` + `reason`
- [x] Handle duplicate report error
- [x] Show "Report submitted" after successful submit
- [x] "My Reports" page - list with status (pending/resolved)
- [x] Redirect to `/login` if not authenticated

### Admin - Abuse Management
- [x] Two tabs: "Pending" / "Resolved"
- [x] Pending tab: reporter, target listing/user, reason, date
- [x] Resolve modal with resolution text, ban user checkbox, hide listing checkbox
- [x] "Resolve" button sends resolveAbuse with status 2
- [x] "Reject" button sends resolveAbuse with status 3
- [x] Resolved tab: admin name, resolution, status badge, date, request details
