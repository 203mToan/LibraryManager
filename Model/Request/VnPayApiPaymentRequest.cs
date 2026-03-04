namespace MyApi.Model.Request
{
    public class VnPayApiPaymentRequest
    {
        public string OrderId { get; set; }
        public long Amount { get; set; }
        public string OrderInfo { get; set; }
        public string OrderType { get; set; } = "other";
        public string Language { get; set; } = "vn";
        public string IpAddr { get; set; }
        public string BankCode { get; set; }
        public int ExpireTime { get; set; } = 15;
    }
}
