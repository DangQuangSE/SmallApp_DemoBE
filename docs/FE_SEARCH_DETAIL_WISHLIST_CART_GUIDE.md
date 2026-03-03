# ???? FE Guide: Tìm ki?m & L?c xe + Xem chi ti?t & Wishlist/Cart

> **UC 1 — Research Bikes & Filter:** Tìm ki?m, l?c danh sách xe ??p (theo giá, size, hãng, lo?i, tình tr?ng, khu v?c) v?i pagination & sorting.
>
> **UC 2 — View Bike Detail & Wishlist/Cart:** Xem thông tin chi ti?t xe và l?u xe yêu thích (Wishlist) ho?c thêm vào Gi? hàng (Cart).
>
> **Base URL:** `http://localhost:5xxx` (ki?m tra port BE)
>
> **Stack FE:** React + Vite + TypeScript + Axios

---

## ?? API Endpoint Summary

### UC 1 — Tìm ki?m & L?c (Public — không c?n ??ng nh?p)

| Method | Endpoint | Auth | Mô t? |
|--------|----------|------|--------|
| `GET` | `/api/bikes` | ? | Tìm ki?m & l?c danh sách xe (có phân trang) |
| `GET` | `/api/bikes/brands` | ? | L?y danh sách hãng xe (cho dropdown filter) |
| `GET` | `/api/bikes/types` | ? | L?y danh sách lo?i xe (cho dropdown filter) |

### UC 2 — Chi ti?t & Wishlist & Cart (chi ti?t Public, Wishlist/Cart c?n Auth)

| Method | Endpoint | Auth | Mô t? |
|--------|----------|------|--------|
| `GET` | `/api/bikes/{id}` | ? | Xem chi ti?t 1 listing |
| `GET` | `/api/wishlist` | ?? | L?y danh sách Wishlist c?a user |
| `POST` | `/api/wishlist/{listingId}` | ?? | Thêm listing vào Wishlist |
| `DELETE` | `/api/wishlist/{listingId}` | ?? | Xoá listing kh?i Wishlist |
| `GET` | `/api/wishlist/{listingId}/check` | ?? | Ki?m tra listing có trong Wishlist không |
| `GET` | `/api/cart` | ?? | L?y danh sách Cart c?a user |
| `POST` | `/api/cart/{listingId}` | ?? | Thêm listing vào Cart |
| `DELETE` | `/api/cart/{listingId}` | ?? | Xoá listing kh?i Cart |
| `DELETE` | `/api/cart` | ?? | Xoá toàn b? Cart |
| `GET` | `/api/cart/{listingId}/check` | ?? | Ki?m tra listing có trong Cart không |
| `GET` | `/api/cart/count` | ?? | ??m s? item trong Cart |

---

## ?? UC 1: TÌM KI?M & L?C XE ??P

### 1.1. Tìm ki?m & L?c — `GET /api/bikes`

#### Request

```http
GET /api/bikes?searchTerm=giant&brandId=1&typeId=2&condition=Used&minPrice=1000000&maxPrice=10000000&frameSize=M&wheelSize=700c&address=HCM&sortBy=price_asc&page=1&pageSize=12
```

#### Query Parameters

| Param | Type | Default | Mô t? |
|-------|------|---------|--------|
| `searchTerm` | string | — | Tìm trong `title`, `description`, `modelName` |
| `brandId` | number | — | L?c theo hãng xe (ID) |
| `typeId` | number | — | L?c theo lo?i xe (ID) |
| `condition` | string | — | `"New"`, `"Used"`, `"Like New"`,... |
| `minPrice` | number | — | Giá t?i thi?u (VND) |
| `maxPrice` | number | — | Giá t?i ?a (VND) |
| `frameSize` | string | — | Kích c? khung: `"XS"`, `"S"`, `"M"`, `"L"`, `"XL"` |
| `wheelSize` | string | — | C? bánh xe: `"26"`, `"27.5"`, `"29"`, `"700c"`,... |
| `address` | string | — | L?c theo khu v?c (search contains) |
| `sortBy` | string | `"newest"` | `"newest"` \| `"oldest"` \| `"price_asc"` \| `"price_desc"` |
| `page` | number | `1` | Trang hi?n t?i |
| `pageSize` | number | `12` | S? item m?i trang |

> **L?u ý:** Ch? tr? v? listing có `ListingStatus = 1` (Active). Listing ?n, pending, sold, rejected s? không hi?n th?.

#### Response `200 OK`

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

### 1.2. L?y danh sách Brands — `GET /api/bikes/brands`

```json
["Giant", "Trek", "Specialized", "Merida", "Java"]
```

### 1.3. L?y danh sách Types — `GET /api/bikes/types`

```json
["Mountain", "Road", "City", "Hybrid", "BMX"]
```

> **L?u ý:** API `brands` và `types` tr? v? **tên** (string), không tr? v? ID.
> ?? l?c theo brand/type, FE c?n dùng `brandId` / `typeId` (number).
> ? FE nên g?i brands/types 1 l?n khi mount component và cache l?i.
> ? Ho?c BE có th? m? r?ng tr? object `{ id, name }` sau này n?u c?n.

---

### ?? FE Implementation — Trang tìm ki?m

#### TypeScript Interfaces

```typescript
interface BikeFilterDto {
  searchTerm?: string;
  brandId?: number;
  typeId?: number;
  condition?: string;
  minPrice?: number;
  maxPrice?: number;
  frameSize?: string;
  wheelSize?: string;
  address?: string;
  sortBy?: 'newest' | 'oldest' | 'price_asc' | 'price_desc';
  page?: number;
  pageSize?: number;
}

interface BikePostDto {
  listingId: number;
  title: string;
  description?: string;
  price: number;
  quantity: number;
  listingStatus?: number;
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

#### Axios Service

```typescript
// src/services/bikeSearchService.ts
import api from './api'; // axios instance ?ã config baseURL + interceptor

export const bikeSearchService = {
  search: (filter: BikeFilterDto) => {
    // Lo?i b? params null/undefined/empty tr??c khi g?i
    const params = Object.fromEntries(
      Object.entries(filter).filter(([_, v]) => v != null && v !== '')
    );
    return api.get<PagedResult<BikePostDto>>('/api/bikes', { params });
  },

  getDetail: (id: number) =>
    api.get<BikePostDto>(`/api/bikes/${id}`),

  getBrands: () =>
    api.get<string[]>('/api/bikes/brands'),

  getTypes: () =>
    api.get<string[]>('/api/bikes/types'),
};
```

#### Search Page Component

```tsx
// src/pages/BikeSearchPage.tsx
import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { bikeSearchService } from '../services/bikeSearchService';

const BikeSearchPage = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [listings, setListings] = useState<BikePostDto[]>([]);
  const [pagination, setPagination] = useState<PagedResult<BikePostDto> | null>(null);
  const [brands, setBrands] = useState<string[]>([]);
  const [types, setTypes] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);

  // ??c filter t? URL search params (h? tr? share link & back button)
  const filter: BikeFilterDto = {
    searchTerm: searchParams.get('searchTerm') || undefined,
    brandId: searchParams.get('brandId') ? Number(searchParams.get('brandId')) : undefined,
    typeId: searchParams.get('typeId') ? Number(searchParams.get('typeId')) : undefined,
    condition: searchParams.get('condition') || undefined,
    minPrice: searchParams.get('minPrice') ? Number(searchParams.get('minPrice')) : undefined,
    maxPrice: searchParams.get('maxPrice') ? Number(searchParams.get('maxPrice')) : undefined,
    frameSize: searchParams.get('frameSize') || undefined,
    wheelSize: searchParams.get('wheelSize') || undefined,
    address: searchParams.get('address') || undefined,
    sortBy: (searchParams.get('sortBy') as BikeFilterDto['sortBy']) || 'newest',
    page: Number(searchParams.get('page')) || 1,
    pageSize: Number(searchParams.get('pageSize')) || 12,
  };

  // Load brands & types 1 l?n khi mount
  useEffect(() => {
    bikeSearchService.getBrands().then(res => setBrands(res.data));
    bikeSearchService.getTypes().then(res => setTypes(res.data));
  }, []);

  // Load listings khi filter thay ??i
  const loadListings = useCallback(async () => {
    setLoading(true);
    try {
      const res = await bikeSearchService.search(filter);
      const data = res.data;
      setListings(data.items);
      setPagination(data);
    } catch {
      setListings([]);
    } finally {
      setLoading(false);
    }
  }, [searchParams.toString()]);

  useEffect(() => { loadListings(); }, [loadListings]);

  // C?p nh?t filter ? ??y lên URL search params
  const updateFilter = (updates: Partial<BikeFilterDto>) => {
    const newParams = new URLSearchParams(searchParams);
    Object.entries(updates).forEach(([key, value]) => {
      if (value != null && value !== '') {
        newParams.set(key, String(value));
      } else {
        newParams.delete(key);
      }
    });
    // Reset v? page 1 khi thay ??i filter (tr? khi ??i page)
    if (!('page' in updates)) {
      newParams.set('page', '1');
    }
    setSearchParams(newParams);
  };

  return (
    <div className="search-page">
      {/* ===== FILTER SIDEBAR ===== */}
      <aside className="filter-sidebar">
        {/* Search box */}
        <input
          type="text"
          placeholder="Tìm ki?m xe..."
          defaultValue={filter.searchTerm}
          onKeyDown={(e) => {
            if (e.key === 'Enter') {
              updateFilter({ searchTerm: e.currentTarget.value });
            }
          }}
        />

        {/* Brand filter */}
        <select
          value={filter.brandId || ''}
          onChange={(e) => updateFilter({ brandId: e.target.value ? Number(e.target.value) : undefined })}
        >
          <option value="">T?t c? hãng</option>
          {/* L?U Ý: API brands tr? string[], FE c?n map sang id n?u BE ch?a tr? id */}
          {brands.map((brand, i) => (
            <option key={brand} value={i + 1}>{brand}</option>
          ))}
        </select>

        {/* Type filter */}
        <select
          value={filter.typeId || ''}
          onChange={(e) => updateFilter({ typeId: e.target.value ? Number(e.target.value) : undefined })}
        >
          <option value="">T?t c? lo?i</option>
          {types.map((type, i) => (
            <option key={type} value={i + 1}>{type}</option>
          ))}
        </select>

        {/* Condition filter */}
        <select
          value={filter.condition || ''}
          onChange={(e) => updateFilter({ condition: e.target.value || undefined })}
        >
          <option value="">Tình tr?ng</option>
          <option value="New">M?i</option>
          <option value="Like New">Nh? m?i</option>
          <option value="Used">?ã s? d?ng</option>
        </select>

        {/* Price range */}
        <div className="price-range">
          <input
            type="number"
            placeholder="Giá t?"
            defaultValue={filter.minPrice}
            onBlur={(e) => updateFilter({ minPrice: e.target.value ? Number(e.target.value) : undefined })}
          />
          <input
            type="number"
            placeholder="Giá ??n"
            defaultValue={filter.maxPrice}
            onBlur={(e) => updateFilter({ maxPrice: e.target.value ? Number(e.target.value) : undefined })}
          />
        </div>

        {/* Frame size */}
        <select
          value={filter.frameSize || ''}
          onChange={(e) => updateFilter({ frameSize: e.target.value || undefined })}
        >
          <option value="">Kích c? khung</option>
          <option value="XS">XS</option>
          <option value="S">S</option>
          <option value="M">M</option>
          <option value="L">L</option>
          <option value="XL">XL</option>
        </select>

        {/* Wheel size */}
        <select
          value={filter.wheelSize || ''}
          onChange={(e) => updateFilter({ wheelSize: e.target.value || undefined })}
        >
          <option value="">C? bánh xe</option>
          <option value="26">26"</option>
          <option value="27.5">27.5"</option>
          <option value="29">29"</option>
          <option value="700c">700c</option>
        </select>

        {/* Address */}
        <input
          type="text"
          placeholder="Khu v?c (VD: HCM, Hà N?i)"
          defaultValue={filter.address}
          onKeyDown={(e) => {
            if (e.key === 'Enter') {
              updateFilter({ address: e.currentTarget.value });
            }
          }}
        />
      </aside>

      {/* ===== MAIN CONTENT ===== */}
      <main>
        {/* Sort bar */}
        <div className="sort-bar">
          <span>Tìm th?y {pagination?.totalCount || 0} k?t qu?</span>
          <select
            value={filter.sortBy || 'newest'}
            onChange={(e) => updateFilter({ sortBy: e.target.value as BikeFilterDto['sortBy'] })}
          >
            <option value="newest">M?i nh?t</option>
            <option value="oldest">C? nh?t</option>
            <option value="price_asc">Giá t?ng d?n</option>
            <option value="price_desc">Giá gi?m d?n</option>
          </select>
        </div>

        {/* Listing grid */}
        {loading ? (
          <div className="loading">?ang t?i...</div>
        ) : listings.length === 0 ? (
          <div className="empty">Không tìm th?y xe nào phù h?p</div>
        ) : (
          <div className="listing-grid">
            {listings.map((listing) => (
              <BikeCard key={listing.listingId} listing={listing} />
            ))}
          </div>
        )}

        {/* Pagination */}
        {pagination && pagination.totalPages > 1 && (
          <div className="pagination">
            <button
              disabled={!pagination.hasPrevious}
              onClick={() => updateFilter({ page: filter.page! - 1 })}
            >
              ? Trang tr??c
            </button>
            <span>Trang {pagination.page} / {pagination.totalPages}</span>
            <button
              disabled={!pagination.hasNext}
              onClick={() => updateFilter({ page: filter.page! + 1 })}
            >
              Trang sau ?
            </button>
          </div>
        )}
      </main>
    </div>
  );
};
```

#### Bike Card Component

```tsx
// src/components/BikeCard.tsx
import { Link } from 'react-router-dom';

const getThumbnail = (images: BikeImageDto[]): string => {
  const thumb = images.find(img => img.isThumbnail);
  return thumb?.mediaUrl || images[0]?.mediaUrl || '/placeholder-bike.png';
};

const formatPrice = (price: number): string => {
  return price.toLocaleString('vi-VN') + '?';
};

const BikeCard = ({ listing }: { listing: BikePostDto }) => {
  return (
    <Link to={`/bikes/${listing.listingId}`} className="bike-card">
      <div className="card-image">
        <img src={getThumbnail(listing.images)} alt={listing.title} />
        {listing.quantity === 0 && <span className="badge sold">H?t hàng</span>}
        {listing.hasInspection && <span className="badge inspected">?ã ki?m ??nh</span>}
      </div>
      <div className="card-body">
        <h3>{listing.title}</h3>
        <p className="price">{formatPrice(listing.price)}</p>
        <div className="card-meta">
          {listing.brandName && <span>{listing.brandName}</span>}
          {listing.condition && <span>{listing.condition}</span>}
          {listing.frameSize && <span>Size {listing.frameSize}</span>}
        </div>
        <p className="card-location">{listing.address || 'Ch?a có ??a ch?'}</p>
      </div>
    </Link>
  );
};
```

---

## ?? UC 2: XEM CHI TI?T & WISHLIST / CART

### 2.1. Xem chi ti?t xe — `GET /api/bikes/{id}` (Public)

#### Response `200 OK`

Cùng c?u trúc `BikePostDto` — ??y ?? thông tin xe, seller, ?nh, inspection.

#### Response `400`

```json
{ "error": "Listing not found" }
```

---

### 2.2. Wishlist APIs (?? Yêu c?u ??ng nh?p)

> Header: `Authorization: Bearer <jwt_token>`

#### Thêm vào Wishlist — `POST /api/wishlist/{listingId}`

**Response `200 OK`:**
```json
{ "message": "Success" }
```

**Response `400`:**

| Error | Nguyên nhân |
|-------|-------------|
| `"Listing not found"` | Listing ID không t?n t?i |
| `"You cannot add your own listing to wishlist"` | Seller không th? t? thêm bài c?a mình |
| `"Already in wishlist"` | ?ã có trong wishlist r?i |

#### Xoá kh?i Wishlist — `DELETE /api/wishlist/{listingId}`

**Response `200 OK`:**
```json
{ "message": "Success" }
```

**Response `400`:**
```json
{ "error": "Not in wishlist" }
```

#### L?y danh sách Wishlist — `GET /api/wishlist`

**Response `200 OK`:** `BikePostDto[]` — danh sách xe yêu thích

#### Ki?m tra trong Wishlist — `GET /api/wishlist/{listingId}/check`

**Response `200 OK`:** `true` ho?c `false`

---

### 2.3. Cart APIs (?? Yêu c?u ??ng nh?p)

> Header: `Authorization: Bearer <jwt_token>`

#### Thêm vào Cart — `POST /api/cart/{listingId}`

**Response `200 OK`:**
```json
{ "message": "Success" }
```

**Response `400`:**

| Error | Nguyên nhân |
|-------|-------------|
| `"Listing not found"` | Listing ID không t?n t?i |
| `"Listing is not available"` | Listing status ? 1 (Active) |
| `"You cannot add your own listing to cart"` | Seller không th? t? thêm bài c?a mình |
| `"Already in cart"` | ?ã có trong cart r?i |

#### Xoá kh?i Cart — `DELETE /api/cart/{listingId}`

**Response `200 OK`:**
```json
{ "message": "Success" }
```

**Response `400`:**
```json
{ "error": "Not in cart" }
```

#### L?y danh sách Cart — `GET /api/cart`

**Response `200 OK`:** `BikePostDto[]`

#### Xoá toàn b? Cart — `DELETE /api/cart`

**Response `200 OK`:**
```json
{ "message": "Success" }
```

#### Ki?m tra trong Cart — `GET /api/cart/{listingId}/check`

**Response `200 OK`:** `true` ho?c `false`

#### ??m s? item trong Cart — `GET /api/cart/count`

**Response `200 OK`:** `number` (VD: `3`)

---

### ?? FE Implementation — Trang chi ti?t & Wishlist/Cart

#### Axios Services

```typescript
// src/services/wishlistService.ts
import api from './api';

export const wishlistService = {
  getMyWishlist: () =>
    api.get<BikePostDto[]>('/api/wishlist'),

  add: (listingId: number) =>
    api.post(`/api/wishlist/${listingId}`),

  remove: (listingId: number) =>
    api.delete(`/api/wishlist/${listingId}`),

  check: (listingId: number) =>
    api.get<boolean>(`/api/wishlist/${listingId}/check`),
};
```

```typescript
// src/services/cartService.ts
import api from './api';

export const cartService = {
  getMyCart: () =>
    api.get<BikePostDto[]>('/api/cart'),

  add: (listingId: number) =>
    api.post(`/api/cart/${listingId}`),

  remove: (listingId: number) =>
    api.delete(`/api/cart/${listingId}`),

  clear: () =>
    api.delete('/api/cart'),

  check: (listingId: number) =>
    api.get<boolean>(`/api/cart/${listingId}/check`),

  count: () =>
    api.get<number>('/api/cart/count'),
};
```

#### Bike Detail Page

```tsx
// src/pages/BikeDetailPage.tsx
import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { bikeSearchService } from '../services/bikeSearchService';
import { wishlistService } from '../services/wishlistService';
import { cartService } from '../services/cartService';
import { useAuth } from '../hooks/useAuth'; // hook ki?m tra ??ng nh?p

const BikeDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAuth();

  const [listing, setListing] = useState<BikePostDto | null>(null);
  const [isInWishlist, setIsInWishlist] = useState(false);
  const [isInCart, setIsInCart] = useState(false);
  const [loading, setLoading] = useState(true);

  // Load chi ti?t xe
  useEffect(() => {
    const load = async () => {
      try {
        const res = await bikeSearchService.getDetail(Number(id));
        setListing(res.data);
      } catch (err: any) {
        if (err.response?.status === 400) {
          navigate('/404');
        }
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [id]);

  // Ki?m tra tr?ng thái wishlist & cart (n?u ?ã ??ng nh?p)
  useEffect(() => {
    if (!isAuthenticated || !id) return;

    const checkStatus = async () => {
      try {
        const [wishRes, cartRes] = await Promise.all([
          wishlistService.check(Number(id)),
          cartService.check(Number(id)),
        ]);
        setIsInWishlist(wishRes.data);
        setIsInCart(cartRes.data);
      } catch {
        // B? qua l?i — user có th? ch?a login
      }
    };
    checkStatus();
  }, [id, isAuthenticated]);

  // Toggle Wishlist
  const handleToggleWishlist = async () => {
    if (!isAuthenticated) return navigate('/login');
    try {
      if (isInWishlist) {
        await wishlistService.remove(listing!.listingId);
        setIsInWishlist(false);
      } else {
        await wishlistService.add(listing!.listingId);
        setIsInWishlist(true);
      }
    } catch (err: any) {
      alert(err.response?.data?.error || 'L?i x? lý wishlist');
    }
  };

  // Add to Cart
  const handleAddToCart = async () => {
    if (!isAuthenticated) return navigate('/login');
    try {
      await cartService.add(listing!.listingId);
      setIsInCart(true);
    } catch (err: any) {
      alert(err.response?.data?.error || 'L?i thêm vào gi? hàng');
    }
  };

  // Remove from Cart
  const handleRemoveFromCart = async () => {
    try {
      await cartService.remove(listing!.listingId);
      setIsInCart(false);
    } catch (err: any) {
      alert(err.response?.data?.error || 'L?i xoá kh?i gi? hàng');
    }
  };

  if (loading) return <div>?ang t?i...</div>;
  if (!listing) return <div>Không tìm th?y xe</div>;

  const isOwner = user?.userId === listing.sellerId;

  return (
    <div className="bike-detail">
      {/* ===== IMAGE GALLERY ===== */}
      <BikeImageGallery images={listing.images} />

      {/* ===== THÔNG TIN CHÍNH ===== */}
      <div className="detail-info">
        <h1>{listing.title}</h1>
        <p className="price">{listing.price.toLocaleString('vi-VN')}?</p>
        <p className="quantity">
          {listing.quantity > 0 ? `Còn ${listing.quantity} chi?c` : 'H?t hàng'}
        </p>

        {/* Action buttons — ?n n?u là ch? bài ??ng */}
        {!isOwner && (
          <div className="actions">
            <button
              className={`btn-wishlist ${isInWishlist ? 'active' : ''}`}
              onClick={handleToggleWishlist}
            >
              {isInWishlist ? '?? ?ã thích' : '?? Yêu thích'}
            </button>

            {isInCart ? (
              <button className="btn-cart in-cart" onClick={handleRemoveFromCart}>
                ? ?ã trong gi? — B? ra
              </button>
            ) : (
              <button
                className="btn-cart"
                onClick={handleAddToCart}
                disabled={listing.quantity === 0}
              >
                ?? Thêm vào gi?
              </button>
            )}
          </div>
        )}

        {/* Thông tin xe */}
        <div className="bike-specs">
          <h2>Thông s? xe</h2>
          <table>
            <tbody>
              {listing.brandName && <tr><td>Hãng</td><td>{listing.brandName}</td></tr>}
              {listing.typeName && <tr><td>Lo?i</td><td>{listing.typeName}</td></tr>}
              {listing.modelName && <tr><td>Model</td><td>{listing.modelName}</td></tr>}
              {listing.condition && <tr><td>Tình tr?ng</td><td>{listing.condition}</td></tr>}
              {listing.color && <tr><td>Màu s?c</td><td>{listing.color}</td></tr>}
              {listing.frameSize && <tr><td>Size khung</td><td>{listing.frameSize}</td></tr>}
              {listing.frameMaterial && <tr><td>Ch?t li?u khung</td><td>{listing.frameMaterial}</td></tr>}
              {listing.wheelSize && <tr><td>C? bánh</td><td>{listing.wheelSize}</td></tr>}
              {listing.brakeType && <tr><td>Phanh</td><td>{listing.brakeType}</td></tr>}
              {listing.weight && <tr><td>Tr?ng l??ng</td><td>{listing.weight} kg</td></tr>}
              {listing.transmission && <tr><td>H? truy?n ??ng</td><td>{listing.transmission}</td></tr>}
              {listing.serialNumber && <tr><td>Serial</td><td>{listing.serialNumber}</td></tr>}
            </tbody>
          </table>
        </div>

        {/* Mô t? */}
        {listing.description && (
          <div className="description">
            <h2>Mô t?</h2>
            <p>{listing.description}</p>
          </div>
        )}

        {/* Thông tin ng??i bán */}
        <div className="seller-info">
          <h2>Ng??i bán</h2>
          <p>{listing.sellerName}</p>
          <p>{listing.address}</p>
        </div>

        {/* Badge ki?m ??nh */}
        {listing.hasInspection && (
          <div className="inspection-badge">
            ? Xe ?ã ???c ki?m ??nh b?i SecondBike
          </div>
        )}
      </div>
    </div>
  );
};
```

#### Image Gallery Component

```tsx
// src/components/BikeImageGallery.tsx
import { useState } from 'react';

const BikeImageGallery = ({ images }: { images: BikeImageDto[] }) => {
  const [activeIndex, setActiveIndex] = useState(0);

  // S?p x?p: thumbnail lên ??u
  const sorted = [...images].sort(
    (a, b) => (b.isThumbnail ? 1 : 0) - (a.isThumbnail ? 1 : 0)
  );

  if (sorted.length === 0) {
    return <img src="/placeholder-bike.png" alt="No image" className="main-image" />;
  }

  return (
    <div className="gallery">
      <img
        src={sorted[activeIndex]?.mediaUrl}
        alt="Bike"
        className="main-image"
      />
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

#### Wishlist Page

```tsx
// src/pages/WishlistPage.tsx
import { useState, useEffect } from 'react';
import { wishlistService } from '../services/wishlistService';

const WishlistPage = () => {
  const [items, setItems] = useState<BikePostDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    wishlistService.getMyWishlist()
      .then(res => setItems(res.data))
      .catch(() => setItems([]))
      .finally(() => setLoading(false));
  }, []);

  const handleRemove = async (listingId: number) => {
    try {
      await wishlistService.remove(listingId);
      setItems(prev => prev.filter(item => item.listingId !== listingId));
    } catch (err: any) {
      alert(err.response?.data?.error || 'L?i xoá kh?i danh sách yêu thích');
    }
  };

  if (loading) return <div>?ang t?i...</div>;

  return (
    <div className="wishlist-page">
      <h1>Danh sách yêu thích ({items.length})</h1>
      {items.length === 0 ? (
        <p>Ch?a có xe nào trong danh sách yêu thích</p>
      ) : (
        <div className="listing-grid">
          {items.map(item => (
            <div key={item.listingId} className="wishlist-item">
              <BikeCard listing={item} />
              <button onClick={() => handleRemove(item.listingId)}>
                Xoá kh?i yêu thích
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
```

#### Cart Page

```tsx
// src/pages/CartPage.tsx
import { useState, useEffect } from 'react';
import { cartService } from '../services/cartService';

const CartPage = () => {
  const [items, setItems] = useState<BikePostDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    cartService.getMyCart()
      .then(res => setItems(res.data))
      .catch(() => setItems([]))
      .finally(() => setLoading(false));
  }, []);

  const handleRemove = async (listingId: number) => {
    try {
      await cartService.remove(listingId);
      setItems(prev => prev.filter(item => item.listingId !== listingId));
    } catch (err: any) {
      alert(err.response?.data?.error || 'L?i xoá kh?i gi? hàng');
    }
  };

  const handleClearAll = async () => {
    if (!window.confirm('Xoá toàn b? gi? hàng?')) return;
    try {
      await cartService.clear();
      setItems([]);
    } catch {
      alert('L?i xoá gi? hàng');
    }
  };

  const totalPrice = items.reduce((sum, item) => sum + item.price, 0);

  if (loading) return <div>?ang t?i...</div>;

  return (
    <div className="cart-page">
      <h1>Gi? hàng ({items.length})</h1>

      {items.length === 0 ? (
        <p>Gi? hàng tr?ng</p>
      ) : (
        <>
          <div className="cart-items">
            {items.map(item => (
              <div key={item.listingId} className="cart-item">
                <BikeCard listing={item} />
                <button onClick={() => handleRemove(item.listingId)}>Xoá</button>
              </div>
            ))}
          </div>

          <div className="cart-summary">
            <p>T?ng: <strong>{totalPrice.toLocaleString('vi-VN')}?</strong></p>
            <button onClick={handleClearAll} className="btn-clear">Xoá t?t c?</button>
            <button className="btn-checkout">Ti?n hành ??t hàng</button>
          </div>
        </>
      )}
    </div>
  );
};
```

#### Cart Badge (Header)

```tsx
// src/components/CartBadge.tsx — Hi?n th? s? l??ng cart ? header
import { useState, useEffect } from 'react';
import { cartService } from '../services/cartService';
import { useAuth } from '../hooks/useAuth';

const CartBadge = () => {
  const { isAuthenticated } = useAuth();
  const [count, setCount] = useState(0);

  useEffect(() => {
    if (!isAuthenticated) return;
    cartService.count().then(res => setCount(res.data));
  }, [isAuthenticated]);

  return (
    <div className="cart-badge">
      ??
      {count > 0 && <span className="badge-count">{count}</span>}
    </div>
  );
};
```

---

## ??? React Router Setup

```tsx
// src/App.tsx ho?c src/router.tsx
import { BrowserRouter, Routes, Route } from 'react-router-dom';

<BrowserRouter>
  <Routes>
    {/* UC1: Public */}
    <Route path="/bikes" element={<BikeSearchPage />} />

    {/* UC2: Public detail */}
    <Route path="/bikes/:id" element={<BikeDetailPage />} />

    {/* UC2: Protected */}
    <Route element={<ProtectedRoute />}>
      <Route path="/wishlist" element={<WishlistPage />} />
      <Route path="/cart" element={<CartPage />} />
    </Route>
  </Routes>
</BrowserRouter>
```

---

## ?? L?U Ý QUAN TR?NG

### 1. Khác bi?t Wishlist vs Cart

| | Wishlist | Cart |
|--|---------|------|
| M?c ?ích | L?u xe yêu thích ?? xem sau | Chu?n b? mua hàng |
| Ki?m tra status | Không ki?m tra `ListingStatus` | Ch? cho thêm listing `Active` (status=1) |
| Clear all | ? Không có | ? `DELETE /api/cart` |
| Count | ? Không có | ? `GET /api/cart/count` |
| Ch?n t? thêm | ? Seller không th? t? thêm | ? Seller không th? t? thêm |

### 2. X? lý l?i Auth

- T?t c? API Wishlist/Cart yêu c?u JWT token trong header.
- N?u token h?t h?n ? BE tr? `401` ? FE redirect v? trang login.
- N?u ch?a login mà b?m nút Wishlist/Cart ? FE t? redirect `/login` tr??c khi g?i API.

### 3. Hi?u su?t tìm ki?m (BE ?ã t?i ?u)

- **Eager loading:** BE dùng `Include/ThenInclude` load t?t c? quan h? trong 1 query (Bike ? Brand, Type, BicycleDetail, Seller, ListingMedia, InspectionRequests).
- **Server-side pagination:** Ch? load `pageSize` items m?i l?n, không load toàn b? DB.
- **Server-side filtering:** T?t c? filter ch?y trên DB, FE ch? c?n g?i params.
- **FE không c?n filter/sort client-side** — m?i logic ?ã x? lý ? BE.

### 4. Debounce cho search

```typescript
// Nên debounce searchTerm ?? tránh g?i API liên t?c khi user gõ
import { useDebouncedCallback } from 'use-debounce';

const debouncedSearch = useDebouncedCallback((term: string) => {
  updateFilter({ searchTerm: term });
}, 500);
```

### 5. Sync URL v?i Filter State

- Dùng `useSearchParams` ?? sync filter state v?i URL ? user có th? share link k?t qu? tìm ki?m, b?m Back v?n gi? filter.

---

## ? FE Checklist

### UC 1 — Tìm ki?m & L?c
- [ ] G?i `GET /api/bikes` v?i query params
- [ ] Lo?i b? params null/undefined tr??c khi g?i
- [ ] Hi?n th? pagination (page, totalPages, hasPrevious, hasNext)
- [ ] Sync filter state v?i URL search params
- [ ] Debounce cho ô tìm ki?m
- [ ] G?i brands/types 1 l?n khi mount, cache l?i
- [ ] Hi?n th? thumbnail ?úng (?u tiên `isThumbnail: true`)
- [ ] Hi?n th? badge "H?t hàng" khi `quantity = 0`
- [ ] Hi?n th? badge "?ã ki?m ??nh" khi `hasInspection = true`
- [ ] Format giá VND b?ng `toLocaleString('vi-VN')`

### UC 2 — Chi ti?t & Wishlist & Cart
- [ ] G?i `GET /api/bikes/{id}` ?? load chi ti?t
- [ ] X? lý 400 ? redirect 404
- [ ] G?i `check` Wishlist + Cart khi load detail (n?u ?ã login)
- [ ] Toggle Wishlist button (add/remove)
- [ ] Add to Cart / Remove from Cart button
- [ ] ?n nút Wishlist/Cart n?u user là ch? bài ??ng (`sellerId === currentUserId`)
- [ ] Disable nút "Thêm vào gi?" n?u `quantity = 0`
- [ ] Hi?n th? ??y ?? thông s? xe trong b?ng specs
- [ ] Image gallery v?i thumbnail selector
- [ ] Cart badge ? header hi?n th? count
- [ ] Trang Wishlist: danh sách + nút xoá t?ng item
- [ ] Trang Cart: danh sách + xoá t?ng item + xoá t?t c? + t?ng ti?n
- [ ] Redirect `/login` n?u ch?a ??ng nh?p mà b?m Wishlist/Cart
