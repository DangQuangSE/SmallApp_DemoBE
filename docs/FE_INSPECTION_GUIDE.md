# ???? FE Guide: Ki?m ??nh xe (Inspect Vehicle & Upload Report)

> **UC 1 — Inspect Vehicle (Seller):** Seller g?i yêu c?u ki?m ??nh xe cho listing c?a mình, theo dõi tr?ng thái, h?y yêu c?u.
>
> **UC 2 — Upload Report (Inspector):** Inspector xem danh sách yêu c?u ch?, nh?n vi?c, ki?m tra xe, upload báo cáo ki?m ??nh.
>
> **Base URL:** `http://localhost:5xxx` (ki?m tra port BE)
>
> **Stack FE:** React + Vite + TypeScript + Axios

---

## ?? API Endpoint Summary

### Seller — Yêu c?u ki?m ??nh (?? Auth)

| Method | Endpoint | Mô t? |
|--------|----------|--------|
| `POST` | `/api/inspections/requests` | T?o yêu c?u ki?m ??nh cho listing |
| `GET` | `/api/inspections/requests/my` | Danh sách yêu c?u ki?m ??nh c?a seller |
| `DELETE` | `/api/inspections/requests/{requestId}` | H?y yêu c?u (ch? khi Pending) |

### Inspector — Nh?n vi?c & Upload report (?? Inspector only)

| Method | Endpoint | Mô t? |
|--------|----------|--------|
| `GET` | `/api/inspections/requests/pending` | Danh sách yêu c?u ch? nh?n |
| `GET` | `/api/inspections/requests/assigned` | Danh sách yêu c?u ?ã nh?n |
| `PATCH` | `/api/inspections/requests/{requestId}/accept` | Nh?n yêu c?u ki?m ??nh |
| `POST` | `/api/inspections/reports` | Upload báo cáo ki?m ??nh |
| `GET` | `/api/inspections/reports/my` | Danh sách report ?ã upload |

### Public — Xem báo cáo ki?m ??nh (? Không c?n Auth)

| Method | Endpoint | Mô t? |
|--------|----------|--------|
| `GET` | `/api/inspections/listing/{listingId}` | Xem report ki?m ??nh c?a listing |
| `GET` | `/api/inspections/reports/{requestId}` | Xem report theo requestId |

---

## ?? Lu?ng nghi?p v?

```
Seller ??ng listing xe
  ?
  ??? B?m "Yêu c?u ki?m ??nh" trên listing
  ?       ??? POST /api/inspections/requests
  ?               ? RequestStatus = 1 (Pending)
  ?
  ?
Inspector xem danh sách yêu c?u ch?
  ?       ??? GET /api/inspections/requests/pending
  ?
  ??? Nh?n yêu c?u
  ?       ??? PATCH /api/inspections/requests/{id}/accept
  ?               ? RequestStatus = 2 (InProgress), InspectorId = assigned
  ?
  ??? Ki?m tra xe th?c t?
  ?
  ??? Upload báo cáo
  ?       ??? POST /api/inspections/reports
  ?               ? T?o InspectionReport + auto RequestStatus = 3 (Completed)
  ?
  ?
Buyer/Public xem report trên trang Bike Detail
  ?       ??? GET /api/inspections/listing/{listingId}
  ?
  ??? Badge "? ?ã ki?m ??nh" hi?n th? trên listing card
```

---

## ?? SELLER: YÊU C?U KI?M ??NH (UC 1)

### T?o yêu c?u — `POST /api/inspections/requests` (?? Auth)

**Request Body:**
```json
{
  "listingId": 15,
  "note": "Xe mua 2 n?m, mu?n ki?m ??nh tr??c khi bán"
}
```

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `listingId` | number | ? | ID listing c?a seller |
| `note` | string | ? | Ghi chú (max 500 ký t?) |

**Response `200 OK`:**
```json
{
  "requestId": 5,
  "listingId": 15,
  "listingTitle": "Xe ??p Giant Escape 3",
  "sellerId": 3,
  "sellerName": "seller_mike",
  "inspectorId": null,
  "inspectorName": null,
  "requestStatus": 1,
  "requestStatusLabel": "Pending",
  "requestDate": "2025-06-25T10:00:00Z",
  "hasReport": false
}
```

**Response `400`:**

| Error | Nguyên nhân |
|-------|-------------|
| `"Listing not found"` | ListingId không t?n t?i |
| `"You can only request inspection for your own listing"` | Không ph?i listing c?a seller |
| `"Listing must be Active or Pending to request inspection"` | Listing ?n/sold/rejected |
| `"An inspection request is already pending or in progress for this listing"` | ?ã có yêu c?u ch?a hoàn thành |

### Danh sách yêu c?u — `GET /api/inspections/requests/my` (?? Auth)

**Response `200 OK`:** `InspectionRequestDto[]`

```json
[
  {
    "requestId": 5,
    "listingId": 15,
    "listingTitle": "Xe ??p Giant Escape 3",
    "sellerId": 3,
    "sellerName": "seller_mike",
    "inspectorId": 10,
    "inspectorName": "inspector_anna",
    "requestStatus": 2,
    "requestStatusLabel": "InProgress",
    "requestDate": "2025-06-25T10:00:00Z",
    "hasReport": false
  },
  {
    "requestId": 3,
    "listingId": 8,
    "listingTitle": "Trek FX3",
    "sellerId": 3,
    "sellerName": "seller_mike",
    "inspectorId": 10,
    "inspectorName": "inspector_anna",
    "requestStatus": 3,
    "requestStatusLabel": "Completed",
    "requestDate": "2025-06-20T08:00:00Z",
    "hasReport": true
  }
]
```

### H?y yêu c?u — `DELETE /api/inspections/requests/{requestId}` (?? Auth)

> Ch? h?y ???c khi `requestStatus = 1` (Pending).

**Response `200 OK`:**
```json
{ "message": "Success" }
```

**Response `400`:**

| Error | Nguyên nhân |
|-------|-------------|
| `"Request not found"` | RequestId không t?n t?i |
| `"Access denied"` | Không ph?i listing c?a seller |
| `"Only pending requests can be cancelled"` | Request ?ã ???c inspector nh?n |

---

## ?? INSPECTOR: NH?N VI?C & UPLOAD REPORT (UC 2)

### Danh sách yêu c?u ch? — `GET /api/inspections/requests/pending` (?? Inspector)

> Tr? v? t?t c? request có `requestStatus = 1` (Pending), ch?a có inspector.

**Response `200 OK`:** `InspectionRequestDto[]` (s?p x?p theo `requestDate` t?ng d?n — yêu c?u c? nh?t tr??c)

### Danh sách yêu c?u ?ã nh?n — `GET /api/inspections/requests/assigned` (?? Inspector)

> Tr? v? t?t c? request mà inspector ?ã nh?n (t?t c? status).

**Response `200 OK`:** `InspectionRequestDto[]`

### Nh?n yêu c?u — `PATCH /api/inspections/requests/{requestId}/accept` (?? Inspector)

**Response `200 OK`:**
```json
{ "message": "Success" }
```

**Response `400`:**

| Error | Nguyên nhân |
|-------|-------------|
| `"Only inspectors can accept requests"` | User không ph?i Inspector (roleId ? 4) |
| `"Request not found"` | RequestId không t?n t?i |
| `"Only pending requests can be accepted"` | Request ?ã b? nh?n/hoàn thành/h?y |

### Upload báo cáo — `POST /api/inspections/reports` (?? Inspector)

> Sau khi ki?m tra xe th?c t?, inspector upload k?t qu?.
> Request t? ??ng chuy?n sang `Completed` (status=3).

**Request Body:**
```json
{
  "requestId": 5,
  "frameCheck": "Khung nhôm, không n?t, không cong. M?i hàn ch?c ch?n.",
  "brakeCheck": "Phanh ??a Shimano ho?t ??ng t?t. Má phanh còn ~70%.",
  "transmissionCheck": "B? truy?n ??ng Shimano Altus 3x8 ho?t ??ng m??t. Xích còn t?t.",
  "inspectorNote": "Xe t?ng th? còn t?t, ?ã s? d?ng ~1 n?m. Khuy?n ngh? thay l?p tr??c.",
  "finalVerdict": 1,
  "reportUrl": "https://res.cloudinary.com/.../reports/inspection_5.pdf"
}
```

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `requestId` | number | ? | ID request ?ã nh?n |
| `frameCheck` | string | ? | K?t qu? ki?m tra khung (max 500) |
| `brakeCheck` | string | ? | K?t qu? ki?m tra phanh (max 500) |
| `transmissionCheck` | string | ? | K?t qu? ki?m tra b? truy?n ??ng (max 500) |
| `inspectorNote` | string | ? | Ghi chú t?ng h?p (max 2000) |
| `finalVerdict` | number | ? | 1=Pass, 2=Fail, 3=Conditional |
| `reportUrl` | string | ? | URL file báo cáo (PDF/image) |

**Response `200 OK`:**
```json
{
  "reportId": 3,
  "requestId": 5,
  "listingId": 15,
  "listingTitle": "Xe ??p Giant Escape 3",
  "requestStatus": 3,
  "requestStatusLabel": "Completed",
  "frameCheck": "Khung nhôm, không n?t, không cong.",
  "brakeCheck": "Phanh ??a Shimano ho?t ??ng t?t.",
  "transmissionCheck": "B? truy?n ??ng Shimano Altus 3x8 ho?t ??ng m??t.",
  "inspectorNote": "Xe t?ng th? còn t?t.",
  "finalVerdict": 1,
  "finalVerdictLabel": "Pass",
  "reportUrl": "https://res.cloudinary.com/.../reports/inspection_5.pdf",
  "completedAt": "2025-06-26T14:00:00Z",
  "inspectorName": "inspector_anna",
  "bikeTitle": "Xe ??p Giant Escape 3"
}
```

**Response `400`:**

| Error | Nguyên nhân |
|-------|-------------|
| `"Request not found"` | RequestId không t?n t?i |
| `"You are not assigned to this request"` | Inspector không ???c assign |
| `"Request must be InProgress to upload report"` | Request ch?a nh?n ho?c ?ã hoàn thành |
| `"A report has already been uploaded for this request"` | ?ã upload report r?i |

### Danh sách report ?ã upload — `GET /api/inspections/reports/my` (?? Inspector)

**Response `200 OK`:** `InspectionReportDto[]`

---

## ?? PUBLIC: XEM BÁO CÁO KI?M ??NH

### Xem report theo listing — `GET /api/inspections/listing/{listingId}` (? Public)

> Tr? v? report m?i nh?t ?ã completed cho listing.

**Response `200 OK`:** `InspectionReportDto`

**Response `400`:**
```json
{ "error": "No completed inspection found for this listing" }
```

### Xem report theo requestId — `GET /api/inspections/reports/{requestId}` (? Public)

**Response `200 OK`:** `InspectionReportDto`

---

## ?? Reference Tables

### Request Status

| Value | Label | Mô t? |
|-------|-------|--------|
| `1` | Pending | Seller ?ã g?i, ch? Inspector nh?n |
| `2` | InProgress | Inspector ?ã nh?n, ?ang ki?m tra |
| `3` | Completed | Inspector ?ã upload report |
| `4` | Cancelled | Seller ?ã h?y yêu c?u |

### Final Verdict (K?t lu?n)

| Value | Label | Mô t? |
|-------|-------|--------|
| `1` | Pass | Xe ??t tiêu chu?n |
| `2` | Fail | Xe không ??t |
| `3` | Conditional | ??t có ?i?u ki?n (c?n s?a ch?a nh?) |

---

## ?? FE Implementation

### TypeScript Interfaces

```typescript
// src/types/inspection.ts

interface CreateInspectionRequestDto {
  listingId: number;
  note?: string;
}

interface InspectionRequestDto {
  requestId: number;
  listingId: number;
  listingTitle: string;
  sellerId: number;
  sellerName: string;
  inspectorId?: number;
  inspectorName?: string;
  requestStatus?: number;
  requestStatusLabel: string;
  requestDate?: string;
  hasReport: boolean;
}

interface UploadInspectionReportDto {
  requestId: number;
  frameCheck?: string;
  brakeCheck?: string;
  transmissionCheck?: string;
  inspectorNote?: string;
  finalVerdict?: number;    // 1=Pass, 2=Fail, 3=Conditional
  reportUrl?: string;
}

interface InspectionReportDto {
  reportId: number;
  requestId: number;
  listingId: number;
  listingTitle: string;
  requestStatus?: number;
  requestStatusLabel: string;
  frameCheck?: string;
  brakeCheck?: string;
  transmissionCheck?: string;
  inspectorNote?: string;
  finalVerdict?: number;
  finalVerdictLabel: string;
  reportUrl?: string;
  completedAt?: string;
  inspectorName: string;
  bikeTitle: string;
}
```

### Axios Service

```typescript
// src/services/inspectionService.ts
import api from './api';

export const inspectionService = {
  // ===== Seller =====
  createRequest: (dto: CreateInspectionRequestDto) =>
    api.post<InspectionRequestDto>('/api/inspections/requests', dto),

  getMyRequests: () =>
    api.get<InspectionRequestDto[]>('/api/inspections/requests/my'),

  cancelRequest: (requestId: number) =>
    api.delete(`/api/inspections/requests/${requestId}`),

  // ===== Inspector =====
  getPendingRequests: () =>
    api.get<InspectionRequestDto[]>('/api/inspections/requests/pending'),

  getAssignedRequests: () =>
    api.get<InspectionRequestDto[]>('/api/inspections/requests/assigned'),

  acceptRequest: (requestId: number) =>
    api.patch(`/api/inspections/requests/${requestId}/accept`),

  uploadReport: (dto: UploadInspectionReportDto) =>
    api.post<InspectionReportDto>('/api/inspections/reports', dto),

  getMyReports: () =>
    api.get<InspectionReportDto[]>('/api/inspections/reports/my'),

  // ===== Public =====
  getByListing: (listingId: number) =>
    api.get<InspectionReportDto>(`/api/inspections/listing/${listingId}`),

  getByRequestId: (requestId: number) =>
    api.get<InspectionReportDto>(`/api/inspections/reports/${requestId}`),
};
```

### Seller — Nút "Yêu c?u ki?m ??nh" trên My Listings

```tsx
// src/components/RequestInspectionButton.tsx
import { useState } from 'react';
import { inspectionService } from '../services/inspectionService';

interface Props {
  listingId: number;
  hasInspection: boolean;
  onSuccess: () => void;
}

const RequestInspectionButton = ({ listingId, hasInspection, onSuccess }: Props) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  if (hasInspection) {
    return <span className="badge inspected">? ?ã ki?m ??nh</span>;
  }

  const handleRequest = async () => {
    setLoading(true);
    setError('');
    try {
      await inspectionService.createRequest({ listingId });
      onSuccess();
    } catch (err: any) {
      setError(err.response?.data?.error || 'G?i yêu c?u th?t b?i');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <button onClick={handleRequest} disabled={loading} className="btn-inspect">
        {loading ? '?ang g?i...' : '?? Yêu c?u ki?m ??nh'}
      </button>
      {error && <p className="error">{error}</p>}
    </div>
  );
};
```

### Seller — Trang qu?n lý yêu c?u ki?m ??nh

```tsx
// src/pages/seller/MyInspectionRequestsPage.tsx
import { useState, useEffect } from 'react';
import { inspectionService } from '../../services/inspectionService';

const STATUS_COLOR: Record<number, string> = {
  1: '#f59e0b', // Pending - vàng
  2: '#3b82f6', // InProgress - xanh
  3: '#22c55e', // Completed - xanh lá
  4: '#ef4444', // Cancelled - ??
};

const MyInspectionRequestsPage = () => {
  const [requests, setRequests] = useState<InspectionRequestDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    inspectionService.getMyRequests()
      .then(res => setRequests(res.data))
      .finally(() => setLoading(false));
  }, []);

  const handleCancel = async (requestId: number) => {
    if (!window.confirm('B?n có ch?c mu?n h?y yêu c?u ki?m ??nh này?')) return;
    try {
      await inspectionService.cancelRequest(requestId);
      setRequests(prev =>
        prev.map(r => r.requestId === requestId
          ? { ...r, requestStatus: 4, requestStatusLabel: 'Cancelled' }
          : r
        )
      );
    } catch (err: any) {
      alert(err.response?.data?.error || 'H?y th?t b?i');
    }
  };

  if (loading) return <div>?ang t?i...</div>;

  return (
    <div className="my-inspections">
      <h1>Yêu c?u ki?m ??nh c?a tôi</h1>
      {requests.length === 0 ? (
        <p>Ch?a có yêu c?u nào</p>
      ) : (
        <div className="inspection-list">
          {requests.map(req => (
            <div key={req.requestId} className="inspection-card">
              <div className="inspection-header">
                <h3>{req.listingTitle}</h3>
                <span
                  className="status-badge"
                  style={{ backgroundColor: STATUS_COLOR[req.requestStatus || 1] }}
                >
                  {req.requestStatusLabel}
                </span>
              </div>
              <div className="inspection-info">
                {req.inspectorName && (
                  <p><strong>Inspector:</strong> {req.inspectorName}</p>
                )}
                <p><strong>Ngày g?i:</strong> {req.requestDate
                  ? new Date(req.requestDate).toLocaleDateString('vi-VN')
                  : ''}</p>
              </div>
              <div className="inspection-actions">
                {req.requestStatus === 1 && (
                  <button onClick={() => handleCancel(req.requestId)} className="btn-cancel">
                    ? H?y yêu c?u
                  </button>
                )}
                {req.hasReport && (
                  <a href={`/inspections/${req.requestId}`} className="btn-view-report">
                    ?? Xem báo cáo
                  </a>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
```

### Inspector — Dashboard: Nh?n vi?c

```tsx
// src/pages/inspector/PendingInspectionsPage.tsx
import { useState, useEffect } from 'react';
import { inspectionService } from '../../services/inspectionService';

const PendingInspectionsPage = () => {
  const [requests, setRequests] = useState<InspectionRequestDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    inspectionService.getPendingRequests()
      .then(res => setRequests(res.data))
      .finally(() => setLoading(false));
  }, []);

  const handleAccept = async (requestId: number) => {
    try {
      await inspectionService.acceptRequest(requestId);
      setRequests(prev => prev.filter(r => r.requestId !== requestId));
    } catch (err: any) {
      alert(err.response?.data?.error || 'Nh?n yêu c?u th?t b?i');
    }
  };

  if (loading) return <div>?ang t?i...</div>;

  return (
    <div className="pending-inspections">
      <h1>Yêu c?u ki?m ??nh ch? nh?n ({requests.length})</h1>
      {requests.length === 0 ? (
        <p>Không có yêu c?u nào ch? x? lý</p>
      ) : (
        <div className="inspection-list">
          {requests.map(req => (
            <div key={req.requestId} className="inspection-card">
              <div className="inspection-info">
                <h3>{req.listingTitle}</h3>
                <p><strong>Seller:</strong> {req.sellerName}</p>
                <p><strong>Ngày yêu c?u:</strong> {req.requestDate
                  ? new Date(req.requestDate).toLocaleString('vi-VN')
                  : ''}</p>
              </div>
              <button
                onClick={() => handleAccept(req.requestId)}
                className="btn-accept"
              >
                ? Nh?n ki?m ??nh
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
```

### Inspector — Upload Report Form

```tsx
// src/pages/inspector/UploadReportPage.tsx
import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { inspectionService } from '../../services/inspectionService';

const VERDICT_OPTIONS = [
  { value: 1, label: '? Pass — Xe ??t tiêu chu?n' },
  { value: 2, label: '? Fail — Xe không ??t' },
  { value: 3, label: '?? Conditional — ??t có ?i?u ki?n' },
];

const UploadReportPage = () => {
  const { requestId } = useParams<{ requestId: string }>();
  const navigate = useNavigate();

  const [form, setForm] = useState({
    frameCheck: '',
    brakeCheck: '',
    transmissionCheck: '',
    inspectorNote: '',
    finalVerdict: 0,
    reportUrl: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const updateField = (field: string, value: string | number) => {
    setForm(prev => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (form.finalVerdict === 0) return setError('Vui lòng ch?n k?t lu?n');

    setLoading(true);
    setError('');
    try {
      await inspectionService.uploadReport({
        requestId: Number(requestId),
        frameCheck: form.frameCheck || undefined,
        brakeCheck: form.brakeCheck || undefined,
        transmissionCheck: form.transmissionCheck || undefined,
        inspectorNote: form.inspectorNote || undefined,
        finalVerdict: form.finalVerdict,
        reportUrl: form.reportUrl || undefined,
      });
      navigate('/inspector/reports');
    } catch (err: any) {
      setError(err.response?.data?.error || 'Upload th?t b?i');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="upload-report">
      <h1>Upload Báo cáo Ki?m ??nh</h1>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label>Ki?m tra khung xe</label>
          <textarea
            value={form.frameCheck}
            onChange={(e) => updateField('frameCheck', e.target.value)}
            placeholder="Tình tr?ng khung, m?i hàn, cong vênh..."
            maxLength={500}
            rows={3}
          />
        </div>

        <div className="form-group">
          <label>Ki?m tra phanh</label>
          <textarea
            value={form.brakeCheck}
            onChange={(e) => updateField('brakeCheck', e.target.value)}
            placeholder="Lo?i phanh, ho?t ??ng, má phanh..."
            maxLength={500}
            rows={3}
          />
        </div>

        <div className="form-group">
          <label>Ki?m tra b? truy?n ??ng</label>
          <textarea
            value={form.transmissionCheck}
            onChange={(e) => updateField('transmissionCheck', e.target.value)}
            placeholder="H? b? ??, xích, líp, pedal..."
            maxLength={500}
            rows={3}
          />
        </div>

        <div className="form-group">
          <label>Ghi chú t?ng h?p</label>
          <textarea
            value={form.inspectorNote}
            onChange={(e) => updateField('inspectorNote', e.target.value)}
            placeholder="Nh?n xét t?ng th?, khuy?n ngh?..."
            maxLength={2000}
            rows={5}
          />
        </div>

        <div className="form-group">
          <label>K?t lu?n *</label>
          <div className="verdict-options">
            {VERDICT_OPTIONS.map(opt => (
              <label key={opt.value} className="verdict-option">
                <input
                  type="radio"
                  name="verdict"
                  value={opt.value}
                  checked={form.finalVerdict === opt.value}
                  onChange={() => updateField('finalVerdict', opt.value)}
                />
                <span>{opt.label}</span>
              </label>
            ))}
          </div>
        </div>

        <div className="form-group">
          <label>URL Báo cáo (PDF/Image)</label>
          <input
            type="url"
            value={form.reportUrl}
            onChange={(e) => updateField('reportUrl', e.target.value)}
            placeholder="https://..."
          />
        </div>

        {error && <p className="error">{error}</p>}

        <button type="submit" disabled={loading} className="btn-submit">
          {loading ? '?ang upload...' : '?? Upload Báo cáo'}
        </button>
      </form>
    </div>
  );
};
```

### Inspector — Danh sách yêu c?u ?ã nh?n

```tsx
// src/pages/inspector/AssignedInspectionsPage.tsx
import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { inspectionService } from '../../services/inspectionService';

const AssignedInspectionsPage = () => {
  const [requests, setRequests] = useState<InspectionRequestDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    inspectionService.getAssignedRequests()
      .then(res => setRequests(res.data))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div>?ang t?i...</div>;

  return (
    <div className="assigned-inspections">
      <h1>Yêu c?u ki?m ??nh c?a tôi</h1>
      {requests.length === 0 ? (
        <p>Ch?a nh?n yêu c?u nào</p>
      ) : (
        <div className="inspection-list">
          {requests.map(req => (
            <div key={req.requestId} className="inspection-card">
              <div className="inspection-info">
                <h3>{req.listingTitle}</h3>
                <p><strong>Seller:</strong> {req.sellerName}</p>
                <p><strong>Tr?ng thái:</strong> {req.requestStatusLabel}</p>
              </div>
              <div className="inspection-actions">
                {req.requestStatus === 2 && !req.hasReport && (
                  <Link
                    to={`/inspector/upload-report/${req.requestId}`}
                    className="btn-upload"
                  >
                    ?? Upload Báo cáo
                  </Link>
                )}
                {req.hasReport && (
                  <Link
                    to={`/inspections/${req.requestId}`}
                    className="btn-view"
                  >
                    ?? Xem Báo cáo
                  </Link>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
```

### Public — Hi?n th? report trên Bike Detail

```tsx
// src/components/InspectionBadge.tsx
import { useState, useEffect } from 'react';
import { inspectionService } from '../services/inspectionService';

interface Props {
  listingId: number;
}

const VERDICT_STYLES: Record<number, { emoji: string; label: string; color: string }> = {
  1: { emoji: '?', label: '??t', color: '#22c55e' },
  2: { emoji: '?', label: 'Không ??t', color: '#ef4444' },
  3: { emoji: '??', label: '??t có ?i?u ki?n', color: '#f59e0b' },
};

const InspectionBadge = ({ listingId }: Props) => {
  const [report, setReport] = useState<InspectionReportDto | null>(null);
  const [showDetail, setShowDetail] = useState(false);

  useEffect(() => {
    inspectionService.getByListing(listingId)
      .then(res => setReport(res.data))
      .catch(() => setReport(null));
  }, [listingId]);

  if (!report) return null;

  const verdict = report.finalVerdict ? VERDICT_STYLES[report.finalVerdict] : null;

  return (
    <div className="inspection-section">
      <div
        className="inspection-badge"
        onClick={() => setShowDetail(!showDetail)}
        style={{ cursor: 'pointer' }}
      >
        <span>{verdict?.emoji || '??'} ?ã ki?m ??nh</span>
        {verdict && (
          <span style={{ color: verdict.color, fontWeight: 'bold' }}>
            {' '}— {verdict.label}
          </span>
        )}
        <span className="toggle">{showDetail ? '?' : '?'}</span>
      </div>

      {showDetail && (
        <div className="inspection-detail">
          <p><strong>Inspector:</strong> {report.inspectorName}</p>
          <p><strong>Ngày ki?m tra:</strong> {report.completedAt
            ? new Date(report.completedAt).toLocaleDateString('vi-VN')
            : ''}</p>

          {report.frameCheck && (
            <div className="check-item">
              <strong>?? Khung xe:</strong>
              <p>{report.frameCheck}</p>
            </div>
          )}
          {report.brakeCheck && (
            <div className="check-item">
              <strong>?? Phanh:</strong>
              <p>{report.brakeCheck}</p>
            </div>
          )}
          {report.transmissionCheck && (
            <div className="check-item">
              <strong>?? Truy?n ??ng:</strong>
              <p>{report.transmissionCheck}</p>
            </div>
          )}
          {report.inspectorNote && (
            <div className="check-item">
              <strong>?? Ghi chú:</strong>
              <p>{report.inspectorNote}</p>
            </div>
          )}
          {report.reportUrl && (
            <a href={report.reportUrl} target="_blank" rel="noreferrer" className="btn-download">
              ?? T?i báo cáo ??y ??
            </a>
          )}
        </div>
      )}
    </div>
  );
};

// S? d?ng trong BikeDetailPage.tsx:
// <InspectionBadge listingId={listing.listingId} />
```

---

## ??? React Router Setup

```tsx
// Seller routes
<Route element={<ProtectedRoute />}>
  <Route path="/my-inspections" element={<MyInspectionRequestsPage />} />
</Route>

// Inspector routes
<Route element={<ProtectedRoute requiredRole="Inspector" />}>
  <Route path="/inspector/pending" element={<PendingInspectionsPage />} />
  <Route path="/inspector/assigned" element={<AssignedInspectionsPage />} />
  <Route path="/inspector/upload-report/:requestId" element={<UploadReportPage />} />
  <Route path="/inspector/reports" element={<MyReportsPage />} />
</Route>

// Public
<Route path="/inspections/:requestId" element={<InspectionReportPage />} />
```

---

## ?? L?U Ý QUAN TR?NG

### 1. Phân quy?n

| Role | Có th? |
|------|--------|
| **Seller** | T?o yêu c?u ki?m ??nh, xem danh sách yêu c?u, h?y yêu c?u pending |
| **Inspector** | Xem yêu c?u ch?, nh?n vi?c, upload report, xem report ?ã upload |
| **Public** | Xem report ki?m ??nh ?ã completed |

> API Inspector yêu c?u `[Authorize(Roles = "Inspector")]` ? user không ph?i Inspector s? nh?n `403`.

### 2. Quy t?c nghi?p v?

- M?i listing ch? có **1 yêu c?u pending/inprogress** t?i 1 th?i ?i?m
- Seller ch? request inspection cho **listing c?a mình** (status Active ho?c Pending)
- Seller ch? **h?y ???c yêu c?u Pending** (ch?a có inspector nh?n)
- Inspector ph?i **nh?n yêu c?u tr??c** r?i m?i upload report
- Upload report s? **t? ??ng complete** request (status ? 3)
- M?i request ch? có **1 report** — không upload l?i

### 3. Hi?n th? badge "?ã ki?m ??nh" trên BikeCard

```tsx
// Trong BikeCard component (listing search/grid):
{listing.hasInspection && (
  <span className="badge inspected">? ?ã ki?m ??nh</span>
)}
```

> `hasInspection` ?ã ???c tr? v? t? `GET /api/bikes` (BikePostDto).

---

## ? FE Checklist

### Seller — Yêu c?u ki?m ??nh
- [ ] Nút "Yêu c?u ki?m ??nh" trên trang My Listings (ch? listing Active/Pending)
- [ ] G?i `POST /api/inspections/requests` v?i `listingId`
- [ ] X? lý l?i: ?ã có yêu c?u pending, listing không h?p l?
- [ ] Trang "Yêu c?u ki?m ??nh c?a tôi" — danh sách v?i status badge
- [ ] Nút "H?y" cho yêu c?u Pending
- [ ] Link "Xem báo cáo" khi `hasReport = true`
- [ ] Hi?n th? inspector name khi ?ã assign

### Inspector — Nh?n vi?c & Upload Report
- [ ] Trang "Yêu c?u ch? nh?n" — danh sách pending requests
- [ ] Nút "Nh?n ki?m ??nh" ? `PATCH .../accept`
- [ ] Trang "Yêu c?u ?ã nh?n" — danh sách assigned requests
- [ ] Nút "Upload Báo cáo" cho request InProgress ch?a có report
- [ ] Form upload report: frameCheck, brakeCheck, transmissionCheck, inspectorNote, finalVerdict, reportUrl
- [ ] Validate: finalVerdict b?t bu?c ch?n (1/2/3)
- [ ] Redirect v? danh sách reports sau khi upload thành công
- [ ] Trang "Reports ?ã upload" — danh sách report

### Public — Xem báo cáo
- [ ] Component `InspectionBadge` trên BikeDetailPage
- [ ] Click toggle m?/?óng chi ti?t report
- [ ] Hi?n th?: inspector, ngày, frameCheck, brakeCheck, transmissionCheck, note
- [ ] Verdict badge (Pass = xanh, Fail = ??, Conditional = vàng)
- [ ] Link t?i báo cáo PDF n?u có `reportUrl`
- [ ] Badge "? ?ã ki?m ??nh" trên BikeCard (listing grid)
