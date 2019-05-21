using System;
using System.Threading;
using System.Threading.Tasks;
using BankService.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace BankService.HostedServices
{
    public class RequestStatsService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;
        private static readonly Gauge TotalSum = Metrics.CreateGauge("TotalSum", "Total balance of all accounts in the bank");

        public RequestStatsService(ILogger<RequestStatsService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Request Stats services is starting.");

            _timer = new Timer(UpdateRequestStats, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }

        private async void UpdateRequestStats(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var bankingContext = scope.ServiceProvider
                    .GetRequiredService<BankingContext>();
                var sum = await bankingContext.Accounts.SumAsync(account => account.Balance);
                _logger.LogInformation("Total sum of money in the bank {sum}", sum);
                TotalSum.Set(sum);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Request Stats services is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}