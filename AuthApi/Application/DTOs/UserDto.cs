namespace AuthApi.Application.DTOs;

public record UserDto(string FirstName, string LastName, string Email)
{
    public string DisplayName => $"{FirstName} {LastName}";
}
