using System;
using System.Collections.Generic;
using System.Text;

namespace LiftIt.Models
{
    public class ExerciseSummaryDto
    {
        public string ExerciseName { get; set; }
        public List<SetDto> Sets { get; set; } = new();
    }
}
