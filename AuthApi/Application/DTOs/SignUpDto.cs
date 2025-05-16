namespace AuthApi.Application.DTOs;

public record SignUpDto(string Email, string Password, string FirstName, string LastName);