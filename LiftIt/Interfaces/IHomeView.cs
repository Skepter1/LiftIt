using System.Collections.Generic;
using LiftIt.Models;

namespace LiftIt.Interfaces
{
    public interface IHomeView
    {
        // Presenter ustawi tę listę, a widok ją po prostu wyświetli
        List<TrainingHistoryDto> Trainings { get; set; }
    }
}