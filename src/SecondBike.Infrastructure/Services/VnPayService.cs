using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// VNPay sandbox payment gateway integration.
/// Implements VNPay v2.1.0 API specification.
/// </summary>
public class VnPayService : IVnPayService
{
    private readonly string _tmnCode;
    private readonly string _hashSecret;
    private readonly string _paymentUrl;
    private readonly string _returnUrl;

    public VnPayService(IConfiguration configuration)
    {
        var section = configuration.GetSection("VnPay");
        _tmnCode = section["TmnCode"] ?? throw new ArgumentNullException("VnPay:TmnCode");
        _hashSecret = section["HashSecret"] ?? throw new ArgumentNullException("VnPay:HashSecret");
        _paymentUrl = section["PaymentUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        _returnUrl = section["ReturnUrl"] ?? throw new ArgumentNullException("VnPay:ReturnUrl");
    }

    public string CreatePaymentUrl(int orderId, decimal amount, string orderInfo, string clientIpAddress)
    {
        var txnRef = $"{orderId}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var createDate = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");

        // VNPay amount must be multiplied by 100 (no decimal)
        var vnpAmount = (long)(amount * 100);

        var queryParams = new SortedDictionary<string, string>
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", _tmnCode },
            { "vnp_Amount", vnpAmount.ToString() },
            { "vnp_CreateDate", createDate },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", clientIpAddress },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", orderInfo },
            { "vnp_OrderType", "other" },
            { "vnp_ReturnUrl", _returnUrl },
            { "vnp_TxnRef", txnRef }
        };

        var queryString = BuildQueryString(queryParams);
        var secureHash = ComputeHmacSha512(_hashSecret, queryString);

        return $"{_paymentUrl}?{queryString}&vnp_SecureHash={secureHash}";
    }

    public bool ValidateCallback(IDictionary<string, string> queryParams)
    {
        if (!queryParams.TryGetValue("vnp_SecureHash", out var receivedHash))
            return false;

        var sorted = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var kvp in queryParams)
        {
            if (kvp.Key.StartsWith("vnp_", StringComparison.Ordinal)
                && kvp.Key != "vnp_SecureHash"
                && kvp.Key != "vnp_SecureHashType")
            {
                sorted[kvp.Key] = kvp.Value;
            }
        }

        var queryString = BuildQueryString(sorted);
        var computedHash = ComputeHmacSha512(_hashSecret, queryString);

        return string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildQueryString(SortedDictionary<string, string> parameters)
    {
        var sb = new StringBuilder();
        foreach (var kvp in parameters)
        {
            if (sb.Length > 0) sb.Append('&');
            sb.Append(WebUtility.UrlEncode(kvp.Key));
            sb.Append('=');
            sb.Append(WebUtility.UrlEncode(kvp.Value));
        }
        return sb.ToString();
    }

    private static string ComputeHmacSha512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
