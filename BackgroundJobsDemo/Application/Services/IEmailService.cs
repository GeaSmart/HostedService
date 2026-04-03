using BackgroundJobsDemo.Application.DTOs;
using BackgroundJobsDemo.Domain;

namespace BackgroundJobsDemo.Application.Services;

public interface IEmailService
{
    Task<int> QueueEmailAsync(EmailRequestDto request);
    Task<IEnumerable<EmailResponseDto>> GetPendingEmailsAsync();
    Task<IReadOnlyList<EmailMessage>> GetAllPendingAsync();
    Task<bool> TryMarkAsProcessingAsync(int id);
    Task MarkAsProcessedAsync(int id);
    Task MarkAsFailedAsync(int id, string error);
}
