using Xunit;
using Calculation_of_optical_systems;

namespace TestsAntonov
{
    public class OpticalPageTests
    {
        // тест парсинга с точкой
        [Fact]
        public void parse_should_handle_dot()
        {
            var result = OpticalPage.Parse("10.5");

            Assert.Equal(10.5, result);
        }

        // тест парсинга с запятой
        [Fact]
        public void parse_should_handle_comma()
        {
            var result = OpticalPage.Parse("10,5");

            Assert.Equal(10.5, result);
        }

        // тест пустого значения
        [Fact]
        public void parse_should_return_zero_empty()
        {
            var result = OpticalPage.Parse("");

            Assert.Equal(0, result);
        }

        // тест некорректного ввода
        [Fact]
        public void parse_should_return_zero_invalid()
        {
            var result = OpticalPage.Parse("abc");

            Assert.Equal(0, result);
        }

        // тест label
        [Fact]
        public void get_label_should_return_correct_value()
        {
            var result = OpticalPage.GetLabel("h");

            Assert.Equal("Высота матрицы (h)", result);
        }

        // тест неизвестного label
        [Fact]
        public void get_label_should_return_name_if_unknown()
        {
            var result = OpticalPage.GetLabel("unknown");

            Assert.Equal("unknown", result);
        }

        // тест unit мм
        [Fact]
        public void get_unit_should_return_mm()
        {
            var result = OpticalPage.GetUnit("h");

            Assert.Equal("мм", result);
        }

        // тест unit градусы
        [Fact]
        public void get_unit_should_return_degrees()
        {
            var result = OpticalPage.GetUnit("δh");

            Assert.Equal("°", result);
        }

        // тест unit пиксели
        [Fact]
        public void get_unit_should_return_pixels()
        {
            var result = OpticalPage.GetUnit("Nh");

            Assert.Equal("пикс", result);
        }

        // тест unit микрометры
        [Fact]
        public void get_unit_should_return_mkm()
        {
            var result = OpticalPage.GetUnit("Δh");

            Assert.Equal("мкм", result);
        }

        // тест неизвестной единицы
        [Fact]
        public void get_unit_should_return_empty()
        {
            var result = OpticalPage.GetUnit("random");

            Assert.Equal("", result);
        }
    }
}