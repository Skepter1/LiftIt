using System.Collections.Generic;

namespace LiftIt.Models
{
    public class RoutineDraft
    {
        public string Name { get; set; } = "";
        public List<int> ExerciseIds { get; set; } = new();
    }
}