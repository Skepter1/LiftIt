using System;
using System.Collections.Generic;
using LiftIt.Models;

namespace LiftIt.Interfaces
{
    public interface INewPlanView
    {
        // Stan i dane przekazywane do wyświetlenia
        int PlanId { get; set; }
        string PlanName { get; set; }
        List<BodyPart> BodyPartsList { get; set; }
        List<Exercise> Exercises { get; set; }
        List<Exercise> TrainingPlan { get; set; }

        // Metody sterujące interfejsem
        void ShowMessage(string message);
        void RefreshUI();

        // Zdarzenia wywoływane przez interakcję użytkownika
        event Action InitializeRequested;
        event Action<int> BodyPartChanged;
        event Action<Exercise> AddToPlanRequested;
        event Action<Exercise> RemoveFromPlanRequested;
        event Action<Exercise> MoveUpRequested;
        event Action<Exercise> MoveDownRequested;
        event Action SavePlanRequested;
    }
}