using job.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace job.Services
{
    public class JobCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobCleanupService> _logger;

        public JobCleanupService(IServiceProvider serviceProvider, ILogger<JobCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Job Cleanup Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<JobPtitContext>();

                        // Find jobs that are Active (Status = 1) but have expired
                        var expiredJobs = await context.Jobs
                            .Where(j => j.Status == 1 && j.ExpiredAt != null && j.ExpiredAt < DateTime.UtcNow)
                            .ToListAsync(stoppingToken);

                        if (expiredJobs.Any())
                        {
                            _logger.LogInformation($"Found {expiredJobs.Count} expired jobs to deactivate.");

                            foreach (var job in expiredJobs)
                            {
                                job.Status = 0; // Mark as inactive/cancelled
                                _logger.LogInformation($"Job {job.Id} ({job.Title}) marked as expired.");
                            }

                            await context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up expired jobs.");
                }

                // Run every 30 minutes
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }

            _logger.LogInformation("Job Cleanup Service is stopping.");
        }
    }
}
