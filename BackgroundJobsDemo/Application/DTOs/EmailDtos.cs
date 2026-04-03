using System.ComponentModel.DataAnnotations;

namespace BackgroundJobsDemo.Application.DTOs;

public record EmailRequestDto(
    [Required(ErrorMessage = "Recipient 'To' is required")]
    [EmailAddress(ErrorMessage = "Invalid email format for 'To'")]
    string To,
    
    [Required(ErrorMessage = "Subject is required")]
    string Subject,
    
    string Body);

public record EmailResponseDto(int Id, string Status);
