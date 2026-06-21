using System;
using System.Collections.Generic;
using System.Text;

namespace LiftIt.Models
{
    public class ExercisesInPlan
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public int ExerciseId { get; set; }
        public int Order { get; set; }
        public string ExerciseName { get; set; }
        public string BodyPartName { get; set; }
    }
}
