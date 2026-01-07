namespace Da3wa.Application.Interfaces
{
    public interface IDocumentProcessingService
    {
        Task<string> ProcessTemplateAndConvertToPngAsync(string templateFilePath, string bridegroomName, string brideName, string eventName, string outputFolder);
    }
}
