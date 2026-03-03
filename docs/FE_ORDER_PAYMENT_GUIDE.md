# ?? FE Guide: Order & VNPay Payment — H??ng d?n tri?n khai ??y ??

> Qu?n lý ??t hàng **nhi?u s?n ph?m**, ??t c?c 20%, thanh toán qua VNPay sandbox.
>
> **BE Base URL:** `http://localhost:5157`
>
> **FE Base URL:** `http://localhost:5174` (BE redirect v? ?ây sau VNPay)
>
> **All endpoints prefix:** `/api/orders`

---

## ?? THAY ??I QUAN TR?NG (v2 — Multi-item Order + OrderDetail)

> **DB schema ?ã thay ??i:**
> - Quan h? `Order ? BicycleListing` là **nhi?u-nhi?u** qua b?ng `OrderDetail`.
> - M?i ??n hàng có th? ch?a **nhi?u listing** v?i **s? l??ng khác nhau**.
> - `BicycleListing` có field `Quantity` — BE t? tr? kho khi ??t hàng, c?ng l?i khi hu?.
> - Order status thêm `6` (Refunded) cho tr??ng h?p admin gi?i quy?t tranh ch?p.
>
> **?nh h??ng ??n FE:**
> - `CreateOrderDto` gi? nh?n `items: [{ listingId, quantity }]` thay vì `{ listingId }`.
> - `OrderDto` response gi? có `items: OrderDetailDto[]` thay vì 1 `bikeTitle` + `sellerName`.
> - M?i item trong order có `quantity`, `unitPrice`, `subtotal` (computed).
> - C? buyer VÀ seller ??u xem ???c order (seller qua OrderDetail).

---

## ?? Payment Flow chi ti?t — FE c?n hi?u rõ

### Toàn b? lifecycle t? ??t hàng ??n hoàn thành

```
 ?????????????????????????????????????????????????????????????????????????????
 ?                     PAYMENT FLOW — STEP BY STEP                          ?
 ?????????????????????????????????????????????????????????????????????????????

 B??C 1: ??T HÀNG
 ?????????????????
   FE g?i:  POST /api/orders
   Body:    { "items": [{ "listingId": 1, "quantity": 1 }, ...] }
   
   BE t?o:  Order (status=1 Pending)
            OrderDetail[] (1 row cho m?i listing)
            Deposit (amount = 20% × totalAmount, status=1 Pending)
   
   BE tr?:  OrderDto (ch?a orderId, totalAmount, depositAmount, items[], ...)
   
   ? FE nh?n OrderDto, hi?n th? trang chi ti?t ??n hàng
   ? FE hi?n nút "??t c?c {depositAmount}"

 B??C 2: T?O LINK ??T C?C (20%)
 ????????????????????????????????
   FE g?i:  POST /api/orders/create-payment-url
   Body:    { "orderId": 1, "paymentType": "deposit" }
   
   BE ki?m tra:  order.OrderStatus == 1 (Pending)
                 deposit.Status == 1 (Pending)
   
   BE t?o URL VNPay:
     - txnRef = "{orderId}-{yyyyMMddHHmmss}"    ? VD: "1-20250625100530"
     - amount = deposit.Amount (20% × total)
     - returnUrl = "http://localhost:5157/api/orders/vnpay-return"
     - Hash HMAC-SHA512 toàn b? params
   
   BE tr?:  { paymentUrl: "https://sandbox.vnpayment.vn/...?vnp_Amount=...&..." }
   
   ? FE dùng window.location.href = paymentUrl
   ? User r?i FE, vào trang VNPay

 B??C 3: USER THANH TOÁN TRÊN VNPAY
 ????????????????????????????????????
   User nh?p thông tin th? test trên trang sandbox VNPay.
   
   Sau khi xong, VNPay redirect browser v?:
     http://localhost:5157/api/orders/vnpay-return?vnp_ResponseCode=00&vnp_TxnRef=1-20250625100530&vnp_Amount=110000000&...

 B??C 4: BE X? LÝ VNPAY CALLBACK
 ?????????????????????????????????
   BE endpoint: GET /api/orders/vnpay-return (AllowAnonymous — vì user không có JWT lúc này)
   
   BE th?c hi?n:
   ? Validate ch? ký HMAC-SHA512 (ch?ng gi? m?o)
   ? Parse vnp_TxnRef ? l?y orderId (ph?n tr??c d?u "-")
   ? Ki?m tra idempotent: n?u payment v?i txnRef ?ã t?n t?i ? skip
   ? Ki?m tra vnp_ResponseCode == "00" (thành công)
   ? T?o Payment record trong DB
   ? C?p nh?t tr?ng thái:
      - N?u order.Status == 1 (Pending): ? deposit.Status = 2 (Confirmed), order.Status = 2 (Paid)
      - N?u order.Status == 2 (Paid) & totalPaid >= totalAmount: ? order.Status = 3 (Shipping)
   ? Redirect browser v? FE:
      ? Thành công: http://localhost:5174/orders/{orderId}?payment=success
      ? Th?t b?i:   http://localhost:5174/orders?payment=failed&error={message}
   
   ? FE nh?n redirect, hi?n th? thông báo, reload order detail

 B??C 5: THANH TOÁN PH?N CÒN L?I (80%)
 ????????????????????????????????????????
   FE g?i:  POST /api/orders/create-payment-url
   Body:    { "orderId": 1, "paymentType": "full" }
   
   BE ki?m tra: order.OrderStatus == 2 (Deposit ?ã tr?)
   BE tính:     amount = totalAmount - ?(payments ?ã tr?)
   
   ? L?p l?i b??c 3-4, nh?ng l?n này order.Status ? 3 (Shipping)

 B??C 6: XÁC NH?N NH?N HÀNG
 ????????????????????????????
   FE g?i:  POST /api/orders/{id}/confirm-delivery
   
   BE ki?m tra: order.OrderStatus == 3
   BE c?p nh?t:  order.OrderStatus = 4 (Completed)
```

### S? ?? tr?ng thái

```
  ????????????  ??t c?c 20%   ????????????  Thanh toán 80%  ????????????
  ? Pending  ? ??????????????? ?   Paid   ? ???????????????? ? Shipping ?
  ?   (1)    ?     VNPay       ?   (2)    ?      VNPay       ?   (3)    ?
  ????????????                 ????????????                  ????????????
       ?                            ?                              ?
    Hu? ??n                      Hu? ??n                    Xác nh?n
       ?                            ?                        nh?n hàng
       ?                            ?                              ?
  ????????????                 ????????????                        ?
  ?Cancelled ?                 ?Cancelled ?                  ????????????
  ?   (5)    ?                 ?   (5)    ?                  ?Completed ?
  ????????????                 ????????????                  ?   (4)    ?
  (qty c?ng l?i)               (qty c?ng l?i)               ????????????
                                                                   
                                                             ????????????
                                                             ? Refunded ?
                                                             ?   (6)    ?
                                                             ????????????
                                                             (Admin only)
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
| `GET` | `/api/orders/vnpay-return` | ? | Query params (VNPay) | **302 Redirect ? FE** |
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
  "items": [
    { "listingId": 1, "quantity": 1 },
    { "listingId": 5, "quantity": 2 }
  ]
}
```

| Field | Type | Required | Mô t? |
|---|---|---|---|
| `items` | array | ? | Danh sách s?n ph?m ??t mua (? 1 item) |
| `items[].listingId` | number | ? | ID listing |
| `items[].quantity` | number | ? | S? l??ng (> 0, ? listing.quantity) |

### BE x? lý khi ??t hàng

```
? Validate t?ng item:
   - listing t?n t?i? ? "Listing {id} not found"
   - listing.status == 1 (Active)? ? "Listing '{title}' is not available"
   - buyer ? seller? ? "You cannot buy your own listing"
   - quantity ? listing.quantity? ? "Not enough stock for '{title}'. Available: {n}"

? Tính t?ng:
   totalAmount = ?(listing.price × item.quantity)
   depositAmount = Math.Round(totalAmount × 0.20, 0)   ? làm tròn, b? th?p phân

? T?o records:
   - Order: { buyerId, totalAmount, orderStatus=1, orderDate=now }
   - OrderDetail[]: 1 row cho m?i item { orderId, listingId, quantity, unitPrice }
   - Deposit: { orderId, amount=depositAmount, status=1, depositDate=now }

? Tr? kho:
   listing.quantity -= item.quantity
   N?u listing.quantity <= 0 ? listing.listingStatus = 3 (Sold)
```

### Response `200 OK` — `OrderDto`

```json
{
  "orderId": 1,
  "orderStatus": 1,
  "totalAmount": 16500000,
  "depositAmount": 3300000,
  "remainingAmount": 16500000,
  "depositStatus": 1,
  "orderDate": "2025-06-25T10:00:00Z",
  "buyerName": "john_doe",
  "items": [
    {
      "orderDetailId": 1,
      "listingId": 1,
      "bikeTitle": "Xe ??p Giant Escape 3",
      "bikeImageUrl": "https://res.cloudinary.com/.../abc123.jpg",
      "sellerName": "seller_01",
      "quantity": 1,
      "unitPrice": 5500000,
      "subtotal": 5500000
    },
    {
      "orderDetailId": 2,
      "listingId": 5,
      "bikeTitle": "Xe ??p Trek FX 3",
      "bikeImageUrl": "https://res.cloudinary.com/.../def456.jpg",
      "sellerName": "seller_02",
      "quantity": 2,
      "unitPrice": 5500000,
      "subtotal": 11000000
    }
  ],
  "payments": []
}
```

### Response field gi?i thích

| Field | Mô t? | Cách tính |
|---|---|---|
| `totalAmount` | T?ng giá tr? ??n hàng | `?(unitPrice × quantity)` cho m?i item |
| `depositAmount` | Ti?n c?c ph?i tr? | `Math.Round(totalAmount × 0.20)` |
| `remainingAmount` | Ti?n còn ph?i tr? | `totalAmount ? ?(payments[].amount)` |
| `depositStatus` | Tr?ng thái c?c | `1`=Pending, `2`=Confirmed, `3`=Cancelled |
| `items` | Danh sách s?n ph?m | M?i item có `quantity`, `unitPrice` |
| `items[].subtotal` | Thành ti?n m?i item | `unitPrice × quantity` (computed, không l?u DB) |
| `payments` | L?ch s? thanh toán | Ban ??u r?ng `[]`, thêm record m?i l?n tr? VNPay |

### Response — Error `400`

| Error | Nguyên nhân |
|---|---|
| `"At least one item is required"` | M?ng items r?ng ho?c null |
| `"Quantity must be greater than 0 for listing {id}"` | Quantity ? 0 |
| `"Listing {id} not found"` | Listing không t?n t?i |
| `"Listing '{title}' is not available"` | Listing không active (?ã ?n/bán/ch? duy?t) |
| `"You cannot buy your own listing"` | Buyer = Seller |
| `"Not enough stock for '{title}'. Available: {n}"` | Quantity v??t quá t?n kho |

### FE Implementation

```tsx
// === ??t t? gi? hàng (nhi?u items) ===
const handleCheckout = async (cartItems: CartItem[]) => {
  if (cartItems.length === 0) return setError('Gi? hàng tr?ng');

  const confirmed = window.confirm(
    `B?n s? ??t ${cartItems.length} s?n ph?m.\n` +
    `Sau khi ??t, b?n c?n ??t c?c 20% qua VNPay ?? xác nh?n.`
  );
  if (!confirmed) return;

  try {
    setLoading(true);
    const res = await api.post<OrderDto>('/api/orders', {
      items: cartItems.map(c => ({
        listingId: c.listingId,
        quantity: c.quantity,
      }}),
    });
    const order = res.data;
    // Chuy?n ??n trang chi ti?t ??n hàng ? user b?m "??t c?c" ? ?ó
    navigate(`/orders/${order.orderId}`);
  } catch (err: any) {
    setError(err.response?.data?.error || '??t hàng th?t b?i');
  } finally {
    setLoading(false);
  }
};

// === Mua ngay 1 s?n ph?m (t? trang chi ti?t listing) ===
const handleBuyNow = async (listingId: number, quantity: number = 1) => {
  try {
    setLoading(true);
    const res = await api.post<OrderDto>('/api/orders', {
      items: [{ listingId, quantity }],
    });
    navigate(`/orders/${res.data.orderId}`);
  } catch (err: any) {
    setError(err.response?.data?.error || '??t hàng th?t b?i');
  } finally {
    setLoading(false);
  }
};
```

---

## ?? 2. T?o link thanh toán VNPay — `POST /api/orders/create-payment-url` (?? Auth)

> **?ây là b??c quan tr?ng nh?t — FE t?o link r?i REDIRECT user sang VNPay.**

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

| Field | Type | Required | Giá tr? | Mô t? |
|---|---|---|---|---|
| `orderId` | number | ? | | ID ??n hàng |
| `paymentType` | string | ? | `"deposit"` ho?c `"full"` | Lo?i thanh toán |

### Lu?t paymentType

| `paymentType` | ?i?u ki?n | S? ti?n | Mô t? |
|---|---|---|---|
| `"deposit"` | `orderStatus == 1` (Pending) | `depositAmount` (20%) | ??t c?c l?n ??u |
| `"full"` | `orderStatus == 2` (Deposit Paid) | `totalAmount - ?payments` (80%) | Thanh toán ph?n còn l?i |

### Response `200 OK` — `PaymentUrlResultDto`

```json
{
  "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Amount=330000000&vnp_Command=pay&vnp_CreateDate=20250625170530&vnp_CurrCode=VND&vnp_IpAddr=127.0.0.1&vnp_Locale=vn&vnp_OrderInfo=SecondBike+-+Dat+coc+don+hang+%231&vnp_OrderType=other&vnp_ReturnUrl=http%3A%2F%2Flocalhost%3A5157%2Fapi%2Forders%2Fvnpay-return&vnp_TmnCode=CGXZLS0Z&vnp_TxnRef=1-20250625100530&vnp_Version=2.1.0&vnp_SecureHash=abc123...",
  "orderId": 1,
  "amount": 3300000,
  "paymentType": "deposit"
}
```

| Field | Mô t? |
|---|---|
| `paymentUrl` | URL ??y ?? — FE redirect user ??n ?ây |
| `amount` | S? ti?n s? thanh toán (VND, ch?a ×100) |
| `paymentType` | `"deposit"` ho?c `"full"` |

### Gi?i thích VNPay URL

```
https://sandbox.vnpayment.vn/paymentv2/vpcpay.html
  ?vnp_Amount=330000000          ? 3,300,000 × 100 (VNPay quy ??c)
  &vnp_Command=pay
  &vnp_CreateDate=20250625170530 ? UTC+7
  &vnp_CurrCode=VND
  &vnp_IpAddr=127.0.0.1         ? IP c?a user (BE t? l?y)
  &vnp_Locale=vn                ? Giao di?n ti?ng Vi?t
  &vnp_OrderInfo=SecondBike+-+Dat+coc+don+hang+%231
  &vnp_OrderType=other
  &vnp_ReturnUrl=http://localhost:5157/api/orders/vnpay-return   ? BE endpoint
  &vnp_TmnCode=CGXZLS0Z         ? Mã website trên VNPay
  &vnp_TxnRef=1-20250625100530  ? orderId + timestamp (unique m?i l?n)
  &vnp_Version=2.1.0
  &vnp_SecureHash=abc123...      ? HMAC-SHA512 signature
```

### Response — Error `400`

| Error | Nguyên nhân | FE nên hi?n th? |
|---|---|---|
| `"Order not found"` | orderId không t?n t?i | "??n hàng không t?n t?i" |
| `"Access denied"` | Không ph?i buyer c?a order | "B?n không có quy?n" |
| `"Order is not in pending status for deposit payment"` | G?i deposit khi order ? Pending | "??n hàng không ? tr?ng thái ch? c?c" |
| `"No pending deposit found"` | Deposit ?ã confirmed/cancelled | "Không tìm th?y kho?n c?c" |
| `"Invalid deposit amount"` | deposit.amount ? 0 | "S? ti?n c?c không h?p l?" |
| `"Deposit must be paid first"` | G?i full khi ch?a c?c | "B?n c?n ??t c?c tr??c" |
| `"Order is already fully paid"` | ?ã thanh toán ?? | "??n hàng ?ã thanh toán ??" |
| `"PaymentType must be 'deposit' or 'full'"` | paymentType sai | "Lo?i thanh toán không h?p l?" |

### FE Implementation — Redirect to VNPay

```tsx
/**
 * T?o link VNPay và redirect user.
 * ?? PH?I dùng window.location.href — KHÔNG dùng navigate()
 *     vì c?n redirect sang domain khác (vnpayment.vn)
 */
const handlePayDeposit = async (orderId: number) => {
  try {
    setLoading(true);
    const res = await api.post<PaymentUrlResultDto>('/api/orders/create-payment-url', {
      orderId,
      paymentType: 'deposit',
    });

    // ?? CRITICAL: dùng window.location.href, KHÔNG dùng navigate()
    window.location.href = res.data.paymentUrl;
    // Lúc này user r?i kh?i FE app ? vào trang VNPay
    // Sau khi thanh toán xong, VNPay redirect v? BE ? BE redirect v? FE
  } catch (err: any) {
    setError(err.response?.data?.error || 'Không th? t?o link thanh toán');
    setLoading(false); // Ch? setLoading(false) khi l?i, vì thành công thì user ?ã r?i page
  }
};

const handlePayRemaining = async (orderId: number) => {
  try {
    setLoading(true);
    const res = await api.post<PaymentUrlResultDto>('/api/orders/create-payment-url', {
      orderId,
      paymentType: 'full',
    });
    window.location.href = res.data.paymentUrl;
  } catch (err: any) {
    setError(err.response?.data?.error || 'Không th? t?o link thanh toán');
    setLoading(false);
  }
};
```

---

## ?? 3. VNPay Return — BE x? lý callback & redirect v? FE

### Flow chi ti?t (FE không g?i API này — nh?ng c?n hi?u ?? x? lý redirect)

```
                    User thanh toán xong trên VNPay
                                ?
                                ?
  VNPay redirect browser ? GET http://localhost:5157/api/orders/vnpay-return
                           ?vnp_ResponseCode=00
                           &vnp_TxnRef=1-20250625100530
                           &vnp_Amount=330000000
                           &vnp_SecureHash=abc123...
                           &... (nhi?u params khác)
                                ?
                                ?
                    BE nh?n request (AllowAnonymous)
                    ? Validate HMAC-SHA512 signature
                    ? Parse txnRef ? orderId = 1
                    ? Check idempotent (txnRef ?ã x? lý ch?a?)
                    ? Check vnp_ResponseCode == "00"
                    ? T?o Payment record
                    ? C?p nh?t Order/Deposit status
                                ?
                    ?????????????????????????
                    ?                       ?
            Thành công                  Th?t b?i
                    ?                       ?
                    ?                       ?
  302 Redirect ?                 302 Redirect ?
  http://localhost:5174           http://localhost:5174
  /orders/1?payment=success      /orders?payment=failed
                                 &error=Payment+failed+...
```

### VNPay Response Codes (quan tr?ng)

| vnp_ResponseCode | Ý ngh?a | BE x? lý |
|---|---|---|
| `"00"` | Giao d?ch thành công | T?o Payment, c?p nh?t status |
| `"07"` | Tr? ti?n thành công nh?ng nghi ng? gian l?n | BE reject |
| `"09"` | Th?/Tài kho?n ch?a ??ng ký Internet Banking | BE reject |
| `"10"` | Xác th?c > 3 l?n | BE reject |
| `"11"` | H?t h?n ch? thanh toán | BE reject |
| `"12"` | Th?/Tài kho?n b? khoá | BE reject |
| `"24"` | Khách hàng hu? giao d?ch | BE reject |
| `"51"` | Không ?? s? d? | BE reject |
| `"65"` | V??t h?n m?c giao d?ch trong ngày | BE reject |
| `"75"` | Ngân hàng b?o trì | BE reject |
| `"99"` | L?i không xác ??nh | BE reject |

> BE ch? ghi nh?n thanh toán khi `vnp_ResponseCode == "00"`. T?t c? code khác ??u reject.

### FE Implementation — X? lý redirect t? VNPay

```tsx
// Trong OrderDetailPage ho?c OrdersPage
import { useSearchParams, useParams } from 'react-router-dom';

const OrderDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const [searchParams, setSearchParams] = useSearchParams();
  const [order, setOrder] = useState<OrderDto | null>(null);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  // ? X? lý query params sau khi VNPay redirect v?
  useEffect(() => {
    const paymentStatus = searchParams.get('payment');
    
    if (paymentStatus === 'success') {
      setMessage('?? Thanh toán thành công! ??n hàng ?ã ???c c?p nh?t.');
      // Xoá query params kh?i URL (clean URL)
      searchParams.delete('payment');
      setSearchParams(searchParams, { replace: true });
    } else if (paymentStatus === 'failed') {
      const errorMsg = searchParams.get('error') || 'Thanh toán th?t b?i';
      setError(`? ${decodeURIComponent(errorMsg)}`);
      searchParams.delete('payment');
      searchParams.delete('error');
      setSearchParams(searchParams, { replace: true });
    }
  }, []);

  // ? Load order detail
  const loadOrder = async () => {
    try {
      const res = await api.get<OrderDto>(`/api/orders/${id}`);
      setOrder(res.data);
    } catch (err: any) {
      if (err.response?.data?.error === 'Access denied') {
        navigate('/403');
      } else {
        setError(err.response?.data?.error || 'Không th? t?i ??n hàng');
      }
    }
  };

  useEffect(() => { if (id) loadOrder(); }, [id]);

  // ...render
};
```

### ?? L?U Ý QUAN TR?NG V? VNPAY RETURN

1. **`vnpay-return` là `AllowAnonymous`** — vì lúc VNPay redirect v?, browser không có JWT token trong URL. BE không yêu c?u auth cho endpoint này.

2. **BE redirect 302 v? FE** — không tr? JSON. FE nh?n redirect t? ??ng qua browser.

3. **FE Base URL ???c c?u hình trong `appsettings.json`:**
   ```json
   { "App": { "BaseUrl": "http://localhost:5174" } }
   ```
   N?u FE ch?y port khác, c?n báo BE s?a config này.

4. **`vnpay-ipn` là server-to-server** — VNPay g?i tr?c ti?p ??n BE, FE không liên quan. ?ây là c? ch? backup ??m b?o BE luôn nh?n ???c k?t qu? thanh toán (phòng tr??ng h?p user ?óng browser tr??c khi redirect).

---

## ?? 4. Xem ??n hàng — `GET /api/orders/{id}` (?? Auth)

### Ai ???c xem?

- **Buyer**: luôn xem ???c order c?a mình
- **Seller**: xem ???c n?u b?t k? `OrderDetail` nào có listing thu?c seller ?ó

### Response `200 OK` — `OrderDto` (c?u trúc gi?ng response PlaceOrder, nh?ng có payments)

```json
{
  "orderId": 1,
  "orderStatus": 2,
  "totalAmount": 16500000,
  "depositAmount": 3300000,
  "remainingAmount": 13200000,
  "depositStatus": 2,
  "orderDate": "2025-06-25T10:00:00Z",
  "buyerName": "john_doe",
  "items": [
    {
      "orderDetailId": 1,
      "listingId": 1,
      "bikeTitle": "Xe ??p Giant Escape 3",
      "bikeImageUrl": "https://res.cloudinary.com/.../abc123.jpg",
      "sellerName": "seller_01",
      "quantity": 1,
      "unitPrice": 5500000,
      "subtotal": 5500000
    }
  ],
  "payments": [
    {
      "paymentId": 1,
      "amount": 3300000,
      "paymentMethod": "VNPay",
      "transactionRef": "1-20250625100530",
      "paymentDate": "2025-06-25T10:05:30Z"
    }
  ]
}
```

### FE Implementation

```tsx
const loadOrder = async () => {
  try {
    const res = await api.get<OrderDto>(`/api/orders/${orderId}`);
    setOrder(res.data);
  } catch (err: any) {
    if (err.response?.data?.error === 'Access denied') {
      navigate('/403');
    } else {
      setError('Không th? t?i ??n hàng');
    }
  }
};
```

---

## ?? 5. ??n hàng c?a tôi — `GET /api/orders/my-purchases` (?? Auth)

### Response `200 OK` — `OrderDto[]` (s?p x?p m?i nh?t tr??c)

### FE Implementation

```tsx
const [orders, setOrders] = useState<OrderDto[]>([]);

const loadMyOrders = async () => {
  try {
    const res = await api.get<OrderDto[]>('/api/orders/my-purchases');
    setOrders(res.data);
  } catch (err) {
    setError('Không th? t?i danh sách ??n hàng');
  }
};
```

---

## ? 6. Hu? ??n — `POST /api/orders/{id}/cancel` (?? Auth)

### ?i?u ki?n

- Ch? **buyer** m?i hu? ???c
- Ch? hu? khi `orderStatus` là `1` (Pending) ho?c `2` (Deposit Paid)
- **KHÔNG** hu? ???c khi: `3` (Shipping), `4` (Completed), `5` (Cancelled)

### BE x? lý khi hu?

```
? order.orderStatus = 5 (Cancelled)
? V?i m?i OrderDetail:
   listing.quantity += detail.quantity     ? c?ng l?i kho
   N?u listing.status == 3 (Sold) && quantity > 0 ? listing.status = 1 (Active)
? Hu? deposit pending:
   deposit.status = 3 (Cancelled)
```

> ?? **Ti?n c?c ?ã tr? KHÔNG t? ??ng hoàn.** N?u c?n hoàn, admin ph?i x? lý riêng.

### Response — Error `400`

| Error | Nguyên nhân |
|---|---|
| `"Order not found"` | ??n hàng không t?n t?i |
| `"Access denied"` | Không ph?i buyer |
| `"Cannot cancel this order"` | Order ?ang ship/completed/?ã hu? |

### FE Implementation

```tsx
const handleCancel = async (orderId: number) => {
  const confirmed = window.confirm(
    'B?n có ch?c mu?n hu? ??n hàng?\n' +
    '?? N?u ?ã ??t c?c, ti?n c?c có th? không ???c hoàn t? ??ng.'
  );
  if (!confirmed) return;

  try {
    await api.post(`/api/orders/${orderId}/cancel`);
    loadOrder(); // Reload ?? c?p nh?t UI
    setMessage('??n hàng ?ã ???c hu?');
  } catch (err: any) {
    setError(err.response?.data?.error || 'Hu? ??n th?t b?i');
  }
};
```

---

## ? 7. Xác nh?n nh?n hàng — `POST /api/orders/{id}/confirm-delivery` (?? Auth)

### ?i?u ki?n

- Ch? **buyer** m?i xác nh?n ???c
- Ch? xác nh?n khi `orderStatus == 3` (Shipping)

### FE Implementation

```tsx
const handleConfirmDelivery = async (orderId: number) => {
  const confirmed = window.confirm(
    'Xác nh?n ?ã nh?n hàng?\nSau khi xác nh?n, ??n hàng s? hoàn thành.'
  );
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
| `1` | Pending | ?ã ??t, ch?a c?c | ?? ??t c?c, ? Hu? |
| `2` | Paid (Deposit) | ?ã c?c 20% | ?? Thanh toán 80%, ? Hu? |
| `3` | Shipping | ?ã thanh toán ?? | ? Xác nh?n nh?n hàng |
| `4` | Completed | Hoàn thành | — (có th? ?ánh giá seller) |
| `5` | Cancelled | ?ã hu? | — |
| `6` | Refunded | Admin hoàn ti?n | — |

### Deposit Status

| Value | Label | Mô t? |
|---|---|---|
| `1` | Pending | Ch? thanh toán c?c |
| `2` | Confirmed | ?ã xác nh?n c?c (VNPay thành công) |
| `3` | Cancelled | ?ã hu? (do hu? ??n) |

---

## ?? FE Components Implementation

### OrderStatusBadge

```tsx
const ORDER_STATUS: Record<number, { label: string; color: string }> = {
  1: { label: 'Ch? ??t c?c', color: 'orange' },
  2: { label: '?ã ??t c?c', color: 'blue' },
  3: { label: '?ang giao hàng', color: 'purple' },
  4: { label: 'Hoàn thành', color: 'green' },
  5: { label: '?ã hu?', color: 'red' },
  6: { label: '?ã hoàn ti?n', color: 'gray' },
};

const OrderStatusBadge = ({ status }: { status?: number }) => {
  const info = ORDER_STATUS[status ?? 1] || ORDER_STATUS[1];
  return <span style={{ color: info.color, fontWeight: 'bold' }}>{info.label}</span>;
};
```

### OrderActions — Nút hành ??ng theo tr?ng thái

```tsx
const OrderActions = ({ order, onReload }: { order: OrderDto; onReload: () => void }) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handlePayDeposit = async () => {
    try {
      setLoading(true);
      const res = await api.post<PaymentUrlResultDto>('/api/orders/create-payment-url', {
        orderId: order.orderId,
        paymentType: 'deposit',
      });
      window.location.href = res.data.paymentUrl; // Redirect sang VNPay
    } catch (err: any) {
      setError(err.response?.data?.error || 'L?i');
      setLoading(false);
    }
  };

  const handlePayFull = async () => {
    try {
      setLoading(true);
      const res = await api.post<PaymentUrlResultDto>('/api/orders/create-payment-url', {
        orderId: order.orderId,
        paymentType: 'full',
      });
      window.location.href = res.data.paymentUrl;
    } catch (err: any) {
      setError(err.response?.data?.error || 'L?i');
      setLoading(false);
    }
  };

  const handleCancel = async () => {
    if (!window.confirm('Hu? ??n hàng?')) return;
    try {
      await api.post(`/api/orders/${order.orderId}/cancel`);
      onReload();
    } catch (err: any) {
      setError(err.response?.data?.error || 'L?i');
    }
  };

  const handleConfirmDelivery = async () => {
    if (!window.confirm('Xác nh?n ?ã nh?n hàng?')) return;
    try {
      await api.post(`/api/orders/${order.orderId}/confirm-delivery`);
      onReload();
    } catch (err: any) {
      setError(err.response?.data?.error || 'L?i');
    }
  };

  return (
    <div className="order-actions">
      {error && <p className="error">{error}</p>}

      {/* Pending ? ??t c?c + Hu? */}
      {order.orderStatus === 1 && (
        <>
          <button onClick={handlePayDeposit} disabled={loading} className="btn-primary">
            ?? ??t c?c {formatVND(order.depositAmount)}
          </button>
          <button onClick={handleCancel} disabled={loading} className="btn-danger">
            Hu? ??n
          </button>
        </>
      )}

      {/* Deposit Paid ? Thanh toán 80% + Hu? */}
      {order.orderStatus === 2 && (
        <>
          <button onClick={handlePayFull} disabled={loading} className="btn-primary">
            ?? Thanh toán {formatVND(order.remainingAmount)}
          </button>
          <button onClick={handleCancel} disabled={loading} className="btn-danger">
            Hu? ??n
          </button>
        </>
      )}

      {/* Shipping ? Xác nh?n nh?n hàng */}
      {order.orderStatus === 3 && (
        <button onClick={handleConfirmDelivery} disabled={loading} className="btn-success">
          ? Xác nh?n nh?n hàng
        </button>
      )}

      {/* Completed, Cancelled, Refunded ? Không có action */}
    </div>
  );
};

const formatVND = (amount?: number) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount || 0);
```

### OrderItemsList — Hi?n th? s?n ph?m trong ??n

```tsx
const OrderItemsList = ({ items }: { items: OrderDetailDto[] }) => (
  <div className="order-items">
    <h3>S?n ph?m ({items.length})</h3>
    {items.map(item => (
      <div key={item.orderDetailId} className="order-item">
        <img
          src={item.bikeImageUrl || '/placeholder-bike.png'}
          alt={item.bikeTitle}
          className="item-image"
        />
        <div className="item-info">
          <h4>
            <a href={`/bikes/${item.listingId}`}>{item.bikeTitle}</a>
          </h4>
          <p className="seller">Ng??i bán: {item.sellerName}</p>
          <p className="price-detail">
            {formatVND(item.unitPrice)} × {item.quantity} ={' '}
            <strong>{formatVND(item.subtotal)}</strong>
          </p>
        </div>
      </div>
    ))}
  </div>
);
```

### PaymentHistory — L?ch s? giao d?ch

```tsx
const PaymentHistory = ({ payments }: { payments: PaymentDto[] }) => {
  if (payments.length === 0) return <p className="muted">Ch?a có giao d?ch nào</p>;

  return (
    <div className="payment-history">
      <h3>L?ch s? thanh toán</h3>
      <table>
        <thead>
          <tr>
            <th>Ngày</th>
            <th>Ph??ng th?c</th>
            <th>Mã giao d?ch</th>
            <th>S? ti?n</th>
          </tr>
        </thead>
        <tbody>
          {payments.map(p => (
            <tr key={p.paymentId}>
              <td>{new Date(p.paymentDate!).toLocaleString('vi-VN')}</td>
              <td>{p.paymentMethod}</td>
              <td><code>{p.transactionRef}</code></td>
              <td className="amount">{formatVND(p.amount)}</td>
            </tr>
          ))}
        </tbody>
      </table>
      <p className="total-paid">
        T?ng ?ã tr?: <strong>{formatVND(payments.reduce((sum, p) => sum + (p.amount || 0), 0))}</strong>
      </p>
    </div>
  );
};
```

### OrderSummary — T?ng h?p ti?n

```tsx
const OrderSummary = ({ order }: { order: OrderDto }) => {
  const totalPaid = order.payments.reduce((sum, p) => sum + (p.amount || 0), 0);

  return (
    <div className="order-summary">
      <div className="summary-row">
        <span>T?ng giá tr?:</span>
        <strong>{formatVND(order.totalAmount)}</strong>
      </div>
      <div className="summary-row">
        <span>Ti?n c?c (20%):</span>
        <span>{formatVND(order.depositAmount)}</span>
      </div>
      <div className="summary-row">
        <span>?ã thanh toán:</span>
        <span className="paid">{formatVND(totalPaid)}</span>
      </div>
      <div className="summary-row highlight">
        <span>Còn ph?i tr?:</span>
        <strong className={order.remainingAmount! > 0 ? 'unpaid' : 'fully-paid'}>
          {formatVND(order.remainingAmount)}
        </strong>
      </div>
    </div>
  );
};
```

---

## ?? VNPay Sandbox Test Cards

> ?? Ch? dùng cho môi tr??ng sandbox (test). Không ph?i th? th?t.

### Th? n?i ??a (Ngân hàng NCB)

| Field | Value |
|---|---|
| Ngân hàng | Ch?n `NCB` |
| S? th? | `9704198526191432198` |
| Tên ch? th? | `NGUYEN VAN A` |
| Ngày phát hành | `07/15` |
| M?t kh?u OTP | `123456` |

### Quy trình test thanh toán

```
1. T?o order     ? POST /api/orders
2. T?o link c?c  ? POST /api/orders/create-payment-url (type=deposit)
3. Redirect VNPay ? window.location.href = paymentUrl
4. Trên VNPay:
   - Ch?n ngân hàng: NCB
   - Nh?p s? th?: 9704198526191432198
   - Tên: NGUYEN VAN A
   - Ngày: 07/15
   - B?m "Ti?p t?c"
   - Nh?p OTP: 123456
   - B?m "Thanh toán"
5. VNPay redirect ? BE ? FE /orders/{id}?payment=success
6. Ki?m tra order status = 2 (Deposit Paid)
7. T?o link full  ? POST /api/orders/create-payment-url (type=full)
8. L?p l?i b??c 3-5
9. Ki?m tra order status = 3 (Shipping)
```

---

## ?? TypeScript Interfaces

```typescript
// ===== Request DTOs =====

interface CreateOrderDto {
  items: OrderItemDto[];          // ? 1 item
}

interface OrderItemDto {
  listingId: number;
  quantity: number;               // > 0, ? listing.quantity
}

interface CreatePaymentUrlDto {
  orderId: number;
  paymentType: 'deposit' | 'full';
}

// ===== Response DTOs =====

interface OrderDto {
  orderId: number;
  orderStatus?: number;           // 1-6 (xem b?ng trên)
  totalAmount?: number;           // ?(unitPrice × quantity)
  depositAmount?: number;         // 20% × totalAmount (làm tròn)
  remainingAmount?: number;       // totalAmount ? ?(payments)
  depositStatus?: number;         // 1=Pending, 2=Confirmed, 3=Cancelled
  orderDate?: string;             // ISO 8601
  buyerName: string;
  items: OrderDetailDto[];        // Danh sách s?n ph?m
  payments: PaymentDto[];         // L?ch s? thanh toán
}

interface OrderDetailDto {
  orderDetailId: number;
  listingId: number;
  bikeTitle: string;
  bikeImageUrl?: string;          // Thumbnail c?a listing
  sellerName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;               // Computed: unitPrice × quantity
}

interface PaymentDto {
  paymentId: number;
  amount?: number;                // S? ti?n ?ã tr? (VND)
  paymentMethod?: string;         // "VNPay"
  transactionRef?: string;        // VD: "1-20250625100530"
  paymentDate?: string;           // ISO 8601
}

interface PaymentUrlResultDto {
  paymentUrl: string;             // URL redirect ??n VNPay
  orderId: number;
  amount: number;                 // S? ti?n s? thanh toán
  paymentType: string;            // "deposit" | "full"
}
```

---

## ?? Axios Order Service

```typescript
import api from './api'; // axios instance v?i JWT interceptor

export const orderService = {
  /** ??t hàng nhi?u s?n ph?m */
  placeOrder: (items: OrderItemDto[]) =>
    api.post<OrderDto>('/api/orders', { items }),

  /** ??t nhanh 1 s?n ph?m (helper) */
  buyNow: (listingId: number, quantity: number = 1) =>
    api.post<OrderDto>('/api/orders', {
      items: [{ listingId, quantity }],
    }),

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

  /**
   * T?o URL thanh toán VNPay.
   * FE g?i xong ph?i dùng window.location.href = res.data.paymentUrl
   */
  createPaymentUrl: (orderId: number, paymentType: 'deposit' | 'full') =>
    api.post<PaymentUrlResultDto>('/api/orders/create-payment-url', {
      orderId,
      paymentType,
    }),
};
```

---

## ??? React Router

```tsx
{/* ??n hàng */}
<Route path="/orders" element={<ProtectedRoute><MyOrdersPage /></ProtectedRoute>} />
<Route path="/orders/:id" element={<ProtectedRoute><OrderDetailPage /></ProtectedRoute>} />
```

---

## ?? Suggested FE File Structure

```
src/
??? pages/
?   ??? orders/
?       ??? MyOrdersPage.tsx          ? Danh sách ??n hàng + filter theo status
?       ??? OrderDetailPage.tsx       ? Chi ti?t + actions + payment history + VNPay return
??? components/
?   ??? orders/
?       ??? OrderCard.tsx             ? Card hi?n th? trong danh sách
?       ??? OrderActions.tsx          ? Nút hành ??ng theo status
?       ??? OrderStatusBadge.tsx      ? Badge tr?ng thái
?       ??? OrderItemsList.tsx        ? Danh sách s?n ph?m trong ??n
?       ??? OrderSummary.tsx          ? T?ng h?p ti?n (total, paid, remaining)
?       ??? PaymentHistory.tsx        ? B?ng l?ch s? giao d?ch
??? services/
?   ??? orderService.ts              ? API calls
??? types/
    ??? order.ts                      ? TypeScript interfaces
```

---

## ? FE Checklist

### Implement
- [ ] `MyOrdersPage` — danh sách ??n, filter theo status, hi?n th? items, ti?n
- [ ] `OrderDetailPage` — chi ti?t, items list, payment history, action buttons
- [ ] `OrderDetailPage` — x? lý `?payment=success` / `?payment=failed` t? VNPay redirect
- [ ] `OrderActions` — nút ??t c?c / Thanh toán / Hu? / Xác nh?n theo `orderStatus`
- [ ] `PaymentHistory` — b?ng l?ch s? giao d?ch VNPay
- [ ] `OrderSummary` — hi?n th? total, deposit, paid, remaining
- [ ] `OrderItemsList` — danh sách s?n ph?m v?i quantity, unitPrice, subtotal

### Logic quan tr?ng
- [ ] ??t hàng g?i `{ items: [{ listingId, quantity }] }` — KHÔNG ph?i `{ listingId }`
- [ ] Redirect VNPay dùng `window.location.href` — **KHÔNG** dùng `navigate()`
- [ ] ??c query params `?payment=success/failed` sau khi VNPay redirect v?
- [ ] Xoá query params sau khi ?ã x? lý (clean URL)
- [ ] Disable buttons khi ?ang loading (tránh double-click ? double payment)
- [ ] `setLoading(false)` ch? khi **l?i** — vì thành công s? redirect kh?i page
- [ ] Format ti?n VND: `new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })`
- [ ] Hi?n th? `items[]` (KHÔNG ph?i 1 bikeTitle flat — c?u trúc c? ?ã b?)
- [ ] `subtotal` là computed (`unitPrice × quantity`) — BE ?ã tính s?n trong response

### Tránh l?i ph? bi?n
- [ ] ? Dùng `navigate()` ?? redirect VNPay ? trang tr?ng (cross-domain)
- [ ] ? Quên x? lý `?payment=success` ? user không bi?t thanh toán xong ch?a
- [ ] ? G?i `paymentType: "full"` khi ch?a c?c ? `"Deposit must be paid first"`
- [ ] ? Cho phép hu? ??n ?ang ship ? `"Cannot cancel this order"`
- [ ] ? Không disable button ? user click 2 l?n ? t?o 2 payment URL
- [ ] ? Dùng `bikeTitle`/`sellerName` flat (c?) ? ph?i dùng `items[]` (m?i)
- [ ] ? Quên hi?n th? quantity per item ? user không bi?t mua m?y chi?c
- [ ] ? Tính `remainingAmount` ? FE ? dùng luôn field t? BE response
