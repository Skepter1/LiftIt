using LiftIt.Interfaces;
using LiftIt.Models;
using LiftIt.Presenters;
using Moq;
using Xunit;

namespace LiftIt.Tests
{
    public class NewPlanPresenterTests
    {
        [Fact]
        public async Task OnSavePlan_BrakNazwyPlanu_PokazujeBladWalidacji()
        {
            // ARRANGE
            var mockView = new Mock<INewPlanView>();
            mockView.Setup(v => v.PlanName).Returns(""); // Pusta nazwa

            var fakeDb = new FakeDatabaseContext();
            var stateService = new StateService { CurrentUser = new Uzytkownik { id = 1 } };
            var presenter = new NewPlanPresenter(mockView.Object, fakeDb, stateService);

            // ACT
            mockView.Raise(v => v.SavePlanRequested += null);
            await Task.Delay(50);

            // ASSERT
            mockView.Verify(v => v.ShowMessage("Podaj nazwę planu!"), Times.Once);
        }

        [Fact]
        public async Task OnSavePlan_UzytkownikNiezalogowany_BlokujeZapis()
        {
            // ARRANGE
            var mockView = new Mock<INewPlanView>();
            mockView.Setup(v => v.PlanName).Returns("Mój Nowy Plan");

            var fakeDb = new FakeDatabaseContext();
            var stateService = new StateService { CurrentUser = null }; // Niezalogowany
            var presenter = new NewPlanPresenter(mockView.Object, fakeDb, stateService);

            // ACT
            mockView.Raise(v => v.SavePlanRequested += null);
            await Task.Delay(50);

            // ASSERT
            mockView.Verify(v => v.ShowMessage("Musisz być zalogowany, aby zapisać plan."), Times.Once);
        }
    }
}