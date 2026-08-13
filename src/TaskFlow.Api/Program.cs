using System.Text.Json.Serialization;
using TaskFlow.Api.Endpoints;
using TaskFlow.Api.Exceptions;
using TaskFlow.Api.Extensions;
using TaskFlow.Api.Services;
using TaskFlow.Application;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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
