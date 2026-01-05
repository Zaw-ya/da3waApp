using Da3wa.Domain.Entities;

namespace Da3wa.Application.Interfaces
{
    public interface IGuestService
    {
        Task<IEnumerable<Guest>> GetAllAsync();
        Task<Guest?> GetByIdAsync(int id);
        Task<Guest> CreateAsync(Guest guest);
        Task UpdateAsync(Guest guest);
        Task DeleteAsync(int id);
        Task ToggleDeleteAsync(int id);
        Task<List<Guest>> ImportFromVcfAsync(Stream vcfStream, int eventId);
        Task<List<Guest>> ParseVcfAsync(Stream vcfStream, int eventId);
        Task<int> CreateManyAsync(List<Guest> guests);
        Task<bool> ConfirmAttendanceAsync(int guestId);
    }
}
