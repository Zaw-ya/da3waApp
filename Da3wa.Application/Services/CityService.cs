using Da3wa.Application.Interfaces;
using Da3wa.Application.Interfaces.Repositories;
using Da3wa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Da3wa.Application.Services
{
    public class CityService : ICityService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<City>> GetAllAsync()
        {
            return await _unitOfWork.Cities.GetQueryable().Include(c=>c.Country).ToListAsync();
        }

        public async Task<City?> GetByIdAsync(int id)
        {
            var city = await _unitOfWork.Cities.GetById(id);
            return city != null ? city : null;
        }

        public async Task<City> CreateAsync(City city)
        {
            city.CreatedOn = DateTime.Now;
            city.IsDeleted = false;
            var addedCity = await _unitOfWork.Cities.Add(city);
            _unitOfWork.Complete();
            return addedCity;
        }

        public async Task UpdateAsync(City city)
        {
            city.LastUpdatedOn = DateTime.Now;
            _unitOfWork.Cities.Update(city);
            _unitOfWork.Complete();
        }

        public async Task DeleteAsync(int id)
        {
            var city = await _unitOfWork.Cities.GetById(id);
            if (city != null && !city.IsDeleted)
            {
                city.IsDeleted = true;
                city.LastUpdatedOn = DateTime.Now;
                _unitOfWork.Cities.Update(city);
                _unitOfWork.Complete();
            }
        }

        public async Task ToggleDeleteAsync(int id)
        {
            var city = await _unitOfWork.Cities.GetById(id);
            if (city != null)
            {
                city.IsDeleted = !city.IsDeleted;
                city.LastUpdatedOn = DateTime.Now;
                _unitOfWork.Cities.Update(city);
                _unitOfWork.Complete();
            }
        }
    }
}
