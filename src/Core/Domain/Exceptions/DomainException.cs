namespace CleanAPIDemo.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-level exceptions.
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }
    public string Type { get; }

    public DomainException(string type, string code, string message, int statusCode)
        : base(message)
    {
        Type = type;
        Code = code;
        StatusCode = statusCode;
    }
}
