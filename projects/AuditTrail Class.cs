public class AuditTrail
{
    public int AuditID { get; set; }

    public int UserID { get; set; }

    public string UserRole { get; set; }

    public string Action { get; set; }

    public string ModuleAffected { get; set; }

    public int RecordID { get; set; }

    public string OldValue { get; set; }

    public string NewValue { get; set; }

    public DateTime ActionDate { get; set; }

    public string Remarks { get; set; }
}
