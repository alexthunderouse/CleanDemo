using System.Diagnostics;
using System.Text.Json;
using CleanAPIDemo.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CleanAPIDemo.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        var problemDetails = exception switch
        {
            DomainException domainException => CreateDomainProblemDetails(domainException, context, traceId),
            ValidationException validationException => CreateValidationProblemDetails(validationException, context, traceId),
            KeyNotFoundException => CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "not_found",
                "Resource Not Found",
                exception.Message,
                context,
                traceId),
            _ => CreateProblemDetails(
                StatusCodes.Status500InternalServerError,
                "internal_error",
                "Internal Server Error",
                "An unexpected error occurred. Please try again later.",
                context,
                traceId)
        };

        LogException(exception, traceId);

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsJsonAsync(problemDetails, options);
    }

    private static ProblemDetails CreateDomainProblemDetails(
        DomainException exception,
        HttpContext context,
        string traceId)
    {
        return new ProblemDetails
        {
            Status = exception.StatusCode,
            Title = FormatTitle(exception.Type),
            Detail = exception.Message,
            Type = exception.Type,
            Instance = context.Request.Path,
            Extensions =
            {
                ["code"] = exception.Code,
                ["traceId"] = traceId,
                ["timestamp"] = DateTime.UtcNow
            }
        };
    }

    private static ProblemDetails CreateValidationProblemDetails(
        ValidationException exception,
        HttpContext context,
        string traceId)
    {
        var errors = exception.Errors
            .Select(e => new
            {
                field = e.PropertyName,
                code = e.ErrorMessage,
                rejectedValue = e.AttemptedValue
            })
            .ToList();

        return new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Error",
            Detail = $"{errors.Count} validation error(s) occurred",
            Type = "validation_error",
            Instance = context.Request.Path,
            Extensions =
            {
                ["code"] = "validation_failed",
                ["traceId"] = traceId,
                ["timestamp"] = DateTime.UtcNow,
                ["errors"] = errors
            }
        };
    }

    private static ProblemDetails CreateProblemDetails(
        int statusCode,
        string type,
        string title,
        string detail,
        HttpContext context,
        string traceId)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = type,
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId,
                ["timestamp"] = DateTime.UtcNow
            }
        };
    }

    private void LogException(Exception exception, string traceId)
    {
        switch (exception)
        {
            case DomainException domainException:
                _logger.LogWarning(
                    exception,
                    "Domain exception occurred. Type: {Type}, Code: {Code}, TraceId: {TraceId}",
                    domainException.Type,
                    domainException.Code,
                    traceId);
                break;
            case ValidationException:
                _logger.LogWarning(
                    exception,
                    "Validation exception occurred. TraceId: {TraceId}",
                    traceId);
                break;
            default:
                _logger.LogError(
                    exception,
                    "Unhandled exception occurred. TraceId: {TraceId}",
                    traceId);
                break;
        }
    }

    private static string FormatTitle(string type) =>
        string.Join(" ", type.Split('_').Select(word =>
            char.ToUpper(word[0]) + word[1..]));
}
