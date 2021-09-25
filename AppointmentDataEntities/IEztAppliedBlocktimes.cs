using System;

public interface IEztAppliedBlocktimes
{
    Guid BtAppliedGuid { get; set; }
    Guid BlocktimeGuid { get; set; }
    Guid ResourceGuid { get; set; }
    string PracticeVid { get; set; }
    DateTime AppliedDay { get; set; }
    DateTime StartTime { get; set; }
    DateTime EndTime { get; set; }
    string WorkFlag { get; set; }
}
