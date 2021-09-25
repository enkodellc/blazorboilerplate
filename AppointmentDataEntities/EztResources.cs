using System;

public class EztResources
{
    public Guid ResourceGuid { get; set; }
    public string Name { get; set; }
    public string PracticeVid { get; set; }
    public Guid DeleteGuid { get; set; }
    public string Description { get; set; }
    public Guid GroupGuid { get; set; }
    public byte[] Icon { get; set; }
    public string Alignment { get; set; }
    public Guid? EmployeeGuid { get; set; }
    public bool Deleted { get; set; }
    public int Sequence { get; set; }
}
