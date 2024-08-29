using Hangfire;

namespace Sample.Options;

public class HangfireJobAutomaticRetryOptions
{
    public int Attempts { get; set; } = 3;
    public AttemptsExceededAction OnAttemptsExceeded { get; set; }
    public bool LogEvents { get; set; } = true;
}