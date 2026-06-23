using LiftIt.Models;

namespace LiftIt.Presenters
{
    public class RejestracjaPresenter
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
            // 1. Walidacja zgodności haseł
            if (_view.Password != _view.PasswordConfirm)
            {
                _view.ShowSignUpError("Passwords do not match");
                return;
            }

            // 2. Unikalność adresu e-mail
            bool emailZajety = await _dbContext.IsEmailRegisteredAsync(_view.Email);
            if (emailZajety)
            {
                _view.ShowSignUpError("This e-mail address is already registered.");
                return;
            }

            // 3. Budowanie obiektu użytkownika
            var nowyUzytkownik = new Models.Uzytkownik
            {
                login = _view.Login,
                email = _view.Email,
                password = _view.Password
            };

            // 4. Zapis do bazy danych — odbieramy wygenerowane ID (np. 12, 13, 14...)
            int wygenerowaneId = await _dbContext.SignUpUserInMySQL(nowyUzytkownik);

            // Jeśli baza zwróciła poprawne ID (większe od zera)
            if (wygenerowaneId > 0)
            {
                // 🔥 KLUCZOWE: Przypisujemy prawdziwe ID z bazy danych do obiektu w pamięci!
                nowyUzytkownik.id = wygenerowaneId;

                _stateService.IsLoggedIn = true;
                _stateService.CurrentUser = nowyUzytkownik; // Teraz sesja pamięta poprawne ID!
                _view.RedirectHomePage();
            }
            else
            {
                _view.ShowSignUpError("An error occurred during registration. Please try again.");
            }
        }

    }
}
