using System;

public class EztResourceGroups
{
    public Guid GroupGuid { get; set; }
    public string Name { get; set; }
    public Guid DeleteGuid { get; set; }
    public string Description { get; set; }
    public string PracticeVid { get; set; }
    public Guid ColorGuid { get; set; }
    public string TimeInterval { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool Deleted { get; set; }
    public int Sequence { get; set; }
    public string FrameBevel { get; set; }
    public string CaptionBevel { get; set; }
}
