using System;

public interface IEztResources
{
    Guid ResourceGuid { get; set; }
    string Name { get; set; }
    string PracticeVid { get; set; }
    Guid DeleteGuid { get; set; }
    string Description { get; set; }
    Guid GroupGuid { get; set; }
    byte[] Icon { get; set; }
    string Alignment { get; set; }
    Guid? EmployeeGuid { get; set; }
    bool Deleted { get; set; }
    int Sequence { get; set; }
}
