namespace AuthApi.Application.DTOs;

public record SignupResponse(int Id, string FirstName, string LastName, string Email)
{
    public string DisplayName => $"{FirstName} {LastName}";
}