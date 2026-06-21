using System;
using System.Linq;
using LiftIt.Interfaces;
using LiftIt.Models;

namespace LiftIt.Presenters
{
    public class RoutinePresenter
    {
        private readonly IRoutineView _view;
        private readonly DatabaseContext _db;
        private readonly StateService _stateService;

        public RoutinePresenter(IRoutineView view, DatabaseContext db, StateService stateService)
        {
            _view = view;
            _db = db;
            _stateService = stateService;

            // Subskrypcja zdarzeń z widoku
            _view.InitializeDataRequested += OnInitializeData;
            _view.CreateDraftRequested += OnCreateDraft;
            _view.SaveRoutineRequested += OnSaveRoutine;
            _view.EditPlanRequested += OnEditPlan;
            _view.DeletePlanRequested += OnDeletePlan;
        }

        private async void OnInitializeData()
        {
            _view.AvailableExercises = await _db.GetAllExercisesAsync() ?? new System.Collections.Generic.List<Exercise>();

            // Inicjalizacja słownika checkboxów
            foreach (var e in _view.AvailableExercises)
            {
                if (!_view.SelectedExercises.ContainsKey(e.Id))
                    _view.SelectedExercises[e.Id] = false;
            }

            await LoadPlansAsync();
            _view.RefreshUI();
        }

        private async System.Threading.Tasks.Task LoadPlansAsync()
        {
            int uid = _stateService?.CurrentUser?.id ?? 0;
            _view.Plans = uid == 0 ? new System.Collections.Generic.List<WorkoutPlan>() : await _db.GetWorkoutPlansForUserAsync(uid);
        }

        private void OnCreateDraft()
        {
            _view.EditingPlanId = 0;
            _view.RoutineName = "";
            foreach (var k in _view.SelectedExercises.Keys.ToList())
                _view.SelectedExercises[k] = false;

            _view.ShowMessage("Draft reset successfully.");
        }

        private async void OnSaveRoutine()
        {
            int uid = _stateService?.CurrentUser?.id ?? 0;
            if (uid == 0)
            {
                _view.ShowMessage("Musisz być zalogowany.");
                return;
            }

            var selectedIds = _view.SelectedExercises.Where(kv => kv.Value).Select(kv => kv.Key).ToList();

            if (_view.EditingPlanId == 0)
            {
                var plan = new WorkoutPlan { UserId = uid, Name = _view.RoutineName, CreationDate = DateTime.Now.Date };
                int planId = await _db.CreateWorkoutPlanAsync(plan);

                if (planId > 0)
                {
                    int order = 1;
                    foreach (var exId in selectedIds) await _db.AddExerciseToPlanAsync(planId, exId, order++);
                    _view.ShowMessage("Routine saved.");
                }
            }
            else
            {
                await _db.UpdateWorkoutPlanAsync(_view.EditingPlanId, uid, _view.RoutineName);
                await _db.UpdateExercisesInPlanAsync(_view.EditingPlanId, selectedIds);
                _view.ShowMessage("Routine updated.");
            }

            await LoadPlansAsync();
            _view.RefreshUI();
        }

        private async void OnEditPlan(int planId)
        {
            _view.EditingPlanId = planId;
            var plan = await _db.GetWorkoutPlanByIdAsync(planId);
            var items = await _db.GetExercisesInPlanAsync(planId);

            if (plan != null)
            {
                _view.RoutineName = plan.Name;
                var ids = items.Select(x => x.ExerciseId).ToHashSet();
                foreach (var k in _view.SelectedExercises.Keys.ToList())
                    _view.SelectedExercises[k] = ids.Contains(k);

                _view.ShowMessage($"Editing: {_view.RoutineName}");
                _view.RefreshUI();
            }
        }

        private async void OnDeletePlan(int planId)
        {
            int uid = _stateService?.CurrentUser?.id ?? 0;
            bool ok = await _db.DeleteWorkoutPlanAsync(planId, uid);
            _view.ShowMessage(ok ? "Routine deleted." : "Failed to delete.");
            await LoadPlansAsync();
            _view.RefreshUI();
        }
    }
}