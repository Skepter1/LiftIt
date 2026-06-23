using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using System.Linq;

namespace LiftIt.Models
{
    public class DatabaseContext
    {
        private readonly string _connectionString = "Server=127.0.0.1;Port=3306;Database=liftit;User ID=root;Password=;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
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

        // ---- Uwierzytelnianie użytkowników ----

        // rejestracja użytkownika w bazie danych MySQL
        public virtual async Task<int> SignUpUserInMySQL(Uzytkownik uzytkownik)
        {
            const string query = @"INSERT INTO users (login, email, password_hash, date_of_registration) 
                                 VALUES (@login, @email, @password_hash, @date_of_registration);
                                 SELECT LAST_INSERT_ID();";
            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@login", uzytkownik.login);
                command.Parameters.AddWithValue("@email", uzytkownik.email);
                command.Parameters.AddWithValue("@password_hash", Sha256(uzytkownik.password));
                command.Parameters.AddWithValue("@date_of_registration", DateTime.Now);

                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas rejestracji: {ex.Message}");
                return 0;
            }
        }

        // logowanie użytkownika w bazie danych MySQL
        public virtual async Task<Uzytkownik> SignInUserInMySQL(string email, string password)
        {
            const string query = "SELECT id, login, email, password_hash FROM users WHERE email = @email";
            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@email", email);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    string dbPasswordHash = reader.IsDBNull("password_hash") ? "" : reader.GetString("password_hash");
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

        // sprawdzanie, czy e-mail jest już zarejestrowany w bazie danych MySQL
        public virtual async Task<bool> IsEmailRegisteredAsync(string email)
        {
            const string query = "SELECT COUNT(*) FROM users WHERE email = @email";
            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@email", email);

                var count = await command.ExecuteScalarAsync();
                return Convert.ToInt32(count) > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas sprawdzania e-maila: {ex.Message}");
                return true;
            }
        }

        // modyfikacja profilu użytkownika w bazie danych MySQL
        public virtual async Task<bool> ModifyProfileInMySQL(int userId, string nowyLogin, string noweHaslo, string nowyEmail)
        {
            bool zmieniamyLogin = !string.IsNullOrWhiteSpace(nowyLogin);
            bool zmieniamyHaslo = !string.IsNullOrWhiteSpace(noweHaslo);
            bool zmieniamyEmail = !string.IsNullOrWhiteSpace(nowyEmail);

            if (!zmieniamyLogin && !zmieniamyHaslo && !zmieniamyEmail) return true;

            string query = "UPDATE users SET ";
            var updates = new List<string>();

            if (zmieniamyLogin) updates.Add("login = @login");
            if (zmieniamyHaslo) updates.Add("password_hash = @password_hash");
            if (zmieniamyEmail) updates.Add("email = @email");

            query += string.Join(", ", updates) + " WHERE id = @user_id";

            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@user_id", userId);

                if (zmieniamyLogin) command.Parameters.AddWithValue("@login", nowyLogin);
                if (zmieniamyHaslo) command.Parameters.AddWithValue("@password_hash", Sha256(noweHaslo));
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

        // ---- BODY PARTS & EXERCISES (Części Ciała i Ćwiczenia) ----

        public virtual async Task<List<BodyPart>> GetBodyParts()
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
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            return list;
        }

        // pobranie wszystkich ćwiczeń z bazy danych MySQL
        public virtual async Task<List<Exercise>> GetAllExercisesAsync()
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

        // pobranie ćwiczeń dla konkretnej części ciała z bazy danych MySQL
        public virtual async Task<List<Exercise>> GetExercisesByBodyPartId(int bodyPartId)
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
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            return list;
        }

        // ---- WORKOUT PLANS (Szablony Treningów) ----

        // pobranie wszystkich szablonów treningów dla konkretnego użytkownika z bazy danych MySQL
        public virtual async Task<List<WorkoutPlan>> GetWorkoutPlansForUserAsync(int userId)
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

        // pobranie szablonu treningu po jego ID z bazy danych MySQL
        public virtual async Task<WorkoutPlan> GetWorkoutPlanByIdAsync(int id)
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

        // utworzenie nowego szablonu treningu w bazie danych MySQL
        public virtual async Task<int> CreateWorkoutPlanAsync(WorkoutPlan plan)
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

        // aktualizacja nazwy szablonu treningu w bazie danych MySQL
        public virtual async Task<bool> UpdateWorkoutPlanAsync(int planId, int userId, string newName)
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

        // usunięcie szablonu treningu i powiązanych ćwiczeń w bazie danych MySQL
        public virtual async Task<bool> DeleteWorkoutPlanAsync(int planId, int userId)
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

        // zapisanie szablonu treningu wraz z ćwiczeniami w bazie danych MySQL
        public virtual async Task SaveWorkoutPlan(string name, List<Exercise> exercises, int userId)
        {
            if (exercises == null || !exercises.Any()) return;

            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                const string insertPlanQuery = @"
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

                const string insertExerciseQuery = @"
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

        // zapisanie ćwiczenia do szablonu treningu w bazie danych MySQL
        public virtual async Task AddExerciseToPlanAsync(int planId, int exerciseId, int order)
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

        // aktualizacja listy ćwiczeń w szablonie treningu w bazie danych MySQL
        public virtual async Task UpdateExercisesInPlanAsync(int planId, List<int> exerciseIds)
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

        // pobranie listy ćwiczeń w szablonie treningu z bazy danych MySQL
        public virtual async Task<List<ExercisesInPlan>> GetExercisesInPlanAsync(int planId)
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

        // ---- TRAINING SESSIONS (Sesje Treningowe) ----

        public virtual async Task<int> StartTrainingSessionAsync(int userId)
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

        // zakończenie sesji treningowej w bazie danych MySQL
        public virtual async Task<bool> EndTrainingSessionAsync(int trainingId, DateTime? endTime, string notes)
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

        // dodanie lub aktualizacja rekordu zestawu ćwiczeń w bazie danych MySQL
        public virtual async Task<int> AddSetAsync(int trainingId, int exerciseId, int setNumber, decimal weight, int reps)
        {
            return await UpsertSetAsync(trainingId, exerciseId, setNumber, weight, reps);
        }

        // pobranie wszystkich zestawów ćwiczeń dla konkretnej sesji treningowej z bazy danych MySQL
        public virtual async Task<List<SetRecord>> GetSetsForSessionAsync(int trainingId)
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

        // dodanie lub aktualizacja rekordu zestawu ćwiczeń w bazie danych MySQL
        public virtual async Task<int> UpsertSetAsync(int trainingId, int exerciseId, int setNumber, decimal weight, int reps)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                const string select = "SELECT id FROM sets WHERE training_id = @t AND exercise_id = @e AND set_number = @sn";
                using (var selCmd = new MySqlCommand(select, conn))
                {
                    selCmd.Parameters.AddWithValue("@t", trainingId);
                    selCmd.Parameters.AddWithValue("@e", exerciseId);
                    selCmd.Parameters.AddWithValue("@sn", setNumber);

                    var existing = await selCmd.ExecuteScalarAsync();
                    if (existing != null && existing != DBNull.Value)
                    {
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

        // pobranie historii treningów użytkownika z bazy danych MySQL
        public virtual async Task<List<TrainingHistoryDto>> GetUserTrainingHistoryAsyncTwo(int userId)
        {
            var trainingList = new List<TrainingHistoryDto>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            const string trainingQuery = "SELECT id, start_time, end_time, notes FROM trainings_history WHERE user_id = @UserId ORDER BY start_time DESC, id DESC";
            using (var cmd = new MySqlCommand(trainingQuery, connection))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    trainingList.Add(new TrainingHistoryDto
                    {
                        Id = reader.GetInt32("id"),
                        TrainingDate = reader.IsDBNull(reader.GetOrdinal("start_time")) ? DateTime.Now : reader.GetDateTime("start_time"),
                        Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? string.Empty : reader.GetString("notes"),
                        Exercises = new List<ExerciseSummaryDto>()
                    });
                }
            }

            foreach (var training in trainingList)
            {
                const string setsQuery = @"
                    SELECT e.name AS ExerciseName, s.set_number, s.weight, s.reps 
                    FROM sets s
                    JOIN exercises e ON s.exercise_id = e.id
                    WHERE s.training_id = @TrainingId
                    ORDER BY s.exercise_id, s.set_number";

                using var cmd = new MySqlCommand(setsQuery, connection);
                cmd.Parameters.AddWithValue("@TrainingId", training.Id);
                using var reader = await cmd.ExecuteReaderAsync();
                var exerciseDict = new Dictionary<string, ExerciseSummaryDto>();

                while (await reader.ReadAsync())
                {
                    var exName = reader.GetString("ExerciseName");
                    if (!exerciseDict.ContainsKey(exName))
                    {
                        exerciseDict[exName] = new ExerciseSummaryDto
                        {
                            ExerciseName = exName,
                            Sets = new List<SetDto>()
                        };
                        training.Exercises.Add(exerciseDict[exName]);
                    }

                    exerciseDict[exName].Sets.Add(new SetDto
                    {
                        SetNumber = reader.GetInt32("set_number"),
                        Weight = (double)reader.GetDecimal("weight"),
                        Reps = reader.GetInt32("reps")
                    });
                }
            }
            return trainingList;
        }

        // uruchomienie szablonu treningu jako sesji treningowej w bazie danych MySQL
        public virtual async Task<(int trainingId, List<ExercisesInPlan> planItems)> RunPlanAsSessionAsync(int planId)
        {
            var plan = await GetWorkoutPlanByIdAsync(planId);
            if (plan == null) return (0, new List<ExercisesInPlan>());

            int trainingId = await StartTrainingSessionAsync(plan.UserId);
            var items = await GetExercisesInPlanAsync(planId);

            return (trainingId, items);
        }
    }
}