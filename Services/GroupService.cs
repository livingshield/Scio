using Microsoft.EntityFrameworkCore;
using ScioApp.Data;
using ScioApp.Models;

namespace ScioApp.Services;

public interface IGroupService
{
    Task<List<Group>> GetTeacherGroupsAsync(int teacherId);
    Task<Group?> GetGroupByIdAsync(int groupId);
    Task<Group?> GetGroupByInviteCodeAsync(string inviteCode);
    Task<Group> CreateGroupAsync(Group group);
    Task<bool> UpdateGroupAsync(Group group);
    Task<bool> DeleteGroupAsync(int groupId);
    Task<List<Message>> GetGroupMessagesAsync(int groupId, int limit = 50);
    Task<List<Student>> GetGroupStudentsAsync(int groupId);
    Task<List<Message>> GetStudentMessagesAsync(int studentId);
}

public class GroupService : IGroupService
{
    private readonly ScioDbContext _context;

    public GroupService(ScioDbContext context)
    {
        _context = context;
    }

    public async Task<List<Group>> GetTeacherGroupsAsync(int teacherId)
    {
        return await _context.Groups
            .Where(g => g.TeacherId == teacherId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<Group?> GetGroupByIdAsync(int groupId)
    {
        return await _context.Groups
            .Include(g => g.Students)
                .ThenInclude(s => s.ProgressLog)
            .Include(g => g.Messages.OrderBy(m => m.Timestamp).Take(50))
            .FirstOrDefaultAsync(g => g.Id == groupId);
    }

    public async Task<List<Message>> GetGroupMessagesAsync(int groupId, int limit = 50)
    {
        return await _context.Messages
            .Include(m => m.Student)
            .Where(m => m.GroupId == groupId)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<Group?> GetGroupByInviteCodeAsync(string inviteCode)
    {
        return await _context.Groups
            .FirstOrDefaultAsync(g => g.InviteCode == inviteCode && g.IsActive);
    }

    public async Task<Group> CreateGroupAsync(Group group)
    {
        group.CreatedAt = DateTime.UtcNow;
        group.InviteCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(); // Shorter code for easier typing if needed
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task<bool> UpdateGroupAsync(Group group)
    {
        _context.Entry(group).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    public async Task<bool> DeleteGroupAsync(int groupId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null) return false;

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Student>> GetGroupStudentsAsync(int groupId)
    {
        return await _context.Students
            .Include(s => s.ProgressLog)
            .Where(s => s.GroupId == groupId)
            .OrderBy(s => s.Nickname)
            .ToListAsync();
    }

    public async Task<List<Message>> GetStudentMessagesAsync(int studentId)
    {
        return await _context.Messages
            .Where(m => m.StudentId == studentId || (m.IsFromTeacher && _context.Messages.Any(dm => dm.Id == m.Id && dm.StudentId == studentId)))
            // Note: In a real app teacher messages might need better filtering, 
            // but for this MVP we'll take messages marked as from teacher that belong to this group link.
            // Actually, Message has StudentId even if from teacher (the target student).
            .Where(m => m.StudentId == studentId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }
}
