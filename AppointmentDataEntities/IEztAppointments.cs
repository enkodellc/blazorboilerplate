using System;

public interface IEztAppointments
{
    Guid AppointmentGuid { get; set; }    //Id 
    string PracticeVid { get; set; }
    Guid ResourceGuid { get; set; }
    Guid StatusTypeGuid { get; set; }
    Guid? RecurringGuid { get; set; }
    Guid? GroupGuid { get; set; }
    DateTime AppointmentDate { get; set; }
    DateTime StartTime { get; set; }
    DateTime EndTime { get; set; }
    Guid IconGuid { get; set; }
    string Comments { get; set; }
    string CreatedByop { get; set; }
    string LastmodByop { get; set; }
    string CreatePracticeVid { get; set; }
    bool Deleted { get; set; }
}

// int Id { get; set; }
// string Subject { get; set; }
// string Location { get; set; }
// DateTime StartTime { get; set; }
// DateTime EndTime { get; set; }
// string Description { get; set; }
// bool IsAllDay { get; set; }
// string RecurrenceRule { get; set; }
// string RecurrenceException { get; set; }
// Nullable<int> RecurrenceID { get; set; }