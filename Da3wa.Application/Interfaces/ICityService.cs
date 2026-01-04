using Da3wa.Domain.Entities;

namespace Da3wa.Application.Interfaces
{
    public interface ICityService
    {
        Task<IEnumerable<City>> GetAllAsync();
        Task<City?> GetByIdAsync(int id);
        Task<City> CreateAsync(City city);
        Task UpdateAsync(City city);
        Task DeleteAsync(int id);
        Task ToggleDeleteAsync(int id);
    }
}
