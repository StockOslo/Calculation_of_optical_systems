using System;

namespace Calculation_of_optical_systems
{
    public static class FirstFileCalculator
    {
        public static FirstFileCalculationResult Calculate(
            FirstFileCalculationInput input)
        {
            var r = new FirstFileCalculationResult();

            // --- Геометрия матрицы ---
            r.h = input.Nh * input.Δh / 1000.0;
            r.i = input.Nv * input.Δv / 1000.0;
            r.d = Math.Sqrt(r.h * r.h + r.i * r.i);

            // --- Углы поля зрения ---
            r.δh = AngleDeg(r.i, input.f);
            r.δv = AngleDeg(r.h, input.f);
            r.δd = AngleDeg(r.d, input.f);

            // минуты (Excel: ОСТАТ(x;1)*60)
            r.δh_min = AngleMinutes(r.δh);
            r.δv_min = AngleMinutes(r.δv);
            r.δd_min = AngleMinutes(r.δd);

            // второй блок
            r.δv_2 = r.δv;
            r.δd_2 = r.δd;

            r.δv_min_2 = r.δv_min;
            r.δd_min2 = r.δd_min;

            // --- пиксельные углы ---
            double pixelH = input.Δh / 1000.0;
            double pixelV = input.Δv / 1000.0;

            r.δh_pix_grad = AngleDeg(pixelH, input.f);
            r.δv_pix_grad = AngleDeg(pixelV, input.f);

            // Excel: ОСТАТ(x;1)*3600
            r.δh_pix_angle = AngleSeconds(r.δh_pix_grad);
            r.δv_pix_angle = AngleSeconds(r.δv_pix_grad);

            r.f_result = input.f;

            return r;
        }


        // Excel ATAN формула

        private static double AngleDeg(double size, double focal)
        {
            return 2.0 *
                   Math.Atan(size / (2.0 * focal)) *
                   180.0 / Math.PI;
        }

        // ОСТАТ(x;1)*60
        private static double AngleMinutes(double angle)
        {
            return (angle - Math.Truncate(angle)) * 60.0;
        }

        // ОСТАТ(x;1)*3600
        private static double AngleSeconds(double angle)
        {
            return (angle - Math.Truncate(angle)) * 3600.0;
        }
    }
}