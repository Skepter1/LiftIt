using LiftIt.Models;
using Xunit;

namespace LiftIt.Tests
{
    public class StateServiceTests
    {
        [Fact]
        public void StateService_InitializesWithCorrectDefaults()
        {
            var stateService = new StateService();
            
            Assert.False(stateService.IsLoggedIn);
            Assert.Null(stateService.CurrentUser);
            Assert.True(stateService.DarkTheme);
            Assert.NotNull(stateService.CurrentRoutineDraft);
        }

        [Fact]
        public void StateService_SetCurrentUser_UpdatesState()
        {
            var stateService = new StateService();
            var user = new Uzytkownik { id = 1, login = "test", email = "test@test.com" };

            stateService.CurrentUser = user;
            stateService.IsLoggedIn = true;

            Assert.True(stateService.IsLoggedIn);
            Assert.Equal("test", stateService.CurrentUser.login);
            Assert.Equal("test@test.com", stateService.CurrentUser.email);
            Assert.Equal(1, stateService.CurrentUser.id);
        }

        [Fact]
        public void StateService_Logout_ClearsUserState()
        {
            // Arrange (Przygotowanie początkowego stanu)
            var stateService = new StateService
            {
                CurrentUser = new Uzytkownik { id = 99, login = "active_user", email = "user@test.pl" },
                IsLoggedIn = true
            };

            // Act (Wykonanie akcji - wylogowanie)
            stateService.CurrentUser = null;
            stateService.IsLoggedIn = false;

            // Assert (Sprawdzenie wyników)
            Assert.False(stateService.IsLoggedIn);
            Assert.Null(stateService.CurrentUser);
        }

        [Fact]
        public void StateService_UpdateRoutineDraft_ModifiesExerciseList()
        {
            // Arrange
            var stateService = new StateService();

            // Act - modyfikowanie zagnieżdżonej listy ćwiczeń
            stateService.CurrentRoutineDraft.Name = "Push Day";
            stateService.CurrentRoutineDraft.ExerciseIds.Add(101);
            stateService.CurrentRoutineDraft.ExerciseIds.Add(205);

            // Assert
            Assert.Equal("Push Day", stateService.CurrentRoutineDraft.Name);
            Assert.Equal(2, stateService.CurrentRoutineDraft.ExerciseIds.Count);
            Assert.Contains(101, stateService.CurrentRoutineDraft.ExerciseIds);
            Assert.Contains(205, stateService.CurrentRoutineDraft.ExerciseIds);
        }

        [Fact]
        public void StateService_ResetRoutineDraft_CreatesNewEmptyInstance()
        {
            // Arrange - symulowanie istniejącego szkicu treningu
            var stateService = new StateService();
            stateService.CurrentRoutineDraft.Name = "Leg Day";
            stateService.CurrentRoutineDraft.ExerciseIds.AddRange(new[] { 1, 2, 3 });

            // Act - resetowanie szkicu po np. zapisaniu treningu
            stateService.CurrentRoutineDraft = new RoutineDraft();

            // Assert
            Assert.Empty(stateService.CurrentRoutineDraft.Name);
            Assert.Empty(stateService.CurrentRoutineDraft.ExerciseIds);
        }

        [Fact]
        public void StateService_ToggleTheme_ChangesThemeState()
        {
            // Arrange
            var stateService = new StateService();
            bool initialTheme = stateService.DarkTheme; // Powinno być true wg. definicji

            // Act
            stateService.DarkTheme = !stateService.DarkTheme;

            // Assert
            Assert.NotEqual(initialTheme, stateService.DarkTheme);
            Assert.False(stateService.DarkTheme); // Skoro domyślnie było true, teraz musi być false
        }
    }
}
