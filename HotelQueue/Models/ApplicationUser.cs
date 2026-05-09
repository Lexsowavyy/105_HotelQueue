using Microsoft.AspNetCore.Identity;

namespace HotelQueue.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsVIP { get; set; } = false;
    
    public string FullName => $"{FirstName} {LastName}";
}
