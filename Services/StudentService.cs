using Microsoft.EntityFrameworkCore;
using ScioApp.Data;
using ScioApp.Models;

namespace ScioApp.Services;

public interface IStudentService
{
    Task<(bool Success, string Message, Student? Student)> JoinGroupAsync(string inviteCode, string nickname, string deviceId);
    Task<Student?> GetStudentByNicknameAndDeviceAsync(int groupId, string nickname, string deviceId);
}

public class StudentService : IStudentService
{
    private readonly ScioDbContext _context;

    public StudentService(ScioDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, Student? Student)> JoinGroupAsync(string inviteCode, string nickname, string deviceId)
    {
        var group = await _context.Groups.FirstOrDefaultAsync(g => g.InviteCode == inviteCode && g.IsActive);
        if (group == null)
            return (false, "Neplatný nebo neaktivní kód skupiny.", null);

        // Check if this specific name and device already in group
        var existingStudent = await _context.Students
            .Include(s => s.ProgressLog)
            .FirstOrDefaultAsync(s => s.GroupId == group.Id && s.DeviceId == deviceId && s.Nickname == nickname);

        if (existingStudent != null)
        {
            return (true, "Vítejte zpět.", existingStudent);
        }

        // Check if nickname taken in this group
        if (await _context.Students.AnyAsync(s => s.GroupId == group.Id && s.Nickname == nickname))
            return (false, "Tato přezdívka je již v této skupině obsazena.", null);

        var student = new Student
        {
            GroupId = group.Id,
            Nickname = nickname,
            DeviceId = deviceId,
            Status = StudentStatus.Active,
            JoinedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        // Create initial progress log
        var progressLog = new ProgressLog
        {
            StudentId = student.Id,
            CurrentValue = 0,
            TargetValue = group.TargetValue,
            IsCompleted = false,
            LastUpdatedAt = DateTime.UtcNow
        };

        _context.ProgressLogs.Add(progressLog);
        await _context.SaveChangesAsync();

        return (true, "Úspěšně připojeno.", student);
    }

    public async Task<Student?> GetStudentByNicknameAndDeviceAsync(int groupId, string nickname, string deviceId)
    {
        return await _context.Students
            .Include(s => s.ProgressLog)
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s => s.GroupId == groupId && s.Nickname == nickname && s.DeviceId == deviceId);
    }
}
