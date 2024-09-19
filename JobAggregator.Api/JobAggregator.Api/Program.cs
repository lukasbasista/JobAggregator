using JobAggregator.Api.Data;
using JobAggregator.Api.Data.Repositories.Implementations;
using JobAggregator.Api.Data.Repositories.Interfaces;
using JobAggregator.Api.Services.Implementations;
using JobAggregator.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IJobPostingRepository, JobPostingRepository>();

builder.Services.AddScoped<IJobPostingService, JobPostingService>();

builder.Services.AddScoped<IJobScraper, JobsContactScraper>();

builder.Services.AddScoped<ScraperManager>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
            .WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

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