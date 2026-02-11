using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ScioApp.Data;
using ScioApp.Models;
using ScioApp.Services;

namespace ScioApp.Hubs;

public class ScioHub : Hub
{
    private readonly IStudentService _studentService;
    private readonly IGroupService _groupService;
    private readonly IAIService _aiService;
    private readonly ScioDbContext _context;

    public ScioHub(IStudentService studentService, IGroupService groupService, IAIService aiService, ScioDbContext context)
    {
        _studentService = studentService;
        _groupService = groupService;
        _aiService = aiService;
        _context = context;
    }

    public async Task JoinGroup(int groupId, string deviceId, string nickname, bool isTeacher)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupId}");
        
        if (!isTeacher)
        {
            var student = await _studentService.GetStudentByNicknameAndDeviceAsync(groupId, nickname, deviceId);
            if (student != null)
            {
                await Clients.Group($"group_{groupId}").SendAsync("UserJoined", student.Nickname);
            }
        }
    }

    public async Task SendMessage(int groupId, int studentId, string content)
    {
        // 1. Save message to database
        var message = new Message
        {
            GroupId = groupId,
            StudentId = studentId,
            Content = content,
            Timestamp = DateTime.UtcNow
        };
        _context.Messages.Add(message);

        // 2. Get context for AI analysis
        var student = await _context.Students
            .Include(s => s.Group)
            .Include(s => s.ProgressLog)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        bool isRelevant = true;
        string? studentFeedback = null;

        if (student != null && student.Group != null)
        {
            // 3. AI Analysis
            var aiResult = await _aiService.AnalyzeMessageAsync(
                student.Group.GoalDescription,
                student.Nickname,
                content,
                student.ProgressLog?.CurrentValue ?? 0,
                student.Group.TargetValue);

            isRelevant = aiResult.IsRelevant;
            studentFeedback = aiResult.StudentFeedback;

            if (aiResult.IsProgress)
            {
                message.IsProgressContribution = true;
                // DO NOT update student.ProgressLog automatically anymore. 
                // Teacher will click "Approve" in the dashboard.
            }

            // Always update activity
            student.LastActivityAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // 4. Broadcast message to all in group with AI metadata
        await Clients.Group($"group_{groupId}").SendAsync("ReceiveMessage", 
            message.Id,
            studentId, 
            content, 
            message.Timestamp, 
            message.IsProgressContribution,
            isRelevant,
            studentFeedback,
            false); // isFromTeacher = false
            
        // 5. Broadcast activity update for online/offline indicator
        if (student != null)
        {
            await Clients.Group($"group_{groupId}").SendAsync("ActivityUpdated", studentId, student.LastActivityAt);
        }
    }

    public async Task SendTeacherMessage(int groupId, int studentId, string content)
    {
        var message = new Message
        {
            GroupId = groupId,
            StudentId = studentId,
            Content = content,
            Timestamp = DateTime.UtcNow,
            IsFromTeacher = true
        };
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        await Clients.Group($"group_{groupId}").SendAsync("ReceiveMessage", 
            message.Id,
            studentId, 
            content, 
            message.Timestamp, 
            false, // isProgress
            true,  // isRelevant
            null,  // feedback
            true); // isFromTeacher = true
    }

    public async Task ApproveMessage(int groupId, int messageId)
    {
        var message = await _context.Messages
            .Include(m => m.Student)
                .ThenInclude(s => s.ProgressLog)
            .Include(m => m.Student)
                .ThenInclude(s => s.Group)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message != null && message.Student?.ProgressLog != null)
        {
            var student = message.Student;
            var group = student.Group;
            if (group == null) return;

            if (message.IsProgressContribution) return;

            message.IsProgressContribution = true;
            
            if (group.GoalType == GoalType.Boolean)
            {
                student.ProgressLog.CurrentValue = group.TargetValue;
                student.ProgressLog.IsCompleted = true;
                student.ProgressLog.CompletedAt = DateTime.UtcNow;
                student.Status = StudentStatus.Completed;
            }
            else
            {
                student.ProgressLog.CurrentValue += 1;
                if (student.ProgressLog.CurrentValue >= group.TargetValue)
                {
                    student.ProgressLog.IsCompleted = true;
                    student.ProgressLog.CompletedAt = DateTime.UtcNow;
                    student.Status = StudentStatus.Completed;
                }
            }

            student.ProgressLog.LastUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Notify about approval update specifically
            await Clients.Group($"group_{groupId}").SendAsync("MessageApproved", message.Id, student.Id);

            await Clients.Group($"group_{groupId}").SendAsync("ProgressUpdated", student.Id, student.ProgressLog.CurrentValue);
            await Clients.Group($"group_{groupId}").SendAsync("StatusChanged", student.Id, student.Status);
        }
    }

    public async Task UpdateProgress(int groupId, int studentId, int newValue)
    {
        var student = await _context.Students.Include(s => s.ProgressLog).FirstOrDefaultAsync(s => s.Id == studentId);
        if (student != null && student.ProgressLog != null)
        {
            student.ProgressLog.CurrentValue = newValue;
            student.ProgressLog.LastUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            await Clients.Group($"group_{groupId}").SendAsync("ProgressUpdated", studentId, newValue);
        }
    }

    public async Task SignalNeedHelp(int groupId, int studentId, bool needsHelp)
    {
        var student = await _context.Students.FindAsync(studentId);
        if (student != null)
        {
            student.Status = needsHelp ? StudentStatus.NeedHelp : StudentStatus.Active;
            student.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await Clients.Group($"group_{groupId}").SendAsync("StatusChanged", studentId, student.Status);
        }
    }
}
