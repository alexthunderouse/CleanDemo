namespace CleanAPIDemo.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to create a duplicate entry.
/// Returns HTTP 409 Conflict.
/// </summary>
public class DuplicateEntryException : DomainException
{
    public DuplicateEntryException(string field)
        : base("duplicate_entry", $"{field}_duplicate",
               $"A record with this {field} already exists", 409)
    { }

    public DuplicateEntryException(string field, object value)
        : base("duplicate_entry", $"{field}_duplicate",
               $"A record with {field} '{value}' already exists", 409)
    { }
}
