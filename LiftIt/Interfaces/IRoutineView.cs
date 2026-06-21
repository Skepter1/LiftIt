using System;
using System.Collections.Generic;
using LiftIt.Models;

namespace LiftIt.Interfaces
{
    public interface IRoutineView
    {
        // Właściwości (Stan UI)
        string RoutineName { get; set; }
        int EditingPlanId { get; set; }
        Dictionary<int, bool> SelectedExercises { get; set; }
        List<Exercise> AvailableExercises { get; set; }
        List<WorkoutPlan> Plans { get; set; }

        // Metody sterujące widokiem
        void ShowMessage(string message);
        void RefreshUI();

        // Zdarzenia (Akcje użytkownika)
        event Action InitializeDataRequested;
        event Action CreateDraftRequested;
        event Action SaveRoutineRequested;
        event Action<int> EditPlanRequested;
        event Action<int> DeletePlanRequested;
    }
}