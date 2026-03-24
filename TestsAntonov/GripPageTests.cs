using Xunit;
using Calculation_of_optical_systems;

namespace TestsAntonov
{
    public class GripPageTests
    {
        // тест парсинга числа с точкой
        [Fact]
        public void parse_should_handle_dot()
        {
            var result = GripPage.Parse("12.34");

            Assert.Equal(12.34, result);
        }

        // тест парсинга числа с запятой
        [Fact]
        public void parse_should_handle_comma()
        {
            var result = GripPage.Parse("12,34");

            Assert.Equal(12.34, result);
        }

        // тест некорректной строки
        [Fact]
        public void parse_should_return_zero_on_invalid()
        {
            var result = GripPage.Parse("abc");

            Assert.Equal(0, result);
        }

        // тест пустой строки
        [Fact]
        public void parse_should_return_zero_on_empty()
        {
            var result = GripPage.Parse("");

            Assert.Equal(0, result);
        }

        // тест граничного значения
        [Fact]
        public void parse_should_handle_zero()
        {
            var result = GripPage.Parse("0");

            Assert.Equal(0, result);
        }

        // тест корректного расчета
        [Fact]
        public void recalculate_logic_should_work()
        {
            var input = new GripCalculationInput
            {
                R = 1000,
                f = 50,
                z = 0.02,
                K = 10
            };

            var result = GripCalculator.Calculate(input);

            Assert.True(result.H > 0);
            Assert.True(result.R1 > 0);
        }

        // тест случая бесконечности
        [Fact]
        public void recalculate_should_handle_infinity()
        {
            var input = new GripCalculationInput
            {
                R = 1,
                f = 1,
                z = 1000,
                K = 1
            };

            var result = GripCalculator.Calculate(input);

            Assert.True(result.R2 < 0 || double.IsInfinity(result.R2));
        }
    }
}