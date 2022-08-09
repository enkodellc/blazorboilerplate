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

            using var newFileStream = fileUploadEntry.OpenReadStream(104857600);

            int bytesRead;
            double totalRead = 0;

            while ((bytesRead = await newFileStream.ReadAsync(buffer)) != 0)
            {
                totalRead += bytesRead;
                await stream.WriteAsync(buffer, 0, bytesRead);
            }
        }
    }
}
