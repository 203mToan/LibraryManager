using MyApi.Model.Response;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace MyApi.Libraries
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());
        
        public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== VnPayLibrary.GetFullResponseData START ===");
                
                var vnPay = new VnPayLibrary();
                foreach (var (key, value) in collection)
                {
                    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    {
                        vnPay.AddResponseData(key, value);
                    }
                }

                var vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
                var vnpSecureHash = collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value.ToString();
                var orderInfo = vnPay.GetResponseData("vnp_OrderInfo");

                System.Diagnostics.Debug.WriteLine($"vnpSecureHash: {vnpSecureHash}");
                System.Diagnostics.Debug.WriteLine($"vnpResponseCode: {vnpResponseCode}");

                // Validate signature using raw query string
                var checkSignature = ValidateSignatureFromQuery(collection, hashSecret);
                
                System.Diagnostics.Debug.WriteLine($"Signature validation result: {checkSignature}");
                
                if (!checkSignature)
                {
                    System.Diagnostics.Debug.WriteLine("Signature validation FAILED - returning 97");
                    return new PaymentResponseModel()
                    {
                        Success = false,
                        VnPayResponseCode = "97",
                        OrderDescription = "Chữ ký không hợp lệ"
                    };
                }

                System.Diagnostics.Debug.WriteLine("Signature validation SUCCESS");

                // Try to parse OrderId and TransactionId safely
                var txnRefStr = vnPay.GetResponseData("vnp_TxnRef");
                var transactionNoStr = vnPay.GetResponseData("vnp_TransactionNo");

                var orderId = string.IsNullOrEmpty(txnRefStr) ? "0" : txnRefStr;
                var vnPayTranId = string.IsNullOrEmpty(transactionNoStr) ? "0" : transactionNoStr;

                System.Diagnostics.Debug.WriteLine($"OrderId: {orderId}, TransactionId: {vnPayTranId}");

                return new PaymentResponseModel()
                {
                    Success = true,
                    PaymentMethod = "VnPay",
                    OrderDescription = orderInfo,
                    OrderId = orderId,
                    PaymentId = vnPayTranId,
                    TransactionId = vnPayTranId,
                    Token = vnpSecureHash,
                    VnPayResponseCode = vnpResponseCode
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetFullResponseData Exception: {ex.Message}");
                return new PaymentResponseModel()
                {
                    Success = false,
                    VnPayResponseCode = "-1",
                    OrderDescription = $"Lỗi xử lý: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Validate signature directly from IQueryCollection - using raw query string
        /// </summary>
        public bool ValidateSignatureFromQuery(IQueryCollection collection, string hashSecret)
        {
            try
            {
                var vnpSecureHash = collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value.ToString();
                
                if (string.IsNullOrEmpty(vnpSecureHash))
                {
                    System.Diagnostics.Debug.WriteLine("vnp_SecureHash is empty");
                    return false;
                }

                // Build raw data string from sorted query params (exclude SecureHash and SecureHashType)
                var sortedParams = collection
                    .Where(k => k.Key.StartsWith("vnp_") && 
                               k.Key != "vnp_SecureHash" && 
                               k.Key != "vnp_SecureHashType")
                    .OrderBy(k => k.Key, StringComparer.InvariantCulture)
                    .ToList();

                var rawData = new StringBuilder();
                foreach (var param in sortedParams)
                {
                    if (!string.IsNullOrEmpty(param.Value))
                    {
                        // Use the decoded value and encode it properly matching VNPay's encoding
                        var encodedValue = UrlEncodeVnPay(param.Value!);
                        rawData.Append(param.Key + "=" + encodedValue + "&");
                    }
                }

                // Remove trailing &
                if (rawData.Length > 0)
                {
                    rawData.Length--;
                }

                var signData = rawData.ToString();
                var myChecksum = HmacSha512(hashSecret, signData);

                System.Diagnostics.Debug.WriteLine($"=== SIGNATURE VALIDATION ===");
                System.Diagnostics.Debug.WriteLine($"Sign Data Length: {signData.Length}");
                System.Diagnostics.Debug.WriteLine($"Sign Data: {signData}");
                System.Diagnostics.Debug.WriteLine($"Input Hash: {vnpSecureHash}");
                System.Diagnostics.Debug.WriteLine($"My Checksum: {myChecksum}");
                System.Diagnostics.Debug.WriteLine($"Match: {myChecksum.Equals(vnpSecureHash, StringComparison.InvariantCultureIgnoreCase)}");

                return myChecksum.Equals(vnpSecureHash, StringComparison.InvariantCultureIgnoreCase);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ValidateSignatureFromQuery Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// URL Encode matching VNPay's encoding format
        /// VNPay uses uppercase hex and encodes all special characters including parentheses
        /// </summary>
        private static string UrlEncodeVnPay(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var sb = new StringBuilder();
            foreach (char c in value)
            {
                if (IsUnreservedCharacter(c))
                {
                    sb.Append(c);
                }
                else if (c == ' ')
                {
                    sb.Append('+');
                }
                else
                {
                    // Encode to %XX format (uppercase)
                    var bytes = Encoding.UTF8.GetBytes(new[] { c });
                    foreach (var b in bytes)
                    {
                        sb.Append('%');
                        sb.Append(b.ToString("X2")); // Uppercase hex
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Check if character is unreserved (should not be encoded)
        /// RFC 3986 unreserved characters: A-Z a-z 0-9 - _ . ~
        /// </summary>
        private static bool IsUnreservedCharacter(char c)
        {
            return (c >= 'A' && c <= 'Z') ||
                   (c >= 'a' && c <= 'z') ||
                   (c >= '0' && c <= '9') ||
                   c == '-' || c == '_' || c == '.' || c == '~';
        }

        public string GetIpAddress(HttpContext context)
        {
            var ipAddress = string.Empty;
            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;

                if (remoteIpAddress != null)
                {
                    if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                            .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                    }

                    if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();

                    return ipAddress;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "127.0.0.1";
        }
        
        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }
        
        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }
        
        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }
        
        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();

            foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                // Use UrlEncodeVnPay for consistency with VNPay's encoding
                data.Append(key + "=" + UrlEncodeVnPay(value) + "&");
            }

            var querystring = data.ToString();

            baseUrl += "?" + querystring;
            var signData = querystring;
            if (signData.Length > 0)
            {
                signData = signData.Remove(data.Length - 1, 1);
            }

            var vnpSecureHash = HmacSha512(vnpHashSecret, signData);
            baseUrl += "vnp_SecureHash=" + vnpSecureHash;

            return baseUrl;
        }
        
        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseData();
            var myChecksum = HmacSha512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }
        
        private string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }
        
        private string GetResponseData()
        {
            var data = new StringBuilder();
            
            var sortedData = new SortedList<string, string>(_responseData, new VnPayCompare());
            
            if (sortedData.ContainsKey("vnp_SecureHashType"))
            {
                sortedData.Remove("vnp_SecureHashType");
            }

            if (sortedData.ContainsKey("vnp_SecureHash"))
            {
                sortedData.Remove("vnp_SecureHash");
            }

            foreach (var (key, value) in sortedData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }

            return data.ToString();
        }
    }
    
    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}

