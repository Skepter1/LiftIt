using System;
using System.Collections.Generic;
using System.Text;

namespace LiftIt.Models
{
    public class TrainingHistoryDto
    {
        public int Id { get; set; }
        public DateTime TrainingDate { get; set; }
        public string Notes { get; set; }
        public List<ExerciseSummaryDto> Exercises { get; set; } = new();
    }
}
