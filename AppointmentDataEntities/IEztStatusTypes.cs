using System;

public interface IEztStatusTypes
{
    Guid StatusGuid { get; set; }
    string Name { get; set; }
    Guid DeleteGuid { get; set; }
    string PracticeVid { get; set; }
    string Description { get; set; }
    int Color { get; set; }
    string ColorDesc { get; set; }
    bool Rpt1 { get; set; }
    bool Rpt2 { get; set; }
    bool Rpt3 { get; set; }
    bool Rpt4 { get; set; }
    bool Rpt5 { get; set; }
    bool Rpt6 { get; set; }
    bool Rpt7 { get; set; }
    bool Rpt8 { get; set; }
    bool Rpt9 { get; set; }
    bool Deleted { get; set; }
}
