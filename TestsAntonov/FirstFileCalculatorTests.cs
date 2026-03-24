using Xunit;
using Calculation_of_optical_systems;

namespace TestsAntonov
{
    public class FirstFileCalculatorTests
    {
        // тест базового режима расчет размеров
        [Theory]
        [InlineData(1920, 1080, 5, 5, 50)]
        [InlineData(1280, 720, 4, 4, 35)]
        [InlineData(0, 0, 5, 5, 50)]
        public void base_mode_should_calculate_sizes(int nh, int nv, double dh, double dv, double f)
        {
            var input = new FirstFileCalculationInput
            {
                Nh = nh,
                Nv = nv,
                Δh = dh,
                Δv = dv,
                f = f
            };

            var result = FirstFileCalculator.Calculate(input, CalculationMode.Base);

            Assert.True(result.h >= 0);
            Assert.True(result.i >= 0);
            Assert.True(result.d >= 0);
        }

        // тест некорректного фокуса
        [Fact]
        public void base_mode_should_throw_when_focal_zero()
        {
            var input = new FirstFileCalculationInput
            {
                Nh = 1920,
                Nv = 1080,
                Δh = 5,
                Δv = 5,
                f = 0
            };

            Assert.Throws<ArgumentException>(() =>
                FirstFileCalculator.Calculate(input, CalculationMode.Base));
        }

        // тест режима через углы
        [Theory]
        [InlineData(30, 20, 50)]
        [InlineData(60, 40, 35)]
        [InlineData(0, 0, 50)]
        public void base_with_angles_should_calculate(double dh, double dv, double f)
        {
            var input = new FirstFileCalculationInput
            {
                δh = dh,
                δv = dv,
                f = f
            };

            var result = FirstFileCalculator.Calculate(input, CalculationMode.BaseWithAngles);

            Assert.True(result.d >= 0);
        }

        // тест отрицательных углов
        [Fact]
        public void base_with_angles_should_fail_negative_angles()
        {
            var input = new FirstFileCalculationInput
            {
                δh = -10,
                δv = -5,
                f = 50
            };

            var result = FirstFileCalculator.Calculate(input, CalculationMode.BaseWithAngles);

            Assert.True(result.h == 0 && result.i == 0);
        }

        // тест поиска разрешения
        [Theory]
        [InlineData(30, 20, 5, 5, 50)]
        [InlineData(60, 40, 4, 4, 35)]
        [InlineData(10, 10, 0, 0, 50)]
        public void solve_resolution_should_calculate(double dh, double dv, double ph, double pv, double f)
        {
            var input = new FirstFileCalculationInput
            {
                δh = dh,
                δv = dv,
                Δh = ph,
                Δv = pv,
                f = f
            };

            var result = FirstFileCalculator.Calculate(input, CalculationMode.SolveResolution);

            Assert.True(result.Nh >= 0);
            Assert.True(result.Nv >= 0);
        }

        // тест нулевого размера пикселя
        [Fact]
        public void solve_resolution_should_fail_pixel_zero()
        {
            var input = new FirstFileCalculationInput
            {
                δh = 30,
                δv = 20,
                Δh = 0,
                Δv = 0,
                f = 50
            };

            var result = FirstFileCalculator.Calculate(input, CalculationMode.SolveResolution);

            Assert.True(result.Nh == 0 && result.Nv == 0);
        }

        // тест поиска размера пикселя
        [Theory]
        [InlineData(30, 20, 1920, 1080, 50)]
        [InlineData(60, 40, 1280, 720, 35)]
        [InlineData(10, 10, 0, 0, 50)]
        public void solve_pixel_size_should_calculate(double dh, double dv, int nh, int nv, double f)
        {
            var input = new FirstFileCalculationInput
            {
                δh = dh,
                δv = dv,
                Nh = nh,
                Nv = nv,
                f = f
            };

            var result = FirstFileCalculator.Calculate(input, CalculationMode.SolvePixelSize);

            Assert.True(result.Δh >= 0);
            Assert.True(result.Δv >= 0);
        }

        // тест нулевого разрешения
        [Fact]
        public void solve_pixel_size_should_fail_resolution_zero()
        {
            var input = new FirstFileCalculationInput
            {
                δh = 30,
                δv = 20,
                Nh = 0,
                Nv = 0,
                f = 50
            };

            var result = FirstFileCalculator.Calculate(input, CalculationMode.SolvePixelSize);

            Assert.True(result.Δh == 0 && result.Δv == 0);
        }

        // тест расчета углов пикселя
        [Fact]
        public void should_calculate_pixel_angles()
        {
            var input = new FirstFileCalculationInput
            {
                Nh = 1920,
                Nv = 1080,
                Δh = 5,
                Δv = 5,
                f = 50
            };

            var result = FirstFileCalculator.Calculate(input, CalculationMode.Base);

            Assert.True(result.δh_pix_grad >= 0);
            Assert.True(result.δv_pix_grad >= 0);
        }

        // тест возврата фокуса
        [Fact]
        public void should_return_focal()
        {
            var input = new FirstFileCalculationInput
            {
                Nh = 1000,
                Nv = 1000,
                Δh = 5,
                Δv = 5,
                f = 75
            };

            var result = FirstFileCalculator.Calculate(input, CalculationMode.Base);

            Assert.Equal(75, result.f_result);
        }
        //  тест расчета размеров матрицы
        [Fact]
        public void base_mode_should_calculate_exact_matrix_size()
        {
            var input = new FirstFileCalculationInput
            {
                Nh = 1000,
                Nv = 500,
                Δh = 2,
                Δv = 2,
                f = 50
            };

            var result = FirstFileCalculator.Calculate(input, CalculationMode.Base);

            // h = Nv * Δv = 500 * 2 = 1000
            // i = Nh * Δh = 1000 * 2 = 2000
            // d = sqrt(h^2 + i^2) = sqrt(1000^2 + 2000^2) ≈ 2236.07

            Assert.Equal(2, result.h);
            Assert.Equal(1, result.i);
            Assert.Equal(2.23606, result.d);
        }


        //  тест угла (проверка формулы atan)
        [Fact]
        public void base_mode_should_calculate_exact_angle()
        {
            var input = new FirstFileCalculationInput
            {
                Nh = 1000,
                Nv = 1000,
                Δh = 1,
                Δv = 1,
                f = 1
            };

            var result = FirstFileCalculator.Calculate(input, CalculationMode.Base);

            // size = 1000
            // δ ≈ 2 * atan(1000 / 2) ≈ ~180 градусов

            Assert.InRange(result.δh, 53, 53.2);
            Assert.InRange(result.δv, 53, 53.2);
        }


        //  тест разрешения
        [Fact]
        public void solve_resolution_should_calculate_exact_pixels()
        {
            var input = new FirstFileCalculationInput
            {
                δh = 90,
                δv = 90,
                Δh = 1,
                Δv = 1,
                f = 1
            };

            var result = FirstFileCalculator.Calculate(input, CalculationMode.SolveResolution);

            
            Assert.Equal(2000, Math.Round(result.Nh));
            Assert.Equal(2000, Math.Round(result.Nv));
        }


        //  тест размера пикселя
        [Fact]
        public void solve_pixel_size_should_calculate_exact_pixel_size()
        {
            var input = new FirstFileCalculationInput
            {
                δh = 90,
                δv = 90,
                Nh = 2,
                Nv = 2,
                f = 1
            };

            var result = FirstFileCalculator.Calculate(input, CalculationMode.SolvePixelSize);

            // Δ ≈ 1

            Assert.Equal(1000, Math.Round(result.Δh));
            Assert.Equal(1000, Math.Round(result.Δv));
        }
    }
}