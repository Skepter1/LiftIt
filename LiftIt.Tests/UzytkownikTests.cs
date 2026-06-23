using LiftIt.Models;
using Xunit;

namespace LiftIt.Tests
{
    public class UzytkownikTests
    {
        [Fact]
        public void Uzytkownik_DefaultInitialization_HasExpectedValues()
        {
            // Arrange & Act
            var user = new Uzytkownik();

            // Assert
            Assert.Equal(0, user.id); // Domyślna wartość int
            Assert.Null(user.login);  // Typy referencyjne są domyślnie null przed przypisaniem
            Assert.Null(user.email);
            Assert.Null(user.password);
        }

        [Fact]
        public void Uzytkownik_PropertyAssignment_StoresValuesCorrectly()
        {
            // Arrange
            var user = new Uzytkownik();

            // Act
            user.id = 15;
            user.login = "Kulturysta99";
            user.email = "kulturysta@gym.com";
            user.password = "H@sl0123";

            // Assert
            Assert.Equal(15, user.id);
            Assert.Equal("Kulturysta99", user.login);
            Assert.Equal("kulturysta@gym.com", user.email);
            Assert.Equal("H@sl0123", user.password);
        }
    }
}
