using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiftIt.Presenters;
using LiftIt.Interfaces;
using LiftIt.Models;

namespace LiftIt.Tests
{
    public class TreningPresenterTests
    {
        [Fact]
        public async Task OnInitializeRequested_UzytkownikZalogowany_LadujePlanyUzytkownika()
        {
            // ARRANGE
            var mockView = new Mock<ITreningView>();
            var fakeDb = new FakeDatabaseContext();

            // Podrzucamy 2 sztuczne plany treningowe
            fakeDb.WorkoutPlansToReturn = new List<WorkoutPlan>
            {
                new WorkoutPlan { Id = 1, Name = "FBW A" },
                new WorkoutPlan { Id = 2, Name = "FBW B" }
            };

            var stateService = new StateService
            {
                IsLoggedIn = true,
                CurrentUser = new Uzytkownik { id = 4 } // Użytkownik zalogowany
            };

            var presenter = new TreningPresenter(mockView.Object, fakeDb, stateService);

            // ACT - Wywołujemy zdarzenie ładowania widoku
            mockView.Raise(v => v.InitializeRequested += null);
            await Task.Delay(50);

            // ASSERT
            mockView.VerifySet(v => v.UserPlans = It.Is<List<WorkoutPlan>>(list => list.Count == 2), Times.Once);
            mockView.Verify(v => v.RefreshUI(), Times.Once);
        }

        [Fact]
        public async Task OnInitializeRequested_UzytkownikNiezalogowany_ZwracaPustaListePlanow()
        {
            // ARRANGE
            var mockView = new Mock<ITreningView>();
            var fakeDb = new FakeDatabaseContext();
            var stateService = new StateService { CurrentUser = null }; // Niezalogowany

            var presenter = new TreningPresenter(mockView.Object, fakeDb, stateService);

            // ACT
            mockView.Raise(v => v.InitializeRequested += null);
            await Task.Delay(50);

            // ASSERT
            mockView.VerifySet(v => v.UserPlans = It.Is<List<WorkoutPlan>>(list => list.Count == 0), Times.Once);
            mockView.Verify(v => v.RefreshUI(), Times.Once);
        }
    }
}