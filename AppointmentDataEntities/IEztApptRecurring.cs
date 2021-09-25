using System;

public interface IEztApptRecurring
{
    Guid RecurringGuid { get; set; }
    string PracticeVid { get; set; }
    string SeqType { get; set; }
    string HowOften { get; set; }
    DateTime StartDate { get; set; }
    DateTime EndDate { get; set; }
    string Comments { get; set; }
    string CreatePracticeVid { get; set; }
}
