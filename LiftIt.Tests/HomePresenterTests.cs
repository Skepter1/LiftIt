using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiftIt.Interfaces;
using LiftIt.Models; 

namespace LiftIt.Tests
{

    public class HomePresenterTests
    {
        [Fact]
        public async Task InitAsync_UzytkownikZalogowany_PrzekazujeTreningiDoWidoku()
        {
            // --- ARRANGE (Przygotowanie danych wejściowych) ---

            // 1. Symulujemy, że użytkownik "Deny" z ID = 4 jest zalogowany w aplikacji
            var stateService = new StateService
            {
                IsLoggedIn = true,
                CurrentUser = new Uzytkownik { id = 4, login = "Deny" }
            };

            // 2. Przygotowujemy sztuczną historię treningów, którą rzekomo zwróci nasza baza
            var mockHistory = new List<TrainingHistoryDto>
            {
                new TrainingHistoryDto { Id = 1, Notes = "Trening klatki" },
                new TrainingHistoryDto { Id = 2, Notes = "Trening nóg" }
            };

            var fakeDbContext = new FakeDatabaseContext();
            fakeDbContext.TrainingsToReturn = mockHistory;

            // 3. Tworzymy atrapę (Mock) dla Twojego interfejsu widoku IHomeView
            var mockView = new Mock<IHomeView>();
            List<TrainingHistoryDto> assignedTrainings = null;

            // Przechwytujemy moment, w którym Prezenter przypisze dane do właściwości 'Trainings' widoku
            mockView.SetupSet(v => v.Trainings = It.IsAny<List<TrainingHistoryDto>>())
                    .Callback<List<TrainingHistoryDto>>(val => assignedTrainings = val);

            // Tworzymy instancję prezentera z naszymi podstawionymi zależnościami
            var presenter = new HomePresenter(fakeDbContext, stateService);

            // --- ACT (Wykonanie testowanej metody) ---
            await presenter.InitAsync(mockView.Object);

            // --- ASSERT (Weryfikacja oczekiwanych rezultatów) ---

            // Sprawdzamy, czy prezenter w ogóle przekazał listę do widoku
            Assert.NotNull(assignedTrainings);

            // Sprawdzamy, czy widok otrzymał dokładnie 2 treningi
            Assert.Equal(2, assignedTrainings.Count);
            Assert.Equal("Trening klatki", assignedTrainings[0].Notes);

            // Weryfikujemy za pomocą Moq, czy właściwość została ustawiona dokładnie jeden raz
            mockView.VerifySet(v => v.Trainings = It.IsAny<List<TrainingHistoryDto>>(), Times.Once);
        }

        [Fact]
        public async Task InitAsync_UzytkownikNiezalogowany_ZwracaPustaListeDoWidoku()
        {
            // --- ARRANGE (Przygotowanie danych wejściowych) ---

            // Symulujemy sytuację, w której użytkownik NIE jest zalogowany
            var stateService = new StateService
            {
                IsLoggedIn = false,
                CurrentUser = null
            };

            var fakeDbContext = new FakeDatabaseContext();
            var mockView = new Mock<IHomeView>();
            List<TrainingHistoryDto> assignedTrainings = null;

            mockView.SetupSet(v => v.Trainings = It.IsAny<List<TrainingHistoryDto>>())
                    .Callback<List<TrainingHistoryDto>>(val => assignedTrainings = val);

            var presenter = new HomePresenter(fakeDbContext, stateService);

            // --- ACT (Wykonanie) ---
            await presenter.InitAsync(mockView.Object);

            // --- ASSERT (Weryfikacja) ---

            // Prezenter przy braku autoryzacji powinien przekazać zainicjalizowaną, pustą listę
            Assert.NotNull(assignedTrainings);
            Assert.Empty(assignedTrainings); // Oczekujemy 0 elementów na liście
        }
    }
}