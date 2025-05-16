using AuthApi.Application.DTOs;

namespace AuthApi.Application.Core;

public interface IAuthService
{
    Task<Result<SignupResponse>> SignUpAsync(SignUpDto dto);
    Task<Result<SignInResponse>> SignInAsync(SignInDto dto);
    Task<Result> SignOutAsync(int userId);
    Task<Result<TokenResponse>> RefreshTokenAsync(string refreshToken);
}