using BlazorBoilerplate.Shared.Interfaces;
using MatBlazor;

namespace BlazorBoilerplate.Theme.Material.Services
{
    public class FileUploadEntry : IFileUploadEntry
    {
        private readonly IMatFileUploadEntry fileUploadEntry;
        public FileUploadEntry(IMatFileUploadEntry fileUploadEntry)
        {
            this.fileUploadEntry = fileUploadEntry;
        }
        public string Name => fileUploadEntry.Name;

        public Task WriteToStreamAsync(Stream stream)
        {
            return fileUploadEntry.WriteToStreamAsync(stream);
        }
    }
}
