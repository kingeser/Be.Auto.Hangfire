// ReSharper disable InconsistentNaming
namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums
{
    /// <summary>
    /// Represents the different HTTP methods that can be used in web requests.
    /// </summary>
    internal enum HttpMethodType
    {
        /// <summary>
        /// The GET method requests a representation of the specified resource. 
        /// Requests using GET should only retrieve data.
        /// </summary>
        GET,

        /// <summary>
        /// The POST method submits an entity to the specified resource, often causing a change in state or side effects on the server.
        /// </summary>
        POST,

        /// <summary>
        /// The PUT method replaces all current representations of the target resource with the request payload.
        /// </summary>
        PUT,

        /// <summary>
        /// The DELETE method deletes the specified resource.
        /// </summary>
        DELETE,

        /// <summary>
        /// The PATCH method is used to apply partial modifications to a resource.
        /// </summary>
        PATCH,

        /// <summary>
        /// The HEAD method asks for a response identical to a GET request, but without the response body.
        /// </summary>
        HEAD,

        /// <summary>
        /// The OPTIONS method describes the communication options for the target resource.
        /// </summary>
        OPTIONS,

        /// <summary>
        /// The TRACE method performs a message loop-back test along the path to the target resource.
        /// </summary>
        TRACE
    }

}