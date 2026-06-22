using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiftIt.Interfaces;
using LiftIt.Models;

namespace LiftIt.Presenters
{
    public class RoutinePresenter
    {
        private readonly IRoutineView _view;
        private readonly DatabaseContext _db;
        private readonly StateService _stateService;
        private int _nextDraftId = 1;

        public RoutinePresenter(IRoutineView view, DatabaseContext db, StateService stateService)
        {
            _view = view;
            _db = db;
            _stateService = stateService;

            _view.InitializeDataRequested += OnInitializeData;
            _view.CreateDraftRequested += OnCreateDraft;
            _view.LoadDraftRequested += OnLoadDraft;
            _view.DeleteDraftRequested += OnDeleteDraft;
            _view.SaveRoutineRequested += OnSaveRoutine;
            _view.EditPlanRequested += OnEditPlan;
            _view.DeletePlanRequested += OnDeletePlan;
        }

        private async void OnInitializeData()
        {
            _view.AvailableExercises = await _db.GetAllExercisesAsync() ?? new List<Exercise>();
            _view.LocalDrafts = new List<RoutineDraft>();

            foreach (var e in _view.AvailableExercises)
            {
                if (!_view.SelectedExercises.ContainsKey(e.Id))
                    _view.SelectedExercises[e.Id] = false;
            }

            await LoadPlansAsync();
            _view.RefreshUI();
        }

        private async Task LoadPlansAsync()
        {
            int uid = _stateService?.CurrentUser?.id ?? 0;
            _view.Plans = uid == 0 ? new List<WorkoutPlan>() : await _db.GetWorkoutPlansForUserAsync(uid);
        }

        private void OnCreateDraft()
        {
            var selectedIds = _view.SelectedExercises.Where(kv => kv.Value).Select(kv => kv.Key).ToList();

            var draft = new RoutineDraft
            {
                // Znajomi użyli RoutineDraftItem z ID. My używamy indeksu listy dla uproszczenia
                Name = _view.RoutineName ?? string.Empty,
                ExerciseIds = selectedIds
            };

            _view.LocalDrafts.Insert(0, draft);

            _view.EditingPlanId = 0;
            _view.RoutineName = "";
            foreach (var k in _view.SelectedExercises.Keys.ToList()) _view.SelectedExercises[k] = false;

            _view.ShowMessage("Draft utworzony lokalnie.");
            _view.RefreshUI();
        }

        private void OnLoadDraft(int index)
        {
            if (index < 0 || index >= _view.LocalDrafts.Count) return;
            var d = _view.LocalDrafts[index];

            _view.EditingPlanId = 0;
            _view.RoutineName = d.Name;
            foreach (var k in _view.SelectedExercises.Keys.ToList())
                _view.SelectedExercises[k] = d.ExerciseIds.Contains(k);

            _view.ShowMessage("Załadowano draft.");
            _view.RefreshUI();
        }

        private void OnDeleteDraft(int index)
        {
            if (index >= 0 && index < _view.LocalDrafts.Count)
            {
                _view.LocalDrafts.RemoveAt(index);
                _view.ShowMessage("Usunięto draft.");
                _view.RefreshUI();
            }
        }

        private async void OnSaveRoutine()
        {
            int uid = _stateService?.CurrentUser?.id ?? 0;
            if (uid == 0)
            {
                _view.ShowMessage("Musisz być zalogowany aby zapisać plan.");
                return;
            }

            var name = _view.RoutineName?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                _view.ShowMessage("Podaj nazwę rutyny.");
                return;
            }

            var selectedIds = _view.SelectedExercises.Where(kv => kv.Value).Select(kv => kv.Key).ToList();

            if (_view.EditingPlanId == 0)
            {
                var plan = new WorkoutPlan { UserId = uid, Name = name, CreationDate = DateTime.Now.Date };
                int planId = await _db.CreateWorkoutPlanAsync(plan);

                if (planId > 0)
                {
                    int order = 1;
                    foreach (var exId in selectedIds) await _db.AddExerciseToPlanAsync(planId, exId, order++);
                    _view.ShowMessage($"Zapisano plan (id={planId}).");
                }
            }
            else
            {
                await _db.UpdateWorkoutPlanAsync(_view.EditingPlanId, uid, name);
                await _db.UpdateExercisesInPlanAsync(_view.EditingPlanId, selectedIds);
                _view.ShowMessage("Zapisano zmiany w planie.");
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

                _view.ShowMessage($"Edytujesz plan: {_view.RoutineName}");
                _view.RefreshUI();
            }
        }

        private async void OnDeletePlan(int planId)
        {
            int uid = _stateService?.CurrentUser?.id ?? 0;
            bool ok = await _db.DeleteWorkoutPlanAsync(planId, uid);
            _view.ShowMessage(ok ? "Usunięto plan." : "Nie udało się usunąć planu.");
            await LoadPlansAsync();
            _view.RefreshUI();
        }
    }
}