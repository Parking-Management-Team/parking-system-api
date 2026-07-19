using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PBMS.Application.ParkingSession.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PBMS.API.Workers
{
    public class OvertimeWarningWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OvertimeWarningWorker> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(1); // Kiểm tra mỗi 1 phút

        public OvertimeWarningWorker(IServiceProvider serviceProvider, ILogger<OvertimeWarningWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OvertimeWarningWorker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var parkingSessionService = scope.ServiceProvider.GetRequiredService<IParkingSessionService>();
                        _logger.LogInformation("OvertimeWarningWorker executing warnings check.");
                        await parkingSessionService.SendOvertimeWarningsAsync();
                        _logger.LogInformation("OvertimeWarningWorker warnings check completed.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in OvertimeWarningWorker.");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("OvertimeWarningWorker is stopping.");
        }
    }
}
