# ?? FE Guide: Order & VNPay Payment (??t hàng & Thanh toán)

> Qu?n lý ??t hàng, ??t c?c 20%, thanh toán qua VNPay sandbox.
>
> **Base URL:** `http://localhost:5157` (ki?m tra port BE)
>
> **All endpoints prefix:** `/api/orders`

---

## ?? Flow t?ng quan

```
???????????????     POST /api/orders          ????????????????
?  FE (Buyer) ? ?????????????????????????????  ?   Backend    ?
?  ??t hàng   ?  { listingId: 1 }             ?              ?
?             ? ?????????????????????????????  ? Order:Pending?
?             ?   OrderDto (depositAmount=20%) ? Deposit:20%  ?
???????????????                                ????????????????
       ?
       ?  POST /api/orders/create-payment-url
       ?  { orderId, paymentType: "deposit" }
       ?
???????????????     PaymentUrlResultDto        ????????????????
?  FE redirect? ?????????????????????????????  ?   VNPay      ?
?  to VNPay   ?   paymentUrl: "https://..."    ?   Sandbox    ?
?             ?                                ?              ?
?             ?  User pays on VNPay page       ?  Test card:  ?
?             ?  ????????????????????????       ?  see below   ?
?             ?                                ????????????????
?             ?                                       ?
?             ?  VNPay redirects to BE ReturnUrl      ?
?             ?  ??????????????????????????????????????
?             ?
?             ?  BE validates ? updates order ? redirects FE
?             ?  ??? Redirect: /orders/{id}?payment=success
???????????????
       ?
       ?  (Order status = 2: Deposit Paid)
       ?
       ?  POST /api/orders/create-payment-url
       ?  { orderId, paymentType: "full" }
       ?
       ?  ... same VNPay flow for remaining 80% ...
       ?
       ?  (Order status = 3: Shipping / Fully Paid)
       ?
       ?  POST /api/orders/{id}/confirm-delivery
       ?
       ?  (Order status = 4: Completed)
```

---

## ?? API Endpoint Summary

| Method | Endpoint | Auth | Body | Response |
|---|---|---|---|---|
| `POST` | `/api/orders` | ?? | `CreateOrderDto` | `OrderDto` |
| `GET` | `/api/orders/{id}` | ?? | — | `OrderDto` |
| `GET` | `/api/orders/my-purchases` | ?? | — | `OrderDto[]` |
| `POST` | `/api/orders/{id}/cancel` | ?? | — | `{ message }` |
| `POST` | `/api/orders/{id}/confirm-delivery` | ?? | — | `{ message }` |
| `POST` | `/api/orders/create-payment-url` | ?? | `CreatePaymentUrlDto` | `PaymentUrlResultDto` |
| `GET` | `/api/orders/vnpay-return` | ? | Query params (VNPay) | Redirect to FE |
| `GET` | `/api/orders/vnpay-ipn` | ? | Query params (VNPay) | `{ RspCode, Message }` |

---

## ?? 1. ??t hàng — `POST /api/orders` (?? Auth)

### Request

```http
POST /api/orders
Authorization: Bearer {token}
Content-Type: application/json
```

```json
{
  "listingId": 1
}
```

### Response `200 OK` — `OrderDto`

```json
{
  "orderId": 1,
  "orderStatus": 1,
  "totalAmount": 5500000,
  "depositAmount": 1100000,
  "remainingAmount": 5500000,
  "depositStatus": 1,
  "orderDate": "2025-06-25T10:00:00Z",
  "bikeTitle": "Xe ??p Giant Escape 3",
  "bikeImageUrl": "https://res.cloudinary.com/.../listings/abc123.jpg",
  "buyerName": "john_doe",
  "sellerName": "jane_doe",
  "payments": []
}
```

### Gi?i thích response

| Field | Mô t? |
|---|---|
| `totalAmount` | T?ng giá tr? ??n hàng (= giá listing) |
| `depositAmount` | S? ti?n c?c = 20% × totalAmount |
| `remainingAmount` | S? ti?n còn ph?i tr? (totalAmount ? ?ã thanh toán) |
| `depositStatus` | `1`=Pending, `2`=Confirmed, `3`=Cancelled |
| `payments` | Danh sách các giao d?ch ?ã thanh toán (ban ??u r?ng) |

### Response — Error `400`

| Error | Nguyên nhân |
|---|---|
| `"Listing not found"` | Listing không t?n t?i |
| `"This listing is not available"` | Listing ?ã bán / ?n |
| `"You cannot buy your own listing"` | Buyer = Seller |
| `"You already have a pending order for this listing"` | ?ã có ??n pending |

### FE Logic

```tsx
const handlePlaceOrder = async (listingId: number) => {
  const confirmed = window.confirm(
    `B?n s? ??t c?c 20% giá tr? xe. Ti?p t?c?`
  );
  if (!confirmed) return;

  try {
    const res = await api.post('/api/orders', { listingId });
    const order: OrderDto = res.data;
    // Redirect to order detail ? user can proceed to pay deposit
    navigate(`/orders/${order.orderId}`);
  } catch (err: any) {
    setError(err.response?.data?.error || '??t hàng th?t b?i');
  }
};
```

---

## ?? 2. T?o URL thanh toán VNPay — `POST /api/orders/create-payment-url` (?? Auth)

### Request

```http
POST /api/orders/create-payment-url
Authorization: Bearer {token}
Content-Type: application/json
```

```json
{
  "orderId": 1,
  "paymentType": "deposit"
}
```

| Field | Type | Required | Mô t? |
|---|---|---|---|
| `orderId` | number | ? | ID ??n hàng |
| `paymentType` | string | ? | `"deposit"` = ??t c?c 20%, `"full"` = thanh toán ph?n còn l?i 80% |

### Response `200 OK` — `PaymentUrlResultDto`

```json
{
  "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Amount=110000000&vnp_Command=pay&...",
  "orderId": 1,
  "amount": 1100000,
  "paymentType": "deposit"
}
```

### Response — Error `400`

| Error | Nguyên nhân |
|---|---|
| `"Order not found"` | ??n hàng không t?n t?i |
| `"Access denied"` | Không ph?i buyer c?a ??n hàng |
| `"Order is not in pending status for deposit payment"` | ??t c?c ch? khi `orderStatus=1` (Pending) |
| `"Deposit must be paid first"` | Thanh toán full ch? khi `orderStatus=2` (Deposit ?ã tr?) |
| `"Order is already fully paid"` | ?ã thanh toán ?? |
| `"PaymentType must be 'deposit' or 'full'"` | Sai `paymentType` |

### FE Logic — Redirect to VNPay

```tsx
const handlePayDeposit = async (orderId: number) => {
  try {
    const res = await api.post('/api/orders/create-payment-url', {
      orderId,
      paymentType: 'deposit',
    });

    const result: PaymentUrlResultDto = res.data;

    // ?? REDIRECT user to VNPay payment page
    window.location.href = result.paymentUrl;
  } catch (err: any) {
    setError(err.response?.data?.error || 'Không th? t?o link thanh toán');
  }
};

const handlePayRemaining = async (orderId: number) => {
  try {
    const res = await api.post('/api/orders/create-payment-url', {
      orderId,
      paymentType: 'full',
    });

    const result: PaymentUrlResultDto = res.data;
    window.location.href = result.paymentUrl;
  } catch (err: any) {
    setError(err.response?.data?.error || 'Không th? t?o link thanh toán');
  }
};
```

> ?? **Dùng `window.location.href`** — KHÔNG dùng `navigate()` vì c?n redirect ra domain VNPay.

---

## ?? 3. VNPay Return — X? lý sau khi thanh toán

Sau khi user thanh toán xong trên VNPay, VNPay s? redirect v?:

```
http://localhost:5157/api/orders/vnpay-return?vnp_Amount=...&vnp_ResponseCode=00&...
```

BE s?:
1. Validate ch? ký VNPay
2. Ghi nh?n payment vào DB
3. C?p nh?t tr?ng thái ??n hàng
4. **Redirect browser v? FE:**

```
? Thành công: http://localhost:5174/orders/{orderId}?payment=success
? Th?t b?i:   http://localhost:5174/orders?payment=failed&error=...
```

### FE — X? lý query params sau redirect

```tsx
// Trong OrderDetailPage ho?c OrdersPage
const [searchParams] = useSearchParams();
const paymentStatus = searchParams.get('payment');

useEffect(() => {
  if (paymentStatus === 'success') {
    setMessage('Thanh toán thành công! ??n hàng ?ã ???c c?p nh?t.');
    // Reload order detail
    loadOrder();
  } else if (paymentStatus === 'failed') {
    const error = searchParams.get('error');
    setError(`Thanh toán th?t b?i: ${error || 'Unknown error'}`);
  }
}, [paymentStatus]);
```

---

## ?? 4. Xem ??n hàng — `GET /api/orders/{id}` (?? Auth)

### Response `200 OK` — `OrderDto`

```json
{
  "orderId": 1,
  "orderStatus": 2,
  "totalAmount": 5500000,
  "depositAmount": 1100000,
  "remainingAmount": 4400000,
  "depositStatus": 2,
  "orderDate": "2025-06-25T10:00:00Z",
  "bikeTitle": "Xe ??p Giant Escape 3",
  "bikeImageUrl": "https://res.cloudinary.com/...",
  "buyerName": "john_doe",
  "sellerName": "jane_doe",
  "payments": [
    {
      "paymentId": 1,
      "amount": 1100000,
      "paymentMethod": "VNPay",
      "transactionRef": "1-20250625100530",
      "paymentDate": "2025-06-25T10:05:30Z"
    }
  ]
}
```

### FE — C? buyer và seller ??u xem ???c

```tsx
const loadOrder = async () => {
  try {
    const res = await api.get(`/api/orders/${orderId}`);
    setOrder(res.data);
  } catch (err: any) {
    if (err.response?.data?.error === 'Access denied') {
      navigate('/403');
    }
  }
};
```

---

## ?? 5. ??n hàng c?a tôi — `GET /api/orders/my-purchases` (?? Auth)

### Response `200 OK` — `OrderDto[]`

---

## ? 6. Hu? ??n — `POST /api/orders/{id}/cancel` (?? Auth)

Ch? hu? ???c khi `orderStatus` là `1` (Pending) ho?c `2` (Deposit Paid).
Không hu? ???c khi ?ang ship (`3`), ?ã hoàn thành (`4`), ho?c ?ã hu? (`5`).

### FE Logic

```tsx
const handleCancel = async (orderId: number) => {
  const confirmed = window.confirm(
    'B?n có ch?c mu?n hu? ??n hàng?\nN?u ?ã ??t c?c, ti?n c?c có th? không ???c hoàn.'
  );
  if (!confirmed) return;

  try {
    await api.post(`/api/orders/${orderId}/cancel`);
    loadOrder(); // Reload
    setMessage('??n hàng ?ã ???c hu?');
  } catch (err: any) {
    setError(err.response?.data?.error || 'Hu? ??n th?t b?i');
  }
};
```

---

## ? 7. Xác nh?n nh?n hàng — `POST /api/orders/{id}/confirm-delivery` (?? Auth)

Ch? xác nh?n ???c khi `orderStatus = 3` (Shipping / Fully Paid).

### FE Logic

```tsx
const handleConfirmDelivery = async (orderId: number) => {
  const confirmed = window.confirm('Xác nh?n ?ã nh?n hàng?');
  if (!confirmed) return;

  try {
    await api.post(`/api/orders/${orderId}/confirm-delivery`);
    loadOrder();
    setMessage('?ã xác nh?n nh?n hàng thành công!');
  } catch (err: any) {
    setError(err.response?.data?.error || 'Xác nh?n th?t b?i');
  }
};
```

---

## ?? Order Status & Deposit Status Reference

### Order Status

| Value | Label | Mô t? | Actions cho Buyer |
|---|---|---|---|
| `1` | Pending | ??t hàng, ch?a ??t c?c | ??t c?c, Hu? |
| `2` | Paid (Deposit) | ?ã ??t c?c 20% | Thanh toán 80%, Hu? |
| `3` | Shipping | ?ã thanh toán ?? | Xác nh?n nh?n hàng |
| `4` | Completed | Hoàn thành | — |
| `5` | Cancelled | ?ã hu? | — |

### Deposit Status

| Value | Label |
|---|---|
| `1` | Pending |
| `2` | Confirmed |
| `3` | Cancelled |

### StatusBadge + Action Buttons

```tsx
const ORDER_STATUS: Record<number, { label: string; color: string }> = {
  1: { label: 'Ch? ??t c?c', color: 'orange' },
  2: { label: '?ã ??t c?c', color: 'blue' },
  3: { label: '?ang giao hàng', color: 'purple' },
  4: { label: 'Hoàn thành', color: 'green' },
  5: { label: '?ã hu?', color: 'red' },
};

const OrderActions = ({ order }: { order: OrderDto }) => (
  <div className="order-actions">
    {/* Pending ? ??t c?c */}
    {order.orderStatus === 1 && (
      <>
        <button onClick={() => handlePayDeposit(order.orderId)} className="btn-primary">
          ?? ??t c?c {formatVND(order.depositAmount)}
        </button>
        <button onClick={() => handleCancel(order.orderId)} className="btn-danger">
          Hu? ??n
        </button>
      </>
    )}

    {/* Deposit Paid ? Thanh toán ph?n còn l?i */}
    {order.orderStatus === 2 && (
      <>
        <button onClick={() => handlePayRemaining(order.orderId)} className="btn-primary">
          ?? Thanh toán {formatVND(order.remainingAmount)}
        </button>
        <button onClick={() => handleCancel(order.orderId)} className="btn-danger">
          Hu? ??n
        </button>
      </>
    )}

    {/* Shipping ? Xác nh?n nh?n hàng */}
    {order.orderStatus === 3 && (
      <button onClick={() => handleConfirmDelivery(order.orderId)} className="btn-success">
        ? Xác nh?n nh?n hàng
      </button>
    )}
  </div>
);

const formatVND = (amount?: number) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount || 0);
```

---

## ?? VNPay Sandbox Test Cards

> ?? Ch? dùng cho sandbox (test). Không ph?i th? th?t.

### Th? n?i ??a (Ngân hàng NCB)

| Field | Value |
|---|---|
| Ngân hàng | `NCB` |
| S? th? | `9704198526191432198` |
| Tên ch? th? | `NGUYEN VAN A` |
| Ngày phát hành | `07/15` |
| M?t kh?u OTP | `123456` |

### Th? qu?c t? (Visa/Mastercard)

| Field | Value |
|---|---|
| Lo?i th? | `Visa` |
| S? th? | `4111111111111111` (ho?c b?t k? test visa) |

> Tham kh?o thêm t?i: https://sandbox.vnpayment.vn/apis/docs/thanh-toan-pay/pay.html

---

## ?? TypeScript Interfaces

```typescript
// ===== Request DTOs =====

interface CreateOrderDto {
  listingId: number;
}

interface CreatePaymentUrlDto {
  orderId: number;
  paymentType: 'deposit' | 'full';
}

// ===== Response DTOs =====

interface OrderDto {
  orderId: number;
  orderStatus?: number;        // 1=Pending, 2=Paid, 3=Shipping, 4=Completed, 5=Cancelled
  totalAmount?: number;        // T?ng giá tr? ??n hàng
  depositAmount?: number;      // S? ti?n c?c (20%)
  remainingAmount?: number;    // S? ti?n còn ph?i tr?
  depositStatus?: number;      // 1=Pending, 2=Confirmed, 3=Cancelled
  orderDate?: string;
  bikeTitle: string;
  bikeImageUrl?: string;
  buyerName: string;
  sellerName: string;
  payments: PaymentDto[];
}

interface PaymentDto {
  paymentId: number;
  amount?: number;
  paymentMethod?: string;
  transactionRef?: string;
  paymentDate?: string;
}

interface PaymentUrlResultDto {
  paymentUrl: string;          // URL redirect ??n VNPay
  orderId: number;
  amount: number;              // S? ti?n s? thanh toán
  paymentType: string;         // "deposit" | "full"
}
```

---

## ??? Axios Order Service

```typescript
import api from './api';

export const orderService = {
  /** ??t hàng (t?o order + deposit 20%) */
  placeOrder: (listingId: number) =>
    api.post<OrderDto>('/api/orders', { listingId }),

  /** Xem chi ti?t ??n hàng */
  getById: (orderId: number) =>
    api.get<OrderDto>(`/api/orders/${orderId}`),

  /** Danh sách ??n hàng c?a tôi */
  getMyPurchases: () =>
    api.get<OrderDto[]>('/api/orders/my-purchases'),

  /** Hu? ??n hàng */
  cancel: (orderId: number) =>
    api.post(`/api/orders/${orderId}/cancel`),

  /** Xác nh?n nh?n hàng */
  confirmDelivery: (orderId: number) =>
    api.post(`/api/orders/${orderId}/confirm-delivery`),

  /** T?o URL thanh toán VNPay (deposit ho?c full) */
  createPaymentUrl: (orderId: number, paymentType: 'deposit' | 'full') =>
    api.post<PaymentUrlResultDto>('/api/orders/create-payment-url', {
      orderId,
      paymentType,
    }),
};
```

---

## ?? React Router

```tsx
<Route path="/orders" element={<ProtectedRoute><MyOrdersPage /></ProtectedRoute>} />
<Route path="/orders/:id" element={<ProtectedRoute><OrderDetailPage /></ProtectedRoute>} />
```

---

## ?? Suggested FE File Structure

```
src/
??? pages/
?   ??? orders/
?       ??? MyOrdersPage.tsx          ? Danh sách ??n hàng + status filter
?       ??? OrderDetailPage.tsx       ? Chi ti?t + actions + payment history
??? components/
?   ??? orders/
?       ??? OrderCard.tsx             ? Card hi?n th? trong danh sách
?       ??? OrderActions.tsx          ? Nút hành ??ng theo status
?       ??? OrderStatusBadge.tsx      ? Badge tr?ng thái
?       ??? PaymentHistory.tsx        ? B?ng l?ch s? giao d?ch
??? services/
?   ??? orderService.ts              ? API calls
??? types/
    ??? order.ts                      ? TypeScript interfaces
```

---

## ?? Payment Flow Diagram

```
             ???????????????????????????????????????????????????????
             ?                    Order Lifecycle                   ?
             ???????????????????????????????????????????????????????

  ?????????     Place       ???????????    Pay 20%     ???????????
  ?Listing? ??????????????? ? Pending ? ?????????????? ?  Paid   ?
  ?Active ?     Order       ?  (1)    ?    Deposit      ?  (2)    ?
  ?????????                 ???????????                 ???????????
                                 ?                           ?
                              Cancel                     Pay 80%
                                 ?                       or Cancel
                                 ?                           ?
                           ????????????                      ?
                           ?Cancelled ?              ????????????
                           ?   (5)    ?              ? Shipping ?
                           ????????????              ?   (3)    ?
                             Listing                 ????????????
                             ? Active                     ?
                                                   Confirm
                                                   Delivery
                                                          ?
                                                          ?
                                                   ????????????
                                                   ?Completed ?
                                                   ?   (4)    ?
                                                   ????????????
```

### Thanh toán 2 b??c

```
????????????  20%   ????????????  80%   ????????????
?  Ch?a    ? ?????  ?  ?ã ??t  ? ?????  ? ?ã thanh ?
?  tr?     ? VNPay  ?   c?c    ? VNPay  ?  toán    ?
?          ?        ?          ?        ?  ??      ?
????????????        ????????????        ????????????
 Pending (1)         Paid (2)           Shipping (3)
```

---

## ? FE Checklist

### Pages c?n t?o
- [ ] `MyOrdersPage` — danh sách ??n hàng, filter theo status, hi?n th? s? ti?n, status badge
- [ ] `OrderDetailPage` — chi ti?t ??n hàng, payment history, action buttons

### Components c?n t?o
- [ ] `OrderCard` — card hi?n th?: thumbnail xe, tên xe, giá, tr?ng thái, ngày ??t
- [ ] `OrderActions` — nút actions theo status (??t c?c / thanh toán / hu? / xác nh?n)
- [ ] `OrderStatusBadge` — badge màu theo tr?ng thái
- [ ] `PaymentHistory` — b?ng danh sách giao d?ch ?ã thanh toán

### Logic quan tr?ng
- [ ] ??t hàng ? `POST /api/orders` ? redirect ??n order detail
- [ ] ??t c?c ? `POST /api/orders/create-payment-url` (type=deposit) ? `window.location.href = paymentUrl`
- [ ] Thanh toán 80% ? `POST /api/orders/create-payment-url` (type=full) ? redirect VNPay
- [ ] X? lý VNPay return ? ??c query `?payment=success` ho?c `?payment=failed&error=...`
- [ ] Hu? ??n ? confirm dialog ? `POST /api/orders/{id}/cancel`
- [ ] Xác nh?n nh?n hàng ? confirm dialog ? `POST /api/orders/{id}/confirm-delivery`
- [ ] Hi?n th? s? ti?n format VND: `Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })`
- [ ] Disable action buttons khi ?ang loading (tránh double-click)

### Tránh l?i ph? bi?n
- [ ] ? Dùng `navigate()` ?? redirect VNPay ? ph?i dùng `window.location.href` (cross-domain)
- [ ] ? Quên x? lý query params `?payment=success` sau VNPay return
- [ ] ? G?i API thanh toán full khi ch?a ??t c?c ? BE tr? `"Deposit must be paid first"`
- [ ] ? Cho phép hu? ??n ?ang ship ? BE tr? `"Cannot cancel this order"`
- [ ] ? Không hi?n th? `depositAmount` và `remainingAmount` cho user ? gây nh?m l?n
