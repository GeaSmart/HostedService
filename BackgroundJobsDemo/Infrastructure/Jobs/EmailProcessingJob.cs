using BackgroundJobsDemo.Application.Options;
using BackgroundJobsDemo.Application.Services;
using Microsoft.Extensions.Options;

namespace BackgroundJobsDemo.Infrastructure.Jobs;

public class EmailProcessingJob(
    IEmailService emailService,
    ILogger<EmailProcessingJob> logger,
    IOptions<EmailProcessingOptions> options) : BackgroundService
{
    private readonly EmailProcessingOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Email Processing Job started with interval of {Interval}s",
            _options.PollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessEmailsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing emails batch");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
        }

        logger.LogInformation("Email Processing Job stopped");
    }

    private async Task ProcessEmailsAsync()
    {
        var pendingEmails = await emailService.GetAllPendingAsync();

        if (pendingEmails.Count == 0)
        {
            logger.LogDebug("No pending emails");
            return;
        }

        logger.LogInformation("Processing {Count} pending emails", pendingEmails.Count);

        foreach (var email in pendingEmails)
        {
            var acquired = await emailService.TryMarkAsProcessingAsync(email.Id);
            if (!acquired) continue;

            try
            {
                logger.LogInformation("Processing email {EmailId} to {Recipient}", email.Id, email.To);
                await Task.Delay(_options.ProcessingDelayMilliseconds);
                await emailService.MarkAsProcessedAsync(email.Id);
                logger.LogInformation("Email {EmailId} sent successfully to {Recipient}", email.Id, email.To);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to process email {EmailId}, marking for retry", email.Id);
                await emailService.MarkAsFailedAsync(email.Id, ex.Message);
            }
        }
    }
}