using System;

public class EztAppointments
{
    public Guid AppointmentGuid { get; set; }
    public string PracticeVid { get; set; }
    public Guid ResourceGuid { get; set; }
    public Guid StatusTypeGuid { get; set; }
    public Guid? RecurringGuid { get; set; }
    public Guid? GroupGuid { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Guid IconGuid { get; set; }
    public string Comments { get; set; }
    public string CreatedByop { get; set; }
    public string LastmodByop { get; set; }
    public string CreatePracticeVid { get; set; }
    public bool Deleted { get; set; }
}
