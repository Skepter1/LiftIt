using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using LiftIt.Presenters;
using LiftIt.Interfaces;
using LiftIt.Models;

namespace LiftIt.Tests
{
    public class LogowaniePresenterTests
    {
        [Fact]
        public async Task OnUserLoginRequested_PoprawneDane_LogujeUzytkownikaIRzekierowuje()
        {
            // ARRANGE
            var mockView = new Mock<ILogowanieView>();
            mockView.Setup(v => v.Email).Returns("test@wp.pl");
            mockView.Setup(v => v.Password).Returns("123");

            var fakeDb = new FakeDatabaseContext();
            fakeDb.UserToReturn = new Uzytkownik { id = 4, login = "Deny", email = "test@wp.pl" };

            var stateService = new StateService();
            var presenter = new LogowaniePresenter(mockView.Object, fakeDb, stateService);

            // ACT - Wywołujemy zdarzenie symulujące kliknięcie przycisku logowania
            mockView.Raise(v => v.SignInUser += null);
            await Task.Delay(50); // Krótkie opóźnienie dla metod async void

            // ASSERT
            Assert.True(stateService.IsLoggedIn);
            Assert.Equal(4, stateService.CurrentUser.id);
            mockView.Verify(v => v.RedirectHomePage(), Times.Once);
        }

        [Fact]
        public async Task OnUserLoginRequested_BledneDane_PokazujeBlad()
        {
            // ARRANGE
            var mockView = new Mock<ILogowanieView>();
            mockView.Setup(v => v.Email).Returns("zly@email.com");
            mockView.Setup(v => v.Password).Returns("zle");

            var fakeDb = new FakeDatabaseContext();
            fakeDb.UserToReturn = null; // Baza nie znalazła użytkownika

            var stateService = new StateService();
            var presenter = new LogowaniePresenter(mockView.Object, fakeDb, stateService);

            // ACT
            mockView.Raise(v => v.SignInUser += null);
            await Task.Delay(50);

            // ASSERT
            Assert.False(stateService.IsLoggedIn);
            mockView.Verify(v => v.ShowSignInError(It.IsAny<string>()), Times.Once);
        }
    }
}