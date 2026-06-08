using System;

namespace project
{
    /// <summary>
    /// Admin/HR Manager Session Management
    /// Use this when you already have a 'Session' class for applicants
    /// </summary>
    public static class AdminSession
    {
        // ADMIN SESSION PROPERTIES
        public static int AdminId { get; set; }
        public static string AdminUsername { get; set; }
        public static string AdminFullName { get; set; }
        public static string AdminEmail { get; set; }
        public static string AdminRole { get; set; }

        // COMPUTED PROPERTIES (Read-only)
        public static bool IsManager
        {
            get { return AdminRole == "HR Manager" || AdminRole == "Admin"; }
        }

        public static bool IsStaff
        {
            get { return AdminRole == "HR Staff"; }
        }

        public static bool IsAdmin
        {
            get { return AdminRole == "Admin"; }
        }

        // CLEAR ALL ADMIN SESSION DATA
        public static void ClearAll()
        {
            AdminId = 0;
            AdminUsername = null;
            AdminFullName = null;
            AdminEmail = null;
            AdminRole = null;
        }
    }
}