using BackgroundJobsDemo.Application.Services;

namespace BackgroundJobsDemo.Infrastructure.Jobs;

public class EmailProcessingJob : BackgroundService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailProcessingJob> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

    public EmailProcessingJob(IEmailService emailService, ILogger<EmailProcessingJob> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Processing Job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessEmailsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing emails");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Email Processing Job stopped");
    }

    private async Task ProcessEmailsAsync()
    {
        var email = await _emailService.GetNextPendingAsync();

        if (email == null)
        {
            _logger.LogDebug("No pending emails");
            return;
        }

        _logger.LogInformation("Processing email {EmailId} to {Recipient}", email.Id, email.To);

        await Task.Delay(1000);

        await _emailService.MarkAsProcessedAsync(email.Id);

        _logger.LogInformation("Email {EmailId} sent successfully to {Recipient}", email.Id, email.To);
    }
}
