using System;
using LiftIt.Interfaces;
using LiftIt.Models;

namespace LiftIt.Presenters
{
    public class WorkoutPresenter
    {
        private readonly IWorkoutView _view;
        private readonly DatabaseContext _db;
        private readonly StateService _stateService;

        public WorkoutPresenter(IWorkoutView view, DatabaseContext db, StateService stateService)
        {
            _view = view;
            _db = db;
            _stateService = stateService;

            _view.StartSessionRequested += OnStartSession;
            _view.EndSessionRequested += OnEndSession;
            _view.AddSetRequested += OnAddSet;
            _view.LoadSetsRequested += OnLoadSets;
        }

        private async void OnStartSession()
        {
            int uid = _stateService?.CurrentUser?.id ?? 0;
            if (uid == 0)
            {
                _view.ShowMessage("Please log in.");
                return;
            }

            // 1. Otwieramy sesję treningową w bazie (to już miałeś)
            _view.CurrentTrainingId = await _db.StartTrainingSessionAsync(uid);

            // 2. KLUCZOWA ZMIANA: Sprawdzamy, czy użytkownik wszedł tu z konkretną rutyną
            if (_view.PlanId > 0)
            {
                // Jeśli tak, pobieramy z bazy XAMPP ćwiczenia przypisane do tego planu
                _view.PlanLoadedItems = await _db.GetExercisesInPlanAsync(_view.PlanId);
                _view.ShowMessage("Routine loaded. Let's work!");
            }
            else
            {
                // Pusty trening
                _view.PlanLoadedItems = new System.Collections.Generic.List<ExercisesInPlan>();
                _view.ShowMessage("Empty session started.");
            }

            _view.RefreshUI();
        }

        private async void OnAddSet(int exerciseId, int setNum, decimal weight, int reps)
        {
            if (_view.CurrentTrainingId == 0) return;

            int id = await _db.UpsertSetAsync(_view.CurrentTrainingId, exerciseId, setNum, weight, reps);
            if (id > 0)
            {
                OnLoadSets(exerciseId); // Odśwież listę serii po dodaniu
                _view.ShowMessage("Set recorded.");
            }
            else
            {
                _view.ShowMessage("Error adding set.");
            }
        }

        private async void OnLoadSets(int exerciseId)
        {
            if (_view.CurrentTrainingId == 0) return;

            var allSets = await _db.GetSetsForTrainingAsync(_view.CurrentTrainingId);

            // Filtrujemy tylko to ćwiczenie i sortujemy
            var filtered = new System.Collections.Generic.List<SetRecord>();
            foreach (var s in allSets)
            {
                if (s.ExerciseId == exerciseId) filtered.Add(s);
            }
            filtered.Sort((a, b) => a.SetNumber.CompareTo(b.SetNumber));

            _view.ExerciseSets[exerciseId] = filtered;
            _view.RefreshUI();
        }

        private async void OnEndSession()
        {
            if (_view.CurrentTrainingId == 0) return;

            bool ok = await _db.EndTrainingSessionAsync(_view.CurrentTrainingId, DateTime.Now, _view.EndNotes);
            if (ok)
            {
                _view.ShowMessage("Workout saved successfully!");
                _view.CurrentTrainingId = 0;
                _view.PlanLoadedItems = null;
                _view.EndNotes = "";
                _view.ExerciseSets.Clear();
            }
            else
            {
                _view.ShowMessage("Error saving workout.");
            }
            _view.RefreshUI();
        }
    }
}