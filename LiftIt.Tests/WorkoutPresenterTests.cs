using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiftIt.Presenters;
using LiftIt.Interfaces;
using LiftIt.Models;

namespace LiftIt.Tests
{
    public class WorkoutPresenterTests
    {
        [Fact]
        public async Task OnInitializeData_UruchomienieEkranu_InicjalizujeWartosciPoczatkowe()
        {
            // ARRANGE
            var mockView = new Mock<IWorkoutView>();
            var fakeDb = new FakeDatabaseContext();
            fakeDb.AllExercisesToReturn = new List<Exercise>
            {
                new Exercise { Id = 1, Name = "Wyciskanie" }
            };

            var stateService = new StateService();
            using var presenter = new WorkoutPresenter(mockView.Object, fakeDb, stateService);

            // ACT
            mockView.Raise(v => v.InitializeDataRequested += null);
            await Task.Delay(50);

            // ASSERT
            mockView.VerifySet(v => v.AvailableExercises = It.Is<List<Exercise>>(l => l.Count == 1), Times.Once);
            mockView.VerifySet(v => v.FormattedTimerText = "00:00:00", Times.Once);
            mockView.Verify(v => v.RefreshUI(), Times.Once);
        }

        [Fact]
        public async Task OnStartSession_UzytkownikZalogowany_RozpoczynaSesjeIUruchamiaTimer()
        {
            // ARRANGE
            var mockView = new Mock<IWorkoutView>();

            mockView.SetupAllProperties();

            mockView.Setup(v => v.PlanId).Returns(0);
            mockView.Setup(v => v.ExerciseSets).Returns(new Dictionary<int, List<SetRecord>>());

            var fakeDb = new FakeDatabaseContext();
            fakeDb.StartedTrainingId = 99;

            var stateService = new StateService
            {
                CurrentUser = new Uzytkownik { id = 4 }
            };

            using var presenter = new WorkoutPresenter(mockView.Object, fakeDb, stateService);

            // ACT
            mockView.Raise(v => v.StartSessionRequested += null);
            await Task.Delay(50);

            // ASSERT
            Assert.Equal(99, mockView.Object.CurrentTrainingId);
            mockView.Verify(v => v.ShowMessage("Empty session started."), Times.Once);
        }

        [Fact]
        public async Task OnEndSession_ZakonczenieTreningu_ResetujeWartosciSesjiIZatrzymujeTimer()
        {
            // ARRANGE
            var mockView = new Mock<IWorkoutView>();

            mockView.Setup(v => v.CurrentTrainingId).Returns(99);

            mockView.Setup(v => v.EndNotes).Returns("Dobry trening");

            mockView.Setup(v => v.ExerciseSets).Returns(new Dictionary<int, List<SetRecord>>());

            var fakeDb = new FakeDatabaseContext();
            var stateService = new StateService();

            using var presenter = new WorkoutPresenter(mockView.Object, fakeDb, stateService);

            // ACT
            mockView.Raise(v => v.EndSessionRequested += null);
            await Task.Delay(50);

            // ASSERT
            mockView.VerifySet(v => v.CurrentTrainingId = 0, Times.Once);
            mockView.VerifySet(v => v.EndNotes = "", Times.Once);
            mockView.VerifySet(v => v.FormattedTimerText = "00:00:00", Times.Once);
            mockView.Verify(v => v.ShowMessage("Session completed and saved."), Times.Once);
        }
    }
}