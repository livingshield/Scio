namespace ScioApp.Models;

/// <summary>
/// Log pokroku studenta v rámci skupiny
/// </summary>
public class ProgressLog
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID studenta
    /// </summary>
    public int StudentId { get; set; }
    
    /// <summary>
    /// Aktuální hodnota progresu
    /// </summary>
    public int CurrentValue { get; set; } = 0;
    
    /// <summary>
    /// Cílová hodnota (kopie z Group pro rychlejší dotazy)
    /// </summary>
    public int TargetValue { get; set; }
    
    /// <summary>
    /// Vypočtené procento (CurrentValue / TargetValue * 100)
    /// </summary>
    public float Percentage => TargetValue > 0 ? (float)CurrentValue / TargetValue * 100 : 0;
    
    /// <summary>
    /// Zda byl cíl dokončen
    /// </summary>
    public bool IsCompleted { get; set; } = false;
    
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Čas dokončení cíle (nullable)
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public Student? Student { get; set; }
}
