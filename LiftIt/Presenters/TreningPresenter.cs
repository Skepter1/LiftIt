using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiftIt.Models;
using LiftIt.Interfaces;

namespace LiftIt.Presenters
{
    public class TreningPresenter
    {
        //private readonly ITreningView _view;
        //private readonly DatabaseContext _db;
        //private readonly StateService _stateService;

        //public List<Exercise> AvailableExercises { get; private set; } = new();
        //public List<WorkoutPlan> UserPlans { get; private set; } = new();

        //public TreningPresenter(ITreningView view, DatabaseContext db, StateService stateService)
        //{
        //    _view = view;
        //    _db = db;
        //    _stateService = stateService;

        //    _view.CreateRoutineRequested += OnCreateRoutineRequested;
        //    _view.SaveRoutineRequested += OnSaveRoutineRequested;

        //    _ = InitializeAsync();
        //}

        //private async Task InitializeAsync()
        //{
        //    await LoadExercisesAsync();
        //    await LoadUserPlansAsync();
        //}

        //private async Task LoadExercisesAsync()
        //{
        //    AvailableExercises = await _db.GetAllExercisesAsync();
        //}

        //public async Task<List<WorkoutPlan>> LoadUserPlansAsync()
        //{
        //    var uid = _stateService?.CurrentUser?.id ?? 0;
        //    if (uid == 0) { UserPlans = new List<WorkoutPlan>(); return UserPlans; }
        //    UserPlans = await _db.GetWorkoutPlansForUserAsync(uid);
        //    return UserPlans;
        //}

        //public async Task<(WorkoutPlan plan, List<ExercisesInPlan> items)> GetPlanDetailsAsync(int planId)
        //{
        //    var plan = await _db.GetWorkoutPlanByIdAsync(planId);
        //    var items = await _db.GetExercisesInPlanAsync(planId);
        //    return (plan, items);
        //}

        //public async Task<bool> UpdatePlanNameAsync(int planId, string newName)
        //{
        //    var uid = _stateService?.CurrentUser?.id ?? 0;
        //    if (uid == 0) return false;
        //    return await _db.UpdateWorkoutPlanAsync(planId, uid, newName);
        //}

        //public async Task<bool> DeletePlanAsync(int planId)
        //{
        //    var uid = _stateService?.CurrentUser?.id ?? 0;
        //    if (uid == 0) return false;
        //    return await _db.DeleteWorkoutPlanAsync(planId, uid);
        //}

        //public async Task<bool> UpdatePlanExercisesAsync(int planId, List<int> exerciseIds)
        //{
        //    await _db.UpdateExercisesInPlanAsync(planId, exerciseIds);
        //    return true;
        //}

        //// event handlers wired to view
        //private void OnCreateRoutineRequested()
        //{
        //    // view will prepare draft; nothing DB-related here
        //    _view.ShowMessage("Draft rutyny utworzony lokalnie.");
        //}

        //private async void OnSaveRoutineRequested()
        //{
        //    // Create new plan from view.RoutineName + SelectedExerciseIds
        //    var currentUser = _stateService?.CurrentUser;
        //    if (currentUser == null)
        //    {
        //        _view.ShowMessage("Musisz być zalogowany aby zapisać plan.");
        //        return;
        //    }

        //    var name = _view.RoutineName?.Trim();
        //    if (string.IsNullOrWhiteSpace(name))
        //    {
        //        _view.ShowMessage("Podaj nazwę rutyny.");
        //        return;
        //    }

        //    var plan = new WorkoutPlan
        //    {
        //        UserId = currentUser.id,
        //        Name = name,
        //        CreationDate = DateTime.Now.Date
        //    };

        //    int planId = await _db.CreateWorkoutPlanAsync(plan);
        //    if (planId == 0)
        //    {
        //        _view.ShowMessage("Błąd zapisu planu do bazy.");
        //        return;
        //    }

        //    int order = 1;
        //    foreach (var exId in _view.SelectedExerciseIds.Distinct())
        //    {
        //        await _db.AddExerciseToPlanAsync(planId, exId, order++);
        //    }

        //    await LoadUserPlansAsync();
        //    _view.ShowMessage($"Zapisano plan (id={planId}).");
        //}

        //// Sesja treningowa (live)
        //public async Task<int> StartSessionAsync()
        //{
        //    var uid = _stateService?.CurrentUser?.id ?? 0;
        //    if (uid == 0) return 0;
        //    return await _db.StartTrainingSessionAsync(uid);
        //}

        //public async Task<bool> EndSessionAsync(int trainingId, string notes)
        //{
        //    return await _db.EndTrainingSessionAsync(trainingId, DateTime.Now, notes);
        //}

        //public async Task<int> AddSetAsync(int trainingId, int exerciseId, int setNumber, decimal weight, int reps)
        //{
        //    // teraz wykonujemy upsert: nadpisanie istniejącej serii o tym samym numerze
        //    return await _db.UpsertSetAsync(trainingId, exerciseId, setNumber, weight, reps);
        //}

        //public async Task<List<SetRecord>> GetSetsForSessionAsync(int trainingId)
        //{
        //    return await _db.GetSetsForTrainingAsync(trainingId);
        //}

        //public async Task<(int trainingId, List<ExercisesInPlan> planItems)> RunPlanAsSessionAsync(int planId)
        //{
        //    var trainingId = await StartSessionAsync();
        //    var items = await _db.GetExercisesInPlanAsync(planId);
        //    return (trainingId, items);
        //}
    }
}