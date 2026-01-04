using Da3wa.Domain.Entities;

namespace Da3wa.Application.Interfaces
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetAllAsync();
        Task<Event?> GetByIdAsync(int id);
        Task<Event> CreateAsync(Event @event);
        Task UpdateAsync(Event @event);
        Task DeleteAsync(int id);
        Task ToggleDeleteAsync(int id);
    }
}
