using System.Collections.Generic;

namespace LiftIt.Interfaces
{
    public interface ITreningView
    {
        event System.Action CreateRoutineRequested;
        event System.Action SaveRoutineRequested;
        string RoutineName { get; }
        List<int> SelectedExerciseIds { get; }
        void ShowMessage(string message);
    }
}