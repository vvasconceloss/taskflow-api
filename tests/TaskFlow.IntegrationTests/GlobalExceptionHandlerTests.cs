using System.Text.Json;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using TaskFlow.Api.Exceptions;
using TaskFlow.Application.Common.Exceptions;

namespace TaskFlow.IntegrationTests;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WithUnknownException_ShouldReturn500WithoutDetails()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(
            context, new InvalidOperationException("secret internal detail"), CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        context.Response.Body.Position = 0;
        var body = await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body);
        body.GetProperty("type").GetString().Should().Be("InternalServerError");
        body.GetProperty("message").GetString().Should().Be("An unexpected error occurred.");
        body.ToString().Should().NotContain("secret internal detail");
    }

    [Fact]
    public async Task TryHandleAsync_WithValidationException_ShouldReturn400WithFieldErrors()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new ValidationException(
            new[] { new ValidationFailure("title", "Title is required.") });

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        context.Response.Body.Position = 0;
        var body = await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body);
        body.GetProperty("type").GetString().Should().Be("ValidationError");
        body.GetProperty("message").GetString().Should().Be("One or more validation errors occurred.");
        body.GetProperty("errors").GetProperty("title")[0].GetString().Should().Be("Title is required.");
        body.GetProperty("traceId").GetString().Should().NotBeNullOrEmpty();
    }
}
