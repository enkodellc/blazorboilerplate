using System;

public class EztAppointmentTypes
{
    public Guid ApptTypeGuid { get; set; }
    public Guid GroupGuid { get; set; }
    public string PracticeVid { get; set; }
    public string Name { get; set; }
    public Guid DeleteGuid { get; set; }
    public string Description { get; set; }
    public string NumHours { get; set; }
    public string NumMinutes { get; set; }
    public byte[] Icon { get; set; }
    public bool Deleted { get; set; }
    public string PopupMessage { get; set; }
}
