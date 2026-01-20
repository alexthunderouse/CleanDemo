namespace CleanAPIDemo.Domain.Exceptions;

/// <summary>
/// Exception thrown when a concurrency conflict occurs.
/// Returns HTTP 409 Conflict.
/// </summary>
public class ConcurrencyConflictException : DomainException
{
    public ConcurrencyConflictException(string resource)
        : base("concurrency_conflict", $"{resource}_concurrency_conflict",
               $"The {resource} was modified by another user. Please refresh and try again.", 409)
    { }
}
