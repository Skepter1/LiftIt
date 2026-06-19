using System;
using System.Collections.Generic;
using System.Text;

namespace LiftIt.Models
{
    public class WorkoutPlan
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
