using PBMS.Application.Common;
using PBMS.Application.ParkingSession.DTOs;

namespace PBMS.Application.Common.Interfaces;

public interface ILicensePlateOcrService
{
    Task<BaseResponse<OcrResultDto>> ScanLicensePlateAsync(string base64Image);
}
