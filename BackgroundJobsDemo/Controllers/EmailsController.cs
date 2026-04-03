using Microsoft.AspNetCore.Mvc;
using BackgroundJobsDemo.Application.DTOs;
using BackgroundJobsDemo.Application.Services;

namespace BackgroundJobsDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailsController(IEmailService emailService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequestDto request)
    {
        var id = await emailService.QueueEmailAsync(request);
        return Accepted(new EmailResponseDto(id, "queued"));
    }

    [HttpGet]
    public async Task<IActionResult> GetPendingEmails()
    {
        var emails = await emailService.GetPendingEmailsAsync();
        return Ok(emails);
    }
}
