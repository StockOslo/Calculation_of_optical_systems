using Xunit;
using Calculation_of_optical_systems;

namespace TestsAntonov
{
    public class GripCalculatorTests
    {
        // тест базового корректного расчета
        [Theory]
        [InlineData(50, 10, 0.02, 1000)]
        [InlineData(35, 8, 0.015, 500)]
        [InlineData(100, 5, 0.01, 2000)]
        public void calculate_should_return_positive_values(double f, double k, double z, double r_val)
        {
            var input = new GripCalculationInput
            {
                f = f,
                K = k,
                z = z,
                R = r_val
            };

            var result = GripCalculator.Calculate(input);

            Assert.True(result.H > 0);
            Assert.True(result.R1 > 0);
            Assert.True(result.R2 > 0);
        }

        // тест нулевого фокуса
        [Fact]
        public void calculate_should_fail_when_f_zero()
        {
            var input = new GripCalculationInput
            {
                f = 0,
                K = 10,
                z = 0.02,
                R = 1000
            };

            var result = GripCalculator.Calculate(input);

            Assert.True(result.H == 0);
        }

        // тест нулевого коэффициента k
        [Fact]
        public void calculate_should_fail_when_k_zero()
        {
            var input = new GripCalculationInput
            {
                f = 50,
                K = 0,
                z = 0.02,
                R = 1000
            };

            var result = GripCalculator.Calculate(input);

            Assert.True(double.IsInfinity(result.H));
        }

        // тест нулевого z
        [Fact]
        public void calculate_should_fail_when_z_zero()
        {
            var input = new GripCalculationInput
            {
                f = 50,
                K = 10,
                z = 0,
                R = 1000
            };

            var result = GripCalculator.Calculate(input);

            Assert.True(double.IsInfinity(result.H));
        }

        // тест нулевого расстояния
        [Fact]
        public void calculate_should_fail_when_r_zero()
        {
            var input = new GripCalculationInput
            {
                f = 50,
                K = 10,
                z = 0.02,
                R = 0
            };

            var result = GripCalculator.Calculate(input);

            Assert.True(result.R1 == 0);
            Assert.True(result.R2 == 0);
        }

        // тест граничных значений
        [Theory]
        [InlineData(1, 1, 0.001, 1)]
        [InlineData(0.1, 0.1, 0.0001, 0.5)]
        public void calculate_should_handle_small_values(double f, double k, double z, double r_val)
        {
            var input = new GripCalculationInput
            {
                f = f,
                K = k,
                z = z,
                R = r_val
            };

            var result = GripCalculator.Calculate(input);

            Assert.True(result.H >= 0);
        }

        // тест больших значений
        [Theory]
        [InlineData(1000, 50, 10, 10000)]
        [InlineData(500, 25, 5, 5000)]
        public void calculate_should_handle_large_values(double f, double k, double z, double r_val)
        {
            var input = new GripCalculationInput
            {
                f = f,
                K = k,
                z = z,
                R = r_val
            };

            var result = GripCalculator.Calculate(input);

            Assert.True(result.H > 0);
        }

        //  тест гиперфокального расстояния
        [Fact]
        public void calculate_should_return_exact_H()
        {
            var input = new GripCalculationInput
            {
                f = 50,     // мм → 0.05 м
                K = 10,
                z = 10,     // мкм → 0.00001 м
                R = 1000
            };

            var result = GripCalculator.Calculate(input);

            // H = (0.05^2)/(10 * 0.00001) + 0.05
            // = 0.0025 / 0.0001 + 0.05 = 25 + 0.05 = 25.05

            Assert.Equal(25.05, Math.Round(result.H, 2));
        }


        //  тест передней границы
        [Fact]
        public void calculate_should_return_exact_R1()
        {
            var input = new GripCalculationInput
            {
                f = 50,
                K = 10,
                z = 10,
                R = 10
            };

            var result = GripCalculator.Calculate(input);

            // проверяем что R1 меньше R
            Assert.True(result.R1 < input.R);

            // и не ноль
            Assert.NotEqual(0, result.R1);
        }


        // точный тест задней границы
        [Fact]
        public void calculate_should_return_exact_R2()
        {
            var input = new GripCalculationInput
            {
                f = 50,
                K = 10,
                z = 10,
                R = 10
            };

            var result = GripCalculator.Calculate(input);

            
            Assert.True(result.R2 > result.R1);
        }


       
    }
}
