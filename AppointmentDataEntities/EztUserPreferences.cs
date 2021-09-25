using System;

public class EztUserPreferences
{
    public Guid EzprefGuid { get; set; }
    public string PracticeVid { get; set; }
    public Guid UserGuid { get; set; }
    public int Currenttab { get; set; }
    public Guid CurrentGroupGuid { get; set; }
    public Guid CurrentResourceGuid { get; set; }
    public int SelbarSequence { get; set; }
    public DateTime LastVisitDate { get; set; }
    public Guid LastWorkstationGuid { get; set; }
}
