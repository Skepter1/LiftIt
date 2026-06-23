using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using LiftIt.Interfaces;
using LiftIt.Models;

namespace LiftIt.Presenters
{
    public class WorkoutPresenter : IDisposable
    {
        private readonly IWorkoutView _view;
        private readonly DatabaseContext _db;
        private readonly StateService _stateService;

        private Timer _workoutTimer;
        private int _elapsedSeconds = 0;

        public WorkoutPresenter(IWorkoutView view, DatabaseContext db, StateService stateService)
        {
            _view = view;
            _db = db;
            _stateService = stateService;

            _view.InitializeDataRequested += OnInitializeData;
            _view.AddExerciseToSessionRequested += OnAddExerciseToSession;
            _view.StartSessionRequested += OnStartSession;
            _view.EndSessionRequested += OnEndSession;
            _view.AddSetRequested += OnAddSet;
            _view.LoadSetsRequested += OnLoadSets;
        }

        private async void OnInitializeData()
        {
            _view.AvailableExercises = await _db.GetAllExercisesAsync() ?? new List<Exercise>();
            _view.FormattedTimerText = "00:00:00";

            if (_view.PlanId > 0)
            {
                _view.PlanLoadedItems = await _db.GetExercisesInPlanAsync(_view.PlanId) ?? new List<ExercisesInPlan>();
            }
            else
            {
                _view.PlanLoadedItems = new List<ExercisesInPlan>();
            }

            _view.RefreshUI();
        }

        private void OnAddExerciseToSession(int exerciseId)
        {
            if (_view.PlanLoadedItems == null) return;
            if (_view.PlanLoadedItems.Any(e => e.ExerciseId == exerciseId))
            {
                _view.ShowMessage("This exercise is already on your training list.");
                return;
            }

            var ex = _view.AvailableExercises?.FirstOrDefault(e => e.Id == exerciseId);
            if (ex != null)
            {
                _view.PlanLoadedItems.Add(new ExercisesInPlan
                {
                    ExerciseId = ex.Id,
                    ExerciseName = ex.Name,
                    BodyPartName = ex.BodyPartName,
                    PlanId = 0,
                    Order = _view.PlanLoadedItems.Count + 1
                });
                _view.ShowMessage($"Added {ex.Name} to current session.");
                _view.RefreshUI();
            }
        }

        private async void OnStartSession()
        {
            int uid = _stateService?.CurrentUser?.id ?? 0;
            if (uid == 0)
            {
                _view.ShowMessage("Log in to start your session.");
                return;
            }

            if (_view.PlanId > 0)
            {
                var res = await _db.RunPlanAsSessionAsync(_view.PlanId);
                _view.CurrentTrainingId = res.trainingId;
                _view.PlanLoadedItems = res.planItems;
                _view.ShowMessage(_view.CurrentTrainingId > 0 ? "Workout plan launched as a session." : "Workout plan launch error.");
            }
            else
            {
                _view.CurrentTrainingId = await _db.StartTrainingSessionAsync(uid);
                _view.PlanLoadedItems = new List<ExercisesInPlan>();
                _view.ShowMessage(_view.CurrentTrainingId > 0 ? "Empty session started." : "Startup error.");
            }

            _view.ExerciseSets.Clear();

            if (_view.CurrentTrainingId > 0)
            {
                StartTimer();
            }

            _view.RefreshUI();
        }

        private async void OnAddSet(int exerciseId, int setNum, decimal weight, int reps)
        {
            if (_view.CurrentTrainingId == 0) return;

            int id = await _db.UpsertSetAsync(_view.CurrentTrainingId, exerciseId, setNum, weight, reps);
            if (id > 0)
            {
                OnLoadSets(exerciseId);
                _view.ShowMessage("Set has been added.");
            }
            else
            {
                _view.ShowMessage("Addition error.");
            }
        }

        private async void OnLoadSets(int exerciseId)
        {
            if (_view.CurrentTrainingId == 0) return;
            var allSets = await _db.GetSetsForSessionAsync(_view.CurrentTrainingId);
            var filtered = allSets.Where(s => s.ExerciseId == exerciseId).OrderBy(s => s.SetNumber).ToList();
            _view.ExerciseSets[exerciseId] = filtered;
            _view.RefreshUI();
        }

        private async void OnEndSession()
        {
            if (_view.CurrentTrainingId == 0) return;

            bool ok = await _db.EndTrainingSessionAsync(_view.CurrentTrainingId, DateTime.Now, _view.EndNotes);
            if (ok)
            {
                _view.ShowMessage("Session completed and saved.");
                _view.CurrentTrainingId = 0;
                _view.PlanLoadedItems = null;
                _view.EndNotes = "";
                _view.ExerciseSets.Clear();

                StopTimer();
            }
            else
            {
                _view.ShowMessage("Session termination error.");
            }
            _view.RefreshUI();
        }

        private void StartTimer()
        {
            StopTimer();
            _elapsedSeconds = 0;
            _workoutTimer = new Timer(TimerCallback, null, 1000, 1000);
        }

        private void StopTimer()
        {
            _workoutTimer?.Dispose();
            _workoutTimer = null;
            _view.FormattedTimerText = "00:00:00";
        }

        private void TimerCallback(object state)
        {
            _elapsedSeconds++;

            TimeSpan time = TimeSpan.FromSeconds(_elapsedSeconds);
            _view.FormattedTimerText = time.ToString(@"hh\:mm\:ss");

            _view.RefreshUI();
        }

        public void Dispose()
        {
            StopTimer();
        }
    }
}