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
            mockView.Setup(v => v.PlanName).Returns("");

            var fakeDb = new FakeDatabaseContext();
            var stateService = new StateService { CurrentUser = new Uzytkownik { id = 1 } };
            var presenter = new NewPlanPresenter(mockView.Object, fakeDb, stateService);

            // ACT
            mockView.Raise(v => v.SavePlanRequested += null);
            await Task.Delay(50);

            // ASSERT
            mockView.Verify(v => v.ShowMessage("Enter the name of the plan!"), Times.Once);
        }

        [Fact]
        public async Task OnSavePlan_UzytkownikNiezalogowany_BlokujeZapis()
        {
            // ARRANGE
            var mockView = new Mock<INewPlanView>();
            mockView.Setup(v => v.PlanName).Returns("Mój Nowy Plan");

            var fakeDb = new FakeDatabaseContext();
            var stateService = new StateService { CurrentUser = null };
            var presenter = new NewPlanPresenter(mockView.Object, fakeDb, stateService);

            // ACT
            mockView.Raise(v => v.SavePlanRequested += null);
            await Task.Delay(50);

            // ASSERT
            mockView.Verify(v => v.ShowMessage("You must be logged in to save your plan."), Times.Once);
        }
    }
}