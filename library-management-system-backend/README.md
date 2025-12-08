# Library Management System - Backend API

A RESTful API built with ASP.NET Core for managing a library system with user authentication, book management, and JWT-based authorization.

## ?? Features

- **User Authentication**
  - User registration with email validation
  - JWT token-based login system
  - Secure password storage with BCrypt hashing

- **Book Management** (Requires Authentication)
  - Create, read, update, and delete books
  - Search and filter books
  - Track book details (title, author, description)

- **Security**
  - BCrypt password hashing (work factor: 11)
  - JWT authentication and authorization
  - CORS enabled for frontend integration
  - Environment-based configuration

## ??? Tech Stack

- **Framework:** ASP.NET Core 10.0
- **Database:** SQLite with Entity Framework Core
- **Authentication:** JWT Bearer Tokens
- **Password Hashing:** BCrypt.Net-Next
- **API Documentation:** OpenAPI/Swagger

## ?? Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [EF Core Tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet) (for database migrations)

## ?? Installation & Setup

### 1. Clone the Repository

```bash
git clone https://github.com/Ashan37/library-management-system-backend.git
cd library-management-system-backend
```

### 2. Install Dependencies

```bash
dotnet restore
```

### 3. Configure Database

The project uses SQLite and includes pre-configured migrations. To set up the database:

```bash
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create migration for password field (if not exists)
dotnet ef migrations add UpdatePasswordFieldLength

# Apply migrations
dotnet ef database update
```

This will create `library.db` in your project directory with the following tables:
- **Users** (id, name, email, password[hashed])
- **Books** (id, title, author, description)

### 4. Configure Application Settings

The `appsettings.json` file is already configured with:
- SQLite connection string
- JWT configuration (Key, Issuer, Audience)

**Note:** The JWT key is pre-configured for development. **Change this in production!**

### 5. Run the Application

```bash
dotnet run
```

The API will be available at:
- **HTTP:** http://localhost:5119
- **HTTPS:** https://localhost:7163

## ?? API Endpoints

### Authentication (Public)

#### Register User
```http
POST /api/Auth/register
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "message": "User registered successfully",
  "userId": 1,
  "name": "John Doe",
  "email": "john@example.com"
}
```

#### Login
```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1,
  "name": "John Doe",
  "email": "john@example.com"
}
```

### Books (Requires Authentication)

All book endpoints require a valid JWT token in the Authorization header:
```
Authorization: Bearer {your-jwt-token}
```

#### Get All Books
```http
GET /api/Book/getAllBooks
```

#### Get Book by ID
```http
GET /api/Book/getBook/{id}
```

#### Add New Book
```http
POST /api/Book/addBook
Content-Type: application/json

{
  "title": "The Great Gatsby",
  "author": "F. Scott Fitzgerald",
  "description": "A classic American novel"
}
```

#### Update Book
```http
PUT /api/Book/updateBook/{id}
Content-Type: application/json

{
  "id": 1,
  "title": "The Great Gatsby - Updated",
  "author": "F. Scott Fitzgerald",
  "description": "Updated description"
}
```

#### Delete Book
```http
DELETE /api/Book/deleteBook/{id}
```

## ?? Password Security Implementation

### BCrypt Hashing

This API uses **BCrypt** for secure password hashing. Passwords are never stored in plaintext.

#### Registration - Password Hashing
```csharp
[HttpPost("register")]
public async Task<IActionResult> Register(UserDTO userDTO)
{
    // Validate password length
    if (userDTO.password.Length < 6)
        return BadRequest(new { message = "Password must be at least 6 characters long" });

    // Check if user exists
    var existingUser = await _appDbContext.Users
        .FirstOrDefaultAsync(u => u.email == userDTO.email);

    if (existingUser != null)
        return BadRequest(new { message = "User with this email already exists" });

    // Hash the password using BCrypt
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDTO.password);

    var newUser = new User
    {
        name = userDTO.name,
        email = userDTO.email,
        password = hashedPassword  // Store hashed password
    };

    _appDbContext.Users.Add(newUser);
    await _appDbContext.SaveChangesAsync();

    return Ok(new { message = "User registered successfully", userId = newUser.id });
}
```

#### Login - Password Verification
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login(LoginDTO loginDTO)
{
    // Find user by email
    var user = await _appDbContext.Users
        .FirstOrDefaultAsync(u => u.email == loginDTO.email);

    // Verify password using BCrypt
    if (user == null || !BCrypt.Net.BCrypt.Verify(loginDTO.password, user.password))
        return Unauthorized(new { message = "Invalid email or password" });

    // Generate JWT token
    var token = GenerateJwtToken(user);

    return Ok(new { message = "Login successful", token, userId = user.id });
}
```

### Password Hash Example

**User Input:**
```
password123
```

**Stored in Database:**
```
$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy
```

### Security Features

- **Work Factor:** 11 (2^11 = 2,048 iterations)
- **Automatic Salting:** Each password gets a unique salt
- **One-way Hashing:** Cannot be decrypted
- **Constant-time Comparison:** Prevents timing attacks
- **Minimum Length:** 6 characters enforced

## ?? Authentication Flow

1. **Register** a new user via `/api/Auth/register`
   - Password is automatically hashed with BCrypt (work factor: 11)
   - Minimum 6 characters required
2. **Login** with credentials via `/api/Auth/login` to receive a JWT token
   - Password is verified against the BCrypt hash
3. Include the token in the `Authorization` header for all protected endpoints:
   ```
   Authorization: Bearer {your-token}
   ```
4. Token expires after 24 hours

## ?? CORS Configuration

The API is configured to accept requests from:
- `http://localhost:5173` (Vite default port)
- `http://localhost:5174` (Alternative Vite port)

To add more origins, update `Program.cs`:
```csharp
policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "your-new-origin")
```

## ?? Project Structure

```
library-management-system-backend/
??? Controllers/
?   ??? AuthController.cs      # Authentication endpoints with BCrypt
?   ??? BookController.cs      # Book CRUD operations
??? Data/
?   ??? AppDbContext.cs        # EF Core database context
?   ??? AppDbContextFactory.cs # Design-time DB factory
??? Models/
?   ??? User.cs                # User entity (password: 255 chars)
?   ??? UserDTO.cs             # User data transfer object
?   ??? LoginDTO.cs            # Login request model
?   ??? Book.cs                # Book entity
??? Migrations/                # EF Core migrations
??? Properties/
?   ??? launchSettings.json    # Launch profiles
??? Program.cs                 # Application entry point
??? appsettings.json          # Configuration
??? library.db                # SQLite database (created after setup)
```

## ?? Testing the API

### Using cURL

```bash
# Register
curl -X POST http://localhost:5119/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"John Doe","email":"john@example.com","password":"password123"}'

# Login
curl -X POST http://localhost:5119/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"john@example.com","password":"password123"}'

# Get all books (with token)
curl -X GET http://localhost:5119/api/Book/getAllBooks \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Using Postman

Import the endpoints or use the OpenAPI documentation available at:
- Development: http://localhost:5119/openapi/v1.json

## ?? Development

### Running in Development Mode

```bash
dotnet run --launch-profile http
```

This will:
- Run on HTTP only (no HTTPS redirection)
- Enable detailed error pages
- Enable OpenAPI documentation
- Use Development environment settings

### Running in Production Mode

```bash
dotnet run --environment Production
```

This will:
- Enable HTTPS redirection
- Disable detailed error pages
- Use production configuration

## ?? Database Migrations

### Create a new migration
```bash
dotnet ef migrations add MigrationName
```

### Update database
```bash
dotnet ef database update
```

### Remove last migration
```bash
dotnet ef migrations remove
```

## ?? Dependencies

```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.2.1" />
```

## ?? Security Considerations

### ? Implemented

1. **Password Hashing with BCrypt**
   - Work factor: 11 (configurable)
   - Automatic salt generation
   - One-way encryption

2. **JWT Authentication**
   - Token-based authentication
   - 24-hour token expiration
   - Secure signing algorithm (HS256)

3. **CORS Protection**
   - Restricted to specific origins
   - Credential support enabled

4. **Input Validation**
   - Password length validation (min 6 chars)
   - Email format validation
   - Required field checks

### ?? For Production Deployment

1. **Change JWT Secret Key**
   - Generate a new secure key (256+ bits)
   - Store in environment variables or Azure Key Vault

2. **Use HTTPS**
   - Enable `app.UseHttpsRedirection()`
   - Configure SSL certificates

3. **Update CORS**
   - Restrict to production frontend domains only
   - Remove development origins

4. **Use Production Database**
   - Switch from SQLite to SQL Server, PostgreSQL, or MySQL
   - Enable connection pooling

5. **Enable Rate Limiting**
   - Prevent brute force attacks on login endpoint
   - Implement account lockout after N failed attempts

6. **Add Email Verification**
   - Verify email addresses before activation
   - Send confirmation emails

7. **Strengthen Password Requirements**
   - Increase minimum length (8+ characters)
   - Require uppercase, lowercase, numbers, symbols
   - Implement password strength meter

8. **Add Logging and Monitoring**
   - Log authentication attempts
   - Monitor for suspicious activity
   - Set up alerts for security events

## ?? Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## ?? License

This project is licensed under the MIT License.

## ?? Author

**Ashan**
- GitHub: [@Ashan37](https://github.com/Ashan37)

## ?? Acknowledgments

- ASP.NET Core documentation
- Entity Framework Core
- JWT authentication best practices
- BCrypt.Net-Next for password hashing

---

For the frontend React application, see: [library-management-system-frontend](https://github.com/Ashan37/library-management-system-frontend)
