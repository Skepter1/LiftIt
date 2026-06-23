using System.Threading.Tasks;
using LiftIt.Models;

namespace LiftIt.Tests
{
    public class FakeDatabaseContext : DatabaseContext
    {
        public Uzytkownik UserToReturn { get; set; }
        public bool ModificationResult { get; set; }
        public int SignUpIdToReturn { get; set; }
        public bool EmailRegisteredResult { get; set; }
        public List<TrainingHistoryDto> TrainingsToReturn { get; set; } = new();

        public List<WorkoutPlan> WorkoutPlansToReturn { get; set; } = new();
        public List<Exercise> AllExercisesToReturn { get; set; } = new();
        public List<ExercisesInPlan> ExercisesInPlanToReturn { get; set; } = new();
        public int StartedTrainingId { get; set; }
        public List<SetRecord> SetsToReturn { get; set; } = new();

        public override Task<List<WorkoutPlan>> GetWorkoutPlansForUserAsync(int userId)
            => Task.FromResult(WorkoutPlansToReturn);

        public override Task<List<Exercise>> GetAllExercisesAsync()
            => Task.FromResult(AllExercisesToReturn);

        public override Task<List<ExercisesInPlan>> GetExercisesInPlanAsync(int planId)
            => Task.FromResult(ExercisesInPlanToReturn);

        public override Task<int> StartTrainingSessionAsync(int userId)
            => Task.FromResult(StartedTrainingId);

        public override Task<List<SetRecord>> GetSetsForSessionAsync(int trainingId)
            => Task.FromResult(SetsToReturn);
        public override Task<bool> EndTrainingSessionAsync(int trainingId, DateTime? endTime, string notes)
            => Task.FromResult(true);
        public override Task<List<TrainingHistoryDto>> GetUserTrainingHistoryAsyncTwo(int userId)
        {
            return Task.FromResult(TrainingsToReturn);
        }

        public override Task<Uzytkownik> SignInUserInMySQL(string email, string password)
            => Task.FromResult(UserToReturn);

        public override Task<bool> ModifyProfileInMySQL(int id, string login, string password, string email)
            => Task.FromResult(ModificationResult);

        public override Task<int> SignUpUserInMySQL(Uzytkownik user)
            => Task.FromResult(SignUpIdToReturn);

        public override Task<bool> IsEmailRegisteredAsync(string email)
            => Task.FromResult(EmailRegisteredResult);
    }
}