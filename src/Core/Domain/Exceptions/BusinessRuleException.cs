namespace CleanAPIDemo.Domain.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated.
/// Returns HTTP 422 Unprocessable Entity.
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string code, string message)
        : base("business_rule_violation", code, message, 422)
    { }
}
