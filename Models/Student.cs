namespace ScioApp.Models;

/// <summary>
/// Status studenta v rámci skupiny
/// </summary>
public enum StudentStatus
{
    /// <summary>
    /// Aktivní student
    /// </summary>
    Active,
    
    /// <summary>
    /// Student potřebuje pomoc
    /// </summary>
    NeedHelp,
    
    /// <summary>
    /// Neaktivní student (dlouho nepsal)
    /// </summary>
    Inactive,
    
    /// <summary>
    /// Student dokončil cíl
    /// </summary>
    Completed
}

/// <summary>
/// Student v rámci skupiny (session-based, identifikován pomocí DeviceId)
/// </summary>
public class Student
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID skupiny, do které student patří
    /// </summary>
    public int GroupId { get; set; }
    
    /// <summary>
    /// Nickname studenta
    /// </summary>
    public required string Nickname { get; set; }
    
    /// <summary>
    /// Unique ID zařízení (uloženo v LocalStorage, unique per group)
    /// </summary>
    public required string DeviceId { get; set; }
    
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Čas poslední aktivity (pro watchdog detekci)
    /// </summary>
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Aktuální status studenta
    /// </summary>
    public StudentStatus Status { get; set; } = StudentStatus.Active;
    
    // Navigation properties
    public Group? Group { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ProgressLog? ProgressLog { get; set; }
}
