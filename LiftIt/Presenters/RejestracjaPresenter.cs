namespace LiftIt.Presenters
{
    internal class RejestracjaPresenter
    {
        private readonly Interfaces.IRejestracjaView _view;
        private readonly Models.DatabaseContext _dbContext;

        public RejestracjaPresenter(Interfaces.IRejestracjaView view, Models.DatabaseContext dbContext)
        {
            _view = view;
            _dbContext = dbContext;

            _view.SignUpUser += OnUserSignUpRequested;
        }

        private async void OnUserSignUpRequested()
        {
            if (_view.Password != _view.PasswordConfirm)
            {
                // Tutaj warto byłoby mieć w interfejsie metodę np. _view.ShowError("Hasła się nie zgadzają");
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
                // Poinformuj widok o sukcesie, np. przekieruj do logowania
            }
        }

    }
}
