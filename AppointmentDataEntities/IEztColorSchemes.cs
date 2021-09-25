using System;

public interface IEztColorSchemes
{
    Guid ColorGuid { get; set; }
    string Name { get; set; }
    string PracticeVid { get; set; }
    int BevelFaceColor { get; set; }
    int BevelFrameColor { get; set; }
    int BevelHighlightColor { get; set; }
    int BackColor { get; set; }
    int BackColorSelected { get; set; }
    int DurationFillColor { get; set; }
    int EditBackColor { get; set; }
    int EditForeColor { get; set; }
    int ForeColor { get; set; }
    int ForeColorSelected { get; set; }
    string DurationFillPattern { get; set; }
  
}
