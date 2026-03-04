# ??? FE Guide: Ki?m duy?t bài ??ng, Qu?n lý User & X? lý báo cáo l?m d?ng

> **UC 1 — Moderate Posts:** Admin duy?t/t? ch?i bài ??ng xe c?a Seller, ??m b?o n?i dung s?ch s?.
>
> **UC 2 — Resolve Disputes & User Management:** Admin x? lý tranh ch?p ??n hàng, qu?n lý tr?ng thái user (khóa/m?). Buyer g?i báo cáo l?m d?ng (Request Abuse) khi phát hi?n l?a ??o.
>
> **Base URL:** `http://localhost:5xxx` (ki?m tra port BE)
>
> **Stack FE:** React + Vite + TypeScript + Axios

---

## ?? API Endpoint Summary

### Buyer — Báo cáo l?m d?ng (?? Auth)

| Method | Endpoint | Mô t? |
|--------|----------|--------|
| `POST` | `/api/abuse` | G?i báo cáo l?m d?ng (report listing/user) |
| `GET` | `/api/abuse/my-reports` | Xem danh sách báo cáo ?ã g?i |

### Admin — Dashboard & Moderation (?? Admin only)

| Method | Endpoint | Mô t? |
|--------|----------|--------|
| `GET` | `/api/admin/dashboard` | Th?ng kê t?ng quan |
| `GET` | `/api/admin/posts/pending` | Danh sách bài ??ng ch? duy?t |
| `POST` | `/api/admin/posts/moderate` | Duy?t/t? ch?i bài ??ng |
| `GET` | `/api/admin/users` | Danh sách users (filter theo role) |
| `PATCH` | `/api/admin/users/{userId}/status` | C?p nh?t tr?ng thái user (ban/unban) |
| `POST` | `/api/admin/disputes/resolve` | X? lý tranh ch?p ??n hàng |
| `GET` | `/api/admin/abuse/pending` | Danh sách báo cáo l?m d?ng ch?a x? lý |
| `GET` | `/api/admin/abuse/reports` | T?t c? báo cáo ?ã x? lý |
| `POST` | `/api/admin/abuse/resolve` | X? lý báo cáo l?m d?ng |

---

## ?? BUYER: G?I BÁO CÁO L?M D?NG

### G?i báo cáo — `POST /api/abuse` (?? Auth)

> Buyer/Seller ??u có th? g?i báo cáo listing ho?c user khác.
> Ph?i ch? ??nh ít nh?t `targetListingId` ho?c `targetUserId`.

**Request Body:**
```json
{
  "targetListingId": 15,
  "targetUserId": 5,
  "reason": "Bài ??ng l?a ??o, hình ?nh không kh?p v?i th?c t?"
}
```

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `targetListingId` | number | ?* | ID listing b? báo cáo |
| `targetUserId` | number | ?* | ID user b? báo cáo |
| `reason` | string | ? | Lý do báo cáo (max 1000 ký t?) |

> \* Ph?i có ít nh?t 1 trong 2: `targetListingId` ho?c `targetUserId`.

**Response `200 OK`:**
```json
{
  "requestAbuseId": 3,
  "reporterId": 2,
  "reporterName": "buyer_user",
  "targetListingId": 15,
  "targetListingTitle": "Xe ??p Giant 2024",
  "targetUserId": 5,
  "targetUserName": "seller_john",
  "reason": "Bài ??ng l?a ??o, hình ?nh không kh?p v?i th?c t?",
  "createdAt": "2025-06-25T10:00:00Z",
  "isResolved": false
}
```

**Response `400`:**

| Error | Nguyên nhân |
|-------|-------------|
| `"Must specify either a listing or a user to report"` | Không ch? ??nh target |
| `"Reason is required"` | Không có lý do |
| `"You cannot report yourself"` | T? report chính mình |
| `"You cannot report your own listing"` | Report listing c?a mình |
| `"Target listing not found"` | Listing không t?n t?i |
| `"Target user not found"` | User không t?n t?i |
| `"You have already submitted a pending report for this target"` | ?ã report và ch?a x? lý |

### Xem báo cáo ?ã g?i — `GET /api/abuse/my-reports` (?? Auth)

**Response `200 OK`:** `AbuseRequestDto[]`

```json
[
  {
    "requestAbuseId": 3,
    "reporterId": 2,
    "reporterName": "buyer_user",
    "targetListingId": 15,
    "targetListingTitle": "Xe ??p Giant 2024",
    "targetUserId": 5,
    "targetUserName": "seller_john",
    "reason": "Bài ??ng l?a ??o",
    "createdAt": "2025-06-25T10:00:00Z",
    "isResolved": true
  }
]
```

> `isResolved: true` ? Admin ?ã x? lý báo cáo này.

---

## ??? ADMIN: KI?M DUY?T BÀI ??NG (UC 1)

### Dashboard — `GET /api/admin/dashboard`

**Response `200 OK`:**
```json
{
  "totalUsers": 150,
  "totalActiveListings": 85,
  "pendingModerations": 5,
  "pendingAbuseReports": 3,
  "totalOrders": 200,
  "totalRevenue": 500000000
}
```

### Danh sách bài ch? duy?t — `GET /api/admin/posts/pending`

**Response `200 OK`:**
```json
[
  {
    "listingId": 42,
    "title": "Xe ??p Trek FX3",
    "sellerName": "seller_mike",
    "price": 8500000,
    "brandName": "Trek",
    "typeName": "Hybrid",
    "postedDate": "2025-06-25T08:00:00Z",
    "primaryImageUrl": "https://res.cloudinary.com/.../listings/xyz.jpg"
  }
]
```

> S?p x?p theo `postedDate` t?ng d?n (bài c? nh?t lên ??u — duy?t tr??c).

### Duy?t/t? ch?i — `POST /api/admin/posts/moderate`

**Request Body:**
```json
{
  "listingId": 42,
  "approve": true,
  "rejectionReason": null,
  "notes": "N?i dung h?p l?"
}
```

| Field | Type | Required | Mô t? |
|-------|------|----------|--------|
| `listingId` | number | ? | ID bài ??ng |
| `approve` | boolean | ? | `true` = duy?t (status?1), `false` = t? ch?i (status?4) |
| `rejectionReason` | string | ? | Lý do t? ch?i (khi approve=false) |
| `notes` | string | ? | Ghi chú n?i b? |

**Response `200 OK`:**
```json
{ "message": "Success" }
```

**Response `400`:**

| Error | Nguyên nhân |
|-------|-------------|
| `"Listing not found"` | ListingId không t?n t?i |
| `"Listing is not pending moderation"` | Listing không ? tr?ng thái Pending (status?2) |

### Listing Status Reference

| Value | Label | Mô t? |
|-------|-------|--------|
| `0` | Hidden | B? ?n (b?i seller ho?c admin) |
| `1` | Active | ?ang hi?n th? công khai |
| `2` | Pending | Ch? admin duy?t |
| `3` | Sold | ?ã bán h?t (quantity=0) |
| `4` | Rejected | B? admin t? ch?i |

---

## ?? ADMIN: QU?N LÝ USER (UC 2)

### Danh sách users — `GET /api/admin/users?roleId={roleId}`

| Query Param | Type | Mô t? |
|-------------|------|--------|
| `roleId` | number (optional) | L?c theo role: 1=Admin, 2=Buyer, 3=Seller, 4=Inspector |

**Response `200 OK`:**
```json
[
  {
    "userId": 5,
    "username": "seller_john",
    "email": "john@email.com",
    "roleName": "Seller",
    "status": 1,
    "createdAt": "2025-01-15T10:00:00Z",
    "totalListings": 12,
    "totalOrders": 0
  }
]
```

### User Status Reference

| Value | Label | Mô t? |
|-------|-------|--------|
| `0` | Banned | B? khóa — không th? ??ng nh?p |
| `1` | Active | Ho?t ??ng bình th??ng |

### C?p nh?t tr?ng thái user — `PATCH /api/admin/users/{userId}/status`

**Request Body:**
```json
{ "status": 0 }
```

**Response `200 OK`:**
```json
{ "message": "Success" }
```

**Response `400`:**
```json
{ "error": "User not found" }
```

---

## ?? ADMIN: X? LÝ TRANH CH?P ??N HÀNG (UC 2)

### X? lý tranh ch?p — `POST /api/admin/disputes/resolve`

> Khi Buyer báo cáo v?n ?? v?i ??n hàng (l?a ??o, hàng không ?úng mô t?, ...).

**Request Body:**
```json
{
  "orderId": 15,
  "resolution": "?ã xác nh?n seller giao hàng không ?úng mô t?. Hoàn ti?n cho buyer.",
  "refundBuyer": true,
  "banSeller": true
}
```

| Field | Type | Required | Mô t? |
|-------|------|----------|--------|
| `orderId` | number | ? | ID ??n hàng tranh ch?p |
| `resolution` | string | ? | N?i dung gi?i quy?t |
| `refundBuyer` | boolean | ? | `true` ? order status chuy?n sang 6 (Disputed/Refund) |
| `banSeller` | boolean | ? | `true` ? khóa tài kho?n seller (status=0) |

**Response `200 OK`:**
```json
{ "message": "Success" }
```

**Response `400`:**
```json
{ "error": "Order not found" }
```

### Order Status Reference

| Value | Label | Mô t? |
|-------|-------|--------|
| `1` | Pending | Ch? xác nh?n |
| `2` | Confirmed | ?ã xác nh?n |
| `3` | Shipping | ?ang giao hàng |
| `4` | Completed | Hoàn thành |
| `5` | Cancelled | ?ã h?y |
| `6` | Disputed | Tranh ch?p / ?ã hoàn ti?n |

---

## ?? ADMIN: QU?N LÝ BÁO CÁO L?M D?NG (UC 2)

### Danh sách báo cáo ch?a x? lý — `GET /api/admin/abuse/pending`

**Response `200 OK`:** `AbuseRequestDto[]` (s?p x?p theo `createdAt` t?ng d?n)

```json
[
  {
    "requestAbuseId": 3,
    "reporterId": 2,
    "reporterName": "buyer_user",
    "targetListingId": 15,
    "targetListingTitle": "Xe ??p Giant 2024",
    "targetUserId": 5,
    "targetUserName": "seller_john",
    "reason": "Bài ??ng l?a ??o, hình ?nh không kh?p",
    "createdAt": "2025-06-25T10:00:00Z",
    "isResolved": false
  }
]
```

### T?t c? báo cáo ?ã x? lý — `GET /api/admin/abuse/reports`

**Response `200 OK`:** `AbuseReportDto[]`

```json
[
  {
    "reportAbuseId": 1,
    "requestAbuseId": 3,
    "adminName": "admin_user",
    "resolution": "?ã xác minh bài ??ng l?a ??o. ?n listing và khóa seller.",
    "status": 2,
    "resolvedAt": "2025-06-26T09:00:00Z",
    "request": {
      "requestAbuseId": 3,
      "reporterId": 2,
      "reporterName": "buyer_user",
      "targetListingId": 15,
      "targetListingTitle": "Xe ??p Giant 2024",
      "targetUserId": 5,
      "targetUserName": "seller_john",
      "reason": "Bài ??ng l?a ??o",
      "createdAt": "2025-06-25T10:00:00Z",
      "isResolved": true
    }
  }
]
```

### X? lý báo cáo — `POST /api/admin/abuse/resolve`

**Request Body:**
```json
{
  "requestAbuseId": 3,
  "resolution": "?ã xác minh bài ??ng l?a ??o. ?n listing và khóa seller.",
  "status": 2,
  "banTargetUser": true,
  "hideTargetListing": true
}
```

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `requestAbuseId` | number | ? | ID báo cáo c?n x? lý |
| `resolution` | string | ? | N?i dung gi?i quy?t (max 2000 chars) |
| `status` | number | ? | 1=Pending, 2=Resolved, 3=Rejected |
| `banTargetUser` | boolean | ? | `true` ? khóa tài kho?n user b? report (status=0) |
| `hideTargetListing` | boolean | ? | `true` ? ?n listing b? report (listingStatus=0) |

**Response `200 OK`:** `AbuseReportDto` (xem c?u trúc ? trên)

**Response `400`:**

| Error | Nguyên nhân |
|-------|-------------|
| `"Abuse request not found"` | RequestAbuseId không t?n t?i |
| `"This request has already been resolved"` | ?ã có ReportAbuse cho request này |

### Abuse Report Status Reference

| Value | Label | Mô t? |
|-------|-------|--------|
| `1` | Pending | ?ang x? lý |
| `2` | Resolved | ?ã gi?i quy?t |
| `3` | Rejected | Báo cáo không h?p l? / B? qua |

---

## ?? FE Implementation

### TypeScript Interfaces

```typescript
// ===== Abuse DTOs =====

interface CreateAbuseRequestDto {
  targetListingId?: number;
  targetUserId?: number;
  reason: string;
}

interface AbuseRequestDto {
  requestAbuseId: number;
  reporterId: number;
  reporterName: string;
  targetListingId?: number;
  targetListingTitle?: string;
  targetUserId?: number;
  targetUserName?: string;
  reason: string;
  createdAt?: string;
  isResolved: boolean;
}

interface ResolveAbuseRequestDto {
  requestAbuseId: number;
  resolution: string;
  status: number;    // 1=Pending, 2=Resolved, 3=Rejected
  banTargetUser: boolean;
  hideTargetListing: boolean;
}

interface AbuseReportDto {
  reportAbuseId: number;
  requestAbuseId: number;
  adminName: string;
  resolution?: string;
  status?: number;
  resolvedAt?: string;
  request: AbuseRequestDto;
}

// ===== Admin DTOs =====

interface DashboardStatsDto {
  totalUsers: number;
  totalActiveListings: number;
  pendingModerations: number;
  pendingAbuseReports: number;
  totalOrders: number;
  totalRevenue: number;
}

interface PendingPostDto {
  listingId: number;
  title: string;
  sellerName: string;
  price: number;
  brandName?: string;
  typeName?: string;
  postedDate?: string;
  primaryImageUrl?: string;
}

interface ModeratePostDto {
  listingId: number;
  approve: boolean;
  rejectionReason?: string;
  notes?: string;
}

interface AdminUserDto {
  userId: number;
  username: string;
  email: string;
  roleName: string;
  status?: number;
  createdAt?: string;
  totalListings: number;
  totalOrders: number;
}

interface ResolveDisputeDto {
  orderId: number;
  resolution: string;
  refundBuyer: boolean;
  banSeller: boolean;
}
```

### Axios Services

```typescript
// src/services/abuseService.ts — Buyer dùng
import api from './api';

export const abuseService = {
  submit: (dto: CreateAbuseRequestDto) =>
    api.post<AbuseRequestDto>('/api/abuse', dto),

  getMyReports: () =>
    api.get<AbuseRequestDto[]>('/api/abuse/my-reports'),
};
```

```typescript
// src/services/adminService.ts — Admin dùng
import api from './api';

export const adminService = {
  // Dashboard
  getDashboard: () =>
    api.get<DashboardStatsDto>('/api/admin/dashboard'),

  // Post Moderation
  getPendingPosts: () =>
    api.get<PendingPostDto[]>('/api/admin/posts/pending'),

  moderatePost: (dto: ModeratePostDto) =>
    api.post('/api/admin/posts/moderate', dto),

  // User Management
  getUsers: (roleId?: number) =>
    api.get<AdminUserDto[]>('/api/admin/users', { params: roleId ? { roleId } : {} }),

  updateUserStatus: (userId: number, status: number) =>
    api.patch(`/api/admin/users/${userId}/status`, { status }),

  // Dispute Resolution
  resolveDispute: (dto: ResolveDisputeDto) =>
    api.post('/api/admin/disputes/resolve', dto),

  // Abuse Management
  getPendingAbuse: () =>
    api.get<AbuseRequestDto[]>('/api/admin/abuse/pending'),

  getAbuseReports: () =>
    api.get<AbuseReportDto[]>('/api/admin/abuse/reports'),

  resolveAbuse: (dto: ResolveAbuseRequestDto) =>
    api.post<AbuseReportDto>('/api/admin/abuse/resolve', dto),
};
```

### Buyer — Nút Báo cáo trên Bike Detail

```tsx
// src/components/ReportAbuseButton.tsx
import { useState } from 'react';
import { abuseService } from '../services/abuseService';
import { useAuth } from '../hooks/useAuth';
import { useNavigate } from 'react-router-dom';

interface Props {
  targetListingId?: number;
  targetUserId?: number;
}

const ReportAbuseButton = ({ targetListingId, targetUserId }: Props) => {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [showForm, setShowForm] = useState(false);
  const [reason, setReason] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async () => {
    if (!isAuthenticated) return navigate('/login');
    if (!reason.trim()) return setError('Vui lòng nh?p lý do');

    setLoading(true);
    setError('');
    try {
      await abuseService.submit({
        targetListingId,
        targetUserId,
        reason: reason.trim(),
      });
      setSuccess(true);
      setShowForm(false);
      setReason('');
    } catch (err: any) {
      setError(err.response?.data?.error || 'G?i báo cáo th?t b?i');
    } finally {
      setLoading(false);
    }
  };

  if (success) return <span className="report-success">? ?ã g?i báo cáo</span>;

  return (
    <div className="report-abuse">
      {!showForm ? (
        <button onClick={() => setShowForm(true)} className="btn-report">
          ?? Báo cáo vi ph?m
        </button>
      ) : (
        <div className="report-form">
          <textarea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder="Lý do báo cáo (b?t bu?c)..."
            maxLength={1000}
            rows={3}
          />
          <span className="char-count">{reason.length}/1000</span>
          {error && <p className="error">{error}</p>}
          <div className="report-actions">
            <button onClick={handleSubmit} disabled={loading}>
              {loading ? '?ang g?i...' : 'G?i báo cáo'}
            </button>
            <button onClick={() => { setShowForm(false); setError(''); }}>
              H?y
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

// S? d?ng trong BikeDetailPage:
// <ReportAbuseButton targetListingId={listing.listingId} targetUserId={listing.sellerId} />
```

### Buyer — Trang xem báo cáo ?ã g?i

```tsx
// src/pages/MyAbuseReportsPage.tsx
import { useState, useEffect } from 'react';
import { abuseService } from '../services/abuseService';

const MyAbuseReportsPage = () => {
  const [reports, setReports] = useState<AbuseRequestDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    abuseService.getMyReports()
      .then(res => setReports(res.data))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div>?ang t?i...</div>;

  return (
    <div className="my-reports-page">
      <h1>Báo cáo vi ph?m c?a tôi</h1>
      {reports.length === 0 ? (
        <p>Ch?a có báo cáo nào</p>
      ) : (
        <div className="reports-list">
          {reports.map(report => (
            <div key={report.requestAbuseId} className="report-card">
              <div className="report-header">
                <span className={`status ${report.isResolved ? 'resolved' : 'pending'}`}>
                  {report.isResolved ? '? ?ã x? lý' : '? ?ang ch?'}
                </span>
                <span className="date">
                  {report.createdAt ? new Date(report.createdAt).toLocaleDateString('vi-VN') : ''}
                </span>
              </div>
              <div className="report-body">
                {report.targetListingTitle && (
                  <p><strong>Bài ??ng:</strong> {report.targetListingTitle}</p>
                )}
                {report.targetUserName && (
                  <p><strong>Ng??i b? báo cáo:</strong> {report.targetUserName}</p>
                )}
                <p><strong>Lý do:</strong> {report.reason}</p>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
```

### Admin — Dashboard

```tsx
// src/pages/admin/AdminDashboard.tsx
import { useState, useEffect } from 'react';
import { adminService } from '../../services/adminService';

const AdminDashboard = () => {
  const [stats, setStats] = useState<DashboardStatsDto | null>(null);

  useEffect(() => {
    adminService.getDashboard().then(res => setStats(res.data));
  }, []);

  if (!stats) return <div>?ang t?i...</div>;

  const formatCurrency = (amount: number) =>
    amount.toLocaleString('vi-VN') + '?';

  return (
    <div className="admin-dashboard">
      <h1>Admin Dashboard</h1>
      <div className="stats-grid">
        <div className="stat-card">
          <h3>T?ng Users</h3>
          <p>{stats.totalUsers}</p>
        </div>
        <div className="stat-card">
          <h3>Listings Active</h3>
          <p>{stats.totalActiveListings}</p>
        </div>
        <div className="stat-card highlight">
          <h3>Ch? Duy?t</h3>
          <p>{stats.pendingModerations}</p>
        </div>
        <div className="stat-card highlight">
          <h3>Báo cáo Ch? XL</h3>
          <p>{stats.pendingAbuseReports}</p>
        </div>
        <div className="stat-card">
          <h3>T?ng ??n hàng</h3>
          <p>{stats.totalOrders}</p>
        </div>
        <div className="stat-card">
          <h3>T?ng Doanh thu</h3>
          <p>{formatCurrency(stats.totalRevenue)}</p>
        </div>
      </div>
    </div>
  );
};
```

### Admin — Ki?m duy?t bài ??ng

```tsx
// src/pages/admin/PostModerationPage.tsx
import { useState, useEffect } from 'react';
import { adminService } from '../../services/adminService';

const PostModerationPage = () => {
  const [posts, setPosts] = useState<PendingPostDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    adminService.getPendingPosts()
      .then(res => setPosts(res.data))
      .finally(() => setLoading(false));
  }, []);

  const handleModerate = async (listingId: number, approve: boolean, rejectionReason?: string) => {
    try {
      await adminService.moderatePost({
        listingId,
        approve,
        rejectionReason,
      });
      setPosts(prev => prev.filter(p => p.listingId !== listingId));
    } catch (err: any) {
      alert(err.response?.data?.error || 'L?i x? lý');
    }
  };

  if (loading) return <div>?ang t?i...</div>;

  return (
    <div className="moderation-page">
      <h1>Ki?m duy?t bài ??ng ({posts.length})</h1>
      {posts.length === 0 ? (
        <p>Không có bài nào ch? duy?t</p>
      ) : (
        <div className="pending-posts">
          {posts.map(post => (
            <div key={post.listingId} className="pending-card">
              <div className="pending-image">
                <img src={post.primaryImageUrl || '/placeholder-bike.png'} alt={post.title} />
              </div>
              <div className="pending-info">
                <h3>{post.title}</h3>
                <p>Seller: {post.sellerName}</p>
                <p>Giá: {post.price.toLocaleString('vi-VN')}?</p>
                {post.brandName && <p>Hãng: {post.brandName}</p>}
                {post.typeName && <p>Lo?i: {post.typeName}</p>}
                <p>Ngày ??ng: {post.postedDate ? new Date(post.postedDate).toLocaleDateString('vi-VN') : ''}</p>
              </div>
              <div className="pending-actions">
                <button
                  className="btn-approve"
                  onClick={() => handleModerate(post.listingId, true)}
                >
                  ? Duy?t
                </button>
                <button
                  className="btn-reject"
                  onClick={() => {
                    const reason = prompt('Lý do t? ch?i:');
                    if (reason !== null) {
                      handleModerate(post.listingId, false, reason);
                    }
                  }}
                >
                  ? T? ch?i
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
```

### Admin — Qu?n lý Users

```tsx
// src/pages/admin/UserManagementPage.tsx
import { useState, useEffect } from 'react';
import { adminService } from '../../services/adminService';

const ROLE_MAP: Record<number, string> = {
  1: 'Admin', 2: 'Buyer', 3: 'Seller', 4: 'Inspector'
};

const UserManagementPage = () => {
  const [users, setUsers] = useState<AdminUserDto[]>([]);
  const [roleFilter, setRoleFilter] = useState<number | undefined>();
  const [loading, setLoading] = useState(true);

  const loadUsers = async () => {
    setLoading(true);
    try {
      const res = await adminService.getUsers(roleFilter);
      setUsers(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadUsers(); }, [roleFilter]);

  const handleToggleStatus = async (userId: number, currentStatus?: number) => {
    const newStatus = currentStatus === 1 ? 0 : 1;
    const action = newStatus === 0 ? 'khóa' : 'm? khóa';
    if (!window.confirm(`B?n có ch?c mu?n ${action} user này?`)) return;

    try {
      await adminService.updateUserStatus(userId, newStatus);
      setUsers(prev =>
        prev.map(u => u.userId === userId ? { ...u, status: newStatus } : u)
      );
    } catch (err: any) {
      alert(err.response?.data?.error || 'L?i c?p nh?t');
    }
  };

  return (
    <div className="user-management">
      <h1>Qu?n lý Users</h1>

      <select
        value={roleFilter || ''}
        onChange={(e) => setRoleFilter(e.target.value ? Number(e.target.value) : undefined)}
      >
        <option value="">T?t c? roles</option>
        {Object.entries(ROLE_MAP).map(([id, name]) => (
          <option key={id} value={id}>{name}</option>
        ))}
      </select>

      {loading ? <p>?ang t?i...</p> : (
        <table className="users-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Username</th>
              <th>Email</th>
              <th>Role</th>
              <th>Status</th>
              <th>Listings</th>
              <th>Orders</th>
              <th>Ngày t?o</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {users.map(user => (
              <tr key={user.userId}>
                <td>{user.userId}</td>
                <td>{user.username}</td>
                <td>{user.email}</td>
                <td>{user.roleName}</td>
                <td>
                  <span className={`status-badge ${user.status === 1 ? 'active' : 'banned'}`}>
                    {user.status === 1 ? 'Active' : 'Banned'}
                  </span>
                </td>
                <td>{user.totalListings}</td>
                <td>{user.totalOrders}</td>
                <td>{user.createdAt ? new Date(user.createdAt).toLocaleDateString('vi-VN') : ''}</td>
                <td>
                  <button onClick={() => handleToggleStatus(user.userId, user.status)}>
                    {user.status === 1 ? '?? Khóa' : '?? M? khóa'}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};
```

### Admin — X? lý báo cáo l?m d?ng

```tsx
// src/pages/admin/AbuseManagementPage.tsx
import { useState, useEffect } from 'react';
import { adminService } from '../../services/adminService';

const ABUSE_STATUS_MAP: Record<number, string> = {
  1: '?ang x? lý',
  2: '?ã gi?i quy?t',
  3: '?ã bác b?',
};

const AbuseManagementPage = () => {
  const [pendingRequests, setPendingRequests] = useState<AbuseRequestDto[]>([]);
  const [resolvedReports, setResolvedReports] = useState<AbuseReportDto[]>([]);
  const [activeTab, setActiveTab] = useState<'pending' | 'resolved'>('pending');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const [pendingRes, reportsRes] = await Promise.all([
          adminService.getPendingAbuse(),
          adminService.getAbuseReports(),
        ]);
        setPendingRequests(pendingRes.data);
        setResolvedReports(reportsRes.data);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const handleResolve = async (
    requestAbuseId: number,
    status: number,
    banTargetUser: boolean,
    hideTargetListing: boolean
  ) => {
    const resolution = prompt('N?i dung gi?i quy?t:');
    if (!resolution) return;

    try {
      await adminService.resolveAbuse({
        requestAbuseId,
        resolution,
        status,
        banTargetUser,
        hideTargetListing,
      });
      setPendingRequests(prev => prev.filter(r => r.requestAbuseId !== requestAbuseId));
      // Reload resolved reports
      const res = await adminService.getAbuseReports();
      setResolvedReports(res.data);
    } catch (err: any) {
      alert(err.response?.data?.error || 'L?i x? lý');
    }
  };

  if (loading) return <div>?ang t?i...</div>;

  return (
    <div className="abuse-management">
      <h1>Qu?n lý Báo cáo Vi ph?m</h1>

      <div className="tabs">
        <button
          className={activeTab === 'pending' ? 'active' : ''}
          onClick={() => setActiveTab('pending')}
        >
          Ch? x? lý ({pendingRequests.length})
        </button>
        <button
          className={activeTab === 'resolved' ? 'active' : ''}
          onClick={() => setActiveTab('resolved')}
        >
          ?ã x? lý ({resolvedReports.length})
        </button>
      </div>

      {activeTab === 'pending' && (
        <div className="pending-list">
          {pendingRequests.length === 0 ? (
            <p>Không có báo cáo nào ch? x? lý</p>
          ) : (
            pendingRequests.map(req => (
              <div key={req.requestAbuseId} className="abuse-card">
                <div className="abuse-info">
                  <p><strong>Ng??i báo cáo:</strong> {req.reporterName}</p>
                  {req.targetListingTitle && (
                    <p><strong>Listing:</strong> {req.targetListingTitle} (ID: {req.targetListingId})</p>
                  )}
                  {req.targetUserName && (
                    <p><strong>User b? report:</strong> {req.targetUserName} (ID: {req.targetUserId})</p>
                  )}
                  <p><strong>Lý do:</strong> {req.reason}</p>
                  <p className="abuse-date">
                    {req.createdAt ? new Date(req.createdAt).toLocaleString('vi-VN') : ''}
                  </p>
                </div>
                <div className="abuse-actions">
                  <button
                    className="btn-resolve"
                    onClick={() => handleResolve(req.requestAbuseId, 2, true, true)}
                  >
                    ?? X? lý (Ban + ?n)
                  </button>
                  <button
                    className="btn-resolve-soft"
                    onClick={() => handleResolve(req.requestAbuseId, 2, false, true)}
                  >
                    ?? Ch? ?n Listing
                  </button>
                  <button
                    className="btn-reject"
                    onClick={() => handleResolve(req.requestAbuseId, 3, false, false)}
                  >
                    ? Bác b?
                  </button>
                </div>
              </div>
            ))
          )}
        </div>
      )}

      {activeTab === 'resolved' && (
        <div className="resolved-list">
          {resolvedReports.length === 0 ? (
            <p>Ch?a có báo cáo nào ???c x? lý</p>
          ) : (
            resolvedReports.map(report => (
              <div key={report.reportAbuseId} className="abuse-card resolved">
                <div className="abuse-info">
                  <p><strong>Ng??i báo cáo:</strong> {report.request.reporterName}</p>
                  {report.request.targetListingTitle && (
                    <p><strong>Listing:</strong> {report.request.targetListingTitle}</p>
                  )}
                  {report.request.targetUserName && (
                    <p><strong>User b? report:</strong> {report.request.targetUserName}</p>
                  )}
                  <p><strong>Lý do:</strong> {report.request.reason}</p>
                  <hr />
                  <p><strong>Admin x? lý:</strong> {report.adminName}</p>
                  <p><strong>K?t qu?:</strong> {report.resolution}</p>
                  <p>
                    <strong>Tr?ng thái:</strong>{' '}
                    <span className={`status-badge status-${report.status}`}>
                      {ABUSE_STATUS_MAP[report.status || 1]}
                    </span>
                  </p>
                  <p className="abuse-date">
                    {report.resolvedAt ? new Date(report.resolvedAt).toLocaleString('vi-VN') : ''}
                  </p>
                </div>
              </div>
            ))
          )}
        </div>
      )}
    </div>
  );
};
```

---

## ??? React Router Setup

```tsx
// Admin routes (protected, role = Admin)
<Route element={<AdminProtectedRoute />}>
  <Route path="/admin" element={<AdminDashboard />} />
  <Route path="/admin/moderation" element={<PostModerationPage />} />
  <Route path="/admin/users" element={<UserManagementPage />} />
  <Route path="/admin/abuse" element={<AbuseManagementPage />} />
</Route>

// Buyer routes (protected, authenticated)
<Route element={<ProtectedRoute />}>
  <Route path="/my-reports" element={<MyAbuseReportsPage />} />
</Route>
```

---

## ?? L?U Ý QUAN TR?NG

### 1. Phân quy?n

| Role | Có th? |
|------|--------|
| **Buyer/Seller** | G?i báo cáo (`POST /api/abuse`), xem báo cáo ?ã g?i |
| **Admin** | T?t c? API `/api/admin/*` — dashboard, moderation, user management, abuse resolve |

> FE c?n ki?m tra `user.roleName === 'Admin'` tr??c khi hi?n th? Admin sidebar/routes.
> N?u user không ph?i Admin g?i API admin ? BE tr? `403 Forbidden`.

### 2. Lu?ng Báo cáo L?m d?ng

```
Buyer phát hi?n vi ph?m
  ?
  ??? B?m "Báo cáo vi ph?m" trên Bike Detail
  ?       ??? POST /api/abuse (targetListingId + targetUserId + reason)
  ?
  ??? Ho?c báo cáo User tr?c ti?p
  ?       ??? POST /api/abuse (targetUserId + reason)
  ?
  ?
RequestAbuse record (isResolved: false)
  ?
  ?
Admin th?y trong GET /api/admin/abuse/pending
  ?
  ??? X? lý: POST /api/admin/abuse/resolve
  ?       ??? banTargetUser: true ? User.Status = 0 (Banned)
  ?       ??? hideTargetListing: true ? Listing.ListingStatus = 0 (Hidden)
  ?       ??? T?o ReportAbuse record
  ?
  ??? Bác b?: status = 3 (Rejected)
```

### 3. Lu?ng Ki?m duy?t Bài ??ng

```
Seller ??ng bài ? ListingStatus = 2 (Pending)
  ?
  ?
Admin xem GET /api/admin/posts/pending
  ?
  ??? Duy?t: approve = true ? ListingStatus = 1 (Active)
  ?
  ??? T? ch?i: approve = false ? ListingStatus = 4 (Rejected)
```

### 4. Lu?ng X? lý Tranh ch?p ??n hàng

```
Buyer g?p v?n ?? v?i ??n hàng
  ?
  ??? G?i báo cáo l?m d?ng (POST /api/abuse)
  ?
  ?
Admin x? lý tranh ch?p
  ?
  ??? POST /api/admin/disputes/resolve
  ?       ??? refundBuyer: true ? Order.Status = 6 (Disputed)
  ?       ??? banSeller: true ? Seller.Status = 0 (Banned)
  ?
  ??? K?t h?p v?i Abuse resolve n?u c?n
```

### 5. Ch?ng duplicate report

- BE ki?m tra: n?u user ?ã g?i báo cáo cho cùng target (listing+user) và ch?a x? lý ? tr? l?i `"You have already submitted a pending report for this target"`.
- FE nên hi?n th? tr?ng thái "?ã báo cáo" n?u user ?ã report.

---

## ? FE Checklist

### Buyer — Báo cáo vi ph?m
- [ ] Nút "Báo cáo vi ph?m" trên trang Bike Detail
- [ ] Form nh?p lý do (required, max 1000 chars)
- [ ] G?i `POST /api/abuse` v?i `targetListingId` + `targetUserId` + `reason`
- [ ] X? lý l?i duplicate report
- [ ] Hi?n th? "?ã g?i báo cáo" sau khi submit thành công
- [ ] Trang "Báo cáo c?a tôi" — danh sách v?i tr?ng thái (ch?/?ã x? lý)
- [ ] Redirect `/login` n?u ch?a ??ng nh?p

### Admin — Dashboard
- [ ] Hi?n th? t?t c? stats (users, listings, pending, abuse, orders, revenue)
- [ ] Highlight `pendingModerations` và `pendingAbuseReports` n?u > 0
- [ ] Format ti?n VND b?ng `toLocaleString('vi-VN')`

### Admin — Ki?m duy?t bài ??ng
- [ ] G?i `GET /api/admin/posts/pending` khi mount
- [ ] Hi?n th? thông tin bài: title, seller, price, brand, type, image, date
- [ ] Nút Duy?t ? `moderatePost({ listingId, approve: true })`
- [ ] Nút T? ch?i ? prompt lý do ? `moderatePost({ listingId, approve: false, rejectionReason })`
- [ ] Remove card kh?i list sau khi x? lý
- [ ] X? lý l?i "Listing is not pending moderation"

### Admin — Qu?n lý Users
- [ ] Filter dropdown theo role (Admin, Buyer, Seller, Inspector)
- [ ] B?ng users: ID, username, email, role, status, listings, orders, date
- [ ] Nút Khóa/M? khóa ? confirm ? `updateUserStatus(userId, status)`
- [ ] Badge status (Active = xanh, Banned = ??)

### Admin — X? lý báo cáo l?m d?ng
- [ ] Tab "Ch? x? lý" / "?ã x? lý"
- [ ] Hi?n th?: reporter, target listing/user, lý do, ngày
- [ ] Nút "X? lý (Ban + ?n)" ? `resolveAbuse({ ..., banTargetUser: true, hideTargetListing: true })`
- [ ] Nút "Ch? ?n Listing" ? `resolveAbuse({ ..., hideTargetListing: true })`
- [ ] Nút "Bác b?" ? `resolveAbuse({ ..., status: 3 })`
- [ ] Prompt nh?p resolution tr??c khi submit
- [ ] L?ch s? ?ã x? lý: admin name, resolution, status badge, date

### Admin — X? lý tranh ch?p
- [ ] Form: orderId, resolution text, toggle refundBuyer, toggle banSeller
- [ ] G?i `POST /api/admin/disputes/resolve`
- [ ] Confirm tr??c khi ban seller
