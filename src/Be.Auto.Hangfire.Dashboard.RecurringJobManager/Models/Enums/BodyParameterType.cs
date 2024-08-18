using System.ComponentModel;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums
{
    /// <summary>
    /// Represents the different types of body parameters that can be used in HTTP requests.
    /// </summary>
    public enum BodyParameterType
    {
        /// <summary>
        /// Represents a JSON formatted body parameter.
        /// </summary>
        [Description("Json")]
        Json,

        /// <summary>
        /// Represents an XML formatted body parameter.
        /// </summary>
        [Description("Xml")]
        Xml,

        /// <summary>
        /// Represents a body parameter formatted as form data, typically used in POST requests.
        /// </summary>
        [Description("Form Data")]
        FormData
    }

}