using System;

public interface IEztOptionFields
{
    Guid EzOpFldGuid { get; set; }
    string PracticeVid { get; set; }
    string SourceTable { get; set; }
    string FieldName { get; set; }
    string InsertOrder { get; set; }
}
