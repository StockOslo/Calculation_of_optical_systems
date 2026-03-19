using System;

namespace Calculation_of_optical_systems
{
    public class FirstFileCalculationResult
    {
        // Полная высота матрицы (мм)
        public double h { get; set; }

        // Полная ширина матрицы (мм)
        public double i { get; set; }

        // Диагональ матрицы (мм), считается по теореме Пифагора
        public double d { get; set; }


        // Горизонтальный угол поля зрения (в градусах)
        public double δv { get; set; }

        // Дробная часть горизонтального угла, переведённая в минуты
        public double δv_min { get; set; }

        // Диагональный угол поля зрения (в градусах)
        public double δd { get; set; }

        // Минуты диагонального угла
        public double δd_min { get; set; }

        // Вертикальный угол поля зрения (в градусах)
        public double δh { get; set; }

        // Минуты вертикального угла
        public double δh_min { get; set; }



        public double δv_2 { get; set; }
        public double δv_min_2 { get; set; }
        public double δd_2 { get; set; }
        public double δd_min2 { get; set; }


        // Фокусное расстояние объектива (мм)
        // если мы работаем с углом h
        public double f_result { get; set; }


        // Угловой размер одного пикселя по вертикали (в градусах)
        public double δh_pix_grad { get; set; }

        // Тот же угол, но уже в угловых секундах
        public double δh_pix_angle { get; set; }

        // Угловой размер одного пикселя по горизонтали (в градусах)
        public double δv_pix_grad { get; set; }

        // И тот же параметр в секундах
        public double δv_pix_angle { get; set; }
    }
}