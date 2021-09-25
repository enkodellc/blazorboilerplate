using System;

public interface IEztBlockDays
{
    Guid BlockdayGuid { get; set; }
    string Name { get; set; }
    Guid DeleteGuid { get; set; }
    int Color { get; set; }
    string Description { get; set; }
    string PracticeVid { get; set; }
    bool Deleted { get; set; }
}
