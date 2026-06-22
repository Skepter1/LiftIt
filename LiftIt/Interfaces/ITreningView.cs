using System;
using System.Collections.Generic;
using LiftIt.Models;

namespace LiftIt.Interfaces
{
    public interface ITreningView
    {
        // Lista gotowych szablonów wczytanych z bazy danych
        List<WorkoutPlan> UserPlans { get; set; }

        void ShowMessage(string message);
        void RefreshUI();

        // Żądanie wczytania danych przy otwarciu pulpitu
        event Action InitializeRequested;
    }
}