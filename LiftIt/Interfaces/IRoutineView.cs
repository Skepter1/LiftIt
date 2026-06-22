using System;
using System.Collections.Generic;
using LiftIt.Models;

namespace LiftIt.Interfaces
{
    public interface IRoutineView
    {
        string RoutineName { get; set; }
        int EditingPlanId { get; set; }
        Dictionary<int, bool> SelectedExercises { get; set; }

        List<Exercise> AvailableExercises { get; set; }
        List<WorkoutPlan> Plans { get; set; }

        List<RoutineDraft> LocalDrafts { get; set; }

        void ShowMessage(string message);
        void RefreshUI();

        event Action InitializeDataRequested;
        event Action CreateDraftRequested;
        event Action<int> LoadDraftRequested;
        event Action<int> DeleteDraftRequested;
        event Action SaveRoutineRequested;
        event Action<int> EditPlanRequested;
        event Action<int> DeletePlanRequested;
    }
}