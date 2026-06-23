using LiftIt.Interfaces;
using LiftIt.Models;
using System.Threading.Tasks;

public class HomePresenter
{
    private readonly DatabaseContext _dbContext;
    private readonly StateService _stateService;
    public HomePresenter(DatabaseContext dbContext, StateService stateService)
    {
        _dbContext = dbContext;
        _stateService = stateService;
    }

    public async Task InitAsync(IHomeView view)
    {
        if (_stateService.IsLoggedIn && _stateService.CurrentUser != null)
        {
            int currentUserId = _stateService.CurrentUser.id;

            var history = await _dbContext.GetUserTrainingHistoryAsyncTwo(currentUserId);
            view.Trainings = history;
        }
        else
        {
            view.Trainings = new System.Collections.Generic.List<TrainingHistoryDto>();
        }
    }
}