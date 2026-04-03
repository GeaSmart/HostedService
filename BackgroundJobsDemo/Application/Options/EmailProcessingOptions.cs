namespace BackgroundJobsDemo.Application.Options;

public class EmailProcessingOptions
{
    public int PollingIntervalSeconds { get; set; } = 10;
    public int ProcessingDelayMilliseconds { get; set; } = 1000;
    public int MaxRetries { get; set; } = 3;
}