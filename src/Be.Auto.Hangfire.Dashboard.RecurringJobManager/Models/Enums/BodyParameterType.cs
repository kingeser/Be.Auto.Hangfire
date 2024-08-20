namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums
{
    /// <summary>
    /// Represents the different types of body parameters that can be used in HTTP requests.
    /// </summary>
    public enum BodyParameterType
    {
        None,
        /// <summary>
        /// The body content is in JSON format.
        /// </summary>
        Json,

        /// <summary>
        /// The body content is in XML format.
        /// </summary>
        Xml,

        /// <summary>
        /// The body content is in application/x-www-form-urlencoded format.
        /// This format is typically used for form submissions.
        /// </summary>
        FormUrlEncoded,
        /// <summary>
        /// The body content is in multipart/form-data format.
        /// This format is commonly used for form submissions that include file uploads.
        /// </summary>
        FormData,

        /// <summary>
        /// The body content is in plain text format.
        /// </summary>
        PlainText,

    }

}