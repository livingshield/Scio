namespace ScioApp.Models;

/// <summary>
/// Chatová zpráva od studenta
/// </summary>
public class Message
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID studenta, který zprávu poslal
    /// </summary>
    public int StudentId { get; set; }
    
    /// <summary>
    /// ID skupiny
    /// </summary>
    public int GroupId { get; set; }
    
    /// <summary>
    /// Obsah zprávy
    /// </summary>
    public required string Content { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Zda jde o systémovou zprávu (oznámení, varování)
    /// </summary>
    public bool IsSystemMessage { get; set; } = false;

    /// <summary>
    /// Zda jde o zprávu od učitele
    /// </summary>
    public bool IsFromTeacher { get; set; } = false;
    
    /// <summary>
    /// Zda AI vyhodnotila zprávu jako příspěvek k cíli (zvýraznění)
    /// </summary>
    public bool IsProgressContribution { get; set; } = false;
    
    /// <summary>
    /// Confidence skóre AI analýzy (0-1)
    /// </summary>
    public float? AIConfidence { get; set; }
    
    // Navigation properties
    public Student? Student { get; set; }
    public Group? Group { get; set; }
}
