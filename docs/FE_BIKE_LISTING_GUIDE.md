# ?? FE Guide: Bicycle Listing CRUD + Cloudinary Image Upload

> Qu?n lý ??ng bài, s?a bài, xoá bài, ?n/hi?n bài ??ng xe ??p.
> ?nh ???c upload tr?c ti?p qua BE lên **Cloudinary** (FE không t??ng tác Cloudinary).
>
> **Base URL:** `http://localhost:5xxx` (ki?m tra port BE)
>
> **All endpoints prefix:** `/api/bikes`

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
| `Images` | `Images` (List\<IFormFile\>) | Không nh?n file |
| `NewImages` | `NewImages` (khi Update) | Không nh?n file |
| `RemoveMediaIds` | `RemoveMediaIds` (khi Update) | Không xoá ?nh |

> ?? Key trong FormData **case-insensitive** v?i ASP.NET model binding, nh?ng nên gi? **PascalCase** ho?c **camelCase** ?úng tên property.

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

> Axios t? set `Content-Type: multipart/form-data; boundary=...` khi body là `FormData`.

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
| `brandId` | number | — | L?c theo brand (l?y t? API brands) |
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
        },
        {
          "mediaId": 2,
          "mediaUrl": "https://res.cloudinary.com/.../listings/def456.jpg",
          "mediaType": "image",
          "isThumbnail": false
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
    // Lo?i b? params undefined/null tr??c khi g?i
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

M?i listing có nhi?u ?nh. ?? hi?n th? ?nh ??i di?n trong danh sách:

```tsx
const getThumbnail = (images: BikeImageDto[]): string => {
  const thumb = images.find(img => img.isThumbnail);
  return thumb?.mediaUrl || images[0]?.mediaUrl || '/placeholder-bike.png';
};

// Trong JSX
<img src={getThumbnail(listing.images)} alt={listing.title} />
```

---

## ?? 2. Xem chi ti?t — `GET /api/bikes/{id}` (Public)

### Request

```http
GET /api/bikes/1
```

### Response `200 OK` — Cùng c?u trúc `BikePostDto` (xem m?u ? trên)

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

      {/* ?nh thumbnail nh? */}
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

## ??? 3. L?y danh sách Brands — `GET /api/bikes/brands` (Public)

### Response `200 OK`

```json
["Giant", "Trek", "Specialized", "Merida", "Java"]
```

### FE Logic — Dùng cho Select/Filter

```tsx
const [brands, setBrands] = useState<string[]>([]);

useEffect(() => {
  api.get('/api/bikes/brands').then(res => setBrands(res.data));
}, []);

// Trong JSX
<select onChange={e => setFilter(prev => ({ ...prev, brandId: e.target.value }))}>
  <option value="">T?t c? th??ng hi?u</option>
  {brands.map(brand => (
    <option key={brand} value={brand}>{brand}</option>
  ))}
</select>
```

> ?? API tr? v? **tên brand** (string[]), nh?ng filter dùng **brandId** (number). FE c?n map tên ? ID n?u BE có API riêng, ho?c dùng searchTerm k?t h?p.

---

## ? 4. ??ng bài m?i — `POST /api/bikes` (?? Auth)

### ? Flow t?ng quan

```
????????????????   FormData (multipart)   ????????????????
?   FE React   ? ???????????????????????? ?   Backend    ?
?  (Seller)    ?                          ?              ?
?              ?                          ?  ??????????? ?
?  Images[] ???????????????????????????????  ?Cloudinary? ?
?  Title       ?                          ?  ? Upload   ? ?
?  Price       ?                          ?  ??????????? ?
?  ...         ?                          ?       ? URLs  ?
?              ? ???????????????????????? ?  Save to DB  ?
?              ?   BikePostDto response   ?              ?
????????????????                          ????????????????
```

### Request

```http
POST /api/bikes
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

### FormData Fields

| Key | Type | Required | Validation |
|---|---|---|---|
| `Title` | string | ? | Not empty, max 200 chars |
| `Price` | number | ? | > 0 |
| `Description` | string | ? | |
| `Address` | string | ? | Max 255 chars |
| `BrandId` | number | ? | ID of brand |
| `TypeId` | number | ? | ID of bike type |
| `ModelName` | string | ? | |
| `SerialNumber` | string | ? | |
| `Color` | string | ? | |
| `Condition` | string | ? | Max 50 chars. VD: `"New"`, `"Used"`, `"Like New"` |
| `FrameSize` | string | ? | VD: `"S"`, `"M"`, `"L"`, `"XL"` |
| `FrameMaterial` | string | ? | VD: `"Aluminum"`, `"Carbon"`, `"Steel"` |
| `WheelSize` | string | ? | VD: `"26"`, `"27.5"`, `"29"`, `"700c"` |
| `BrakeType` | string | ? | VD: `"Disc"`, `"V-Brake"`, `"Rim"` |
| `Weight` | number | ? | Kg (decimal) |
| `Transmission` | string | ? | VD: `"Shimano Deore 1x12"` |
| `Images` | File[] | ?* | Upload file tr?c ti?p. Max **10 ?nh**, m?i ?nh max **5 MB** |
| `ImageUrls` | string[] | ?* | URL ?nh ?ã upload tr??c (backup) |

> \* B?t bu?c ít nh?t 1 ?nh: `Images.length + ImageUrls.length > 0`

### File Validation (BE s? reject n?u vi ph?m)

| Rule | Giá tr? |
|---|---|
| T?ng s? ?nh t?i ?a | **10** |
| Kích th??c m?i file | Max **5 MB** (5,242,880 bytes) |
| ??nh d?ng cho phép | `.jpg`, `.jpeg`, `.png`, `.webp` |

### Response `200 OK` — `BikePostDto` (bài v?a t?o, bao g?m URLs ?nh t? Cloudinary)

### Response — Error `400`

| Error | Nguyên nhân |
|---|---|
| `"Seller not found"` | Token không h?p l? / user b? xoá |
| `"At least one image is required"` | Không có ?nh nào |
| `"Title is required"` | Title r?ng |
| `"Price must be greater than 0"` | Price ? 0 |
| `"Maximum 10 images allowed"` | Quá 10 ?nh |
| `"Each image must be at most 5 MB"` | File > 5 MB |
| `"Only .jpg, .jpeg, .png, .webp images are allowed"` | Sai ??nh d?ng |

### FE Logic — ??y ??

```tsx
const handleCreate = async (formValues: CreateBikeFormValues) => {
  // 1. Client-side validation
  const totalImages = formValues.imageFiles.length;
  if (totalImages === 0) {
    return setError('Vui lòng ch?n ít nh?t 1 ?nh');
  }
  if (totalImages > 10) {
    return setError('T?i ?a 10 ?nh');
  }

  // 2. Build FormData
  const formData = new FormData();

  // Text fields
  formData.append('Title', formValues.title);
  formData.append('Price', formValues.price.toString());
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

  // Image files — append t?ng file cùng key "Images"
  formValues.imageFiles.forEach(file => {
    formData.append('Images', file);
  });

  // 3. Call API
  try {
    setLoading(true);
    const res = await api.post('/api/bikes', formData);
    // Không set Content-Type header — ?? browser t? thêm boundary
    const created: BikePostDto = res.data;
    navigate(`/bikes/${created.listingId}`);
  } catch (err: any) {
    const errorData = err.response?.data;
    // BE có th? tr? { error: "..." } ho?c { errors: [...] }
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

### Image Preview Component — Tr??c khi upload

```tsx
const ImagePicker = ({
  files,
  onAdd,
  onRemove,
}: {
  files: File[];
  onAdd: (newFiles: File[]) => void;
  onRemove: (index: number) => void;
}) => {
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selected = Array.from(e.target.files || []);
    const MAX_SIZE = 5 * 1024 * 1024;
    const ALLOWED = ['image/jpeg', 'image/png', 'image/webp'];

    const valid = selected.filter(f => {
      if (f.size > MAX_SIZE) { alert(`${f.name} v??t quá 5 MB`); return false; }
      if (!ALLOWED.includes(f.type)) { alert(`${f.name} không ?úng ??nh d?ng`); return false; }
      return true;
    });

    if (files.length + valid.length > 10) {
      alert('T?i ?a 10 ?nh');
      return;
    }
    onAdd(valid);
    e.target.value = ''; // Reset input
  };

  return (
    <div className="image-picker">
      <div className="preview-grid">
        {files.map((file, i) => (
          <div key={i} className="preview-item">
            <img src={URL.createObjectURL(file)} alt={`Preview ${i + 1}`} />
            {i === 0 && <span className="badge">?nh bìa</span>}
            <button onClick={() => onRemove(i)}>?</button>
          </div>
        ))}

        {files.length < 10 && (
          <label className="add-image-btn">
            <span>+ Thêm ?nh</span>
            <input
              type="file"
              accept=".jpg,.jpeg,.png,.webp"
              multiple
              onChange={handleFileChange}
              hidden
            />
          </label>
        )}
      </div>
      <p className="hint">{files.length}/10 ?nh · ?nh ??u tiên là ?nh bìa</p>
    </div>
  );
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
| B?t bu?c ?nh | ? ? 1 | ? (gi? ?nh c? n?u không thêm/xoá) |

### Request

```http
PUT /api/bikes
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

### FormData Fields (thêm so v?i Create)

| Key | Type | Mô t? |
|---|---|---|
| `ListingId` | number | **B?t bu?c** — ID bài c?n s?a |
| `NewImages` | File[] | ?nh m?i upload lên Cloudinary |
| `ImageUrls` | string[] | URL ?nh m?i (backup) |
| `RemoveMediaIds` | number[] | Danh sách `mediaId` c?n xoá |
| `ThumbnailMediaId` | number | `mediaId` mu?n set làm ?nh bìa |
| *(các field khác)* | *(gi?ng Create)* | Title, Price, Description,... |

### Response `200 OK` — `BikePostDto` (bài ?ã c?p nh?t)

### Response — Error `400`

| Error | Nguyên nhân |
|---|---|
| `"Listing not found"` | `ListingId` không t?n t?i |
| `"You can only edit your own listings"` | Không ph?i ch? bài |
| *(các l?i validation gi?ng Create)* | |

### FE Logic — Update v?i qu?n lý ?nh

```tsx
interface EditFormState {
  // ... text fields
  existingImages: BikeImageDto[];   // ?nh hi?n t?i t? API
  newFiles: File[];                 // file m?i ch?a upload
  removeMediaIds: number[];         // mediaId c?n xoá
  thumbnailMediaId?: number;        // mediaId ??t làm ?nh bìa
}

const handleUpdate = async (listingId: number, state: EditFormState) => {
  const formData = new FormData();

  // ListingId — b?t bu?c
  formData.append('ListingId', listingId.toString());

  // Text fields (gi?ng Create)
  formData.append('Title', state.title);
  formData.append('Price', state.price.toString());
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

  // ?? ?nh m?i — key là "NewImages" (KHÔNG ph?i "Images")
  state.newFiles.forEach(file => {
    formData.append('NewImages', file);
  });

  // ?? Xoá ?nh — m?i ID append riêng, CÙNG key
  state.removeMediaIds.forEach(id => {
    formData.append('RemoveMediaIds', id.toString());
  });

  // ??i ?nh bìa
  if (state.thumbnailMediaId) {
    formData.append('ThumbnailMediaId', state.thumbnailMediaId.toString());
  }

  try {
    setLoading(true);
    const res = await api.put('/api/bikes', formData);
    const updated: BikePostDto = res.data;
    navigate(`/bikes/${updated.listingId}`);
  } catch (err: any) {
    setError(err.response?.data?.error || 'C?p nh?t th?t b?i');
  } finally {
    setLoading(false);
  }
};
```

### Edit Image Manager Component

```tsx
const EditImageManager = ({
  existingImages,
  newFiles,
  removeMediaIds,
  thumbnailMediaId,
  onMarkRemove,
  onUndoRemove,
  onSetThumbnail,
  onAddFiles,
  onRemoveNewFile,
}: EditImageManagerProps) => {
  // ?nh c? (ch?a b? ?ánh d?u xoá)
  const activeExisting = existingImages.filter(
    img => !removeMediaIds.includes(img.mediaId)
  );

  return (
    <div className="edit-images">
      <h4>?nh hi?n t?i</h4>
      <div className="image-grid">
        {existingImages.map(img => {
          const isRemoved = removeMediaIds.includes(img.mediaId);
          const isThumb = img.mediaId === thumbnailMediaId
            || (!thumbnailMediaId && img.isThumbnail);

          return (
            <div key={img.mediaId} className={`img-item ${isRemoved ? 'removed' : ''}`}>
              <img src={img.mediaUrl} alt="" />

              {isThumb && <span className="badge-thumb">?nh bìa</span>}

              {!isRemoved ? (
                <div className="img-actions">
                  <button onClick={() => onSetThumbnail(img.mediaId)}>
                    ??t ?nh bìa
                  </button>
                  <button onClick={() => onMarkRemove(img.mediaId)}>
                    Xoá
                  </button>
                </div>
              ) : (
                <button onClick={() => onUndoRemove(img.mediaId)}>
                  Hoàn tác
                </button>
              )}
            </div>
          );
        })}
      </div>

      <h4>?nh m?i</h4>
      {/* Dùng l?i ImagePicker component t? Create */}
      <ImagePicker
        files={newFiles}
        onAdd={onAddFiles}
        onRemove={onRemoveNewFile}
      />

      <p className="hint">
        T?ng ?nh sau khi l?u: {activeExisting.length + newFiles.length} / 10
      </p>
    </div>
  );
};
```

---

## ??? 6. Xoá bài ??ng — `DELETE /api/bikes/{id}` (?? Auth)

Xoá bài ??ng + t?t c? ?nh trên Cloudinary + Bicycle + BicycleDetail.

### Request

```http
DELETE /api/bikes/1
Authorization: Bearer {token}
```

### Response `200 OK`

```json
{ "message": "Success" }
```

### Response — Error `400`

| Error | Nguyên nhân |
|---|---|
| `"Listing not found"` | Bài không t?n t?i |
| `"You can only delete your own listings"` | Không ph?i ch? bài |

### FE Logic

```tsx
const handleDelete = async (listingId: number) => {
  const confirmed = window.confirm(
    'B?n có ch?c mu?n xoá bài ??ng này?\n?nh trên Cloudinary c?ng s? b? xoá v?nh vi?n.'
  );
  if (!confirmed) return;

  try {
    await api.delete(`/api/bikes/${listingId}`);
    // Xoá kh?i danh sách local
    setMyPosts(prev => prev.filter(p => p.listingId !== listingId));
    setMessage('?ã xoá bài ??ng');
  } catch (err: any) {
    setError(err.response?.data?.error || 'Xoá th?t b?i');
  }
};
```

---

## ??? 7. ?n / Hi?n bài ??ng — `PATCH /api/bikes/{id}/visibility` (?? Auth)

Toggle gi?a `ListingStatus = 1` (Active) ? `0` (Hidden).

### Request

```http
PATCH /api/bikes/1/visibility
Authorization: Bearer {token}
```

### Response `200 OK`

```json
{ "message": "Success" }
```

### FE Logic

```tsx
const handleToggleVisibility = async (listingId: number) => {
  try {
    await api.patch(`/api/bikes/${listingId}/visibility`);
    // C?p nh?t local state
    setMyPosts(prev =>
      prev.map(p =>
        p.listingId === listingId
          ? { ...p, listingStatus: p.listingStatus === 1 ? 0 : 1 }
          : p
      )
    );
  } catch (err: any) {
    setError(err.response?.data?.error || 'Thao tác th?t b?i');
  }
};

// Trong JSX
<button onClick={() => handleToggleVisibility(post.listingId)}>
  {post.listingStatus === 1 ? '?? ?n bài' : '??? Hi?n bài'}
</button>
```

### Listing Status Reference

| Value | Label | Ý ngh?a |
|---|---|---|
| `0` | Hidden | Bài b? ?n, buyer không th?y |
| `1` | Active | ?ang hi?n th? công khai |
| `2` | Pending | Ch? duy?t |
| `3` | Sold | ?ã bán |
| `4` | Rejected | B? t? ch?i |

```tsx
const STATUS_MAP: Record<number, { label: string; color: string }> = {
  0: { label: '?ã ?n', color: 'gray' },
  1: { label: '?ang bán', color: 'green' },
  2: { label: 'Ch? duy?t', color: 'orange' },
  3: { label: '?ã bán', color: 'blue' },
  4: { label: 'B? t? ch?i', color: 'red' },
};

const StatusBadge = ({ status }: { status?: number }) => {
  const info = STATUS_MAP[status ?? 0] || STATUS_MAP[0];
  return <span style={{ color: info.color }}>{info.label}</span>;
};
```

---

## ?? 8. Bài ??ng c?a tôi — `GET /api/bikes/my-posts` (?? Auth)

### Request

```http
GET /api/bikes/my-posts
Authorization: Bearer {token}
```

### Response `200 OK` — `BikePostDto[]`

```json
[
  {
    "listingId": 1,
    "title": "Xe ??p Giant",
    "listingStatus": 1,
    "images": [...],
    ...
  },
  {
    "listingId": 5,
    "title": "Xe ??p Trek c?",
    "listingStatus": 0,
    "images": [...],
    ...
  }
]
```

### FE Logic

```tsx
const [myPosts, setMyPosts] = useState<BikePostDto[]>([]);

const loadMyPosts = async () => {
  try {
    const res = await api.get('/api/bikes/my-posts');
    setMyPosts(res.data);
  } catch (err) {
    setError('Không th? t?i bài ??ng');
  }
};
```

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
  page?: number;   // default 1
  pageSize?: number; // default 12
}

/** Dùng cho form state FE — KHÔNG g?i tr?c ti?p, ph?i convert sang FormData */
interface CreateBikeFormValues {
  title: string;
  description?: string;
  price: number;
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
  imageFiles: File[];     // ? append as "Images" in FormData
}

/** Dùng cho form state FE khi update */
interface UpdateBikeFormValues extends CreateBikeFormValues {
  listingId: number;
  existingImages: BikeImageDto[];
  newFiles: File[];           // ? append as "NewImages" in FormData
  removeMediaIds: number[];   // ? append each as "RemoveMediaIds"
  thumbnailMediaId?: number;
}

// ===== Response DTOs =====

interface BikePostDto {
  listingId: number;
  title: string;
  description?: string;
  price: number;
  listingStatus?: number;   // 0=Hidden, 1=Active, 2=Pending, 3=Sold, 4=Rejected
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

## ??? Axios Bike Service

```typescript
import api from './api'; // axios instance v?i JWT interceptor

// ===== Helper: Build FormData t? form values =====

const buildCreateFormData = (values: CreateBikeFormValues): FormData => {
  const fd = new FormData();
  fd.append('Title', values.title);
  fd.append('Price', values.price.toString());
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

// ===== Service =====

export const bikeService = {
  /** Tìm ki?m xe (public) */
  search: (filter: BikeFilterDto) =>
    api.get<PagedResult<BikePostDto>>('/api/bikes', { params: filter }),

  /** Xem chi ti?t (public) */
  getDetail: (id: number) =>
    api.get<BikePostDto>(`/api/bikes/${id}`),

  /** L?y danh sách brands (public) */
  getBrands: () =>
    api.get<string[]>('/api/bikes/brands'),

  /** ??ng bài m?i (auth) */
  create: (values: CreateBikeFormValues) =>
    api.post<BikePostDto>('/api/bikes', buildCreateFormData(values)),

  /** C?p nh?t bài (auth) */
  update: (values: UpdateBikeFormValues) =>
    api.put<BikePostDto>('/api/bikes', buildUpdateFormData(values)),

  /** Xoá bài (auth) */
  delete: (id: number) =>
    api.delete(`/api/bikes/${id}`),

  /** Toggle ?n/hi?n (auth) */
  toggleVisibility: (id: number) =>
    api.patch(`/api/bikes/${id}/visibility`),

  /** Bài ??ng c?a tôi (auth) */
  getMyPosts: () =>
    api.get<BikePostDto[]>('/api/bikes/my-posts'),
};
```

---

## ?? React Router

```tsx
{/* Public */}
<Route path="/bikes" element={<BikeListPage />} />
<Route path="/bikes/:id" element={<BikeDetailPage />} />

{/* Protected — Seller */}
<Route path="/seller/posts" element={<ProtectedRoute><MyPostsPage /></ProtectedRoute>} />
<Route path="/seller/posts/create" element={<ProtectedRoute><CreatePostPage /></ProtectedRoute>} />
<Route path="/seller/posts/:id/edit" element={<ProtectedRoute><EditPostPage /></ProtectedRoute>} />
```

---

## ?? Suggested FE File Structure

```
src/
??? pages/
?   ??? bikes/
?   ?   ??? BikeListPage.tsx          ? Danh sách + filter + pagination
?   ?   ??? BikeDetailPage.tsx        ? Chi ti?t xe + gallery ?nh
?   ??? seller/
?       ??? MyPostsPage.tsx           ? Danh sách bài c?a tôi + ?n/hi?n/xoá
?       ??? CreatePostPage.tsx        ? Form ??ng bài + upload ?nh
?       ??? EditPostPage.tsx          ? Form s?a bài + qu?n lý ?nh
??? components/
?   ??? bikes/
?   ?   ??? BikeCard.tsx              ? Card hi?n th? trong danh sách
?   ?   ??? BikeImageGallery.tsx      ? Gallery ?nh chi ti?t
?   ?   ??? BikeFilter.tsx            ? B? l?c sidebar / topbar
?   ?   ??? StatusBadge.tsx           ? Badge hi?n th? tr?ng thái
?   ??? common/
?       ??? ImagePicker.tsx           ? Ch?n + preview ?nh (Create)
?       ??? EditImageManager.tsx      ? Qu?n lý ?nh (Update)
?       ??? Pagination.tsx            ? Phân trang
??? services/
?   ??? bikeService.ts               ? API calls + FormData helpers
??? types/
    ??? bike.ts                       ? TypeScript interfaces
```

---

## ??? Cloudinary Image Flow

```
FE                              BE                          Cloudinary
?                               ?                              ?
?  FormData (Images: File[])    ?                              ?
? ???????????????????????????   ?                              ?
?                               ?  UploadAsync(stream, name)   ?
?                               ? ?????????????????????????    ?
?                               ?                              ?
?                               ?  ? secure URL               ?
?                               ? ?????????????????????????    ?
?                               ?                              ?
?                               ?  Save URL to ListingMedia    ?
?                               ?  table in DB                 ?
?                               ?                              ?
?  ? BikePostDto               ?                              ?
?    (images[].mediaUrl =       ?                              ?
?     Cloudinary URLs)          ?                              ?
? ???????????????????????????   ?                              ?
```

### ??c ?i?m ?nh listing (khác avatar)

| | Avatar | Listing |
|---|---|---|
| Cloudinary folder | `secondbike/avatars/` | `secondbike/listings/` |
| Transformation | 500×500, face crop | 1200×900, limit, auto quality |
| S? l??ng | 1 | T?i ?a 10 |
| T? xoá khi delete | ? | ? |

> FE **KHÔNG** c?n bi?t Cloudinary config. Ch? c?n g?i `File` lên BE, BE tr? v? URL.

---

## ? FE Checklist

### Pages c?n t?o
- [ ] `BikeListPage` — hi?n th? danh sách xe, filter, sort, pagination
- [ ] `BikeDetailPage` — chi ti?t xe, gallery ?nh, thông tin seller
- [ ] `MyPostsPage` — danh sách bài ??ng c?a seller, nút ?n/hi?n/xoá/s?a
- [ ] `CreatePostPage` — form ??ng bài + ch?n ?nh upload
- [ ] `EditPostPage` — form s?a bài + qu?n lý ?nh (thêm/xoá/??i thumbnail)

### Components c?n t?o
- [ ] `BikeCard` — card hi?n th? trong danh sách (thumbnail, title, price, status)
- [ ] `BikeFilter` — b? l?c (brand, type, condition, price range, address, sort)
- [ ] `BikeImageGallery` — gallery ?nh trong trang chi ti?t
- [ ] `ImagePicker` — component ch?n + preview ?nh m?i (dùng cho Create)
- [ ] `EditImageManager` — qu?n lý ?nh c? + m?i (dùng cho Update)
- [ ] `StatusBadge` — hi?n th? tr?ng thái listing
- [ ] `Pagination` — phân trang

### Logic quan tr?ng
- [ ] **Create/Update dùng `FormData`** — KHÔNG dùng JSON body
- [ ] **KHÔNG set `Content-Type` header** — ?? browser t? thêm boundary
- [ ] **Append array ?úng cách** — m?i ph?n t? g?i `append()` riêng, cùng key
- [ ] Key ?nh khi Create: `Images` — khi Update: `NewImages`
- [ ] Client-side validate file tr??c khi g?i (size ? 5MB, type, count ? 10)
- [ ] X? lý error response: check c? `error` và `errors` array
- [ ] Confirm dialog tr??c khi xoá bài
- [ ] Reload `my-posts` sau khi xoá/?n/hi?n thành công
- [ ] Optimistic UI cho toggle visibility (c?p nh?t state tr??c, rollback n?u l?i)

### Tránh l?i ph? bi?n
- [ ] ? G?i JSON thay vì FormData ? BE tr? 415 Unsupported Media Type
- [ ] ? Set `Content-Type: multipart/form-data` th? công ? m?t boundary ? BE parse l?i
- [ ] ? Append array b?ng `JSON.stringify([1,2,3])` ? BE nh?n string thay vì int[]
- [ ] ? Dùng key `Images` khi Update ? BE không nh?n (ph?i dùng `NewImages`)
- [ ] ? Quên append `ListingId` khi Update ? BE tr? validation error
- [ ] ? Không cleanup `URL.createObjectURL()` ? memory leak

### Memory Leak Prevention

```tsx
// Cleanup object URLs khi component unmount ho?c files thay ??i
useEffect(() => {
  const urls = files.map(f => URL.createObjectURL(f));
  return () => urls.forEach(url => URL.revokeObjectURL(url));
}, [files]);
```
