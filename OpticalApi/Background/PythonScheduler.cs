using OpticalApi.Parsers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpticalApi.Background
{
    public class PythonScheduler : BackgroundService
    {
        private readonly ILogger<PythonScheduler> _logger;

        public PythonScheduler(ILogger<PythonScheduler> logger)
        {
            _logger = logger;
        }
        public static DateTime GetNextRun(DateTime now)
        {
            var nextRun = new DateTime(now.Year, now.Month, 23);

            if (now.Day >= 23)
                nextRun = nextRun.AddMonths(1);

            return nextRun;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("scheduler started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;

                    var nextRun = new DateTime(now.Year, now.Month, 23);

                    if (now.Day >= 23)
                        nextRun = nextRun.AddMonths(1);

                    var delay = nextRun - now;

                    // защита от отрицательного времени
                    if (delay.TotalMilliseconds < 0)
                        delay = TimeSpan.FromMinutes(1);

                    _logger.LogInformation($"next run in {delay}");

                    await Task.Delay(delay, stoppingToken);

                    await RunPython(stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("scheduler stopped");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "error in scheduler loop");

                    // чтобы не уйти в бесконечный краш
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task RunPython(CancellationToken token)
        {
            _logger.LogInformation("starting python update");

            try
            {
                // timeout защита
                var task = Task.Run(async () =>
                {
                    await LensParserPython.UpdateFromPythonAsync();
                    await LensParserPython.UpdateAzimpFromPythonAsync();
                }, token);

                // максимум 5 минут
                var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromMinutes(5), token));

                if (completed != task)
                {
                    _logger.LogWarning("python execution timeout");
                }
                else
                {
                    _logger.LogInformation("python update completed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error during python execution");
            }
        }
    }
}