using System;

public interface IEztResourceGroups
{
    Guid GroupGuid { get; set; }
    string Name { get; set; }
    Guid DeleteGuid { get; set; }
    string Description { get; set; }
    string PracticeVid { get; set; }
    Guid ColorGuid { get; set; }
    string TimeInterval { get; set; }
    DateTime StartTime { get; set; }
    DateTime EndTime { get; set; }
    bool Deleted { get; set; }
    int Sequence { get; set; }
    string FrameBevel { get; set; }
    string CaptionBevel { get; set; }
}
