using System;

public interface IEztBlockTimes
{
    Guid BlocktimeGuid { get; set; }
    string Name { get; set; }
    Guid DeleteGuid { get; set; }
    string PracticeVid { get; set; }
    string Description { get; set; }
    int Color { get; set; }
    DateTime StartTime { get; set; }
    DateTime EndTime { get; set; }
    bool Deleted { get; set; }
}
