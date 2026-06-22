using System;
using System.Collections.Generic;
using System.Text;

namespace LiftIt.Models
{
    public class SetDto
    {
        public int SetNumber { get; set; } // <-- Upewnij się, że 'S' i 'N' są wielkie!
        public double Weight { get; set; }
        public int Reps { get; set; }
    }
}
