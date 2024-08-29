namespace Sample.Options;

public class HangfireOptions
{
    public required HangfireServerOptions ServerOptions { get; set; }
    public required HangfireSqlServerStorageOptions SqlServerStorageOptions { get; set; }
    public  required HangfireDashboardOptions DashboardOptions { get; set; }
    public required HangfireJobAutomaticRetryOptions JobAutomaticRetryOptions { get; set; }
    public required HangfireJobManagerOptions JobManagerOptions { get; set; }
}