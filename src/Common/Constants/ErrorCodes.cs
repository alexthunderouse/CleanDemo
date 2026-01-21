namespace CleanAPIDemo.Common.Constants;

public static class ErrorCodes
{
    public static class General
    {
        public const string InternalError = "INTERNAL_ERROR";
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string NotFound = "NOT_FOUND";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string BadRequest = "BAD_REQUEST";
        public const string Conflict = "CONFLICT";
    }

    public static class Product
    {
        public const string NotFound = "PRODUCT_NOT_FOUND";
        public const string AlreadyExists = "PRODUCT_ALREADY_EXISTS";
        public const string InvalidPrice = "PRODUCT_INVALID_PRICE";
        public const string NameRequired = "PRODUCT_NAME_REQUIRED";
        public const string NameTooLong = "PRODUCT_NAME_TOO_LONG";
        public const string DescriptionTooLong = "PRODUCT_DESCRIPTION_TOO_LONG";
    }
}
