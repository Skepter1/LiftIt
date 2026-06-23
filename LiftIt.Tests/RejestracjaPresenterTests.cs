using LiftIt.Interfaces;
using LiftIt.Models;
using LiftIt.Presenters;
using Moq;
using Xunit;

namespace LiftIt.Tests
{
    public class RejestracjaPresenterTests
    {
        [Fact]
        public async Task OnUserSignUpRequested_HaslaSieNieZgadzaja_ZwracaBlad()
        {
            // ARRANGE
            var mockView = new Mock<IRejestracjaView>();
            mockView.Setup(v => v.Password).Returns("haslo1");
            mockView.Setup(v => v.PasswordConfirm).Returns("haslo2"); // Różne hasła

            var fakeDb = new FakeDatabaseContext();
            var stateService = new StateService();
            var presenter = new RejestracjaPresenter(mockView.Object, fakeDb, stateService);

            // ACT
            mockView.Raise(v => v.SignUpUser += null);
            await Task.Delay(50);

            // ASSERT
            mockView.Verify(v => v.ShowSignUpError("Passwords do not match"), Times.Once);
            Assert.False(stateService.IsLoggedIn);
        }

        [Fact]
        public async Task OnUserSignUpRequested_EmailZajety_ZwracaBlad()
        {
            // ARRANGE
            var mockView = new Mock<IRejestracjaView>();
            mockView.Setup(v => v.Password).Returns("123");
            mockView.Setup(v => v.PasswordConfirm).Returns("123");
            mockView.Setup(v => v.Email).Returns("zajety@wp.pl");

            var fakeDb = new FakeDatabaseContext();
            fakeDb.EmailRegisteredResult = true; // Email zajęty w bazie

            var stateService = new StateService();
            var presenter = new RejestracjaPresenter(mockView.Object, fakeDb, stateService);

            // ACT
            mockView.Raise(v => v.SignUpUser += null);
            await Task.Delay(50);

            // ASSERT
            mockView.Verify(v => v.ShowSignUpError("This e-mail address is already registered."), Times.Once);
        }
    }
}