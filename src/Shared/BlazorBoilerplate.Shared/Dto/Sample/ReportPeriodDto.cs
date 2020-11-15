﻿using System;

namespace BlazorBoilerplate.Shared.Dto.Sample
{
    public class ReportPeriodDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsBenchmark { get; set; }
        public bool IsComparison { get; set; }

        public string Title
        {
            get
            {
                return $"{StartDate:MM/dd/yy} - {EndDate:MM/dd/yy}";
            }
        }
    }
}