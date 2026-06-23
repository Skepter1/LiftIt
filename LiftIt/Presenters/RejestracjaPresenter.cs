using LiftIt.Models;
using LiftIt.Interfaces;

namespace LiftIt.Presenters
{
    internal class RejestracjaPresenter
    {
        private readonly IRejestracjaView _view;
        private readonly DatabaseContext _dbContext;
        private readonly StateService _stateService;

        public RejestracjaPresenter(IRejestracjaView view, DatabaseContext dbContext, StateService stateService)
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

            bool emailZajety = await _dbContext.IsEmailRegisteredAsync(_view.Email);
            if (emailZajety)
            {
                _view.ShowSignUpError("This e-mail address is already registered.");
                return;
            }

            var nowyUzytkownik = new Uzytkownik
            {
                login = _view.Login,
                email = _view.Email,
                password = _view.Password
            };

            int wygenerowaneId = await _dbContext.SignUpUserInMySQL(nowyUzytkownik);

            if (wygenerowaneId > 0)
            {
                nowyUzytkownik.id = wygenerowaneId;
                _stateService.IsLoggedIn = true;
                _stateService.CurrentUser = nowyUzytkownik;
                _view.RedirectHomePage();
            }
            else
            {
                _view.ShowSignUpError("An error occurred during registration. Please try again.");
            }
        }
    }
}