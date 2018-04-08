namespace CorrelationId
{
    /// <summary>
    /// Options for correlation ids.
    /// </summary>
    public class CorrelationIdOptions
    {
        private const string DefaultHeader = "X-Correlation-ID";

        /// <summary>
        /// The name of the header from which the Correlation ID is read/written.
        /// </summary>
        public string Header { get; set; } = DefaultHeader;

        /// <summary>
        /// <para>
        /// Controls whether the correlation ID is returned in the response headers.
        /// </para>
        /// <para>Default: true</para>
        /// </summary>
        public bool IncludeInResponse { get; set; } = true;

        /// <summary>
        /// <para>
        /// Controls whether the ASP.NET Core TraceIdentifier will be set to match the CorrelationId.
        /// </para>
        /// <para>Default: true</para>
        /// </summary>
        public bool UpdateTraceIdentifier { get; set; } = true;

        /// <summary>
        /// <para>
        /// Controls whether a GUID will be used in cases where no correlation ID is retrieved from the request header.
        /// When false the TraceIdentifier for the current request will be used.
        /// </para>
        /// <para> Default: false.</para>
        /// </summary>
        public bool UseGuidForCorrelationId { get; set; } = false;
    }
}
