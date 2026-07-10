namespace PBMS.Application.ParkingSession.DTOs;

public class OcrResultDto
{
    public string LicensePlate { get; set; } = string.Empty;
    public double Confidence { get; set; }
}
