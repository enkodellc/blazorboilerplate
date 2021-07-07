using System.IO;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface IFileUploadEntry
    {
        string Name { get; }
        Task WriteToStreamAsync(Stream stream);
    }
}
