namespace Sample.Options;

public class HangfireServerOptions
{
    public string ServerName { get; set; }
    public bool IsLightweightServer { get; set; }
    public int WorkerCount { get; set; }
    public string[] Queues { get; set; }
    public TimeSpan StopTimeout { get; set; }
    public TimeSpan ShutdownTimeout { get; set; }
    public TimeSpan SchedulePollingInterval { get; set; }
    public TimeSpan HeartbeatInterval { get; set; }
    public TimeSpan ServerCheckInterval { get; set; }
    public TimeSpan ServerTimeout { get; set; }
    public TimeSpan CancellationCheckInterval { get; set; }
    public int MaxDegreeOfParallelismForSchedulers { get; set; }
}