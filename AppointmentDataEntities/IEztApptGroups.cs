using System;

public interface IEztApptGroups
{
    Guid AptGroupGuid { get; set; }
    string PracticeVid { get; set; }
    string Comments { get; set; }
    string CreatePracticeVid { get; set; }
}
