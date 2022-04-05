namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface IFileUploadEntry
    {
        string Name { get; }
        Task WriteToStreamAsync(Stream stream);
    }
}
