using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;

namespace LiftIt.Models
{
    public class DatabaseContext
    {
        // 127.0.0.1 działa TYLKO na Windows.
        // Jeśli odpalisz emulator Androida, użyj adresu: 10.0.2.2 (to specjalny alias w emulatorze wskazujący na Twój PC)
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
            string query = @"INSERT INTO users (login, email, password_hash, date_of_registration) 
                             VALUES (@login, @email, SHA2(@password_hash, 256), @date_of_registration)";

            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@login", uzytkownik.login);
                command.Parameters.AddWithValue("@email", uzytkownik.email);

                // UWAGA: tu przechowujesz has�o w polu password_hash � zast�p hashowaniem w przysz�o�ci
                command.Parameters.AddWithValue("@password_hash", uzytkownik.password);
                command.Parameters.AddWithValue("@date_of_registration", DateTime.Now);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"B��d podczas rejestracji: {ex.Message}");
                return false;
            }
        }

        public async Task<Uzytkownik> SignInUserInMySQL(string email, string password)
        {
            string query = "SELECT id, login, email, password_hash FROM users WHERE email = @email";

            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@email", email);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    string dbPasswordHash = reader.IsDBNull("password_hash")
                        ? ""
                        : reader.GetString("password_hash");

                    // 🔥 HASH z C# (SHA256 = MySQL SHA2(..., 256))
                    string inputHash = Sha256(password);

                    if (dbPasswordHash == inputHash)
                    {
                        return new Uzytkownik
                        {
                            id = reader.GetInt32("id"),
                            login = reader.IsDBNull("login") ? "" : reader.GetString("login"),
                            email = reader.IsDBNull("email") ? "" : reader.GetString("email"),
                            password = dbPasswordHash
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas logowania: {ex.Message}");
                return null;
            }
        }


        public static string Sha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));

            var sb = new StringBuilder();
            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }

        // ---- Exercises / BodyParts ----
        public async Task<List<Exercise>> GetAllExercisesAsync()
        {
            var list = new List<Exercise>();
            const string query = @"
                SELECT e.id, e.name, e.body_part_id, bp.name AS body_part_name, e.user_id
                FROM exercises e
                JOIN body_parts bp ON e.body_part_id = bp.id
                ORDER BY bp.name, e.name";

            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();
                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new Exercise
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.IsDBNull("name") ? "" : reader.GetString("name"),
                        BodyPartId = reader.IsDBNull("body_part_id") ? 0 : reader.GetInt32("body_part_id"),
                        BodyPartName = reader.IsDBNull("body_part_name") ? "" : reader.GetString("body_part_name"),
                        UserId = reader.IsDBNull("user_id") ? null : reader.GetInt32("user_id")
                    });
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            return list;
        }

        // ---- Workout plans ----
        public async Task<List<WorkoutPlan>> GetWorkoutPlansForUserAsync(int userId)
        {
            var list = new List<WorkoutPlan>();
            const string query = "SELECT id, user_id, name, creation_date FROM workout_plans WHERE user_id = @uid ORDER BY creation_date DESC";
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new WorkoutPlan
                    {
                        Id = reader.GetInt32("id"),
                        UserId = reader.GetInt32("user_id"),
                        Name = reader.IsDBNull("name") ? "" : reader.GetString("name"),
                        CreationDate = reader.IsDBNull("creation_date") ? DateTime.Now : reader.GetDateTime("creation_date")
                    });
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            return list;
        }

        public async Task<WorkoutPlan> GetWorkoutPlanByIdAsync(int id)
        {
            const string query = "SELECT id, user_id, name, creation_date FROM workout_plans WHERE id = @id";
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new WorkoutPlan
                    {
                        Id = reader.GetInt32("id"),
                        UserId = reader.GetInt32("user_id"),
                        Name = reader.IsDBNull("name") ? "" : reader.GetString("name"),
                        CreationDate = reader.IsDBNull("creation_date") ? DateTime.Now : reader.GetDateTime("creation_date")
                    };
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            return null;
        }

        public async Task<int> CreateWorkoutPlanAsync(WorkoutPlan plan)
        {
            const string insert = "INSERT INTO workout_plans (user_id, name, creation_date) VALUES (@uid, @name, @date); SELECT LAST_INSERT_ID();";
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();
                using var cmd = new MySqlCommand(insert, conn);
                cmd.Parameters.AddWithValue("@uid", plan.UserId);
                cmd.Parameters.AddWithValue("@name", plan.Name);
                cmd.Parameters.AddWithValue("@date", plan.CreationDate);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); return 0; }
        }

        public async Task<bool> UpdateWorkoutPlanAsync(int planId, int userId, string newName)
        {
            const string update = "UPDATE workout_plans SET name = @name WHERE id = @id AND user_id = @uid";
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();
                using var cmd = new MySqlCommand(update, conn);
                cmd.Parameters.AddWithValue("@name", newName);
                cmd.Parameters.AddWithValue("@id", planId);
                cmd.Parameters.AddWithValue("@uid", userId);
                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); return false; }
        }

        public async Task<bool> DeleteWorkoutPlanAsync(int planId, int userId)
        {
            const string deleteExercises = "DELETE FROM exercises_in_plan WHERE plan_id = @id";
            const string deletePlan = "DELETE FROM workout_plans WHERE id = @id AND user_id = @uid";
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();
                using var tran = await conn.BeginTransactionAsync();
                using (var cmd = new MySqlCommand(deleteExercises, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@id", planId);
                    await cmd.ExecuteNonQueryAsync();
                }
                using (var cmd2 = new MySqlCommand(deletePlan, conn, tran))
                {
                    cmd2.Parameters.AddWithValue("@id", planId);
                    cmd2.Parameters.AddWithValue("@uid", userId);
                    int rows = await cmd2.ExecuteNonQueryAsync();
                    await tran.CommitAsync();
                    return rows > 0;
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); return false; }
        }

        public async Task AddExerciseToPlanAsync(int planId, int exerciseId, int order)
        {
            const string insert = "INSERT INTO exercises_in_plan (plan_id, exercise_id, exercise_order) VALUES (@p, @e, @o)";
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();
                using var cmd = new MySqlCommand(insert, conn);
                cmd.Parameters.AddWithValue("@p", planId);
                cmd.Parameters.AddWithValue("@e", exerciseId);
                cmd.Parameters.AddWithValue("@o", order);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        public async Task UpdateExercisesInPlanAsync(int planId, List<int> exerciseIds)
        {
            const string delete = "DELETE FROM exercises_in_plan WHERE plan_id = @p";
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();
                using var tran = await conn.BeginTransactionAsync();
                using (var cmd = new MySqlCommand(delete, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@p", planId);
                    await cmd.ExecuteNonQueryAsync();
                }
                int order = 1;
                foreach (var exId in exerciseIds)
                {
                    using var cmd2 = new MySqlCommand("INSERT INTO exercises_in_plan (plan_id, exercise_id, exercise_order) VALUES (@p,@e,@o)", conn, tran);
                    cmd2.Parameters.AddWithValue("@p", planId);
                    cmd2.Parameters.AddWithValue("@e", exId);
                    cmd2.Parameters.AddWithValue("@o", order++);
                    await cmd2.ExecuteNonQueryAsync();
                }
                await tran.CommitAsync();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        public async Task<List<ExercisesInPlan>> GetExercisesInPlanAsync(int planId)
        {
            var list = new List<ExercisesInPlan>();
            const string query = @"
                SELECT eip.id, eip.plan_id, eip.exercise_id, eip.exercise_order,
               ex.name AS exercise_name, bp.name AS body_part_name
                FROM exercises_in_plan eip
                JOIN exercises ex ON eip.exercise_id = ex.id
                JOIN body_parts bp ON ex.body_part_id = bp.id
                WHERE eip.plan_id = @planId
                ORDER BY eip.exercise_order";
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@planId", planId);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new ExercisesInPlan
                    {
                        Id = reader.GetInt32("id"),
                        PlanId = reader.GetInt32("plan_id"),
                        ExerciseId = reader.GetInt32("exercise_id"),
                        Order = reader.GetInt32("exercise_order"),
                        ExerciseName = reader.IsDBNull("exercise_name") ? "" : reader.GetString("exercise_name"),
                        BodyPartName = reader.IsDBNull("body_part_name") ? "" : reader.GetString("body_part_name")
                    });
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            return list;
        }

        // --- metody sesji treningowej i setów ---
        public async Task<int> StartTrainingSessionAsync(int userId)
        {
            const string insert = "INSERT INTO trainings_history (user_id, start_time) VALUES (@uid, CURRENT_TIMESTAMP()); SELECT LAST_INSERT_ID();";
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();
                using var cmd = new MySqlCommand(insert, conn);
                cmd.Parameters.AddWithValue("@uid", userId);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); return 0; }
        }

        public async Task<bool> EndTrainingSessionAsync(int trainingId, DateTime? endTime, string notes)
        {
            const string update = "UPDATE trainings_history SET end_time = @end, notes = @notes WHERE id = @id";
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();
                using var cmd = new MySqlCommand(update, conn);
                cmd.Parameters.AddWithValue("@end", endTime ?? DateTime.Now);
                cmd.Parameters.AddWithValue("@notes", (object)notes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", trainingId);
                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); return false; }
        }

        public async Task<int> AddSetAsync(int trainingId, int exerciseId, int setNumber, decimal weight, int reps)
        {
            // Wrapper — używamy bezpiecznego upsertu (nadpisze istniejącą serię o tym samym training+exercise+setNumber)
            return await UpsertSetAsync(trainingId, exerciseId, setNumber, weight, reps);
        }

        // ZMIANA NAZWY: Metoda wcześniej nazywała się GetSetsForTrainingAsync. 
        // Zmieniłem ją, żeby pasowała do Prezentera
        public async Task<List<SetRecord>> GetSetsForSessionAsync(int trainingId)
        {
            var list = new List<SetRecord>();
            const string query = "SELECT id, training_id, exercise_id, set_number, weight, reps FROM sets WHERE training_id = @t ORDER BY exercise_id, set_number";
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@t", trainingId);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new SetRecord
                    {
                        Id = reader.GetInt32("id"),
                        TrainingId = reader.GetInt32("training_id"),
                        ExerciseId = reader.GetInt32("exercise_id"),
                        SetNumber = reader.GetInt32("set_number"),
                        Weight = reader.GetDecimal("weight"),
                        Reps = reader.GetInt32("reps")
                    });
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            return list;
        }

        public async Task<int> UpsertSetAsync(int trainingId, int exerciseId, int setNumber, decimal weight, int reps)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                // Sprawdź czy istnieje taki wiersz
                const string select = "SELECT id FROM sets WHERE training_id = @t AND exercise_id = @e AND set_number = @sn";
                using (var selCmd = new MySqlCommand(select, conn))
                {
                    selCmd.Parameters.AddWithValue("@t", trainingId);
                    selCmd.Parameters.AddWithValue("@e", exerciseId);
                    selCmd.Parameters.AddWithValue("@sn", setNumber);

                    var existing = await selCmd.ExecuteScalarAsync();
                    if (existing != null && existing != DBNull.Value)
                    {
                        // aktualizuj istniejącą serię
                        const string update = "UPDATE sets SET weight = @w, reps = @r WHERE id = @id";
                        using var updCmd = new MySqlCommand(update, conn);
                        updCmd.Parameters.AddWithValue("@w", weight);
                        updCmd.Parameters.AddWithValue("@r", reps);
                        updCmd.Parameters.AddWithValue("@id", Convert.ToInt32(existing));
                        int rows = await updCmd.ExecuteNonQueryAsync();
                        return rows > 0 ? Convert.ToInt32(existing) : 0;
                    }
                    else
                    {
                        // wstaw nową
                        const string insert = "INSERT INTO sets (training_id, exercise_id, set_number, weight, reps) VALUES (@t,@e,@sn,@w,@r); SELECT LAST_INSERT_ID();";
                        using var insCmd = new MySqlCommand(insert, conn);
                        insCmd.Parameters.AddWithValue("@t", trainingId);
                        insCmd.Parameters.AddWithValue("@e", exerciseId);
                        insCmd.Parameters.AddWithValue("@sn", setNumber);
                        insCmd.Parameters.AddWithValue("@w", weight);
                        insCmd.Parameters.AddWithValue("@r", reps);
                        var idObj = await insCmd.ExecuteScalarAsync();
                        return idObj == null ? 0 : Convert.ToInt32(idObj);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpsertSetAsync error: {ex.Message}");
                return 0;
            }
        }

        // NOWA METODA: Wczytuje wybrany plan jako nową, pustą sesję treningową w bazie
        public async Task<(int trainingId, List<ExercisesInPlan> planItems)> RunPlanAsSessionAsync(int planId)
        {
            // Najpierw pobieramy plan, żeby poznać ID użytkownika
            var plan = await GetWorkoutPlanByIdAsync(planId);

            if (plan == null)
            {
                return (0, new List<ExercisesInPlan>());
            }

            // Otwieramy nową sesję dla tego użytkownika
            int trainingId = await StartTrainingSessionAsync(plan.UserId);

            // Zwracamy ID nowej sesji oraz listę ćwiczeń przypisanych do wybranego planu
            var items = await GetExercisesInPlanAsync(planId);

            return (trainingId, items);
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
        public async Task<List<BodyPart>> GetBodyParts()
        {
            var list = new List<BodyPart>();
            const string query = "SELECT id, name FROM body_parts ORDER BY name";

            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new BodyPart
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.IsDBNull("name") ? "" : reader.GetString("name")
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd GetBodyParts: {ex.Message}");
            }

            return list;
        }
        public async Task<List<Exercise>> GetExercisesByBodyPartId(int bodyPartId)
        {
            var list = new List<Exercise>();
            const string query = @"
                SELECT e.id, e.name, e.body_part_id, bp.name AS body_part_name, e.user_id
                FROM exercises e
                JOIN body_parts bp ON e.body_part_id = bp.id
                WHERE e.body_part_id = @bpId
                ORDER BY e.name";

            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@bpId", bodyPartId);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new Exercise
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.IsDBNull("name") ? "" : reader.GetString("name"),
                        BodyPartId = reader.IsDBNull("body_part_id") ? 0 : reader.GetInt32("body_part_id"),
                        BodyPartName = reader.IsDBNull("body_part_name") ? "" : reader.GetString("body_part_name"),
                        UserId = reader.IsDBNull("user_id") ? null : reader.GetInt32("user_id")
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd GetExercisesByBodyPartId: {ex.Message}");
            }

            return list;
        }
        public async Task SaveWorkoutPlan(string name, List<Exercise> exercises, int userId)
        {
            if (exercises == null || !exercises.Any())
            {
                return;
            }

            using var conn = GetConnection();
            await conn.OpenAsync();

            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                var insertPlanQuery = @"
                    INSERT INTO workout_plans (user_id, name, creation_date)
                    VALUES (@user_id, @name, @date);
                    SELECT LAST_INSERT_ID();";

                int planId;

                using (var cmd = new MySqlCommand(insertPlanQuery, conn, (MySqlTransaction)transaction))
                {
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now);

                    planId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                var insertExerciseQuery = @"
                    INSERT INTO exercises_in_plan (plan_id, exercise_id, exercise_order)
                    VALUES (@plan_id, @exercise_id, @order);";

                int order = 1;

                foreach (var ex in exercises)
                {
                    using var cmd = new MySqlCommand(insertExerciseQuery, conn, (MySqlTransaction)transaction);
                    cmd.Parameters.AddWithValue("@plan_id", planId);
                    cmd.Parameters.AddWithValue("@exercise_id", ex.Id);
                    cmd.Parameters.AddWithValue("@order", order++);

                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}