namespace AuthApi.Application.DTOs;

public record SignInResponse(UserDto User, string Token, string RefreshToken);
