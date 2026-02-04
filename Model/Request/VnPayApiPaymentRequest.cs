namespace MyApi.Model.Request
{
    /// <summary>
    /// Request model for VnPay API payment
    /// </summary>
    public class VnPayApiPaymentRequest
    {
        /// <summary>
        /// Order ID - Mã ??n hàng
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Amount in VND (before multiplying by 100)
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// Order information - Thông tin ??n hàng
        /// </summary>
        public string OrderInfo { get; set; }

        /// <summary>
        /// Order type (default: other)
        /// </summary>
        public string OrderType { get; set; } = "other";

        /// <summary>
        /// Language (vn or en)
        /// </summary>
        public string Language { get; set; } = "vn";

        /// <summary>
        /// Client IP address
        /// </summary>
        public string IpAddr { get; set; }

        /// <summary>
        /// Bank code (optional) - VNPAYQR, VNBANK, INTCARD
        /// </summary>
        public string BankCode { get; set; }

        /// <summary>
        /// Expiration time in minutes (default: 15)
        /// </summary>
        public int ExpireTime { get; set; } = 15;
    }
}
