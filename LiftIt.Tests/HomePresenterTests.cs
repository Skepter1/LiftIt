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
            // ARRANGE 

            var stateService = new StateService
            {
                IsLoggedIn = true,
                CurrentUser = new Uzytkownik { id = 4, login = "Deny" }
            };

            var mockHistory = new List<TrainingHistoryDto>
            {
                new TrainingHistoryDto { Id = 1, Notes = "Trening klatki" },
                new TrainingHistoryDto { Id = 2, Notes = "Trening nóg" }
            };

            var fakeDbContext = new FakeDatabaseContext();
            fakeDbContext.TrainingsToReturn = mockHistory;

            var mockView = new Mock<IHomeView>();
            List<TrainingHistoryDto> assignedTrainings = null;

            mockView.SetupSet(v => v.Trainings = It.IsAny<List<TrainingHistoryDto>>())
                    .Callback<List<TrainingHistoryDto>>(val => assignedTrainings = val);

            var presenter = new HomePresenter(fakeDbContext, stateService);

            // ACT
            await presenter.InitAsync(mockView.Object);

            // ASSERT

            Assert.NotNull(assignedTrainings);

            Assert.Equal(2, assignedTrainings.Count);
            Assert.Equal("Trening klatki", assignedTrainings[0].Notes);

  
            mockView.VerifySet(v => v.Trainings = It.IsAny<List<TrainingHistoryDto>>(), Times.Once);
        }

        [Fact]
        public async Task InitAsync_UzytkownikNiezalogowany_ZwracaPustaListeDoWidoku()
        {
            // ARRANGE

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

            // ACT
            await presenter.InitAsync(mockView.Object);

            // ASSERT

            Assert.NotNull(assignedTrainings);
            Assert.Empty(assignedTrainings);
        }
    }
}