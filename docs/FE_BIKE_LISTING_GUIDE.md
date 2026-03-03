# ?? FE Guide: Bicycle Listing CRUD + Cloudinary Image Upload

> Qu?n lý ??ng bài, s?a bài, xoá bài, ?n/hi?n bài ??ng xe ??p.
> ?nh ???c upload tr?c ti?p qua BE lên **Cloudinary** (FE không t??ng tác Cloudinary).
>
> **Base URL:** `http://localhost:5xxx` (ki?m tra port BE)
>
> **All endpoints prefix:** `/api/bikes`

---

## ?? THAY ??I QUAN TR?NG (v2 — Many-to-Many Order + Quantity)

> **DB schema ?ã thay ??i:**
> - `BicycleListing` có thêm field **`Quantity`** (s? l??ng xe cùng lo?i, m?c ??nh `1`).
> - Quan h? `Order ? BicycleListing` chuy?n sang **nhi?u-nhi?u** qua b?ng `OrderDetail`.
>
> **?nh h??ng ??n FE Listing:**
> - Create/Update form c?n field **`Quantity`** (m?c ??nh `1`).
> - Response `BikePostDto` có thêm field **`quantity`**.
> - Listing ?ã có order **không th? xoá** — ch? ?n ???c (toggle visibility).
> - Listing h?t hàng (`quantity = 0`) t? ??ng chuy?n status `3` (Sold).
> - Validation: `Quantity` ph?i ? 1.

---

## ?? L?U Ý QUAN TR?NG — TRÁNH L?I TH??NG G?P

### 1. `multipart/form-data` — KHÔNG ph?i JSON

Endpoint **Create** và **Update** dùng `multipart/form-data` (vì có file upload), **KHÔNG** ph?i `application/json`.

```
? SAI:  Content-Type: application/json    ? body: { title: "..." }
? ?ÚNG: Content-Type: multipart/form-data ? body: FormData
```

### 2. FormData append — tên field ph?i kh?p chính xác v?i BE

| FE FormData key | BE Property | Sai s? b? |
|---|---|---|
| `Title` | `Title` | 400 — "Title is required" |
| `Quantity` | `Quantity` | M?c ??nh `1` n?u không g?i |
| `Images` | `Images` (List\<IFormFile\>) | Không nh?n file |
| `NewImages` | `NewImages` (khi Update) | Không nh?n file |
| `RemoveMediaIds` | `RemoveMediaIds` (khi Update) | Không xoá ?nh |

### 3. Append array trong FormData

```typescript
// ? ?ÚNG — m?i ph?n t? append riêng, CÙNG tên key
removeIds.forEach(id => formData.append('RemoveMediaIds', id.toString()));

// ? SAI — append c? array thành 1 string
formData.append('RemoveMediaIds', JSON.stringify(removeIds));
```

### 4. Không truy?n `Content-Type` header khi dùng FormData

```typescript
// ? ?ÚNG — ?? browser t? thêm boundary
await api.post('/api/bikes', formData);

// ? SAI — ghi ?è s? m?t boundary ? BE parse l?i
await api.post('/api/bikes', formData, {
  headers: { 'Content-Type': 'multipart/form-data' }
});
```

---

## ?? API Endpoint Summary

| Method | Endpoint | Auth | Body | Response |
|---|---|---|---|---|
| `GET` | `/api/bikes` | ? | Query params | `PagedResult<BikePostDto>` |
| `GET` | `/api/bikes/{id}` | ? | — | `BikePostDto` |
| `GET` | `/api/bikes/brands` | ? | — | `string[]` |
| `POST` | `/api/bikes` | ?? | `FormData` | `BikePostDto` |
| `PUT` | `/api/bikes` | ?? | `FormData` | `BikePostDto` |
| `DELETE` | `/api/bikes/{id}` | ?? | — | `{ message }` |
| `PATCH` | `/api/bikes/{id}/visibility` | ?? | — | `{ message }` |
| `GET` | `/api/bikes/my-posts` | ?? | — | `BikePostDto[]` |

---

## ?? 1. Tìm ki?m & L?c — `GET /api/bikes` (Public)

### Request

```http
GET /api/bikes?searchTerm=giant&brandId=1&typeId=2&condition=Used&minPrice=1000000&maxPrice=10000000&address=HCM&sortBy=price_asc&page=1&pageSize=12
```

### Query Parameters

| Param | Type | Default | Mô t? |
|---|---|---|---|
| `searchTerm` | string | — | Tìm trong title, description, modelName |
| `brandId` | number | — | L?c theo brand |
| `typeId` | number | — | L?c theo lo?i xe (Mountain, Road,...) |
| `condition` | string | — | `"New"`, `"Used"`, `"Like New"`,... |
| `minPrice` | number | — | Giá t?i thi?u |
| `maxPrice` | number | — | Giá t?i ?a |
| `address` | string | — | L?c theo khu v?c |
| `sortBy` | string | `"newest"` | `"newest"` \| `"oldest"` \| `"price_asc"` \| `"price_desc"` |
| `page` | number | `1` | Trang hi?n t?i |
| `pageSize` | number | `12` | S? item m?i trang |

### Response `200 OK`

```json
{
  "items": [
    {
      "listingId": 1,
      "title": "Xe ??p Giant Escape 3",
      "description": "Xe còn m?i 95%",
      "price": 5500000,
      "quantity": 2,
      "listingStatus": 1,
      "address": "Qu?n 1, TP.HCM",
      "postedDate": "2025-06-20T10:00:00Z",
      "bikeId": 10,
      "modelName": "Escape 3",
      "serialNumber": "GNT2024001",
      "color": "?en",
      "condition": "Used",
      "brandName": "Giant",
      "typeName": "City",
      "frameSize": "M",
      "frameMaterial": "Aluminum",
      "wheelSize": "700c",
      "brakeType": "V-Brake",
      "weight": 11.5,
      "transmission": "Shimano Altus 3x8",
      "sellerId": 5,
      "sellerName": "john_doe",
      "images": [
        {
          "mediaId": 1,
          "mediaUrl": "https://res.cloudinary.com/.../listings/abc123.jpg",
          "mediaType": "image",
          "isThumbnail": true
        }
      ],
      "hasInspection": false
    }
  ],
  "totalCount": 48,
  "page": 1,
  "pageSize": 12,
  "totalPages": 4,
  "hasPrevious": false,
  "hasNext": true
}
```

### FE Logic

```tsx
const [listings, setListings] = useState<BikePostDto[]>([]);
const [pagination, setPagination] = useState<PaginationInfo | null>(null);
const [filter, setFilter] = useState<BikeFilterDto>({ page: 1, pageSize: 12 });

const loadListings = async () => {
  try {
    const params = Object.fromEntries(
      Object.entries(filter).filter(([_, v]) => v != null && v !== '')
    );
    const res = await api.get('/api/bikes', { params });
    const data: PagedResult<BikePostDto> = res.data;

    setListings(data.items);
    setPagination({
      totalCount: data.totalCount,
      page: data.page,
      pageSize: data.pageSize,
      totalPages: data.totalPages,
      hasPrevious: data.hasPrevious,
      hasNext: data.hasNext,
    });
  } catch (err) {
    setError('Failed to load listings');
  }
};

useEffect(() => { loadListings(); }, [filter]);
```

### Thumbnail Tip

```tsx
const getThumbnail = (images: BikeImageDto[]): string => {
  const thumb = images.find(img => img.isThumbnail);
  return thumb?.mediaUrl || images[0]?.mediaUrl || '/placeholder-bike.png';
};
```

### Quantity Display

```tsx
// Hi?n th? trên listing card
<span className={listing.quantity > 0 ? 'in-stock' : 'out-of-stock'}>
  {listing.quantity > 0 ? `Còn ${listing.quantity} chi?c` : 'H?t hàng'}
</span>
```

---

## ?? 2. Xem chi ti?t — `GET /api/bikes/{id}` (Public)

### Response `200 OK` — Cùng c?u trúc `BikePostDto` (bao g?m `quantity`)

### Response — Error `400`

```json
{ "error": "Listing not found" }
```

### FE Logic

```tsx
const { id } = useParams<{ id: string }>();

const loadDetail = async () => {
  try {
    const res = await api.get(`/api/bikes/${id}`);
    setListing(res.data);
  } catch (err) {
    if (err.response?.status === 400) {
      navigate('/404');
    }
  }
};
```

### Image Gallery Component

```tsx
const BikeImageGallery = ({ images }: { images: BikeImageDto[] }) => {
  const [activeIndex, setActiveIndex] = useState(0);

  // S?p x?p: thumbnail lên ??u
  const sorted = [...images].sort((a, b) =>
    (b.isThumbnail ? 1 : 0) - (a.isThumbnail ? 1 : 0)
  );

  return (
    <div className="gallery">
      {/* ?nh chính */}
      <img
        src={sorted[activeIndex]?.mediaUrl}
        alt="Bike"
        className="main-image"
      />

      {/* ?nh thumbnail nh? */}?
      <div className="thumbnails">
        {sorted.map((img, i) => (
          <img
            key={img.mediaId}
            src={img.mediaUrl}
            alt={`Thumbnail ${i + 1}`}
            className={i === activeIndex ? 'active' : ''}
            onClick={() => setActiveIndex(i)}
          />
        ))}
      </div>
    </div>
  );
};
```

---

## ?? 3. L?y danh sách Brands — `GET /api/bikes/brands` (Public)

### Response `200 OK`

```json
["Giant", "Trek", "Specialized", "Merida", "Java"]
```

---

## ? 4. ??ng bài m?i — `POST /api/bikes` (?? Auth)

### FormData Fields

| Key | Type | Required | Validation |
|---|---|---|---|
| `Title` | string | ? | Not empty, max 200 chars |
| `Price` | number | ? | > 0 |
| `Quantity` | number | ? | ? 1 (default `1`). S? l??ng xe cùng lo?i |
| `Description` | string | ? | |
| `Address` | string | ? | Max 255 chars |
| `BrandId` | number | ? | ID of brand |
| `TypeId` | number | ? | ID of bike type |
| `ModelName` | string | ? | |
| `SerialNumber` | string | ? | |
| `Color` | string | ? | |
| `Condition` | string | ? | Max 50 chars |
| `FrameSize` | string | ? | |
| `FrameMaterial` | string | ? | |
| `WheelSize` | string | ? | |
| `BrakeType` | string | ? | |
| `Weight` | number | ? | Kg (decimal) |
| `Transmission` | string | ? | |
| `Images` | File[] | ?* | Max **10 ?nh**, m?i ?nh max **5 MB** |
| `ImageUrls` | string[] | ?* | URL ?nh ?ã upload tr??c |

> \* B?t bu?c ít nh?t 1 ?nh: `Images.length + ImageUrls.length > 0`

### Response — Error `400`

| Error | Nguyên nhân |
|---|---|
| `"Seller not found"` | Token không h?p l? |
| `"At least one image is required"` | Không có ?nh |
| `"Title is required"` | Title r?ng |
| `"Price must be greater than 0"` | Price ? 0 |
| `"Quantity must be at least 1"` | Quantity ? 0 |
| `"Maximum 10 images allowed"` | Quá 10 ?nh |
| `"Each image must be at most 5 MB"` | File > 5 MB |
| `"Only .jpg, .jpeg, .png, .webp images are allowed"` | Sai ??nh d?ng |

### FE Logic

```tsx
const handleCreate = async (formValues: CreateBikeFormValues) => {
  const totalImages = formValues.imageFiles.length;
  if (totalImages === 0) return setError('Vui lòng ch?n ít nh?t 1 ?nh');
  if (totalImages > 10) return setError('T?i ?a 10 ?nh');

  const formData = new FormData();
  formData.append('Title', formValues.title);
  formData.append('Price', formValues.price.toString());
  formData.append('Quantity', (formValues.quantity || 1).toString());
  if (formValues.description) formData.append('Description', formValues.description);
  if (formValues.address) formData.append('Address', formValues.address);
  if (formValues.brandId) formData.append('BrandId', formValues.brandId.toString());
  if (formValues.typeId) formData.append('TypeId', formValues.typeId.toString());
  if (formValues.modelName) formData.append('ModelName', formValues.modelName);
  if (formValues.serialNumber) formData.append('SerialNumber', formValues.serialNumber);
  if (formValues.color) formData.append('Color', formValues.color);
  if (formValues.condition) formData.append('Condition', formValues.condition);
  if (formValues.frameSize) formData.append('FrameSize', formValues.frameSize);
  if (formValues.frameMaterial) formData.append('FrameMaterial', formValues.frameMaterial);
  if (formValues.wheelSize) formData.append('WheelSize', formValues.wheelSize);
  if (formValues.brakeType) formData.append('BrakeType', formValues.brakeType);
  if (formValues.weight) formData.append('Weight', formValues.weight.toString());
  if (formValues.transmission) formData.append('Transmission', formValues.transmission);
  formValues.imageFiles.forEach(file => formData.append('Images', file));

  try {
    setLoading(true);
    const res = await api.post('/api/bikes', formData);
    navigate(`/bikes/${res.data.listingId}`);
  } catch (err: any) {
    const errorData = err.response?.data;
    if (errorData?.errors?.length) {
      setError(errorData.errors.join(', '));
    } else {
      setError(errorData?.error || '??ng bài th?t b?i');
    }
  } finally {
    setLoading(false);
  }
};
```

---

## ?? 5. C?p nh?t bài ??ng — `PUT /api/bikes` (?? Auth)

### ?i?m khác so v?i Create

| Tính n?ng | Create (`POST`) | Update (`PUT`) |
|---|---|---|
| Key ?nh m?i | `Images` | `NewImages` |
| Xoá ?nh c? | — | `RemoveMediaIds` |
| ??i ?nh bìa | Auto (?nh ??u) | `ThumbnailMediaId` |
| B?t bu?c ?nh | ? ? 1 | ? (gi? ?nh c?) |
| Quantity | ? | ? (có th? c?p nh?t) |

### FormData Fields (thêm so v?i Create)

| Key | Type | Mô t? |
|---|---|---|
| `ListingId` | number | **B?t bu?c** — ID bài c?n s?a |
| `Quantity` | number | S? l??ng xe (? 1) |
| `NewImages` | File[] | ?nh m?i upload |
| `ImageUrls` | string[] | URL ?nh m?i |
| `RemoveMediaIds` | number[] | Danh sách `mediaId` c?n xoá |
| `ThumbnailMediaId` | number | `mediaId` mu?n set làm ?nh bìa |

### Response — Error `400`

| Error | Nguyên nhân |
|---|---|
| `"Listing not found"` | `ListingId` không t?n t?i |
| `"You can only edit your own listings"` | Không ph?i ch? bài |
| `"Quantity must be at least 1"` | Quantity ? 0 |

### FE Logic

```tsx
const handleUpdate = async (listingId: number, state: EditFormState) => {
  const formData = new FormData();
  formData.append('ListingId', listingId.toString());
  formData.append('Title', state.title);
  formData.append('Price', state.price.toString());
  formData.append('Quantity', (state.quantity || 1).toString());
  if (state.description) formData.append('Description', state.description);
  if (state.address) formData.append('Address', state.address);
  if (state.brandId) formData.append('BrandId', state.brandId.toString());
  if (state.typeId) formData.append('TypeId', state.typeId.toString());
  if (state.modelName) formData.append('ModelName', state.modelName);
  if (state.color) formData.append('Color', state.color);
  if (state.condition) formData.append('Condition', state.condition);
  if (state.frameSize) formData.append('FrameSize', state.frameSize);
  if (state.frameMaterial) formData.append('FrameMaterial', state.frameMaterial);
  if (state.wheelSize) formData.append('WheelSize', state.wheelSize);
  if (state.brakeType) formData.append('BrakeType', state.brakeType);
  if (state.weight) formData.append('Weight', state.weight.toString());
  if (state.transmission) formData.append('Transmission', state.transmission);
  state.newFiles.forEach(file => formData.append('NewImages', file));
  state.removeMediaIds.forEach(id => formData.append('RemoveMediaIds', id.toString()));
  if (state.thumbnailMediaId) formData.append('ThumbnailMediaId', state.thumbnailMediaId.toString());

  try {
    setLoading(true);
    const res = await api.put('/api/bikes', formData);
    navigate(`/bikes/${res.data.listingId}`);
  } catch (err: any) {
    setError(err.response?.data?.error || 'C?p nh?t th?t b?i');
  } finally {
    setLoading(false);
  }
};
```

---

## ??? 6. Xoá bài ??ng — `DELETE /api/bikes/{id}` (?? Auth)

> ?? **Không th? xoá listing ?ã có ??n hàng** (có `OrderDetail` tham chi?u).
> BE tr? l?i và g?i ý dùng Toggle Visibility ?? ?n thay vì xoá.

### Response — Error `400`

| Error | Nguyên nhân |
|---|---|
| `"Listing not found"` | Bài không t?n t?i |
| `"You can only delete your own listings"` | Không ph?i ch? bài |
| `"Cannot delete a listing that has been ordered. You can hide it instead."` | ? Listing ?ã có ??n hàng |

### FE Logic

```tsx
const handleDelete = async (listingId: number) => {
  const confirmed = window.confirm('B?n có ch?c mu?n xoá bài ??ng này?');
  if (!confirmed) return;

  try {
    await api.delete(`/api/bikes/${listingId}`);
    setMyPosts(prev => prev.filter(p => p.listingId !== listingId));
    setMessage('?ã xoá bài ??ng');
  } catch (err: any) {
    const error = err.response?.data?.error;
    if (error?.includes('has been ordered')) {
      setError('Bài ??ng ?ã có ??n hàng, không th? xoá. B?n có th? ?n bài thay th?.');
    } else {
      setError(error || 'Xoá th?t b?i');
    }
  }
};
```

---

## ??? 7. ?n / Hi?n bài ??ng — `PATCH /api/bikes/{id}/visibility` (?? Auth)

Toggle gi?a `ListingStatus = 1` (Active) ? `0` (Hidden).

### Listing Status Reference

| Value | Label | Ý ngh?a |
|---|---|---|
| `0` | Hidden | Bài b? ?n, buyer không th?y |
| `1` | Active | ?ang hi?n th? công khai |
| `2` | Pending | Ch? duy?t |
| `3` | Sold | ?ã bán h?t (quantity = 0) |
| `4` | Rejected | B? t? ch?i |

```tsx
const STATUS_MAP: Record<number, { label: string; color: string }> = {
  0: { label: '?ã ?n', color: 'gray' },
  1: { label: '?ang bán', color: 'green' },
  2: { label: 'Ch? duy?t', color: 'orange' },
  3: { label: '?ã bán h?t', color: 'blue' },
  4: { label: 'B? t? ch?i', color: 'red' },
};
```

---

## ?? 8. Bài ??ng c?a tôi — `GET /api/bikes/my-posts` (?? Auth)

### Response `200 OK` — `BikePostDto[]`

M?i item có ??y ?? `quantity`, `listingStatus`, `images`, etc.

---

## ?? TypeScript Interfaces

```typescript
// ===== Request DTOs =====

interface BikeFilterDto {
  searchTerm?: string;
  brandId?: number;
  typeId?: number;
  condition?: string;
  minPrice?: number;
  maxPrice?: number;
  address?: string;
  sortBy?: 'newest' | 'oldest' | 'price_asc' | 'price_desc';
  page?: number;
  pageSize?: number;
}

/** Form state — convert sang FormData tr??c khi g?i */
interface CreateBikeFormValues {
  title: string;
  description?: string;
  price: number;
  quantity: number;             // ? M?I — m?c ??nh 1
  address?: string;
  brandId?: number;
  typeId?: number;
  modelName?: string;
  serialNumber?: string;
  color?: string;
  condition?: string;
  frameSize?: string;
  frameMaterial?: string;
  wheelSize?: string;
  brakeType?: string;
  weight?: number;
  transmission?: string;
  imageFiles: File[];           // append as "Images" in FormData
}

/** Form state khi update */
interface UpdateBikeFormValues extends CreateBikeFormValues {
  listingId: number;
  existingImages: BikeImageDto[];
  newFiles: File[];             // append as "NewImages" in FormData
  removeMediaIds: number[];     // append each as "RemoveMediaIds"
  thumbnailMediaId?: number;
}

// ===== Response DTOs =====

interface BikePostDto {
  listingId: number;
  title: string;
  description?: string;
  price: number;
  quantity: number;             // ? M?I — s? l??ng còn l?i
  listingStatus?: number;       // 0=Hidden, 1=Active, 2=Pending, 3=Sold, 4=Rejected
  address?: string;
  postedDate?: string;
  bikeId: number;
  modelName?: string;
  serialNumber?: string;
  color?: string;
  condition?: string;
  brandName?: string;
  typeName?: string;
  frameSize?: string;
  frameMaterial?: string;
  wheelSize?: string;
  brakeType?: string;
  weight?: number;
  transmission?: string;
  sellerId: number;
  sellerName: string;
  images: BikeImageDto[];
  hasInspection: boolean;
}

interface BikeImageDto {
  mediaId: number;
  mediaUrl: string;
  mediaType?: string;
  isThumbnail?: boolean;
}

interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}
```

---

## ?? Axios Bike Service

```typescript
import api from './api';

const buildCreateFormData = (values: CreateBikeFormValues): FormData => {
  const fd = new FormData();
  fd.append('Title', values.title);
  fd.append('Price', values.price.toString());
  fd.append('Quantity', (values.quantity || 1).toString());
  if (values.description) fd.append('Description', values.description);
  if (values.address) fd.append('Address', values.address);
  if (values.brandId) fd.append('BrandId', values.brandId.toString());
  if (values.typeId) fd.append('TypeId', values.typeId.toString());
  if (values.modelName) fd.append('ModelName', values.modelName);
  if (values.serialNumber) fd.append('SerialNumber', values.serialNumber);
  if (values.color) fd.append('Color', values.color);
  if (values.condition) fd.append('Condition', values.condition);
  if (values.frameSize) fd.append('FrameSize', values.frameSize);
  if (values.frameMaterial) fd.append('FrameMaterial', values.frameMaterial);
  if (values.wheelSize) fd.append('WheelSize', values.wheelSize);
  if (values.brakeType) fd.append('BrakeType', values.brakeType);
  if (values.weight) fd.append('Weight', values.weight.toString());
  if (values.transmission) fd.append('Transmission', values.transmission);
  values.imageFiles.forEach(f => fd.append('Images', f));
  return fd;
};

const buildUpdateFormData = (values: UpdateBikeFormValues): FormData => {
  const fd = new FormData();
  fd.append('ListingId', values.listingId.toString());
  fd.append('Title', values.title);
  fd.append('Price', values.price.toString());
  fd.append('Quantity', (values.quantity || 1).toString());
  if (values.description) fd.append('Description', values.description);
  if (values.address) fd.append('Address', values.address);
  if (values.brandId) fd.append('BrandId', values.brandId.toString());
  if (values.typeId) fd.append('TypeId', values.typeId.toString());
  if (values.modelName) fd.append('ModelName', values.modelName);
  if (values.serialNumber) fd.append('SerialNumber', values.serialNumber);
  if (values.color) fd.append('Color', values.color);
  if (values.condition) fd.append('Condition', values.condition);
  if (values.frameSize) fd.append('FrameSize', values.frameSize);
  if (values.frameMaterial) fd.append('FrameMaterial', values.frameMaterial);
  if (values.wheelSize) fd.append('WheelSize', values.wheelSize);
  if (values.brakeType) fd.append('BrakeType', values.brakeType);
  if (values.weight) fd.append('Weight', values.weight.toString());
  if (values.transmission) fd.append('Transmission', values.transmission);
  values.newFiles.forEach(f => fd.append('NewImages', f));
  values.removeMediaIds.forEach(id => fd.append('RemoveMediaIds', id.toString()));
  if (values.thumbnailMediaId) fd.append('ThumbnailMediaId', values.thumbnailMediaId.toString());
  return fd;
};

export const bikeService = {
  search: (filter: BikeFilterDto) =>
    api.get<PagedResult<BikePostDto>>('/api/bikes', { params: filter }),
  getDetail: (id: number) =>
    api.get<BikePostDto>(`/api/bikes/${id}`),
  getBrands: () =>
    api.get<string[]>('/api/bikes/brands'),
  create: (values: CreateBikeFormValues) =>
    api.post<BikePostDto>('/api/bikes', buildCreateFormData(values)),
  update: (values: UpdateBikeFormValues) =>
    api.put<BikePostDto>('/api/bikes', buildUpdateFormData(values)),
  delete: (id: number) =>
    api.delete(`/api/bikes/${id}`),
  toggleVisibility: (id: number) =>
    api.patch(`/api/bikes/${id}/visibility`),
  getMyPosts: () =>
    api.get<BikePostDto[]>('/api/bikes/my-posts'),
};
```

---

## ? FE Checklist

### Logic quan tr?ng
- [ ] **Create/Update dùng `FormData`** — KHÔNG dùng JSON body
- [ ] **KHÔNG set `Content-Type` header** — ?? browser t? thêm boundary
- [ ] **Append `Quantity`** trong FormData (m?c ??nh `1`)
- [ ] Key ?nh khi Create: `Images` — khi Update: `NewImages`
- [ ] Client-side validate file tr??c khi g?i (size ? 5MB, type, count ? 10)
- [ ] X? lý l?i xoá listing ?ã có order ? g?i ý ?n bài thay th?
- [ ] Hi?n th? `quantity` trên listing card và detail page
- [ ] Listing `quantity = 0` ? hi?n th? "H?t hàng" badge
- [ ] Append array ?úng cách (m?i ph?n t? riêng, cùng key)

### Tránh l?i ph? bi?n
- [ ] ? G?i JSON thay vì FormData ? BE tr? 415
- [ ] ? Set `Content-Type: multipart/form-data` th? công ? m?t boundary
- [ ] ? Append array b?ng `JSON.stringify([1,2,3])` ? BE nh?n string
- [ ] ? Dùng key `Images` khi Update ? ph?i dùng `NewImages`
- [ ] ? Quên append `ListingId` khi Update
- [ ] ? Không cleanup `URL.createObjectURL()` ? memory leak
