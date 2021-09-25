using System;

public interface IEztOptions
{
    Guid EztOptGuid { get; set; }
    string PracticeVid { get; set; }
    string Firstday { get; set; }
    bool OpidReq { get; set; }
    bool OverlapOk { get; set; }
    bool AccountReq { get; set; }
    string PrintSpeed { get; set; }
    string PrintDensity { get; set; }
    string Numcopies { get; set; }
    string Header1 { get; set; }
    string Header2 { get; set; }
    string Header3 { get; set; }
    string Footer { get; set; }
    Guid DefaultStatusType { get; set; }
    Guid MoveStatusType { get; set; }
    bool DateOutaRangeWarn { get; set; }
    bool HideResources { get; set; }
    byte ApptBlockingOption { get; set; }
    Guid CensusCheckinStatusType { get; set; }
    bool EnableDragDrop { get; set; }
}
