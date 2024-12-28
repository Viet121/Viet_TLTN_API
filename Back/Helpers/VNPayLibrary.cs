using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using Org.BouncyCastle.Asn1.Ocsp;
using Back.Models;

namespace Back.Helpers
{
    public class VNPayLibrary
    {
        private SortedList<string, string> requestData = new SortedList<string, string>(new VnPayCompare());
        private SortedList<string, string> responseData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string value) => requestData[key] = value;
        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in requestData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            string queryString = data.ToString();

            baseUrl += "?" + queryString;
            String signData = queryString;
            if (signData.Length > 0)
            {

                signData = signData.Remove(data.Length - 1, 1);
            }
            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);
            baseUrl += "vnp_SecureHash=" + vnp_SecureHash;

            return baseUrl;
        }
        public static String HmacSHA512(string key, String inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }
        public void AddResponseData(string key, string value) => responseData[key] = value;

        //vnpay
        public bool VerifyVNPaySignature(VNPayReturnRequest request)
        {
            // Khóa bí mật từ VNPAY
            string secretKey = "TSTJ49OGEXRTDFUGHZ2A9FVOC1HOGR7T";

            // Lấy dữ liệu raw để tạo chữ ký
            var rawData = GetRawData(request);

            // Tính chữ ký từ raw data
            var computedHash = HmacSHA512(secretKey, rawData);

            // So sánh chữ ký được tính toán với chữ ký nhận từ callback
            return string.Equals(computedHash, request.vnp_SecureHash, StringComparison.OrdinalIgnoreCase);
        }

        // Hàm hỗ trợ để lấy raw data từ request
        private string GetRawData(VNPayReturnRequest request)
        {
            // Lấy tất cả tham số trừ `vnp_SecureHash` và sắp xếp theo thứ tự tên tham số
            var properties = request.GetType().GetProperties()
                .Where(p => p.Name != nameof(request.vnp_SecureHash))
                .OrderBy(p => p.Name)
                .ToDictionary(p => p.Name, p => p.GetValue(request)?.ToString());

            // Tạo chuỗi `key=value` ngăn cách bởi `&`
            return string.Join("&", properties.Select(kv => $"{kv.Key}={kv.Value}"));
        }
        // gpt
        public string GetResponseData(string key) => responseData.TryGetValue(key, out var value) ? value : null;
        public bool ValidateSignature2(string secureHash, string vnpHashSecret)
        {
            string signData = string.Join("&", responseData.Where(kvp => kvp.Key != "vnp_SecureHash")
                .Select(kvp => $"{kvp.Key}={kvp.Value}"));
            return secureHash == HashData(vnpHashSecret, signData);
        }
        private string HashData(string secret, string data)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
            return BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).Replace("-", "").ToLower();
        }
        public string GetRequestRawData()
        {
            // Sắp xếp key theo thứ tự bảng chữ cái để đảm bảo tính nhất quán khi tạo checksum
            var sortedRequestData = requestData
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => $"{kvp.Key}={kvp.Value}");

            // Nối các cặp key=value bằng ký tự '&'
            return string.Join("&", sortedRequestData);
        }
        public Dictionary<string, string> GetRequestData()
        {
            return new Dictionary<string, string>(requestData); // Trả về bản sao dữ liệu để tránh thay đổi dữ liệu gốc
        }

        /*
        public static string GetIpAddress()
        {
            string ipAddress;
            try
            {
                ipAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress) || (ipAddress.ToLower() == "unknown") || ipAddress.Length > 45)
                    ipAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            catch (Exception ex)
            {
                ipAddress = "Invalid IP:" + ex.Message;
            }

            return ipAddress;
        }*/
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
