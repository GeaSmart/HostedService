using BackgroundJobsDemo.Application.DTOs;
using BackgroundJobsDemo.Domain;

namespace BackgroundJobsDemo.Application.Services;

public class EmailService : IEmailService
{
    private readonly List<EmailMessage> _queue = new();
    private int _nextId = 1;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private const int MaxRetries = 3;

    public async Task<int> QueueEmailAsync(EmailRequestDto request)
    {
        var email = new EmailMessage
        {
            Id = _nextId++,
            To = request.To,
            Subject = request.Subject,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow,
            Status = EmailStatus.Pending,
            RetryCount = 0
        };

        await _semaphore.WaitAsync();
        try
        {
            _queue.Add(email);
        }
        finally
        {
            _semaphore.Release();
        }

        return email.Id;
    }

    public async Task<IEnumerable<EmailResponseDto>> GetPendingEmailsAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return _queue
                .Where(e => e.Status == EmailStatus.Pending || e.Status == EmailStatus.Processing || e.Status == EmailStatus.Failed)
                .Select(e => new EmailResponseDto(e.Id, e.Status.ToString().ToLowerInvariant()))
                .ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IReadOnlyList<EmailMessage>> GetAllPendingAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return _queue
                .Where(e => e.Status == EmailStatus.Pending)
                .OrderBy(e => e.CreatedAt)
                .ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> TryMarkAsProcessingAsync(int id)
    {
        await _semaphore.WaitAsync();
        try
        {
            var email = _queue.FirstOrDefault(e => e.Id == id && e.Status == EmailStatus.Pending);
            if (email == null) return false;
            email.Status = EmailStatus.Processing;
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task MarkAsProcessedAsync(int id)
    {
        await _semaphore.WaitAsync();
        try
        {
            var email = _queue.FirstOrDefault(e => e.Id == id);
            if (email != null)
            {
                email.Status = EmailStatus.Processed;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task MarkAsFailedAsync(int id, string error)
    {
        await _semaphore.WaitAsync();
        try
        {
            var email = _queue.FirstOrDefault(e => e.Id == id);
            if (email == null) return;
            
            email.RetryCount++;
            email.LastError = error;
            
            if (email.RetryCount >= MaxRetries)
            {
                email.Status = EmailStatus.Failed;
            }
            else
            {
                email.Status = EmailStatus.Pending;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}