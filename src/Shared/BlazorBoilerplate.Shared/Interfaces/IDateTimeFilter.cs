using System.ComponentModel;

namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface IDateTimeFilter : INotifyPropertyChanged
    {
        DateTime? From { get; set; }
        DateTime? To { get; set; }
    }
}
