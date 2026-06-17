using MySqlConnector;
using System.Data;

namespace LiftIt.Models
{
    public class DatabaseContext
    {
        // 127.0.0.1 działa TYLKO na Windows. 
        // Jeśli odpalsz emulator Androida, użyj adresu: 10.0.2.2 (to specjalny alias w emulatorze wskazujący na Twój PC)
        private readonly string _connectionString = "Server=127.0.0.1;Port=3306;Database=liftit;User ID=root;Password=;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        // Przykładowa metoda testowa, którą wywołacie w Modelu
        public async Task<bool> TestPolaczenia()
        {
            using var connection = GetConnection();
            try
            {
                await connection.OpenAsync();
                return connection.State == ConnectionState.Open;
            }
            catch (Exception ex)
            {
                // Tutaj możecie podejrzeć błąd w razie problemów
                System.Diagnostics.Debug.WriteLine($"Błąd bazy: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> SignUpUserInMySQL(Uzytkownik uzytkownik)
        {
            // Przygotowujemy zapytanie SQL zgodnie z kolumnami Twojej tabeli.
            // Pomijamy 'user_id', ponieważ baza (Auto Increment) nada je sama.
            string query = @"INSERT INTO users (login, email, password_hash, date_of_registration) 
                             VALUES (@login, @email, @password_hash, @date_of_registration)";

            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);

                // Mapujemy dane z obiektu C# na parametry zapytania SQL
                command.Parameters.AddWithValue("@login", uzytkownik.login);
                command.Parameters.AddWithValue("@email", uzytkownik.email);

                // UWAGA AKADEMICKA: Na tym etapie przekazujemy czyste hasło. 
                // Jeśli prowadzący będzie wymagał hashowania, w tym miejscu wywołacie funkcję haszującą (np. BCrypt/SHA256)
                command.Parameters.AddWithValue("@password_hash", uzytkownik.password);

                // Automatycznie przypisujemy aktualną datę i godzinę rejestracji
                command.Parameters.AddWithValue("@date_of_registration", DateTime.Now);

                // Wykonujemy zapytanie. ExecuteNonQueryAsync zwraca liczbę zmodyfikowanych wierszy.
                int rowsAffected = await command.ExecuteNonQueryAsync();

                // Jeśli baza dodała 1 wiersz, oznacza to, że rejestracja się powiodła
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // W razie błędu (np. duplikacja adresu e-mail, jeśli nałożysz na kolumnę klucz UNIQUE), 
                // błąd zostanie wypisany w oknie Output w Visual Studio
                System.Diagnostics.Debug.WriteLine($"Błąd podczas rejestracji: {ex.Message}");
                return false;
            }
        }
    }
}