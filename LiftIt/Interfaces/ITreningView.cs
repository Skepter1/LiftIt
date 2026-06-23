using System;
using System.Collections.Generic;
using LiftIt.Models;

namespace LiftIt.Interfaces
{
    public interface ITreningView
    {
        List<WorkoutPlan> UserPlans { get; set; }

        void ShowMessage(string message);
        void RefreshUI();

        event Action InitializeRequested;
    }
}