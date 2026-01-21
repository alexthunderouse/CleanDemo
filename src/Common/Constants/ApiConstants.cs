namespace CleanAPIDemo.Common.Constants;

public static class ApiConstants
{
    public const string ApiTitle = "CleanAPIDemo API";
    public const string ApiDescription = "A Clean Architecture API Demo";

    public static class Versions
    {
        public const string V1 = "1.0";
        public const string V2 = "2.0";
    }

    public static class Routes
    {
        public const string BaseRoute = "api/v{version:apiVersion}";
        public const string HealthCheck = "/health";
        public const string Ready = "/ready";
    }

    public static class Headers
    {
        public const string CorrelationId = "X-Correlation-ID";
        public const string ApiVersion = "X-Api-Version";
        public const string RequestId = "X-Request-ID";
    }
}
