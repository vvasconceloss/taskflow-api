using System.Text.Json.Serialization;
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

var builder = WebApplication.CreateBuilder(args);

var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}{Properties}");

if (!builder.Environment.IsEnvironment("Testing"))
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapAuthEndpoints();
app.MapWorkspaceEndpoints();
app.MapProjectEndpoints();
app.MapTaskEndpoints();
app.Run();
