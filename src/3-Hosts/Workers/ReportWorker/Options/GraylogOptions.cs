namespace CleanAPIDemo.Worker.Options;

public class GraylogOptions
{
    public const string SectionName = "Graylog";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 12201;
    public string Facility { get; set; } = "CleanAPIDemo.Worker";
}
