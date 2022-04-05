using BlazorBoilerplate.Shared.Interfaces;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBoilerplate.Theme.Material.Services
{
    public class FileUploadEntry : IFileUploadEntry
    {
        private readonly IBrowserFile fileUploadEntry;
        public FileUploadEntry(IBrowserFile fileUploadEntry)
        {
            this.fileUploadEntry = fileUploadEntry;
        }
        public string Name => fileUploadEntry.Name;

        public async Task WriteToStreamAsync(Stream stream)
        {
            var buffer = new byte[fileUploadEntry.Size];

            await fileUploadEntry.OpenReadStream(104857600).ReadAsync(buffer);

            await stream.WriteAsync(buffer);
        }
    }
}
