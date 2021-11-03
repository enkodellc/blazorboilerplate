using System;

namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface IDateTimeFilter
    {
        DateTime? From { get; set; }
        DateTime? To { get; set; }
    }
}
