namespace CleanAPIDemo.Common.Constants;

public static class ValidationConstants
{
    public static class Product
    {
        public const int NameMaxLength = 200;
        public const int DescriptionMaxLength = 1000;
        public const decimal MinPrice = 0.01m;
        public const decimal MaxPrice = 999999.99m;
    }

    public static class Pagination
    {
        public const int DefaultPage = 1;
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
    }
}
