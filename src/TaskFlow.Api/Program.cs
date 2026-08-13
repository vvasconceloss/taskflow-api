using System.Text.Json.Serialization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using TaskFlow.Api.Endpoints;
using TaskFlow.Api.Exceptions;
using TaskFlow.Api.Extensions;
using TaskFlow.Api.Logging;
using TaskFlow.Api.Services;
using TaskFlow.Application;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Infrastructure;
using TaskFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}{Properties}");

if (builder.Environment.IsDevelopment())
{
    loggerConfiguration.WriteTo.File(
        path: "logs/taskflow-.json",
        rollingInterval: RollingInterval.Day,
        formatter: new JsonFormatter(renderMessage: true));
}

var logger = loggerConfiguration.CreateLogger();
Log.Logger = logger;
builder.Host.UseSerilog(logger);
builder.Services.AddSerilog(logger, dispose: false);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("login", limiter =>
    {
        limiter.PermitLimit = builder.Configuration.GetValue("RateLimiting:Login:PermitLimit", 5);
        limiter.Window = TimeSpan.FromMinutes(builder.Configuration.GetValue("RateLimiting:Login:WindowMinutes", 1));
        limiter.QueueLimit = 0;
    });
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy("ApiCors", policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskFlow API",
        Version = "v1",
        Description = "A task management API for teams — workspaces, projects and tasks with role-based access."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Copy the token returned by POST /auth/login and paste it below."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document, string.Empty)] = new List<string>()
        });
});

var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("traceId", httpContext.TraceIdentifier);

        var userId = RequestLogEnricher.GetUserId(httpContext.User);
        if (userId is not null)
        {
            diagnosticContext.Set("userId", userId);
        }
    };
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ApiCors");
app.UseRateLimiter();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Configuration.GetValue("Database:AutoMigrate", true))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.MapHealthChecks("/health");
app.MapAuthEndpoints();
app.MapWorkspaceEndpoints();
app.MapProjectEndpoints();
app.MapTaskEndpoints();
app.Run();
