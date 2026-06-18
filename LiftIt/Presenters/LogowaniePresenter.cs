using System;
using System.Collections.Generic;
using System.Text;

namespace LiftIt.Presenters
{
    internal class LogowaniePresenter
    {
        private readonly Interfaces.ILogowanieView _view;
        private readonly Models.DatabaseContext _dbContext;
        private readonly Models.StateService _stateService;

        public LogowaniePresenter(
            Interfaces.ILogowanieView view,
            Models.DatabaseContext dbContext,
            Models.StateService stateService)
        {
            _view = view;
            _dbContext = dbContext;
            _stateService = stateService; // <-- ZAPISUJEMY W POLU KLASY

            // Subskrypcja zdarzenia kliknięcia
            _view.SignInUser += OnUserLoginRequested;
            _view.LogoutUser += OnLogoutUser;

        }

        private async void OnUserLoginRequested()
        {
            var znalezionyUzytkownik = await _dbContext.SignInUserInMySQL(_view.Email, _view.Password);

            if (znalezionyUzytkownik != null)
            {
                _stateService.IsLoggedIn = true;
                _stateService.CurrentUser = znalezionyUzytkownik;
                _view.RedirectHomePage();
                // _view.PozytywneZalogowanie();
            }
            else
            {
                // PREZENTER WYDAJE ROZKAZ: Widoku, pokaż błąd z taką treścią!
                _view.ShowSignInError("Niepoprawny e-mail lub hasło. Spróbuj ponownie.");
            }
        }
        private void OnLogoutUser()
        {
            _stateService.IsLoggedIn = false;
            _stateService.CurrentUser = null;
        }
    }
}
