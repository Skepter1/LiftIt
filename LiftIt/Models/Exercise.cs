using System;
using System.Collections.Generic;
using System.Text;

namespace LiftIt.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int BodyPartId { get; set; }
        public string BodyPartName { get; set; }
        public int? UserId { get; set; }
    }
}