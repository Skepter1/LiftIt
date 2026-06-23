using System;
using System.Collections.Generic;
using LiftIt.Interfaces;
using LiftIt.Models;

namespace LiftIt.Presenters
{
    public class TreningPresenter
    {
        private readonly ITreningView _view;
        private readonly DatabaseContext _db;
        private readonly StateService _stateService;

        public TreningPresenter(ITreningView view, DatabaseContext db, StateService stateService)
        {
            _view = view;
            _db = db;
            _stateService = stateService;

            _view.InitializeRequested += OnInitializeRequested;
        }

        private async void OnInitializeRequested()
        {
            var uid = _stateService?.CurrentUser?.id ?? 0;

            if (uid == 0)
            {
                _view.UserPlans = new List<WorkoutPlan>();
                _view.RefreshUI();
                return;
            }

            _view.UserPlans = await _db.GetWorkoutPlansForUserAsync(uid) ?? new List<WorkoutPlan>();
            _view.RefreshUI();
        }
    }
}