namespace TaskFlow.Application.Common.Options
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public required string Issuer { get; init; }
        public required string Audience { get; init; }
        public required string Secret { get; init; }
        public int ExpiryMinutes { get; init; } = 60;
    }
}
