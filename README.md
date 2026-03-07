# ASP.NET Core MVC JWT Login System

A simple authentication system built using **ASP.NET Core MVC**, **SQL Server**, and **JWT (JSON Web Token)**.
The application allows users to **log in using database credentials**, generates a **JWT token**, stores it in a **browser cookie**, and redirects authenticated users to a **dashboard page**.

---

# Features

* User Login using **ASP.NET Core MVC**
* Authentication with **JWT Token**
* **SQL Server Database Integration**
* **Stored Procedure for Login Validation**
* **Cookie-based Token Storage**
* **Dashboard Page After Login**
* **Logout Functionality**
* Clean **MVC Architecture**

---

# Technologies Used

* ASP.NET Core MVC
* C#
* SQL Server
* ADO.NET
* JWT Authentication
* Razor Views
* HTML / CSS

---

# Project Structure

```
JwtMvcLoginProject
│
├── Controllers
│      AccountController.cs
│
├── Models
│      LoginModel.cs
│
├── Views
│   └── Account
│        Login.cshtml
│        Dashboard.cshtml
│
├── appsettings.json
├── Program.cs
└── README.md
```

---

# Application Flow

```
User Opens Application
        ↓
Login Page Displayed
        ↓
User Enters Username & Password
        ↓
Controller Sends Data to SQL Server
        ↓
Stored Procedure Validates Credentials
        ↓
If Valid → JWT Token Generated
        ↓
Token Stored in Browser Cookie
        ↓
User Redirected to Dashboard
        ↓
User Can Logout (Cookie Deleted)
```

---

# Database Setup

Create a database named:

```
JwtMvcDb
```

Create a **Users table**:

```sql
CREATE TABLE Users
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(50),
    Password VARCHAR(50)
);
```

Insert a sample user:

```sql
INSERT INTO Users VALUES ('admin','admin123');
```

---

# Stored Procedure

Create a stored procedure for login validation.

```sql
CREATE PROCEDURE sp_UserLogin
(
    @Username VARCHAR(50),
    @Password VARCHAR(50)
)
AS
BEGIN
    SELECT Id, Username
    FROM Users
    WHERE Username = @Username
    AND Password = @Password
END
```

---

# Configuration

Update **appsettings.json** with your database connection and JWT settings.

```json
{
  "ConnectionStrings": {
    "dbconn": "Server=.;Database=JwtMvcDb;Trusted_Connection=True;TrustServerCertificate=True"
  },

  "Jwt": {
    "Key": "this_is_a_super_secret_key_for_jwt_authentication_123456",
    "Issuer": "JwtMvcDemo",
    "Audience": "JwtMvcDemoUsers"
  }
}
```

---

# Running the Project

1. Clone the repository

```
git clone https://github.com/yourusername/yourrepository.git
```

2. Open the project in **Visual Studio**

3. Restore NuGet packages

4. Update **connection string** in `appsettings.json`

5. Run the application

The login page will open at:

```
https://localhost:xxxx
```

---

# Login Credentials

Example credentials:

```
Username: admin
Password: admin123
```

---

# Logout

When the user clicks **Logout**:

* JWT cookie is deleted
* User is redirected back to the login page

---

# Security Notes

For production environments it is recommended to:

* Use **hashed passwords** instead of plain text
* Use **HTTPS-only cookies**
* Add **JWT validation middleware**
* Implement **token expiration and refresh tokens**

---

# Author

Developed as a learning project for understanding:

* ASP.NET Core MVC authentication
* JWT token generation
* SQL Server integration

---

# License

This project is open-source and available for educational purposes.
