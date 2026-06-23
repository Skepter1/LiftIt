using System.Collections.Generic;
using LiftIt.Models;

namespace LiftIt.Interfaces
{
    public interface IHomeView
    {
        List<TrainingHistoryDto> Trainings { get; set; }
    }
}