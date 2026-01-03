namespace Da3wa.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string subject, string body);
    }
}
