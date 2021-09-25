using System;

public interface IEztLabelComments
{
    Guid CommentGuid { get; set; }
    string PracticeVid { get; set; }
    string Comment { get; set; }
}
