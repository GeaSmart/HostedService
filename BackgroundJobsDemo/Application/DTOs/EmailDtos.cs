namespace BackgroundJobsDemo.Application.DTOs;

public record EmailRequestDto(string To, string Subject, string Body);
public record EmailResponseDto(int Id, string Status);
