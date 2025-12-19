using Microsoft.EntityFrameworkCore;
using AuthService.Models;

namespace AuthService.Data;

public static class DataSeeder
{
    public static void SeedData(AuthDbContext context)
    {
        if (context.Users.Any()) return;

        // Create demo user
        var demoUser = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "demo@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
            FirstName = "Demo",
            LastName = "User",
            CustomerId = "CUST001",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Users.Add(demoUser);
        context.SaveChanges();
    }
}
