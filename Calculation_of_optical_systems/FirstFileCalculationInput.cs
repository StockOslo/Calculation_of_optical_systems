using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculation_of_optical_systems
{
    public class FirstFileCalculationInput
    {
        // Количество пикселей по вертикали
        public double Nh { get; set; }

        // Количество пикселей по горизонтали
        public double Nv { get; set; }

        // Размер одного пикселя по вертикали (в микрометрах)
        public double Δh { get; set; }

        // Размер одного пикселя по горизонтали (в микрометрах)
        public double Δv { get; set; }

        // Фокусное расстояние объектива (в мм)
        public double f { get; set; }

        // Дополнительный параметр угла по вертикали
        // (если задаётся вручную или используется во втором этапе расчёта)
        public double δh { get; set; }
    }
}