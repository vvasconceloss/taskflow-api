using FluentValidation.Results;

namespace TaskFlow.Application.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IEnumerable<ValidationFailure> failures)
            : base("One or more validation errors occurred.")
        {
            Errors = failures
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).ToArray());
        }

        public IReadOnlyDictionary<string, string[]> Errors { get; }
    }
}
