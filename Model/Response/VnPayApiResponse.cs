namespace MyApi.Model.Response
{
    /// <summary>
    /// Response model for VnPay API operations
    /// </summary>
    public class VnPayApiResponse
    {
        /// <summary>
        /// Response code from VnPay
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Response data (URL or transaction details)
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Success flag
        /// </summary>
        public bool Success { get; set; }
    }
}
