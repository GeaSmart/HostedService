using BackgroundJobsDemo.Application.Options;
using BackgroundJobsDemo.Application.Services;
using BackgroundJobsDemo.Infrastructure.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<EmailProcessingOptions>(
    builder.Configuration.GetSection("EmailProcessing"));
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddHostedService<EmailProcessingJob>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();