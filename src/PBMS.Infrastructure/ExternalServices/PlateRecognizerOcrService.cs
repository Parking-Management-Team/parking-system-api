using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using PBMS.Application.Common;
using PBMS.Application.Common.Interfaces;
using PBMS.Application.ParkingSession.DTOs;

namespace PBMS.Infrastructure.ExternalServices;

public class PlateRecognizerOcrService : ILicensePlateOcrService
{
    private readonly HttpClient _httpClient;
    private readonly string _token;
    private readonly string _apiUrl;

    public PlateRecognizerOcrService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _token = configuration["PlateRecognizer:Token"] ?? "";
        _apiUrl = configuration["PlateRecognizer:ApiUrl"] ?? "https://api.platerecognizer.com/v1/plate-reader/";
    }

    public async Task<BaseResponse<OcrResultDto>> ScanLicensePlateAsync(string base64Image)
    {
        if (string.IsNullOrWhiteSpace(base64Image))
        {
            return BaseResponse<OcrResultDto>.Fail("INVALID_IMAGE", "Image data is empty.");
        }

        if (string.IsNullOrWhiteSpace(_token) || _token == "YOUR_PLATE_RECOGNIZER_TOKEN")
        {
            return BaseResponse<OcrResultDto>.Fail("CONFIG_ERROR", "Plate Recognizer API Token is not configured. Please sign up at platerecognizer.com and add your token to appsettings.json.");
        }

        try
        {
            // Clean base64 string if it contains the data URL prefix
            var base64Data = base64Image;
            if (base64Image.Contains(","))
            {
                base64Data = base64Image.Split(',')[1];
            }

            var imageBytes = Convert.FromBase64String(base64Data);

            using var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", _token);

            var content = new MultipartFormDataContent();
            var byteContent = new ByteArrayContent(imageBytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(byteContent, "upload", "plate.jpg");
            content.Add(new StringContent("vn"), "regions");

            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return BaseResponse<OcrResultDto>.Fail("API_ERROR", $"Plate Recognizer API error: {response.StatusCode} - {errorBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<PlateRecognizerResponse>();
            if (result == null || result.Results == null || result.Results.Count == 0)
            {
                return BaseResponse<OcrResultDto>.Fail("NO_PLATE_DETECTED", "No license plate detected in the image.");
            }

            var bestResult = result.Results.OrderByDescending(r => r.Confidence).First();
            var plateText = bestResult.Plate.ToUpper().Replace(" ", "").Replace("-", "").Replace(".", "");

            return BaseResponse<OcrResultDto>.Ok(new OcrResultDto
            {
                LicensePlate = plateText,
                Confidence = bestResult.Confidence
            }, "License plate scanned successfully.");
        }
        catch (FormatException)
        {
            return BaseResponse<OcrResultDto>.Fail("INVALID_IMAGE", "Image data is not a valid base64 string.");
        }
        catch (Exception ex)
        {
            return BaseResponse<OcrResultDto>.Fail("OCR_EXCEPTION", $"An error occurred during OCR: {ex.Message}");
        }
    }
}

public class PlateRecognizerResponse
{
    [JsonPropertyName("results")]
    public List<PlateRecognizerResult> Results { get; set; } = new();
}

public class PlateRecognizerResult
{
    [JsonPropertyName("plate")]
    public string Plate { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}
