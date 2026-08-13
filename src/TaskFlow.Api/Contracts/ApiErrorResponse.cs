namespace TaskFlow.Api.Contracts
{
    public record ApiErrorResponse(string Type, string Message, IReadOnlyDictionary<string, string[]>? Errors, string TraceId);
}
