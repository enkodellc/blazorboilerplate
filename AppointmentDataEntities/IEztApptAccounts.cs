using System;

public interface IEztApptAccounts
{
    Guid ApptActGuid { get; set; }
    Guid AppointmentGuid { get; set; }
    Guid ClientGuid { get; set; }
    Guid PatientGuid { get; set; }
    string PracticeVid { get; set; }
    string CreatePracticeVid { get; set; }
}
