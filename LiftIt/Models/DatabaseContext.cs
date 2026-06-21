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
        public async Task<Uzytkownik> SignInUserInMySQL(string email, string password)
        {
            // Pobieramy rekord użytkownika na podstawie adresu e-mail
            string query = "SELECT id, login, email, password_hash FROM users WHERE email = @email";

            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@email", email);

                using var reader = await command.ExecuteReaderAsync();

                // Jeśli znaleziono użytkownika o takim adresie e-mail
                if (await reader.ReadAsync())
                {
                    // Pobieramy zahaszowane hasło z bazy danych
                    string dbPasswordHash = reader.GetString("password_hash");

                    // WERYFIKACJA HASŁA:
                    // Jeśli w rejestracji zapisałeś czyste hasło, używasz zwykłego porównania (jak poniżej).
                    // Jeśli używasz hashowania (np. BCrypt), w tym miejscu wywołujesz: BCrypt.Verify(password, dbPasswordHash)
                    if (dbPasswordHash == password)
                    {
                        // Hasło się zgadza! Tworzymy i zwracamy obiekt użytkownika,
                        // który później przekażemy do StateService.
                        return new Uzytkownik
                        {
                            // Zakładam, że dodałeś właściwość id_uzytkownika do klasy Uzytkownik
                            id = reader.GetInt32("id"), 
                            login = reader.GetString("login"),
                            email = reader.GetString("email")
                        };
                    }
                }

                // Jeśli e-mail nie istnieje lub hasło jest niepoprawne
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas logowania: {ex.Message}");
                return null;
            }
        }
        public async Task<bool> ModifyProfileInMySQL(int userId, string nowyLogin, string noweHaslo, string nowyEmail)
        {
            // Sprawdzamy, które pola użytkownik faktycznie uzupełnił
            bool zmieniamyLogin = !string.IsNullOrWhiteSpace(nowyLogin);
            bool zmieniamyHaslo = !string.IsNullOrWhiteSpace(noweHaslo);
            bool zmieniamyEmail = !string.IsNullOrWhiteSpace(nowyEmail);

            // Jeśli wszystkie pola są puste, nie ma potrzeby pytać bazy danych
            if (!zmieniamyLogin && !zmieniamyHaslo && !zmieniamyEmail) return true;

            // Budujemy dynamiczną kwerendę UPDATE
            string query = "UPDATE users SET ";
            List<string> updates = new List<string>();

            if (zmieniamyLogin) updates.Add("login = @login");
            if (zmieniamyHaslo) updates.Add("password_hash = @password_hash");
            if (zmieniamyEmail) updates.Add("email = @email");

            // Łączymy elementy listy za pomocą przecinków
            query += string.Join(", ", updates);
            query += " WHERE id = @user_id";

            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@user_id", userId);

                // Dodajemy parametry tylko dla modyfikowanych kolumn
                if (zmieniamyLogin) command.Parameters.AddWithValue("@login", nowyLogin);
                if (zmieniamyHaslo) command.Parameters.AddWithValue("@password_hash", noweHaslo);
                if (zmieniamyEmail) command.Parameters.AddWithValue("@email", nowyEmail);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd aktualizacji profilu: {ex.Message}");
                return false;
            }
        }

    }
}