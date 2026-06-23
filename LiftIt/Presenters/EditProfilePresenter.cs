
namespace LiftIt.Presenters
{
    internal class EditProfilePresenter
    {
        private readonly Interfaces.IEditProfileView _view;
        private readonly Models.DatabaseContext _dbContext;
        private readonly Models.StateService _stateService;
        public EditProfilePresenter(
            Interfaces.IEditProfileView view,
            Models.DatabaseContext dbContext,
            Models.StateService stateService)
        {
            _view = view;
            _dbContext = dbContext;
            _stateService = stateService;

            _view.ModificationRequested += OnModificationRequested;
        }

        public async void OnModificationRequested()
        {
            _view.SuccesfulModification("");
            int currentUserId = _stateService.CurrentUser.id;
            string nowyLogin = _view.Name;
            string noweHaslo = _view.Password;
            string nowyEmail = _view.Email;

            bool sukces = await _dbContext.ModifyProfileInMySQL(currentUserId, nowyLogin, noweHaslo, nowyEmail);

            if (sukces)
            {
                if (!string.IsNullOrWhiteSpace(nowyLogin)) _stateService.CurrentUser.login = nowyLogin;
                if (!string.IsNullOrWhiteSpace(nowyEmail)) _stateService.CurrentUser.email = nowyEmail;

                _view.SuccesfulModification("The modification was successful");
            } else
            {
                _view.SuccesfulModification("Something went wrong, try again.");
            }
        }
        
    }
}
