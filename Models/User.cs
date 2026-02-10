namespace ScioApp.Models;

/// <summary>
/// Učitelský účet s podporou klasické i Google autentifikace
/// </summary>
public class User
{
    public int Id { get; set; }
    
    /// <summary>
    /// Přihlašovací jméno nebo email
    /// </summary>
    public required string Login { get; set; }
    
    /// <summary>
    /// Email uživatele
    /// </summary>
   public required string Email { get; set; }
    
    /// <summary>
    /// Zobrazované jméno
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// BCrypt hash hesla (pro klasickou autentifikaci)
    /// </summary>
    public required string PasswordHash { get; set; }
    
    /// <summary>
    /// Google ID pro OAuth (nullable - použije se ve Fázi 2)
    /// </summary>
    public string? GoogleId { get; set; }
    
    /// <summary>
    /// Role uživatele (Teacher, Admin)
    /// </summary>
    public required string Role { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Group> Groups { get; set; } = new List<Group>();
}
