using System;

public interface IEztUserPreferences
{
    Guid EzprefGuid { get; set; }
    string PracticeVid { get; set; }
    Guid UserGuid { get; set; }
    int Currenttab { get; set; }
    Guid CurrentGroupGuid { get; set; }
    Guid CurrentResourceGuid { get; set; }
    int SelbarSequence { get; set; }
    DateTime LastVisitDate { get; set; }
}
