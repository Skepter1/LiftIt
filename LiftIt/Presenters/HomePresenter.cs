using LiftIt.Interfaces;
using LiftIt.Models;
using System.Threading.Tasks;

public class HomePresenter
{
    private readonly DatabaseContext _dbContext;
    private readonly StateService _stateService;

    // DatabaseContext wstrzykuje się automatycznie przez konstruktor
    public HomePresenter(DatabaseContext dbContext, StateService stateService)
    {
        _dbContext = dbContext;
        _stateService = stateService;
    }

    public async Task InitAsync(IHomeView view)
    {
        if (_stateService.IsLoggedIn && _stateService.CurrentUser != null)
        {
            // 2. Pobieramy ID dynamicznie z obiektu stanu
            int currentUserId = _stateService.CurrentUser.id;

            // 3. Pobieramy historię z bazy dla TEGO konkretnego ID
            var history = await _dbContext.GetUserTrainingHistoryAsync(currentUserId);
            view.Trainings = history;
        }
        else
        {
            view.Trainings = new System.Collections.Generic.List<TrainingHistoryDto>();
        }
    }
}