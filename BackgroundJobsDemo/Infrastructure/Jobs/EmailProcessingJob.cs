using BackgroundJobsDemo.Application.Options;
using BackgroundJobsDemo.Application.Services;
using Microsoft.Extensions.Options;

namespace BackgroundJobsDemo.Infrastructure.Jobs;

public class EmailProcessingJob : BackgroundService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailProcessingJob> _logger;
    private readonly EmailProcessingOptions _options;

    public EmailProcessingJob(
        IEmailService emailService,
        ILogger<EmailProcessingJob> logger,
        IOptions<EmailProcessingOptions> options)
    {
        _emailService = emailService;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Processing Job started with interval of {Interval}s",
            _options.PollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessEmailsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing emails batch");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Email Processing Job stopped");
    }

    private async Task ProcessEmailsAsync()
    {
        var pendingEmails = await _emailService.GetAllPendingAsync();

        if (pendingEmails.Count == 0)
        {
            _logger.LogDebug("No pending emails");
            return;
        }

        _logger.LogInformation("Processing {Count} pending emails", pendingEmails.Count);

        foreach (var email in pendingEmails)
        {
            var acquired = await _emailService.TryMarkAsProcessingAsync(email.Id);
            if (!acquired) continue;

            try
            {
                _logger.LogInformation("Processing email {EmailId} to {Recipient}", email.Id, email.To);
                await Task.Delay(_options.ProcessingDelayMilliseconds);
                await _emailService.MarkAsProcessedAsync(email.Id);
                _logger.LogInformation("Email {EmailId} sent successfully to {Recipient}", email.Id, email.To);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process email {EmailId}, marking for retry", email.Id);
                await _emailService.MarkAsFailedAsync(email.Id, ex.Message);
            }
        }
    }
}