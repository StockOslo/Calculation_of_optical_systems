using System;

namespace Calculation_of_optical_systems
{
    public enum CalculationMode
    {
        Base,               // 1. как Excel
        BaseWithAngles,     // 2. те же входы, но через углы
        SolveResolution,    // 3. Δh Δv + углы → Nh Nv
        SolvePixelSize      // 4. Nh Nv + углы → Δh Δv
    }
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

        // Фокусное расстояние объектива (мм)
        public double f { get; set; }

        // Угол поля зрения по горизонтали (если задаётся вручную)
        public double δh { get; set; }

        // Угол поля зрения по вертикали (дополнительно)
        public double δv { get; set; }

        // Угол поля зрения по диагонали
        public double δd { get; set; }

        // Фокусное расстояние, используемое во втором этапе расчёта
        public double f2 { get; set; }

        // Размер пикселя (универсальный параметр если одинаковый)
        public double pixelSize { get; set; }
    }
}