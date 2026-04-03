using BackgroundJobsDemo.Application.DTOs;

namespace BackgroundJobsDemo.Application.Services;

public interface IEmailService
{
    Task<int> QueueEmailAsync(EmailRequestDto request);
    Task<IEnumerable<EmailResponseDto>> GetPendingEmailsAsync();
    Task<EmailQueueItem?> GetNextPendingAsync();
    Task MarkAsProcessedAsync(int id);
}

public record EmailQueueItem(int Id, string To, string Subject, string Body);
