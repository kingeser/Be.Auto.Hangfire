namespace Sample.Options;

public class HangfireJobManagerOptions
{
    public bool DisableConcurrentlyJobExecution { get; set; }
    public TimeSpan WebRequestJobTimeout { get; set; } 
}