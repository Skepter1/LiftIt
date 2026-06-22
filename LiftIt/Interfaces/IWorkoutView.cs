using System;
using System.Collections.Generic;
using LiftIt.Models;

namespace LiftIt.Interfaces
{
    public interface IWorkoutView
    {
        int PlanId { get; set; }
        int CurrentTrainingId { get; set; }
        string EndNotes { get; set; }
        string FormattedTimerText { get; set; }
        List<ExercisesInPlan> PlanLoadedItems { get; set; }
        Dictionary<int, List<SetRecord>> ExerciseSets { get; set; }

        List<Exercise> AvailableExercises { get; set; }


        void ShowMessage(string message);
        void RefreshUI();

        // NOWE: Zdarzenia cyklu życia i dodawania ćwiczeń
        event Action InitializeDataRequested;
        event Action<int> AddExerciseToSessionRequested;

        event Action StartSessionRequested;
        event Action EndSessionRequested;
        event Action<int, int, decimal, int> AddSetRequested;
        event Action<int> LoadSetsRequested;
    }
}