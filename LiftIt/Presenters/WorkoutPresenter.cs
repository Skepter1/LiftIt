using System;
using System.Linq;
using System.Threading.Tasks;
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
                _view.ShowMessage("Zaloguj się aby uruchomić sesję.");
                return;
            }

            // Znajomi świetnie połączyli odpalanie sesji z planem w jednej metodzie DB
            if (_view.PlanId > 0)
            {
                var res = await _db.RunPlanAsSessionAsync(_view.PlanId); // Metoda znajomych
                _view.CurrentTrainingId = res.trainingId;
                _view.PlanLoadedItems = res.planItems;
                _view.ShowMessage(_view.CurrentTrainingId > 0 ? "Uruchomiono plan jako sesję." : "Błąd uruchomienia planu.");
            }
            else
            {
                _view.CurrentTrainingId = await _db.StartTrainingSessionAsync(uid);
                _view.PlanLoadedItems = new System.Collections.Generic.List<ExercisesInPlan>();
                _view.ShowMessage(_view.CurrentTrainingId > 0 ? "Sesja rozpoczęta." : "Błąd uruchomienia.");
            }

            _view.ExerciseSets.Clear();
            _view.RefreshUI();
        }

        private async void OnAddSet(int exerciseId, int setNum, decimal weight, int reps)
        {
            if (_view.CurrentTrainingId == 0) return;

            // Używamy UpsertSetAsync tak jak zrobili znajomi
            int id = await _db.UpsertSetAsync(_view.CurrentTrainingId, exerciseId, setNum, weight, reps);
            if (id > 0)
            {
                OnLoadSets(exerciseId);
                _view.ShowMessage("Dodano serię.");
            }
            else
            {
                _view.ShowMessage("Błąd dodawania.");
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
                _view.ShowMessage("Sesja zakończona.");
                _view.CurrentTrainingId = 0;
                _view.PlanLoadedItems = null;
                _view.EndNotes = "";
                _view.ExerciseSets.Clear();
            }
            else
            {
                _view.ShowMessage("Błąd zakończenia sesji.");
            }
            _view.RefreshUI();
        }
    }
}