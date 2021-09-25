using System;

public interface IEztAppointmentTypes
{
    Guid ApptTypeGuid { get; set; }
    Guid GroupGuid { get; set; }
    string PracticeVid { get; set; }
    string Name { get; set; }
    Guid DeleteGuid { get; set; }
    string Description { get; set; }
    string NumHours { get; set; }
    string NumMinutes { get; set; }
    byte[] Icon { get; set; }
    bool Deleted { get; set; }
    string PopupMessage { get; set; }
}
