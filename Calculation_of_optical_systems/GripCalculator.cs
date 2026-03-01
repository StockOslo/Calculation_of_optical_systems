using System;

namespace Calculation_of_optical_systems
{
    public static class GripCalculator
    {
        public static GripCalculationResult Calculate(GripCalculationInput input)
        {
            var r = new GripCalculationResult();

            // Переводы единиц как в Excel
            double f_m = input.f * 1e-3;   // мм → м
            double z_m = input.z * 1e-6;   // мкм → м

            // H (Excel F10)
            // =(f^2)/(K*z) + f

            r.H = (f_m * f_m) / (input.K * z_m) + f_m;


            // R1 (Excel F4)

            r.R1 =
                (input.R * (f_m * f_m)) /
                ((f_m * f_m)
                 - input.K * f_m * z_m
                 + input.K * input.R * z_m);


            // R2 (Excel F7)

            r.R2 =
                (input.R * (f_m * f_m)) /
                ((f_m * f_m)
                 + input.K * f_m * z_m
                 - input.K * input.R * z_m);

            return r;
        }
    }
}