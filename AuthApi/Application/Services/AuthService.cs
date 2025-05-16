using AuthApi.Application.Core;
using AuthApi.Application.DTOs;
using AuthApi.Database;
using AuthApi.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Application.Services;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly IValidator<SignUpDto> _signUpDtoValidator;
    private readonly IValidator<SignInDto> _signInDtoValidator;
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db,
        ITokenService tokenService,
        IConfiguration configuration,
        IValidator<SignUpDto> signUpDtoValidator,
        IValidator<SignInDto> signInDtoValidator)
    {
        
        _db = db;
        _tokenService = tokenService;
        _configuration = configuration;
        _signUpDtoValidator = signUpDtoValidator;
        _signInDtoValidator = signInDtoValidator;
    }
    public async Task<Result<SignupResponse>> SignUpAsync(SignUpDto signUpDto)
    {
        var validateResult = await _signUpDtoValidator.ValidateAsync(signUpDto);
        if (!validateResult.IsValid)
            return Result<SignupResponse>.Failure(string.Join(";", validateResult.Errors));
        
        var existUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == signUpDto.Email);
        if(existUser is not null)
            return Result<SignupResponse>.Failure(ErrorCodes.EmailExists);
        
        var user = new AppUser
        {
            Email = signUpDto.Email,
            FirstName = signUpDto.FirstName,
            LastName = signUpDto.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(signUpDto.Password),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Result<SignupResponse>.Success(new SignupResponse(user.Id, user.FirstName, user.LastName, user.Email));
    }

    public async Task<Result<SignInResponse>> SignInAsync(SignInDto signInDto)
    {
        var validateResult = await _signInDtoValidator.ValidateAsync(signInDto);
        if (!validateResult.IsValid)
            return Result<SignInResponse>.Failure(string.Join(";", validateResult.Errors));
        
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == signInDto.Email);
        if(user is null || !BCrypt.Net.BCrypt.Verify(signInDto.Password, user.PasswordHash))
            return Result<SignInResponse>.Failure(ErrorCodes.InvalidCredentials);
        var jwtToken = _tokenService.CreateToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync();
        return Result<SignInResponse>.Success(
            new SignInResponse(
                new UserDto(user.FirstName, user.LastName, user.Email),
                jwtToken,
                refreshToken
            )
        );
    }

    public async Task<Result> SignOutAsync(int userId)
    {
        var tokens = await _db.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
        _db.RefreshTokens.RemoveRange(tokens);
        await _db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<TokenResponse>> RefreshTokenAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);
        if (token is null)
            return Result<TokenResponse>.Failure(ErrorCodes.NotFound);
        //because there isn't expireIn in database use the CreatedAt to check the token is expired or not
        var refreshLifetime = int.Parse(_configuration["Jwt:RefreshTokenLifetimeDays"]!);
        if (token.CreatedAt.AddDays(refreshLifetime) < DateTime.Now)
            return Result<TokenResponse>.Failure(ErrorCodes.Expired);

        _db.RefreshTokens.Remove(token);

        var newJwt = _tokenService.CreateToken(token.User);
        var newRefresh = _tokenService.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = token.UserId,
            Token = newRefresh,
            UpdatedAt = DateTime.Now,
            CreatedAt = DateTime.Now
        });
        await _db.SaveChangesAsync();

        return Result<TokenResponse>.Success(new TokenResponse(newJwt, newRefresh));
    }
}