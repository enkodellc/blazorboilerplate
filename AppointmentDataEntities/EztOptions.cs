using System;

public class EztOptions
{
    public Guid EztOptGuid { get; set; }
    public string PracticeVid { get; set; }
    public string Firstday { get; set; }
    public bool OpidReq { get; set; }
    public bool OverlapOk { get; set; }
    public bool AccountReq { get; set; }
    public string PrintSpeed { get; set; }
    public string PrintDensity { get; set; }
    public string Numcopies { get; set; }
    public string Header1 { get; set; }
    public string Header2 { get; set; }
    public string Header3 { get; set; }
    public string Footer { get; set; }
    public Guid DefaultStatusType { get; set; }
    public Guid MoveStatusType { get; set; }
    public bool DateOutaRangeWarn { get; set; }
    public bool HideResources { get; set; }
    public byte ApptBlockingOption { get; set; }
    public Guid CensusCheckinStatusType { get; set; }
    public bool EnableDragDrop { get; set; }
}
