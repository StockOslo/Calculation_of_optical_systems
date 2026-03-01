using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculation_of_optical_systems
{
    public class FirstFileCalculationResult
    {
        public double h { get; set; }
        public double i { get; set; }
        public double d { get; set; }
        public double δv { get; set; }
        public double δv_min { get; set; }
        public double δd { get; set; }
        public double δd_min { get; set; }
        public double δh { get; set; }
        public double δh_min { get; set; }
        public double δv_2 { get; set; }
        public double δv_min_2 { get; set; }
        public double δd_2 { get; set; }
        public double δd_min2 { get; set; }
        public double f_result { get; set; }

        public double δh_pix_grad { get; set; }
        public double δh_pix_angle { get; set; }
        public double δv_pix_grad { get; set; }
        public double δv_pix_angle { get; set; }
    }
}
