# ?? Google Login Implementation Guide

## ?? T?ng quan

Google Login ?ã ???c **tri?n khai ??y ??** v?i các tính n?ng:
- ? Validate Google ID token qua Google.Apis.Auth library
- ? Auto-register user m?i t? Google account
- ? Auto-verify email (Google emails ?ã verified)
- ? Return JWT token gi?ng normal login
- ? Security checks (email verified, account not banned)
- ? FluentValidation cho input

---

## ??? Ki?n trúc & SOLID Principles

### Dependency Inversion (D)
```
AuthController (API Layer)
    ? depends on
IAuthService (Application Layer)  
    ? depends on
IGoogleTokenValidator (Application Layer - Interface)
    ? implemented by
GoogleTokenValidatorService (Infrastructure Layer)
```

**L?i ích:**
- Controller KHÔNG bi?t Google.Apis.Auth library t?n t?i
- Business logic (AuthService) KHÔNG bi?t infrastructure details
- Có th? swap implementation (VD: mock validator cho testing)

### Single Responsibility (S)
- `GoogleTokenValidatorService`: CH? validate token & extract user info
- `AuthService.GoogleLoginAsync()`: CH? x? lý business logic (find/create user, generate JWT)
- `GoogleLoginValidator`: CH? validate input DTO format

---

## ?? Backend Implementation Details

### 1. Enhanced `GoogleUserInfo` Record
```csharp
public record GoogleUserInfo(
    string Email,
    bool EmailVerified,      // ? NEW - Security check
    string Name,
    string? Picture,
    string GoogleId,         // ? NEW - Unique Google identifier (payload.Subject)
    string? GivenName,       // ? NEW - First name
    string? FamilyName,      // ? NEW - Last name
    string? Locale           // ? NEW - Language preference
);
```

### 2. `GoogleTokenValidatorService` Mapping
```csharp
public async Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken ct = default)
{
    var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
    
    return new GoogleUserInfo(
        Email: payload.Email,
        EmailVerified: payload.EmailVerified,  // ? Security check
        Name: payload.Name ?? payload.Email,
        Picture: payload.Picture,
        GoogleId: payload.Subject,             // Unique Google user ID
        GivenName: payload.GivenName,
        FamilyName: payload.FamilyName,
        Locale: payload.Locale
    );
}
```

### 3. `AuthService.GoogleLoginAsync()` Business Logic

#### Step 1: Validate Google ID Token
```csharp
var googleUser = await _googleValidator.ValidateAsync(dto.IdToken, ct);
if (googleUser is null)
    return Result<AuthResultDto>.Failure("Invalid or expired Google token");
```

#### Step 2: Security Check - Only Verified Emails
```csharp
if (!googleUser.EmailVerified)
    return Result<AuthResultDto>.Failure("Google email is not verified");
```

#### Step 3: Find or Create User
```csharp
var existingUsers = await _userRepo.FindAsync(u => u.Email == googleUser.Email, ct);
var user = existingUsers.FirstOrDefault();

if (user is not null)
{
    // Existing user - check ban status + auto-verify
    if (user.Status == 0)
        return Result<AuthResultDto>.Failure("Your account has been banned");
        
    if (user.IsVerified != true)
    {
        user.IsVerified = true;
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(ct);
    }
}
else
{
    // New user - Auto-register
    user = new User
    {
        Username = GenerateUsernameFromEmail(googleUser.Email), // e.g., "john.doe_1234"
        Email = googleUser.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Random password
        RoleId = 2,        // Buyer role
        IsVerified = true, // Google emails are pre-verified
        Status = 1,        // Active
        CreatedAt = DateTime.UtcNow
    };
    
    await _userRepo.AddAsync(user, ct);
    await _uow.SaveChangesAsync(ct);
    
    // Create profile from Google data
    var profile = new UserProfile
    {
        UserId = user.UserId,
        FullName = googleUser.Name,
        AvatarUrl = googleUser.Picture
    };
    await _profileRepo.AddAsync(profile, ct);
    await _uow.SaveChangesAsync(ct);
}
```

#### Step 4: Generate JWT & Return
```csharp
return Result<AuthResultDto>.Success(new AuthResultDto
{
    Succeeded = true,
    Token = _jwtService.GenerateToken(user, role?.RoleName ?? "Buyer"),
    User = MapToDto(user, userProfile, role?.RoleName ?? "Buyer")
});
```

---

## ?? Frontend Integration Guide

### API Endpoint
```
POST /api/auth/google
Content-Type: application/json

{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6..."
}
```

### React Example (Using @react-oauth/google)

#### 1. Install Dependencies
```bash
npm install @react-oauth/google
```

#### 2. Setup Google OAuth Provider
```tsx
// App.tsx
import { GoogleOAuthProvider } from '@react-oauth/google';

function App() {
  return (
    <GoogleOAuthProvider clientId="YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com">
      <YourAppComponents />
    </GoogleOAuthProvider>
  );
}
```

#### 3. Google Login Component
```tsx
import { GoogleLogin } from '@react-oauth/google';
import axios from 'axios';

function LoginPage() {
  const handleGoogleSuccess = async (credentialResponse) => {
    try {
      const response = await axios.post('https://your-api.com/api/auth/google', {
        idToken: credentialResponse.credential
      });
      
      if (response.data.succeeded) {
        // Store JWT token
        localStorage.setItem('token', response.data.token);
        
        // Store user info
        localStorage.setItem('user', JSON.stringify(response.data.user));
        
        // Navigate to dashboard
        navigate('/dashboard');
        
        toast.success(`Welcome ${response.data.user.fullName}!`);
      } else {
        toast.error(response.data.errorMessage || 'Login failed');
      }
    } catch (error) {
      console.error('Google login error:', error);
      toast.error(error.response?.data?.error || 'Network error occurred');
    }
  };

  const handleGoogleError = () => {
    toast.error('Google login was cancelled or failed');
  };

  return (
    <div>
      <h2>Login to SecondBike</h2>
      
      {/* Traditional login form */}
      <form>...</form>
      
      {/* Google login button */}
      <GoogleLogin
        onSuccess={handleGoogleSuccess}
        onError={handleGoogleError}
        theme="filled_blue"
        size="large"
        text="signin_with"
        shape="rectangular"
      />
    </div>
  );
}
```

### Response Structure
```json
{
  "succeeded": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "userId": 123,
    "username": "john.doe_5847",
    "email": "john.doe@gmail.com",
    "fullName": "John Doe",
    "avatarUrl": "https://lh3.googleusercontent.com/a/...",
    "roleName": "Buyer",
    "status": 1,
    "isVerified": true,
    "createdAt": "2024-01-20T10:30:00Z"
  },
  "requiresEmailConfirmation": false
}
```

### Error Responses
```json
// Invalid token
{
  "error": "Invalid or expired Google token"
}

// Unverified Google email (rare case)
{
  "error": "Google email is not verified"
}

// Banned account
{
  "error": "Your account has been banned"
}
```

---

## ?? Security Features

### 1. Token Validation
- Validates token signature using Google's public keys
- Checks token expiration (`exp` claim)
- Verifies audience (`aud`) matches your Google Client ID
- Checks issuer (`iss`) is `https://accounts.google.com`

### 2. Email Verification Check
```csharp
if (!googleUser.EmailVerified)
    return Result<AuthResultDto>.Failure("Google email is not verified");
```
**Why?** Prevents account takeover via unverified emails

### 3. Account Status Check
```csharp
if (user.Status == 0)
    return Result<AuthResultDto>.Failure("Your account has been banned");
```
**Why?** Banned users cannot login even with valid Google token

### 4. Random Password for OAuth Users
```csharp
PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString())
```
**Why?** OAuth users don't have traditional passwords, but DB requires PasswordHash field

---

## ?? Testing Guide

### Manual Testing via Swagger

1. Go to `https://localhost:{port}/swagger`
2. Find `/api/auth/google` endpoint
3. Click "Try it out"
4. Get a Google ID token from [Google OAuth Playground](https://developers.google.com/oauthplayground/)
5. Paste token into request body:
```json
{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6..."
}
```
6. Execute and verify response

### Unit Testing Scenarios

#### ? Happy Path - New User
```csharp
[Fact]
public async Task GoogleLogin_NewUser_ShouldAutoRegister()
{
    // Arrange
    var mockValidator = new Mock<IGoogleTokenValidator>();
    mockValidator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new GoogleUserInfo(...));
    
    // Act
    var result = await _authService.GoogleLoginAsync(dto, ct);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value.Token);
    Assert.Equal("Buyer", result.Value.User.RoleName);
}
```

#### ? Existing User
```csharp
[Fact]
public async Task GoogleLogin_ExistingUser_ShouldLogin()
{
    // Arrange - User already exists in DB
    
    // Act
    var result = await _authService.GoogleLoginAsync(dto, ct);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(existingUser.UserId, result.Value.User.UserId);
}
```

#### ? Invalid Token
```csharp
[Fact]
public async Task GoogleLogin_InvalidToken_ShouldReturnError()
{
    // Arrange
    mockValidator.Setup(...).ReturnsAsync((GoogleUserInfo?)null);
    
    // Act
    var result = await _authService.GoogleLoginAsync(dto, ct);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal("Invalid or expired Google token", result.ErrorMessage);
}
```

#### ? Banned User
```csharp
[Fact]
public async Task GoogleLogin_BannedUser_ShouldReturnError()
{
    // Arrange - User exists but status = 0 (banned)
    
    // Act
    var result = await _authService.GoogleLoginAsync(dto, ct);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("banned", result.ErrorMessage);
}
```

---

## ?? Best Practices Applied

### 1. **Separation of Concerns**
- Validation logic ? `GoogleTokenValidatorService`
- Business logic ? `AuthService`
- Input validation ? `GoogleLoginValidator`
- API routing ? `AuthController`

### 2. **Result Pattern (No Exceptions for Business Logic)**
```csharp
return Result<AuthResultDto>.Failure("Invalid token"); // ? Good
throw new InvalidTokenException("..."); // ? Avoid
```

### 3. **Dependency Injection**
```csharp
public AuthService(
    ...
    IGoogleTokenValidator googleValidator, // ? Inject interface
    ...
)
```

### 4. **Async/Await Properly**
```csharp
var googleUser = await _googleValidator.ValidateAsync(dto.IdToken, ct);
```

### 5. **Logging for Observability**
```csharp
_logger.LogInformation("New user auto-registered via Google: {Email}", googleUser.Email);
_logger.LogInformation("User logged in via Google: {Email}", googleUser.Email);
```

---

## ?? Configuration (appsettings.json)

```json
{
  "Google": {
    "ClientId": "123456789-abc.apps.googleusercontent.com"
  },
  "Jwt": {
    "Key": "your-super-secret-key-min-32-characters",
    "Issuer": "SecondBikeAPI",
    "Audience": "SecondBikeClient",
    "ExpireHours": 24
  }
}
```

**?? IMPORTANT:** Never commit `appsettings.json` with real ClientId to Git!

Use `appsettings.Development.json` for local development:
```json
{
  "Google": {
    "ClientId": "your-dev-client-id.apps.googleusercontent.com"
  }
}
```

---

## ?? Deployment Checklist

- [ ] Update `Google:ClientId` in production appsettings
- [ ] Add production domain to [Google Cloud Console](https://console.cloud.google.com/) ? Credentials ? Authorized JavaScript origins
- [ ] Add production redirect URI to Authorized redirect URIs
- [ ] Test Google Login on staging environment
- [ ] Monitor logs for failed login attempts
- [ ] Set up alerts for high failure rate

---

## ?? Common Issues & Solutions

### Issue 1: "Invalid or expired Google token"
**Cause:** Token expired (Google tokens expire after 1 hour) or ClientId mismatch

**Solution:** 
- Ensure frontend gets fresh token from Google
- Verify `Google:ClientId` in appsettings matches Google Cloud Console

### Issue 2: "Google email is not verified"
**Cause:** User's Google account email is not verified (rare)

**Solution:**
- Ask user to verify email in their Google account
- Or allow unverified emails (remove the check - not recommended)

### Issue 3: Username collision
**Cause:** Generated username (e.g., `john.doe_1234`) already exists

**Solution:** 
Current implementation uses random suffix (1000-9999), collision chance ~0.01%
If needed, add retry logic:
```csharp
private async Task<string> GenerateUniqueUsernameAsync(string email, CancellationToken ct)
{
    var prefix = email.Split('@')[0];
    for (int i = 0; i < 10; i++) // 10 attempts
    {
        var username = $"{prefix}_{Random.Shared.Next(1000, 9999)}";
        var exists = await _userRepo.AnyAsync(u => u.Username == username, ct);
        if (!exists) return username;
    }
    return $"{prefix}_{Guid.NewGuid():N}"; // Fallback to GUID
}
```

---

## ?? References

- [Google Sign-In for Web](https://developers.google.com/identity/sign-in/web)
- [Google.Apis.Auth NuGet](https://www.nuget.org/packages/Google.Apis.Auth/)
- [JWT Best Practices](https://datatracker.ietf.org/doc/html/rfc8725)
- [@react-oauth/google Docs](https://www.npmjs.com/package/@react-oauth/google)

---

## ? Summary

**?i?m m?nh:**
- ? Tuân th? SOLID principles (??c bi?t D và S)
- ? Comprehensive error handling
- ? Security-first approach (email verified check)
- ? Auto-registration UX
- ? Proper logging for debugging
- ? FluentValidation cho input
- ? Clean Architecture separation

**So v?i code c?:**
| Aspect | Before | After |
|--------|--------|-------|
| Implementation | Hardcoded error message | Full working implementation |
| SOLID | Violated DIP (no injection) | Follows all SOLID principles |
| Security | None | Email verification + account status checks |
| Payload | 3 fields (Email, Name, Picture) | 8 fields (complete Google info) |
| Error Handling | Basic | Comprehensive with specific error messages |
| Testing | Not testable | Fully mockable via interfaces |

Bây gi? b?n có m?t **production-ready Google Login implementation** ?áp ?ng chu?n enterprise! ??
