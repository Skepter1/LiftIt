using System;
using System.Collections.Generic;
using LiftIt.Models;

namespace LiftIt.Interfaces
{
    public interface IWorkoutView
    {
        int PlanId { get; set; } // Z adresu URL
        int CurrentTrainingId { get; set; }
        string EndNotes { get; set; }
        List<ExercisesInPlan> PlanLoadedItems { get; set; }
        Dictionary<int, List<SetRecord>> ExerciseSets { get; set; }

        void ShowMessage(string message);
        void RefreshUI();

        event Action StartSessionRequested;
        event Action EndSessionRequested;
        event Action<int, int, decimal, int> AddSetRequested;
        event Action<int> LoadSetsRequested;
    }
}