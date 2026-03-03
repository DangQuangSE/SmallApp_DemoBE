# ?? FE Guide: Register / Login with OTP Email Verification

> Full flow: Register ? Receive OTP via email ? Enter OTP ? Login.
>
> **Base URL:** `http://localhost:5xxx` (check your BE port)
>
> **All endpoints prefix:** `/api/auth`

---

## ?? Flow Overview

```
????????????????     POST /api/auth/register       ????????????????
?   FE React   ? ?????????????????????????????????? ?   Backend    ?
?  (Register)  ? ?????????????????????????????????? ?              ?
?              ?  succeeded=true, token=null         ?  ? Send OTP  ?
?              ?  requiresEmailConfirmation=true     ?    to email   ?
????????????????                                    ????????????????
       ?  redirect ? /verify-email
       ?
????????????????  User opens email ? copies 6-digit code
?   FE React   ?
?  (OTP Input) ?
????????????????
       ?  user enters OTP
       ?
????????????????     POST /api/auth/confirm-email   ????????????????
?   FE React   ?     { email, otp: "123456" }       ?   Backend    ?
?   (Verify)   ? ?????????????????????????????????? ?  IsVerified  ?
?              ? ?????????????????????????????????? ?  = true      ?
?              ?  "Email confirmed successfully!"    ????????????????
????????????????
       ?  redirect ? /login
       ?
????????????????     POST /api/auth/login           ????????????????
?   FE React   ? ?????????????????????????????????? ?   Backend    ?
?   (Login)    ? ?????????????????????????????????? ?  ? JWT token ?
?              ?  succeeded=true, token="eyJ..."     ????????????????
????????????????
```

---

## ?? Step 1: Register — `POST /api/auth/register`

### Request
```http
POST /api/auth/register
Content-Type: application/json
```

```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "MyPass@123",
  "fullName": "John Doe",
  "phoneNumber": "0901234567",
  "roleId": 2
}
```

### Field Validation

| Field | Type | Required | Validation Rules |
|---|---|---|---|
| `username` | string | ? | Not empty, max 50 chars, must be unique |
| `email` | string | ? | Not empty, valid email format, must be unique |
| `password` | string | ? | Min 8 chars + uppercase + lowercase + digit + special char |
| `fullName` | string | ? | Max 100 chars |
| `phoneNumber` | string | ? | Max 20 chars |
| `roleId` | number | ? | Must be > 0. Values: `2`=Buyer (default), `3`=Seller |

### Response — Success `200 OK`
```json
{
  "succeeded": true,
  "token": null,
  "user": {
    "userId": 1,
    "username": "john_doe",
    "email": "john@example.com",
    "fullName": "John Doe",
    "phoneNumber": "0901234567",
    "avatarUrl": null,
    "address": null,
    "roleName": "Buyer",
    "status": 1,
    "isVerified": false,
    "createdAt": "2025-06-20T10:00:00Z"
  },
  "errorMessage": null,
  "requiresEmailConfirmation": true
}
```

> ?? `token` = `null`. **DO NOT** save any token. Redirect to OTP page.

### Response — Error `400`
```json
{
  "error": "Email already registered",
  "errors": []
}
```

| Error Message | Cause |
|---|---|
| `"Email already registered"` | Email exists in DB |
| `"Username already taken"` | Username exists in DB |
| `"Invalid role"` | `roleId` not found in `UserRole` table |

### FE Logic
```tsx
const handleRegister = async (formData: RegisterDto) => {
  try {
    const res = await api.post('/api/auth/register', formData);
    const result: AuthResultDto = res.data;

    if (result.requiresEmailConfirmation) {
      // DO NOT save token — it's null
      navigate('/verify-email', { state: { email: formData.email } });
    }
  } catch (err) {
    setError(err.response?.data?.error || 'Registration failed');
  }
};
```

---

## ?? Step 2: Verify OTP — `POST /api/auth/confirm-email`

After register, user receives an email with a **6-digit OTP code** (expires in **10 minutes**).

### UI Layout
```
???????????????????????????????????????????
?                                         ?
?   ?? Verify Your Email                 ?
?                                         ?
?   We sent a 6-digit code to:           ?
?   john@example.com                      ?
?                                         ?
?   ?????????????????????????           ?
?   ?   ?   ?   ?   ?   ?   ?           ?
?   ?????????????????????????           ?
?                                         ?
?   [     Verify Email     ]              ?
?                                         ?
?   Code expires in 10 minutes.           ?
?   Didn't receive the code?              ?
?   [Resend Code]                         ?
?                                         ?
???????????????????????????????????????????
```

### Request
```http
POST /api/auth/confirm-email
Content-Type: application/json
```

```json
{
  "email": "john@example.com",
  "otp": "123456"
}
```

| Field | Type | Required | Note |
|---|---|---|---|
| `email` | string | ? | The email used during registration |
| `otp` | string | ? | 6-digit code from email |

### Response — Success `200 OK`
```json
{
  "message": "Email confirmed successfully!"
}
```

### Response — Error `400`
```json
{
  "error": "Invalid verification code"
}
```

| Error Message | Cause |
|---|---|
| `"User not found"` | Email doesn't exist |
| `"Email is already confirmed"` | Already verified |
| `"Invalid verification code"` | Wrong OTP |
| `"Verification code has expired. Please request a new one"` | OTP expired (10 min) |

### FE Logic
```tsx
const handleVerify = async () => {
  try {
    const res = await api.post('/api/auth/confirm-email', { email, otp });
    // Success ? redirect to login
    navigate('/login', { state: { message: 'Email verified! Please log in.' } });
  } catch (err) {
    setError(err.response?.data?.error || 'Verification failed');
  }
};
```

---

## ?? Step 2b: Resend OTP — `POST /api/auth/resend-confirmation`

### Request
```http
POST /api/auth/resend-confirmation
Content-Type: application/json
```

```json
{
  "email": "john@example.com"
}
```

### Response — Success `200 OK`
```json
{
  "message": "Success"
}
```

### Response — Error `400`
```json
{
  "error": "Email is already confirmed"
}
```

| Error Message | Cause |
|---|---|
| `"User not found"` | Email doesn't exist |
| `"Email is already confirmed"` | Already verified |

### FE Logic
```tsx
const handleResend = async () => {
  try {
    await api.post('/api/auth/resend-confirmation', { email });
    setMessage('A new code has been sent!');
    setCountdown(60); // Disable button for 60s to prevent spam
  } catch (err) {
    setError(err.response?.data?.error || 'Failed to resend code');
  }
};
```

---

## ?? Step 3: Login — `POST /api/auth/login`

### Request
```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "email": "john@example.com",
  "password": "MyPass@123"
}
```

| Field | Type | Required |
|---|---|---|
| `email` | string | ? |
| `password` | string | ? |

### Response — Success `200 OK`
```json
{
  "succeeded": true,
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "userId": 1,
    "username": "john_doe",
    "email": "john@example.com",
    "fullName": "John Doe",
    "phoneNumber": "0901234567",
    "avatarUrl": null,
    "address": null,
    "roleName": "Buyer",
    "status": 1,
    "isVerified": true,
    "createdAt": "2025-06-20T10:00:00Z"
  },
  "errorMessage": null,
  "requiresEmailConfirmation": false
}
```

### Response — Email NOT verified `200 OK` (but `succeeded = false`)

> ?? This is a **200** response, NOT 400. Check `succeeded` and `requiresEmailConfirmation`.

```json
{
  "succeeded": false,
  "token": null,
  "user": {
    "userId": 1,
    "email": "john@example.com",
    "username": "john_doe"
  },
  "errorMessage": "Please verify your email before logging in",
  "requiresEmailConfirmation": true
}
```

### Response — Wrong credentials `400`
```json
{
  "error": "Invalid email or password",
  "errors": []
}
```

### Response — Banned `400`
```json
{
  "error": "Your account has been banned",
  "errors": []
}
```

### FE Logic
```tsx
const handleLogin = async (formData: LoginDto) => {
  try {
    const res = await api.post('/api/auth/login', formData);
    const result: AuthResultDto = res.data;

    // Case 1: Email not verified ? redirect to OTP page
    if (result.requiresEmailConfirmation) {
      navigate('/verify-email', { state: { email: formData.email } });
      return;
    }

    // Case 2: Login success ? save token, redirect home
    if (result.succeeded && result.token) {
      localStorage.setItem('token', result.token);
      localStorage.setItem('user', JSON.stringify(result.user));
      navigate('/');
    }
  } catch (err) {
    // Case 3: 400 errors (wrong password, banned)
    setError(err.response?.data?.error || 'Login failed');
  }
};
```

---

## ?? Step 4: Profile — `GET` & `PUT /api/auth/profile`

### Get Profile (?? Requires JWT)
```http
GET /api/auth/profile
Authorization: Bearer {token}
```

### Response `200 OK`
```json
{
  "userId": 1,
  "username": "john_doe",
  "email": "john@example.com",
  "fullName": "John Doe",
  "phoneNumber": "0901234567",
  "avatarUrl": null,
  "address": null,
  "roleName": "Buyer",
  "status": 1,
  "isVerified": true,
  "createdAt": "2025-06-20T10:00:00Z"
}
```

### Update Profile (?? Requires JWT)
```http
PUT /api/auth/profile
Authorization: Bearer {token}
Content-Type: application/json
```

```json
{
  "fullName": "John Updated",
  "phoneNumber": "0909999999",
  "avatarUrl": "https://example.com/avatar.jpg",
  "address": "123 Main Street"
}
```

> All fields are optional. Only non-null fields will be updated.

### Logout (?? Requires JWT)
```http
POST /api/auth/logout
Authorization: Bearer {token}
```

Response: `{ "message": "Logged out successfully" }`

---

## ?? TypeScript Interfaces

```typescript
// === Request DTOs ===

interface RegisterDto {
  username: string;
  email: string;
  password: string;
  fullName?: string;
  phoneNumber?: string;
  roleId: number; // 2=Buyer, 3=Seller
}

interface LoginDto {
  email: string;
  password: string;
}

interface ConfirmEmailDto {
  email: string;
  otp: string; // 6-digit code
}

interface ResendConfirmationDto {
  email: string;
}

interface UpdateProfileDto {
  fullName?: string;
  phoneNumber?: string;
  avatarUrl?: string;
  address?: string;
}

// === Response DTOs ===

interface AuthResultDto {
  succeeded: boolean;
  token?: string;          // null on register or unverified
  user?: UserProfileDto;
  errorMessage?: string;
  requiresEmailConfirmation: boolean;
}

interface UserProfileDto {
  userId: number;
  username: string;
  email: string;
  fullName?: string;
  phoneNumber?: string;
  avatarUrl?: string;
  address?: string;
  roleName: string;        // "Admin" | "Buyer" | "Seller" | "Inspector"
  status?: number;         // 0=Banned, 1=Active
  isVerified?: boolean;
  createdAt?: string;
}
```

---

## ??? React Router

```tsx
<Route path="/register" element={<RegisterPage />} />
<Route path="/login" element={<LoginPage />} />
<Route path="/verify-email" element={<VerifyEmailPage />} />
```

---

## ?? Axios API Service

```typescript
import axios from 'axios';

const API_BASE = 'http://localhost:5xxx'; // your BE port

const api = axios.create({
  baseURL: API_BASE,
  headers: { 'Content-Type': 'application/json' },
});

// Auto-attach JWT token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Auto-redirect on 401
api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(err);
  }
);

export const authService = {
  register:           (data: RegisterDto) => api.post<AuthResultDto>('/api/auth/register', data),
  login:              (data: LoginDto) => api.post<AuthResultDto>('/api/auth/login', data),
  confirmEmail:       (data: ConfirmEmailDto) => api.post('/api/auth/confirm-email', data),
  resendConfirmation: (email: string) => api.post('/api/auth/resend-confirmation', { email }),
  getProfile:         () => api.get<UserProfileDto>('/api/auth/profile'),
  updateProfile:      (data: UpdateProfileDto) => api.put<UserProfileDto>('/api/auth/profile', data),
  logout:             () => api.post('/api/auth/logout'),
};
```

---

## ?? API Endpoint Summary

| Method | Endpoint | Auth | Request Body | Response |
|---|---|---|---|---|
| `POST` | `/api/auth/register` | ? | `RegisterDto` | `AuthResultDto` |
| `POST` | `/api/auth/login` | ? | `LoginDto` | `AuthResultDto` |
| `POST` | `/api/auth/confirm-email` | ? | `ConfirmEmailDto` | `{ message }` |
| `POST` | `/api/auth/resend-confirmation` | ? | `{ email }` | `{ message }` |
| `GET` | `/api/auth/profile` | ?? | — | `UserProfileDto` |
| `PUT` | `/api/auth/profile` | ?? | `UpdateProfileDto` | `UserProfileDto` |
| `POST` | `/api/auth/logout` | ?? | — | `{ message }` |

---

## ? FE Checklist

### Pages to create
- [ ] `RegisterPage` — form: username, email, password, fullName, phoneNumber, roleId
- [ ] `LoginPage` — form: email, password
- [ ] `VerifyEmailPage` — 6-digit OTP input + Verify button + Resend button

### Logic to implement
- [ ] Register success ? redirect `/verify-email` with `email` in route state
- [ ] Login `requiresEmailConfirmation=true` ? redirect `/verify-email`
- [ ] Login `succeeded=true` + `token` ? save to localStorage, redirect `/`
- [ ] Verify OTP success ? redirect `/login` with success message
- [ ] Verify OTP error (expired) ? show "Resend Code" button
- [ ] Resend OTP ? call API + disable button 60 seconds
- [ ] FE password validation must match BE rules: min 8, uppercase, lowercase, digit, special char

### JWT handling
- [ ] Save `token` to `localStorage` on login success
- [ ] Attach `Authorization: Bearer {token}` header on every request
- [ ] Remove `token` + `user` from localStorage on logout
- [ ] Auto-redirect to `/login` on any `401` response
