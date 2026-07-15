using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.ParkingSystemConfig.DTOs;
using PBMS.Application.ParkingSystemConfig.Interfaces;
using PBMS.Infrastructure.Data;

namespace PBMS.API.Controllers;

/// <summary>
/// Controller for managing system-wide configuration settings.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ParkingSystemConfigController : ControllerBase
{
    private readonly IParkingSystemConfigService _configService;

    public ParkingSystemConfigController(IParkingSystemConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    }

    /// <summary>
    /// Retrieves all system configuration settings.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var configs = await _configService.GetAllConfigsAsync();
        return Ok(BaseResponse<IEnumerable<ParkingSystemConfigDto>>.Ok(configs));
    }

    /// <summary>
    /// Retrieves a specific configuration setting by its key.
    /// </summary>
    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        var config = await _configService.GetByKeyAsync(key);
        if (config == null)
        {
            return NotFound(BaseResponse<ParkingSystemConfigDto>.Fail("CONFIG_NOT_FOUND", $"Configuration key '{key}' was not found."));
        }
        return Ok(BaseResponse<ParkingSystemConfigDto>.Ok(config));
    }

    /// <summary>
    /// Creates or updates a configuration setting.
    /// Only accessible by Managers/Admins.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] UpsertParkingSystemConfigRequest request)
    {
        var config = await _configService.UpsertConfigAsync(request);
        return Ok(BaseResponse<ParkingSystemConfigDto>.Ok(config, "Configuration updated successfully."));
    }

    /// <summary>
    /// Resets and re-seeds relative past, present, and future demo data.
    /// </summary>
    [HttpPost("reset-demo")]
    public async Task<IActionResult> ResetDemo([FromServices] AppDbContext context)
    {
        await DbInitializer.ResetDemoDataAsync(context);
        return Ok(BaseResponse<object>.Ok(null, "Demo data reset and re-seeded successfully."));
    }
}
