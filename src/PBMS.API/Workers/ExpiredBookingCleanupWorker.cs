using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PBMS.Application.Booking.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PBMS.API.Workers
{
    public class ExpiredBookingCleanupWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiredBookingCleanupWorker> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(5); // Chạy định kỳ mỗi 5 phút

        public ExpiredBookingCleanupWorker(IServiceProvider serviceProvider, ILogger<ExpiredBookingCleanupWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExpiredBookingCleanupWorker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                        _logger.LogInformation("ExpiredBookingCleanupWorker executing cleanup.");
                        await bookingService.CleanupExpiredBookingsAsync();
                        _logger.LogInformation("ExpiredBookingCleanupWorker cleanup completed.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in ExpiredBookingCleanupWorker.");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("ExpiredBookingCleanupWorker is stopping.");
        }
    }
}
