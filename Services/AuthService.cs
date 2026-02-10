using Microsoft.EntityFrameworkCore;
using ScioApp.Data;
using ScioApp.Models;
using BC = BCrypt.Net.BCrypt;

namespace ScioApp.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, User? User)> RegisterTeacherAsync(string login, string email, string name, string password);
    Task<(bool Success, string Message, User? User)> LoginAsync(string login, string password);
    Task<(bool Success, string Message, User? User)> LoginOrRegisterGoogleAsync(string googleId, string email, string name);
}

public class AuthService : IAuthService
{
    private readonly ScioDbContext _context;

    public AuthService(ScioDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, User? User)> RegisterTeacherAsync(string login, string email, string name, string password)
    {
        try
        {
            // Check if user exists
            if (await _context.Users.AnyAsync(u => u.Login == login))
                return (false, "Uživatel s tímto loginem již existuje.", null);

            if (await _context.Users.AnyAsync(u => u.Email == email))
                return (false, "Email je již používán.", null);

            var user = new User
            {
                Login = login,
                Email = email,
                Name = name,
                PasswordHash = BC.HashPassword(password),
                Role = "Teacher"
            };

            _context.Users.Add(user);
            await _context.Set<User>().AddAsync(user); // Redundant but explicit
            await _context.SaveChangesAsync();

            return (true, "Registrace úspěšná.", user);
        }
        catch (Exception ex)
        {
            return (false, $"Chyba při registraci: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message, User? User)> LoginAsync(string login, string password)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
            
            if (user == null || !BC.Verify(password, user.PasswordHash))
                return (false, "Nesprávné jméno nebo heslo.", null);

            return (true, "Přihlášení úspěšné.", user);
        }
        catch (Exception ex)
        {
            return (false, $"Chyba při přihlašování: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message, User? User)> LoginOrRegisterGoogleAsync(string googleId, string email, string name)
    {
        try
        {
            // Try find user by GoogleId
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
            
            if (user == null)
            {
                // Try find by Email (merge accounts)
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                
                if (user != null)
                {
                    // Update existing user with GoogleId
                    user.GoogleId = googleId;
                }
                else
                {
                    // Create new user
                    user = new User
                    {
                        Login = email.Split('@')[0] + "_" + Guid.NewGuid().ToString().Substring(0, 4),
                        Email = email,
                        Name = name,
                        GoogleId = googleId,
                        PasswordHash = "GOOGLE_AUTH", // Placeholder, since they use Google
                        Role = "Teacher"
                    };
                    _context.Users.Add(user);
                }
                
                await _context.SaveChangesAsync();
            }

            return (true, "Přihlášení přes Google úspěšné.", user);
        }
        catch (Exception ex)
        {
            return (false, $"Chyba při Google přihlášení: {ex.Message}", null);
        }
    }
}
