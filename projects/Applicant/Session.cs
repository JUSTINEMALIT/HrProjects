namespace HRApplicant
{
    public static class Session
    {
        public static int ApplicantId { get; set; }
        public static string FirstName { get; set; } = "";
        public static string LastName { get; set; } = "";
        public static string Email { get; set; } = "";
        public static string FullName => FirstName + " " + LastName;

        public static void Clear()
        {
            ApplicantId = 0;
            FirstName = "";
            LastName = "";
            Email = "";
        }
    }
}