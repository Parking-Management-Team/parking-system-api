using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PBMS.Application.Pricing.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PBMS.API.Workers
{
    public class ExpiredPricingPolicyCleanupWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiredPricingPolicyCleanupWorker> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(12); // Chạy định kỳ mỗi 12 giờ

        public ExpiredPricingPolicyCleanupWorker(IServiceProvider serviceProvider, ILogger<ExpiredPricingPolicyCleanupWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExpiredPricingPolicyCleanupWorker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var pricingPolicyService = scope.ServiceProvider.GetRequiredService<IPricingPolicyService>();
                        _logger.LogInformation("ExpiredPricingPolicyCleanupWorker executing pricing policies cleanup.");
                        var count = await pricingPolicyService.CleanupExpiredPricingPoliciesAsync();
                        _logger.LogInformation("ExpiredPricingPolicyCleanupWorker cleanup completed. Expired policies count: {Count}", count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in ExpiredPricingPolicyCleanupWorker.");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("ExpiredPricingPolicyCleanupWorker is stopping.");
        }
    }
}
