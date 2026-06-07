using System.Collections.Generic;

public class AuditTrailManager
{
    public List<AuditTrail> AuditLogs { get; set; }

    public AuditTrailManager()
    {
        AuditLogs = new List<AuditTrail>();
    }

    public void AddLog(AuditTrail log)
    {
        AuditLogs.Add(log);
    }
}
