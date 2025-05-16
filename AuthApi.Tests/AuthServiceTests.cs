using AuthApi.Application.Core;
using AuthApi.Application.DTOs;
using AuthApi.Application.Services;
using AuthApi.Database;
using AuthApi.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AuthApi.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IValidator<SignUpDto>> _signUpValidatorMock;
    private readonly Mock<IValidator<SignInDto>> _signInValidatorMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique in-memory db per test class instance
            .Options;

        _dbContext = new AppDbContext(options);
        _tokenServiceMock = new Mock<ITokenService>();
        _configurationMock = new Mock<IConfiguration>();
        _signUpValidatorMock = new Mock<IValidator<SignUpDto>>();
        _signInValidatorMock = new Mock<IValidator<SignInDto>>();
        _configurationMock.Setup(c => c["Jwt:RefreshTokenLifetimeDays"]).Returns("30");
        _authService = new AuthService(
            _dbContext,
            _tokenServiceMock.Object,
            _configurationMock.Object,
            _signUpValidatorMock.Object,
            _signInValidatorMock.Object);
    }
    

    [Fact]
    public async Task SignUpAsync_ShouldReturnSuccess_WhenValid()
    {
        var dto = new SignUpDto("test@example.com", "Password12345", "User",  "Test");
        _signUpValidatorMock.Setup(v => v.ValidateAsync(dto, CancellationToken.None))
            .ReturnsAsync(new ValidationResult());
        
        var result = await _authService.SignUpAsync(dto);
        var signUpResult =  result.Match<SignupResponse?>((v) => v, (_) => null);
        
        Assert.NotNull(signUpResult);
        Assert.Equal(dto.Email, signUpResult.Email);
        Assert.Equal(dto.FirstName, signUpResult.FirstName);
        Assert.Equal(dto.LastName, signUpResult.LastName);
      
    }

    [Fact]
    public async Task SignUpAsync_ShouldReturnFailure_WhenEmailExists()
    {
        var existingUser = new AppUser { Email = "test@example.com", PasswordHash = "hash", FirstName = "test", LastName = "test"};
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();
        var dto = new SignUpDto("test@example.com", "Password1234", "User", "Test");
        _signUpValidatorMock.Setup(v => v.ValidateAsync(dto, CancellationToken.None))
            .ReturnsAsync(new ValidationResult());
        
        var result = await _authService.SignUpAsync(dto);
        
        var signUpResult =  result.Match<string?>((_) => null, (err) => err);
        
        Assert.Equal(ErrorCodes.EmailExists, signUpResult);
        
    }

    [Fact]
    public async Task SignInAsync_ShouldReturnSuccess_WhenValidCredentials()
    {
        var password = "Password123";
        var hashed = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new AppUser { Email = "test@example.com", PasswordHash = hashed, FirstName = "User", LastName = "Test" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        var dto = new SignInDto("test@example.com", password);
        _signInValidatorMock.Setup(v => v.ValidateAsync(dto, CancellationToken.None))
            .ReturnsAsync(new ValidationResult());
        _tokenServiceMock.Setup(t => t.CreateToken(user)).Returns("jwt_token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh_token");

        var result = await _authService.SignInAsync(dto);
        var signInResult = result.Match<SignInResponse?>((r) => r, _ => null);
        
        Assert.NotNull(signInResult);
        Assert.Equal("jwt_token", signInResult.Token);
        Assert.Equal("refresh_token", signInResult.RefreshToken);
        Assert.Equal(user.Email, signInResult.User.Email);
        Assert.Equal(user.FirstName, signInResult.User.FirstName);
        Assert.Equal(user.LastName, signInResult.User.LastName);
        
    }

    [Fact]
    public async Task SignInAsync_ShouldReturnFailure_WhenInvalidCredentials()
    {
        var user = new AppUser { Email = "test@example.com", FirstName = "User", LastName = "Test", PasswordHash = BCrypt.Net.BCrypt.HashPassword("rightpassword") };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        var dto = new SignInDto("test@example.com", "wrongpassword");
        _signInValidatorMock.Setup(v => v.ValidateAsync(dto, CancellationToken.None))
            .ReturnsAsync(new ValidationResult());
        
        var result = await _authService.SignInAsync(dto);
        var signInResult = result.Match<string?>((_) => null, (err) => err);
        
        Assert.Equal(ErrorCodes.InvalidCredentials, signInResult);
    }

    [Fact]
    public async Task SignOutAsync_ShouldReturnSuccess_WhenCalled()
    {
        var user = new AppUser
            { Email = "user@example.com", FirstName = "User", LastName = "Test", PasswordHash = "hash" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        _dbContext.RefreshTokens.Add(new RefreshToken
            { UserId = user.Id, Token = "token1", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });
        await _dbContext.SaveChangesAsync();

        await _authService.SignOutAsync(user.Id);
        var tokens = await _dbContext.RefreshTokens.Where(t => t.UserId == user.Id).ToListAsync();
        
        Assert.Empty(tokens);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnSuccess_WhenValidAndNotExpired()
    {
        var user = new AppUser { Email = "user@example.com", FirstName = "User", LastName = "Test", PasswordHash = "hash" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = "refresh_token",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            User = user
        };
        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();
        _tokenServiceMock.Setup(t => t.CreateToken(user)).Returns("new_jwt_token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("new_refresh_token");

        var result = await _authService.RefreshTokenAsync("refresh_token");
        var newTokenResult = result.Match<TokenResponse?>(v => v, (_) => null);
        
        Assert.NotNull(newTokenResult);
        Assert.Equal("new_jwt_token", newTokenResult.Token);
        Assert.Equal("new_refresh_token", newTokenResult.RefreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _authService.RefreshTokenAsync("nonexistent_token");

        var newTokenResult = result.Match<string?>(_ => null, (err) => err);
        
        Assert.Equal(ErrorCodes.NotFound, newTokenResult);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnFailure_WhenExpired()
    {
        var user = new AppUser { Email = "user@example.com", FirstName = "User", LastName = "Test", PasswordHash = "hash" };
        _dbContext.Users.Add(user);
        var expiredToken = new RefreshToken
        {
            UserId = user.Id,
            Token = "expired_token",
            CreatedAt = DateTime.Now.AddDays(-40), // older than refresh token lifetime 30 days
            UpdatedAt = DateTime.Now.AddDays(-40),
            User = user
        };
        _dbContext.RefreshTokens.Add(expiredToken);
        await _dbContext.SaveChangesAsync();
        
        var result = await _authService.RefreshTokenAsync("expired_token");
        var newTokenResult = result.Match<string?>(_ => null, (err) => err);
        
        Assert.Equal(ErrorCodes.Expired, newTokenResult);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}