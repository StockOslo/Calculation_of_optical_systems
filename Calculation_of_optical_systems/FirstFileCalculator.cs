using System;

namespace Calculation_of_optical_systems
{
    public static class FirstFileCalculator
    {
        public static FirstFileCalculationResult Calculate(
            FirstFileCalculationInput input,
            CalculationMode mode)
        {
            // создаём объект результата
            var r = new FirstFileCalculationResult();

            // просто для удобства выносим фокус
            double f = input.f;

            // копируем входные значения (чтобы они тоже отображались в результате)
            r.Δh = input.Δh;
            r.Δv = input.Δv;
            r.Nh = input.Nh;
            r.Nv = input.Nv;



            // 1. БАЗОВЫЙ РЕЖИМ

            if (mode == CalculationMode.Base)
            {
                // размеры матрицы по горизонтали и вертикали
                r.h = Truncate5(input.Nh * input.Δh / 1000.0);
                r.i = Truncate5(input.Nv * input.Δv / 1000.0);

                // диагональ
                r.d = Truncate5(Math.Sqrt(r.h * r.h + r.i * r.i));

                // углы обзора
                r.δh = Truncate5(AngleDeg(r.i, f));
                r.δv = Truncate5(AngleDeg(r.h, f));
                r.δd = Truncate5(AngleDeg(r.d, f));
            }


            // 2. ЧЕРЕЗ УГЛЫ

            if (mode == CalculationMode.BaseWithAngles)
            {
                // если есть горизонтальный угол — считаем вертикальный размер
                if (input.δh > 0)
                    r.i = SizeFromAngle(input.δh, f);

                // если есть вертикальный угол — считаем горизонтальный размер
                if (input.δv > 0)
                    r.h = SizeFromAngle(input.δv, f);

                // диагональ
                r.d = Truncate5(Math.Sqrt(r.h * r.h + r.i * r.i));

                // углы просто переписываем
                r.δh = Truncate5(input.δh);
                r.δv = Truncate5(input.δv);

                // диагональный угол считаем
                r.δd = Truncate5(AngleDeg(r.d, f));
            }


            // 3. НАЙТИ РАЗРЕШЕНИЕ (Nh, Nv)


            if (mode == CalculationMode.SolveResolution)
            {
                // сначала восстанавливаем размеры
                if (input.δh > 0)
                    r.i = SizeFromAngle(input.δh, f);

                if (input.δv > 0)
                    r.h = SizeFromAngle(input.δv, f);

                r.d = Math.Sqrt(r.h * r.h + r.i * r.i);

                // теперь считаем количество пикселей
                if (input.Δh > 0 && r.h > 0)
                    r.Nh = Truncate5(r.h / (input.Δh / 1000.0));

                if (input.Δv > 0 && r.i > 0)
                    r.Nv = Truncate5(r.i / (input.Δv / 1000.0));

                r.δh = Truncate5(input.δh);
                r.δv = Truncate5(input.δv);
                r.δd = Truncate5(AngleDeg(r.d, f));
            }

            if (mode == CalculationMode.SolvePixelSize)
            {
                // опять же сначала размеры
                if (input.δh > 0)
                    r.i = SizeFromAngle(input.δh, f);

                if (input.δv > 0)
                    r.h = SizeFromAngle(input.δv, f);

                r.d = Math.Sqrt(r.h * r.h + r.i * r.i);

                // теперь считаем размер пикселя
                if (input.Nh > 0 && r.h > 0)
                    r.Δh = Truncate5((r.h * 1000) / input.Nh);

                if (input.Nv > 0 && r.i > 0)
                    r.Δv = Truncate5((r.i * 1000) / input.Nv);

                r.δh = Truncate5(input.δh);
                r.δv = Truncate5(input.δv);
                r.δd = Truncate5(AngleDeg(r.d, f));
            }


            // перевод углов в минуты
            r.δh_min = Truncate5(AngleMinutes(r.δh));
            r.δv_min = Truncate5(AngleMinutes(r.δv));
            r.δd_min = Truncate5(AngleMinutes(r.δd));

            // углы одного пикселя (горизонталь)
            if (r.Δh > 0)
            {
                double ph = r.Δh / 1000.0;
                r.δh_pix_grad = Truncate5(AngleDeg(ph, f));
                r.δh_pix_angle = Truncate5(AngleSeconds(r.δh_pix_grad));
            }

            // углы одного пикселя (вертикаль)
            if (r.Δv > 0)
            {
                double pv = r.Δv / 1000.0;
                r.δv_pix_grad = Truncate5(AngleDeg(pv, f));
                r.δv_pix_angle = Truncate5(AngleSeconds(r.δv_pix_grad));
            }

            // итоговый фокус (просто чтобы был в результате)
            r.f_result = Truncate5(f);

            return r;
        }

        // ФОРМУЛЫ

        // угол по размеру и фокусу (основная формула)
        private static double AngleDeg(double size, double f)
        {
            return 2.0 * Math.Atan(size / (2.0 * f)) * 180.0 / Math.PI;
        }

        // обратная задача: по углу найти размер
        private static double SizeFromAngle(double angle, double f)
        {
            return 2.0 * f * Math.Tan(angle * Math.PI / 180.0 / 2.0);
        }

        // градусы → минуты
        private static double AngleMinutes(double angle)
        {
            return (angle - Math.Truncate(angle)) * 60.0;
        }

        // градусы → секунды
        private static double AngleSeconds(double angle)
        {
            return (angle - Math.Truncate(angle)) * 3600.0;
        }

        // обрезка до 5 знаков (как в Excel, без округления)
        private static double Truncate5(double value)
        {
            return Math.Truncate(value * 100000) / 100000;
        }
    }
}