namespace ScioApp.Models;

/// <summary>
/// Typ cíle skupiny
/// </summary>
public enum GoalType
{
    /// <summary>
    /// Boolean cíl (splněno/nesplněno)
    /// </summary>
    Boolean,
    
    /// <summary>
    /// Procentuální cíl (0-100%)
    /// </summary>
    Percentage
}

/// <summary>
/// Skupina studentů s definovaným cílem
/// </summary>
public class Group
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID učitele, který skupinu vytvořil
    /// </summary>
    public int TeacherId { get; set; }
    
    /// <summary>
    /// Název skupiny (např. "A2 - kvadratické rovnice 1")
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Textový popis cíle (např. "vyřeší samostatně 3 různé kvadratické rovnice")
    /// </summary>
    public required string GoalDescription { get; set; }
    
    /// <summary>
    /// Typ cíle (Boolean nebo Percentage)
    /// </summary>
    public GoalType GoalType { get; set; }
    
    /// <summary>
    /// Cílová hodnota (např. 3 pro 3 rovnice, nebo 100 pro procenta)
    /// </summary>
    public int TargetValue { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Invite kód pro QR kód (GUID)
    /// </summary>
    public string InviteCode { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Zda je skupina aktivní nebo archivovaná
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public User? Teacher { get; set; }
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
