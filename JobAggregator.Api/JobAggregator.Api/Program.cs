using JobAggregator.Api.Data;
using JobAggregator.Api.Data.Repositories.Implementations;
using JobAggregator.Api.Data.Repositories.Interfaces;
using JobAggregator.Api.Services.Implementations;
using JobAggregator.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;
using Quartz;
using JobAggregator.Api.Helpers;
using JobAggregator.Api.Jobs;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.IsDevelopment();

var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Scope}{Message:lj}{NewLine}{Exception}");

if (isDevelopment)
{
    loggerConfiguration.MinimumLevel.Debug()
        .WriteTo.File($"Logs/{DateTime.Now:yyyy-MM-dd}_DevelopmentLog.txt",
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Scope}{Message:lj}{NewLine}{Exception}");
}
else
{
    loggerConfiguration.MinimumLevel.Information()
        .WriteTo.File($"Logs/{DateTime.Now:yyyy-MM-dd}_ProductionLog.txt",
            restrictedToMinimumLevel: LogEventLevel.Error,
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Scope}{Message:lj}{NewLine}{Exception}");
}

Log.Logger = loggerConfiguration.CreateLogger();

// Add Serilog to the logging providers
builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IJobPostingRepository, JobPostingRepository>();
builder.Services.AddScoped<IJobPostingService, JobPostingService>();

// Register scrapers
builder.Services.AddScoped<IJobScraper, PraceCzScraper>();
builder.Services.AddScoped<IJobScraper, JobsContactScraper>();

builder.Services.AddScoped<ScraperManager>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
            .WithOrigins("http://localhost:3000", "https://localhost:3000", "https://localhost:7139")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("ScrapingJob");
    q.AddJob<ScrapingJob>(opts => opts.WithIdentity(jobKey));

    var nextFireTime = SchedulerHelper.GetNextRandomTime();

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("ScrapingJob-trigger")
        .StartAt(nextFireTime)
    );
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

builder.Logging.ClearProviders();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
