using LiftIt.Models;
using Xunit;

namespace LiftIt.Tests
{
    public class RoutineDraftTests
    {
        [Fact]
        public void RoutineDraft_Initialization_CreatesEmptyList()
        {
            // Arrange & Act
            var draft = new RoutineDraft();

            // Assert
            Assert.NotNull(draft.ExerciseIds);
            Assert.Empty(draft.ExerciseIds);
            Assert.Equal(string.Empty, draft.Name);
        }

        [Fact]
        public void RoutineDraft_AddAndRemoveExercises_WorksCorrectly()
        {
            // Arrange
            var draft = new RoutineDraft();

            // Act - dodawanie
            draft.ExerciseIds.Add(1);
            draft.ExerciseIds.Add(2);
            draft.ExerciseIds.Add(3);

            // Act - usuwanie
            draft.ExerciseIds.Remove(2);

            // Assert
            Assert.Equal(2, draft.ExerciseIds.Count);
            Assert.Contains(1, draft.ExerciseIds);
            Assert.Contains(3, draft.ExerciseIds);
            Assert.DoesNotContain(2, draft.ExerciseIds);
        }

        [Fact]
        public void RoutineDraft_AllowsDuplicateExercises()
        {
            // Czasami w treningu wykonujemy to samo ćwiczenie dwa razy (np. w różnych wariantach)
            // Ten test upewnia się, że model na to pozwala.
            
            // Arrange
            var draft = new RoutineDraft();

            // Act
            draft.ExerciseIds.Add(5);
            draft.ExerciseIds.Add(5);

            // Assert
            Assert.Equal(2, draft.ExerciseIds.Count);
            Assert.Equal(5, draft.ExerciseIds[0]);
            Assert.Equal(5, draft.ExerciseIds[1]);
        }
    }
}
