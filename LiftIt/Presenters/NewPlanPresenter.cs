using System;
using System.Collections.Generic;
using System.Linq;
using LiftIt.Interfaces;
using LiftIt.Models;

namespace LiftIt.Presenters
{
    public class NewPlanPresenter
    {
        private readonly INewPlanView _view;
        private readonly DatabaseContext _db;
        private readonly StateService _stateService;

        public NewPlanPresenter(INewPlanView view, DatabaseContext db, StateService stateService)
        {
            _view = view;
            _db = db;
            _stateService = stateService;

            // Wiązanie zdarzeń z widoku do metod prezentera
            _view.InitializeRequested += OnInitialize;
            _view.BodyPartChanged += OnBodyPartChanged;
            _view.AddToPlanRequested += OnAddToPlan;
            _view.RemoveFromPlanRequested += OnRemoveFromPlan;
            _view.MoveUpRequested += OnMoveUp;
            _view.MoveDownRequested += OnMoveDown;
            _view.SavePlanRequested += OnSavePlan;
        }

        private async void OnInitialize()
        {
            // 1. Ładujemy listę części ciała do selecta (to już było)
            _view.BodyPartsList = await _db.GetBodyParts() ?? new List<BodyPart>();
            _view.Exercises = new List<Exercise>();

            // 2. KLUCZOWA ZMIANA: Sprawdzamy, czy użytkownik wszedł w tryb edycji konkretnego planu
            if (_view.PlanId > 0)
            {
                // Pobieramy nagłówek planu (żeby wyciągnąć jego nazwę)
                var istniejącyPlan = await _db.GetWorkoutPlanByIdAsync(_view.PlanId);

                if (istniejącyPlan != null)
                {
                    _view.PlanName = istniejącyPlan.Name; // Wpisujemy nazwę planu do inputa

                    // Pobieramy ćwiczenia przypisane do tego planu z bazy danych
                    var cwiczeniaWPlanie = await _db.GetExercisesInPlanAsync(_view.PlanId);

                    // Mapujemy obiekty ExercisesInPlan z bazy danych na obiekty Exercise i wrzucamy do listy "Mój plan"
                    _view.TrainingPlan = cwiczeniaWPlanie.Select(item => new Exercise
                    {
                        Id = item.ExerciseId,
                        Name = item.ExerciseName,
                        BodyPartName = item.BodyPartName
                    }).ToList();

                    _view.ShowMessage($"Wczytano plan: {istniejącyPlan.Name}");
                }
            }
            else
            {
                // Jeśli PlanId == 0, otwieramy czysty kreator nowego planu
                _view.TrainingPlan = new List<Exercise>();
            }

            _view.RefreshUI();
        }

        private async void OnBodyPartChanged(int bodyPartId)
        {
            if (bodyPartId > 0)
            {
                _view.Exercises = await _db.GetExercisesByBodyPartId(bodyPartId) ?? new List<Exercise>();
            }
            else
            {
                _view.Exercises = new List<Exercise>();
            }
            _view.RefreshUI();
        }

        private void OnAddToPlan(Exercise ex)
        {
            if (!_view.TrainingPlan.Any(x => x.Id == ex.Id))
            {
                _view.TrainingPlan.Add(ex);
                _view.RefreshUI();
            }
        }

        private void OnRemoveFromPlan(Exercise ex)
        {
            _view.TrainingPlan.Remove(ex);
            _view.RefreshUI();
        }

        private void OnMoveUp(Exercise ex)
        {
            var index = _view.TrainingPlan.IndexOf(ex);
            if (index <= 0) return;

            (_view.TrainingPlan[index - 1], _view.TrainingPlan[index]) =
                (_view.TrainingPlan[index], _view.TrainingPlan[index - 1]);

            _view.RefreshUI();
        }

        private void OnMoveDown(Exercise ex)
        {
            var index = _view.TrainingPlan.IndexOf(ex);
            if (index < 0 || index >= _view.TrainingPlan.Count - 1) return;

            (_view.TrainingPlan[index + 1], _view.TrainingPlan[index]) =
                (_view.TrainingPlan[index], _view.TrainingPlan[index + 1]);

            _view.RefreshUI();
        }

        private async void OnSavePlan()
        {
            if (string.IsNullOrWhiteSpace(_view.PlanName))
            {
                _view.ShowMessage("Podaj nazwę planu!");
                return;
            }

            var currentUser = _stateService?.CurrentUser;
            if (currentUser == null)
            {
                _view.ShowMessage("Musisz być zalogowany, aby zapisać plan.");
                return;
            }

            if (!_view.TrainingPlan.Any())
            {
                _view.ShowMessage("Twój plan jest pusty! Dodaj przynajmniej jedno ćwiczenie.");
                return;
            }

            try
            {
                await _db.SaveWorkoutPlan(_view.PlanName, _view.TrainingPlan, currentUser.id);
                _view.ShowMessage("Plan został pomyślnie zapisany w bazie danych!");

                // Reset formularza po sukcesie
                _view.PlanName = "";
                _view.TrainingPlan = new List<Exercise>();
                _view.RefreshUI();
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Błąd zapisu: {ex.Message}");
            }
        }

    }
}