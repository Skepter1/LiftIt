using System;

namespace LiftIt.Models
{
    public class SetRecord
    {
        public int Id { get; set; }
        public int TrainingId { get; set; }
        public int ExerciseId { get; set; }
        public int SetNumber { get; set; }
        public decimal Weight { get; set; }
        public int Reps { get; set; }
    }
}
