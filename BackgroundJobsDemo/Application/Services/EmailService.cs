using BackgroundJobsDemo.Application.DTOs;
using BackgroundJobsDemo.Domain;

namespace BackgroundJobsDemo.Application.Services;

public class EmailService : IEmailService
{
    private readonly List<EmailMessage> _queue = new();
    private int _nextId = 1;
    private readonly object _lock = new();

    public Task<int> QueueEmailAsync(EmailRequestDto request)
    {
        lock (_lock)
        {
            var email = new EmailMessage
            {
                Id = _nextId++,
                To = request.To,
                Subject = request.Subject,
                Body = request.Body,
                CreatedAt = DateTime.UtcNow,
                IsProcessed = false
            };
            _queue.Add(email);
            return Task.FromResult(email.Id);
        }
    }

    public Task<IEnumerable<EmailResponseDto>> GetPendingEmailsAsync()
    {
        lock (_lock)
        {
            var result = _queue
                .Where(e => !e.IsProcessed)
                .Select(e => new EmailResponseDto(e.Id, "pending"))
                .ToList();
            return Task.FromResult<IEnumerable<EmailResponseDto>>(result);
        }
    }

    public Task<EmailQueueItem?> GetNextPendingAsync()
    {
        lock (_lock)
        {
            var email = _queue.FirstOrDefault(e => !e.IsProcessed);
            if (email == null) return Task.FromResult<EmailQueueItem?>(null);
            return Task.FromResult<EmailQueueItem?>(new EmailQueueItem(email.Id, email.To, email.Subject, email.Body));
        }
    }

    public Task MarkAsProcessedAsync(int id)
    {
        lock (_lock)
        {
            var email = _queue.FirstOrDefault(e => e.Id == id);
            if (email != null)
            {
                email.IsProcessed = true;
            }
        }
        return Task.CompletedTask;
    }
}
