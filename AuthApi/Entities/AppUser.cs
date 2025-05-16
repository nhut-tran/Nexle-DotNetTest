using Microsoft.AspNetCore.Identity;

namespace AuthApi.Entities;

public class AppUser
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; } 
    public DateTime UpdatedAt { get; set; }

}