using OpticalApi.Parsers;

namespace OpticalApi.Background
{
    public class PythonScheduler : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                var nextRun = new DateTime(now.Year, now.Month, 23);

                if (now.Day >= 23)
                    nextRun = nextRun.AddMonths(1);

                var delay = nextRun - now;

                await Task.Delay(delay, stoppingToken);

                await RunPython();
            }
        }

        private async Task RunPython()
        {
            Console.WriteLine("🔄 Обновление данных...");

            await LensParserPython.UpdateFromPythonAsync();
            await LensParserPython.UpdateAzimpFromPythonAsync();

            Console.WriteLine("✅ Готово");
        }
    }
}