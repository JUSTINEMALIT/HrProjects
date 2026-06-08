using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace project
{
    // ── Keep original class name so existing forms compile ──────────
    public class DatabaseConnection
    {
        private readonly string _cs =
            "server=127.0.0.1;database=applicantsss;uid=root;pwd=;CharSet=utf8mb4;";

        public MySqlConnection GetConnection() => new MySqlConnection(_cs);

        // ── Original helpers (unchanged) ─────────────────────────────
        public object ExecuteScalar(string sql, params (string k, object v)[] parms)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    foreach (var (k, v) in parms)
                        cmd.Parameters.AddWithValue(k, v ?? DBNull.Value);
                    return cmd.ExecuteScalar();
                }
            }
        }

        public int ExecuteNonQuery(string sql, params (string k, object v)[] parms)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    foreach (var (k, v) in parms)
                        cmd.Parameters.AddWithValue(k, v ?? DBNull.Value);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // ── New helpers used by Applicant module pages ────────────────

        /// <summary>Returns a filled DataTable from a SELECT query.</summary>
        public DataTable Query(string sql, params (string k, object v)[] parms)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    foreach (var (k, v) in parms)
                        cmd.Parameters.AddWithValue(k, v ?? DBNull.Value);
                    var dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>Runs INSERT/UPDATE/DELETE and returns rows affected.</summary>
        public int Execute(string sql, params (string k, object v)[] parms)
            => ExecuteNonQuery(sql, parms);

        /// <summary>Runs INSERT and returns the new auto-increment ID.</summary>
        public int InsertGetId(string sql, params (string k, object v)[] parms)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    foreach (var (k, v) in parms)
                        cmd.Parameters.AddWithValue(k, v ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                    return (int)cmd.LastInsertedId;
                }
            }
        }

        /// <summary>Shortcut — same as ExecuteScalar but named Scalar for clarity.</summary>
        public object Scalar(string sql, params (string k, object v)[] parms)
            => ExecuteScalar(sql, parms);
    }

    // ── Session singleton ────────────────────────────────────────────
    public static class Session
    {
        public static int ApplicantId { get; set; }
        public static string ApplicantEmail { get; set; } = "";
        public static string ApplicantName { get; set; } = "";

        // Convenience splits used by Applicant pages
        public static string FirstName => ApplicantName.Contains(" ")
            ? ApplicantName.Substring(0, ApplicantName.IndexOf(' '))
            : ApplicantName;
        public static string LastName => ApplicantName.Contains(" ")
            ? ApplicantName.Substring(ApplicantName.IndexOf(' ') + 1)
            : "";
        public static string FullName => ApplicantName;

        public static int AdminId { get; set; }
        public static string AdminUsername { get; set; } = "";
        public static string AdminFullName { get; set; } = "";
        public static string AdminRole { get; set; } = ""; // Admin | HR Manager | HR Staff

        public static bool IsManager => AdminRole == "Admin" || AdminRole == "HR Manager";
        public static bool IsStaff => AdminRole == "HR Staff";

        public static void ClearAll()
        {
            ApplicantId = 0; ApplicantEmail = ""; ApplicantName = "";
            AdminId = 0; AdminUsername = ""; AdminFullName = ""; AdminRole = "";
        }
    }

    // ── Audit logger ─────────────────────────────────────────────────
    public static class Audit
    {
        public static void Log(string action, string table = "", int recordId = 0, string details = "")
        {
            try
            {
                string userType = Session.AdminId > 0 ? "HR" : "Applicant";
                string username = Session.AdminId > 0 ? Session.AdminUsername : Session.ApplicantEmail;
                new DatabaseConnection().ExecuteNonQuery(
                    "INSERT INTO audit_trail(user_type,username,action,table_name,record_id,details) VALUES(@ut,@un,@ac,@tb,@rid,@det)",
                    ("@ut", userType), ("@un", username), ("@ac", action),
                    ("@tb", table),
                    ("@rid", recordId == 0 ? (object)DBNull.Value : recordId),
                    ("@det", details));
            }
            catch { }
        }
    }
}