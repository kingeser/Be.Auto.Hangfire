using System.ComponentModel;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums
{
    /// <summary>
    /// Represents the types of jobs that can be processed in the system.
    /// </summary>
    internal enum JobType
    {
        /// <summary>
        /// Represents a job that involves calling a method.
        /// </summary>
        [Description("Method Call")]
        MethodCall,

        /// <summary>
        /// Represents a job that involves making a web request.
        /// </summary>
        [Description("Web Request")]
        WebRequest
    }
}