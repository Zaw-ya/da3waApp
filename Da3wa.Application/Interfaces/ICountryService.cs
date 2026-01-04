using Da3wa.Domain.Entities;

namespace Da3wa.Application.Interfaces
{
    public interface ICountryService
    {
        Task<IEnumerable<Country>> GetAllAsync();
        Task<Country?> GetByIdAsync(int id);
        Task<Country> CreateAsync(Country country);
        Task UpdateAsync(Country country);
        Task DeleteAsync(int id);
        Task ToggleDeleteAsync(int id);
    }
}
