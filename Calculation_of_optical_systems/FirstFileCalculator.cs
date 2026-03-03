using System;

namespace Calculation_of_optical_systems
{
    public static class FirstFileCalculator
    {
        public static FirstFileCalculationResult Calculate(
            FirstFileCalculationInput input)
        {
            var r = new FirstFileCalculationResult();

            // Геометрия матрицы
            r.h = Truncate5(input.Nh * input.Δh / 1000.0);
            r.i = Truncate5(input.Nv * input.Δv / 1000.0);
            r.d = Truncate5(Math.Sqrt(r.h * r.h + r.i * r.i));

            // Углы поля зрения
            r.δh = Truncate5(AngleDeg(r.i, input.f));
            r.δv = Truncate5(AngleDeg(r.h, input.f));
            r.δd = Truncate5(AngleDeg(r.d, input.f));

            r.δh_min = Truncate5(AngleMinutes(r.δh));
            r.δv_min = Truncate5(AngleMinutes(r.δv));
            r.δd_min = Truncate5(AngleMinutes(r.δd));

            // второй блок
            r.δv_2 = r.δv;
            r.δd_2 = r.δd;

            r.δv_min_2 = r.δv_min;
            r.δd_min2 = r.δd_min;

            // пиксельные углы
            double pixelH = input.Δh / 1000.0;
            double pixelV = input.Δv / 1000.0;

            r.δh_pix_grad = Truncate5(AngleDeg(pixelH, input.f));
            r.δv_pix_grad = Truncate5(AngleDeg(pixelV, input.f));

            r.δh_pix_angle = Truncate5(AngleSeconds(r.δh_pix_grad));
            r.δv_pix_angle = Truncate5(AngleSeconds(r.δv_pix_grad));

            r.f_result = Truncate5(input.f);

            return r;
        }

        // Формула угла поля зрения
        private static double AngleDeg(double size, double focal)
        {
            return 2.0 *
                   Math.Atan(size / (2.0 * focal)) *
                   180.0 / Math.PI;
        }

        // Перевод дробной части градусов в минуты
        private static double AngleMinutes(double angle)
        {
            return (angle - Math.Truncate(angle)) * 60.0;
        }

        // Перевод дробной части градусов в секунды
        private static double AngleSeconds(double angle)
        {
            return (angle - Math.Truncate(angle)) * 3600.0;
        }

        // Обрезка до 5 знаков после запятой БЕЗ округления
        private static double Truncate5(double value)
        {
            return Math.Truncate(value * 100000) / 100000;
        }
    }
}