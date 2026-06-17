using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using PBMS.Application.Payment.Interfaces;

namespace PBMS.Infrastructure.ExternalServices;

/// <summary>
/// Triển khai kết nối cổng thanh toán VNPay Sandbox sử dụng mã hóa HMAC-SHA512.
/// </summary>
public class VNPayGateway : IVNPayGateway
{
    private readonly string _tmnCode;
    private readonly string _hashSecret;
    private readonly string _baseUrl;
    private readonly string _returnUrl;

    public VNPayGateway(IConfiguration configuration)
    {
        _tmnCode = configuration["VNPay:TmnCode"] ?? "";
        _hashSecret = configuration["VNPay:HashSecret"] ?? "";
        _baseUrl = configuration["VNPay:BaseUrl"] ?? "";
        _returnUrl = configuration["VNPay:ReturnUrl"] ?? "";
    }

    /// <summary>
    /// Tạo URL Redirect chuyển hướng sang cổng thanh toán VNPay.
    /// </summary>
    public string CreatePaymentUrl(long orderCode, decimal amount, string description, string ipAddress)
    {
        var vnpayParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", _tmnCode },
            { "vnp_Amount", ((long)(amount * 100)).ToString() }, // Đơn vị tiền tệ VNPay nhân 100
            { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", string.IsNullOrEmpty(ipAddress) ? "127.0.0.1" : ipAddress },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", description },
            { "vnp_OrderType", "other" },
            { "vnp_ReturnUrl", _returnUrl },
            { "vnp_TxnRef", orderCode.ToString() }
        };

        var signDataBuilder = new StringBuilder();
        var queryBuilder = new StringBuilder();

        foreach (var kvp in vnpayParams)
         {
            if (!string.IsNullOrEmpty(kvp.Value))
            {
                // Mã hóa URL theo tiêu chuẩn của VNPay (khoảng trắng chuyển đổi thành + hoặc %20)
                string keyEncoded = WebUtility.UrlEncode(kvp.Key);
                string valueEncoded = WebUtility.UrlEncode(kvp.Value);

                signDataBuilder.Append(keyEncoded).Append('=').Append(valueEncoded).Append('&');
                queryBuilder.Append(keyEncoded).Append('=').Append(valueEncoded).Append('&');
            }
        }

        // Loại bỏ ký tự '&' ở cuối
        if (signDataBuilder.Length > 0)
        {
            signDataBuilder.Length--;
            queryBuilder.Length--;
        }

        string signData = signDataBuilder.ToString();
        string secureHash = CalculateHmacSha512(_hashSecret, signData);

        return $"{_baseUrl}?{queryBuilder}&vnp_SecureHash={secureHash}";
    }

    /// <summary>
    /// Xác thực chữ ký phản hồi của VNPay (IPN/Return URL).
    /// </summary>
    public bool VerifySignature(SortedDictionary<string, string> fields, string secureHash)
    {
        var signDataBuilder = new StringBuilder();
        foreach (var kvp in fields)
        {
            if (!string.IsNullOrEmpty(kvp.Value) && !kvp.Key.Equals("vnp_SecureHash") && !kvp.Key.Equals("vnp_SecureHashType"))
            {
                string keyEncoded = WebUtility.UrlEncode(kvp.Key);
                string valueEncoded = WebUtility.UrlEncode(kvp.Value);
                signDataBuilder.Append(keyEncoded).Append('=').Append(valueEncoded).Append('&');
            }
        }

        if (signDataBuilder.Length > 0)
        {
            signDataBuilder.Length--;
        }

        string signData = signDataBuilder.ToString();
        string calculatedHash = CalculateHmacSha512(_hashSecret, signData);

        return string.Equals(calculatedHash, secureHash, StringComparison.OrdinalIgnoreCase);
    }

    // Hàm mã hóa phụ trợ HMAC-SHA512
    private static string CalculateHmacSha512(string key, string inputData)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
        using var hmac = new HMACSHA512(keyBytes);
        byte[] hashValue = hmac.ComputeHash(inputBytes);
        return Convert.ToHexString(hashValue).ToLower();
    }
}
