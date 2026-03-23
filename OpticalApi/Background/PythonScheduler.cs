using OpticalApi.Parsers;

namespace OpticalApi.Background
{
    public class PythonScheduler : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // бесконечный цикл пока сервис работает
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now; // текущее время

                // ставим запуск на 23 число текущего месяца
                var nextRun = new DateTime(now.Year, now.Month, 23);

                // если уже позже 23 — переносим на следующий месяц
                if (now.Day >= 23)
                    nextRun = nextRun.AddMonths(1);

                var delay = nextRun - now; // сколько ждать до запуска

                // ждем нужное время
                await Task.Delay(delay, stoppingToken);

                // запускаем парсеры
                await RunPython();
            }
        }

        private async Task RunPython()
        {
            Console.WriteLine("обновление данных"); // просто лог в консоль

            // обновляем данные из python парсеров
            await LensParserPython.UpdateFromPythonAsync();
            await LensParserPython.UpdateAzimpFromPythonAsync();

            Console.WriteLine("готово"); // сигнал что всё завершилось
        }
    }
}