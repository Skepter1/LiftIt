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
            _view.BodyPartsList = await _db.GetBodyParts() ?? new List<BodyPart>();
            _view.Exercises = new List<Exercise>();

            if (_view.PlanId > 0)
            {
                var istniejącyPlan = await _db.GetWorkoutPlanByIdAsync(_view.PlanId);

                if (istniejącyPlan != null)
                {
                    _view.PlanName = istniejącyPlan.Name;

                    var cwiczeniaWPlanie = await _db.GetExercisesInPlanAsync(_view.PlanId);

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
                _view.TrainingPlan = new List<Exercise>();
            }

            _view.RefreshUI();
        }

        private async void OnBodyPartChanged(int bodyPartId)
        {
            _view.Exercises = bodyPartId > 0
                ? await _db.GetExercisesByBodyPartId(bodyPartId) ?? new List<Exercise>()
                : new List<Exercise>();

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
                _view.ShowMessage("Enter the name of the plan!");
                return;
            }

            var currentUser = _stateService?.CurrentUser;
            if (currentUser == null)
            {
                _view.ShowMessage("You must be logged in to save your plan.");
                return;
            }

            if (!_view.TrainingPlan.Any())
            {
                _view.ShowMessage("Your plan is empty! Add at least one exercise.");
                return;
            }

            try
            {
                await _db.SaveWorkoutPlan(_view.PlanName, _view.TrainingPlan, currentUser.id);
                _view.ShowMessage("The plan was successfully saved in the database!");

                _view.PlanName = "";
                _view.TrainingPlan = new List<Exercise>();
                _view.RefreshUI();
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Saving error: {ex.Message}");
            }
        }

    }
}