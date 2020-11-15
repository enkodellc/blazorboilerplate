namespace BlazorBoilerplate.Shared.Dto
{
    /// <summary>
    /// Models the data for the error page.
    /// </summary>
    public class ErrorMessage
    {
        /// <summary>
        /// The display mode passed from the authorization request.
        /// </summary>
        /// <value>
        /// The display mode.
        /// </value>
        public string DisplayMode { get; set; }

        /// <summary>
        /// The UI locales passed from the authorization request.
        /// </summary>
        /// <value>
        /// The UI locales.
        /// </value>
        public string UiLocales { get; set; }

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        /// <value>
        /// The error code.
        /// </value>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the error description.
        /// </summary>
        /// <value>
        /// The error description.
        /// </value>
        public string ErrorDescription { get; set; }

        /// <summary>
        /// The per-request identifier. This can be used to display to the end user and can be used in diagnostics.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public string RequestId { get; set; }

        /// <summary>
        /// The redirect URI.
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// The response mode.
        /// </summary>
        public string ResponseMode { get; set; }

        /// <summary>
        /// The client id making the request (if available).
        /// </summary>
        public string ClientId { get; set; }
    }
}
