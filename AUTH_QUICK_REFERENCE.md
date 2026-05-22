# 🎯 QUICK REFERENCE - AUTHENTICATION & AUTHORIZATION

## How the System Works Now

### 1️⃣ **User Registration**
```
User → Register Form → API (POST /api/auth/register) → DB
```
- Validates email uniqueness
- Hashes password with BCrypt
- Stores user role

### 2️⃣ **User Login**
```
User → Login Form → API (POST /api/auth/login) → JWT Token
→ Session Storage → Role-based Redirect
```
- Validates credentials
- Generates JWT token (1 hour expiry)
- Extracts UserId from token
- Stores in session:
  - Token
  - Role
  - FullName
  - Email
  - UserId
- Redirects based on role:
  - Admin → /Admin/Index
  - Manager → /Manager/Index
  - User → /User/Index

### 3️⃣ **Protected Routes**
```
User makes request → Session check → Token validation
→ API Authorization header added → Protected Resource
```

### 4️⃣ **Document Upload (Secure)**
```
User (Logged In) → Upload Form → UserController
→ Get Token from Session → Set Auth Header
→ API DocumentController → Verify Token & User ID
→ Save with UploadedByUserId → Success
```

---

## Key Files & Their Responsibilities

### Authentication
- **API**: `DocLocker.API/Controllers/AuthController.cs`
  - Register endpoint
  - Login endpoint
  - JWT token generation

- **Web**: `DocLocker.Web/Controllers/AccountController.cs`
  - Register page
  - Login page
  - Logout (clears session)

### Authorization
- **API**: `DocLocker.API/Controllers/DocumentController.cs`
  - `[Authorize]` attribute protects all endpoints
  - Extracts UserId from JWT claims

- **Web**: All role-specific controllers
  - UserController (any authenticated user)
  - ManagerController (managers only)
  - AdminController (admins only)

### Session Management
- **Program.cs** (Web)
  - 30-minute timeout
  - HttpOnly cookies
  - Essential flag

---

## Security Features

✅ **JWT Authentication**
- Token expires in 1 hour
- Signed with secure key
- Includes UserId in claims

✅ **Password Security**
- BCrypt hashing
- 6+ character requirement
- Comparison validation

✅ **Authorization**
- [Authorize] attributes on API
- Session checks on Web controllers
- Role-based redirects

✅ **CORS Protection**
- Only allows Web app origins
- Prevents cross-origin attacks

✅ **Anti-Forgery**
- CSRF token on all POST forms

---

## Session Data Structure

After login, user session contains:

```csharp
Session["Token"]    = "eyJhbGciOiJIUzI1NiIs..." // JWT token
Session["Role"]     = "User/Manager/Admin"        // User role
Session["FullName"] = "John Doe"                  // Full name
Session["Email"]    = "john@example.com"          // Email
Session["UserId"]   = "123"                       // User ID
```

---

## Checking User Status in Views

```razor
@{
    var role = Context.Session.GetString("Role");
    var token = Context.Session.GetString("Token");
}

@if (string.IsNullOrEmpty(role))
{
    // Not logged in → Show login/register
}
else
{
    // Logged in → Show dashboard & sidebar
}
```

---

## Adding Authorization Header to API Calls

```csharp
private void SetAuthorizationHeader()
{
    var token = HttpContext.Session.GetString("Token");
    if (!string.IsNullOrEmpty(token))
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }
}

// Then use in API call
SetAuthorizationHeader();
var response = await _httpClient.PostAsync("api/endpoint", content);
```

---

## Navigation Rules

### Before Login
- User sees: Home, Features, How It Works, Login, Register

### After Login (User Role)
- Sidebar: Dashboard, Upload Document, My Documents
- Navbar: Role badge, Logout

### After Login (Manager Role)
- Sidebar: Dashboard, Pending Approvals
- Navbar: Role badge, Logout

### After Login (Admin Role)
- Sidebar: Dashboard, Users
- Navbar: Role badge, Logout

---

## Error Handling

**Unauthorized Access:**
```
If Session["Token"] is null/empty
→ Redirect to Account/Login
```

**Token Expired (on API):**
```
If JWT validation fails
→ Return 401 Unauthorized
→ Web detects 401
→ Redirect to Account/Login
```

**Invalid Role Access:**
```
If User tries to access Manager/Admin routes
→ Check role in session
→ Redirect to appropriate role's dashboard
```

---

## Testing the System

### 1. Register New User
1. Go to Register page
2. Fill form with valid data
3. Should show "Registration successful! Please log in."
4. Auto-redirect to Login

### 2. Login
1. Go to Login page
2. Enter credentials
3. Should show success toast
4. Auto-redirect to role's dashboard

### 3. Upload Document
1. Login as User
2. Go to Upload Document
3. Fill title and select file
4. Click Upload
5. Should show success message

### 4. Access Control
1. Login as User
2. Try to access /Manager/Index
3. Should see "Access denied" or redirect to User dashboard

---

## Configuration Files to Check

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=DocLocker;..."
  },
  "Jwt": {
    "Key": "your-256-bit-secret-key-here-minimum-32-characters"
  },
  "ApiSettings": {
    "BaseUrl": "https://localhost:7261/"
  }
}
```

---

## Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| "Token not found in header" | Check `SetAuthorizationHeader()` is called |
| "Unauthorized 401" | Token expired or invalid - re-login |
| "Role redirect not working" | Check `Context.Session.GetString("Role")` |
| "CORS error" | Verify CORS policy includes Web app origin |
| "Session expires too quickly" | Check timeout setting in Program.cs |

---

## Next Steps for Development

1. **Create API Endpoints** for document operations
2. **Implement Frontend APIs** - Connect views to endpoints
3. **Add Document Management** - Approve/Reject/Download
4. **Add Notifications** - Email alerts for approvals
5. **Add Audit Logging** - Track all actions
