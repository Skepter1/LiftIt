using LiftIt.Interfaces;
using LiftIt.Models;
using LiftIt.Presenters;
using Moq;
using Xunit;

namespace LiftIt.Tests
{
    public class EditProfilePresenterTests
    {
        [Fact]
        public async Task OnModificationRequested_SukcesModyfikacji_AktualizujeStateService()
        {
            // ARRANGE
            var mockView = new Mock<IEditProfileView>();
            mockView.Setup(v => v.Name).Returns("NowyMichal");
            mockView.Setup(v => v.Email).Returns("nowy@wp.pl");

            var fakeDb = new FakeDatabaseContext();
            fakeDb.ModificationResult = true;

            var stateService = new StateService
            {
                CurrentUser = new Uzytkownik { id = 1, login = "StaryMichal", email = "stary@wp.pl" }
            };

            var presenter = new EditProfilePresenter(mockView.Object, fakeDb, stateService);

            // ACT
            mockView.Raise(v => v.ModificationRequested += null);
            await Task.Delay(50);

            // ASSERT
            Assert.Equal("NowyMichal", stateService.CurrentUser.login);
            Assert.Equal("nowy@wp.pl", stateService.CurrentUser.email);
            mockView.Verify(v => v.SuccesfulModification("The modification was successful"), Times.Once);
        }
    }
}