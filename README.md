# Auth API - .NET Entrance Test

This is a sample **Authentication API** built with **ASP.NET Core**, **Entity Framework Core**, and **MySQL**. It was developed as part of a .NET developer entrance test.

## Features

- User Registration (SignUp)
- User Login (SignIn) with JWT Access & Refresh Tokens
- Logout (SignOut) and Refresh Token handling
- Secure password hashing with BCrypt
- Unit tests using xUnit & Moq
- FluentValidation for input validation

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/nhut-tran/Nexle-DotNetTest.git
```
### 2. Change the appsettings.json

- Go to appsettings.json with database credentials in the test document
- If change the JWT make sure it is at least 16 characters

### 3. Run the API

```bash
cd AuthApi
dotnet run
```
### 4. Test the API
Go to http://localhost:5063/swagger/index.html to test using Swagger UI

To run the tests on the AuthApi.Tests project
```bash
cd AuthApi.Tests
dotnet test 
```
