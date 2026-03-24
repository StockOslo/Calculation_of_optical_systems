using Xunit;
using OpticalApi.Background;

namespace TestsAntonov
{
    public class PythonSchedulerTests
    {
        // тест когда сегодня до 23 числа
        [Fact]
        public void get_next_run_should_return_same_month()
        {
            var now = new DateTime(2026, 3, 10);

            var result = PythonScheduler.GetNextRun(now);

            Assert.Equal(new DateTime(2026, 3, 23), result);
        }

        // тест когда сегодня ровно 23
        [Fact]
        public void get_next_run_should_go_next_month_if_23()
        {
            var now = new DateTime(2026, 3, 23);

            var result = PythonScheduler.GetNextRun(now);

            Assert.Equal(new DateTime(2026, 4, 23), result);
        }

        // тест когда позже 23
        [Fact]
        public void get_next_run_should_go_next_month_if_after()
        {
            var now = new DateTime(2026, 3, 30);

            var result = PythonScheduler.GetNextRun(now);

            Assert.Equal(new DateTime(2026, 4, 23), result);
        }

        // тест перехода года
        [Fact]
        public void get_next_run_should_handle_new_year()
        {
            var now = new DateTime(2026, 12, 25);

            var result = PythonScheduler.GetNextRun(now);

            Assert.Equal(new DateTime(2027, 1, 23), result);
        }
    }
}