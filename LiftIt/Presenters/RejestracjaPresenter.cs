using LiftIt.Models;

namespace LiftIt.Presenters
{
    internal class RejestracjaPresenter
    {
        private readonly Interfaces.IRejestracjaView _view;
        private readonly Models.DatabaseContext _dbContext;
        private readonly Models.StateService _stateService;

        public RejestracjaPresenter(Interfaces.IRejestracjaView view, Models.DatabaseContext dbContext, Models.StateService stateService)
        {
            _view = view;
            _dbContext = dbContext;
            _stateService = stateService;

            _view.SignUpUser += OnUserSignUpRequested;
        }

        private async void OnUserSignUpRequested()
        {
            if (_view.Password != _view.PasswordConfirm)
            {
                _view.ShowSignUpError("Passwords do not match");
                return;
            }

            var nowyUzytkownik = new Models.Uzytkownik
            {
                login = _view.Login,
                email = _view.Email,
                password = _view.Password
            };

            bool sukces = await _dbContext.SignUpUserInMySQL(nowyUzytkownik);

            if (sukces)
            {
                _stateService.IsLoggedIn = true;
                _stateService.CurrentUser = nowyUzytkownik;
                _view.RedirectHomePage();
            }
        }

    }
}
