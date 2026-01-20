namespace CleanAPIDemo.Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// Returns HTTP 404 Not Found.
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string resource)
        : base("not_found", $"{resource}_not_found",
               $"The requested {resource} was not found", 404)
    { }

    public NotFoundException(string resource, object key)
        : base("not_found", $"{resource}_not_found",
               $"The {resource} with key '{key}' was not found", 404)
    { }
}
