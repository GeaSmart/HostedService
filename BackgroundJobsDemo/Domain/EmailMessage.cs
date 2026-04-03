namespace BackgroundJobsDemo.Domain;

public enum EmailStatus
{
    Pending,
    Processing,
    Processed,
    Failed
}

public class EmailMessage
{
    public int Id { get; set; }
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public EmailStatus Status { get; set; } = EmailStatus.Pending;
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
}
