using Da3wa.Application.Interfaces;
using Da3wa.Application.Interfaces.Repositories;
using Da3wa.Domain.Entities;

namespace Da3wa.Application.Services
{
    public class CountryService : ICountryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CountryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Country>> GetAllAsync()
        {
            return await _unitOfWork.Countries.GetAll();
        }



        public async Task<Country?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Countries.GetById(id);
        }

        public async Task<Country> CreateAsync(Country country)
        {
            country.CreatedOn = DateTime.Now;
            country.IsDeleted = false;
            var addedCountry = await _unitOfWork.Countries.Add(country);
            _unitOfWork.Complete();
            return addedCountry;
        }

        public async Task UpdateAsync(Country country)
        {
            country.LastUpdatedOn = DateTime.UtcNow;
            _unitOfWork.Countries.Update(country);
            _unitOfWork.Complete();
        }

        public async Task DeleteAsync(int id)
        {
            var country = await _unitOfWork.Countries.GetById(id);
            if (country != null && !country.IsDeleted)
            {
                country.IsDeleted = true;
                country.LastUpdatedOn = DateTime.UtcNow;
                _unitOfWork.Countries.Update(country);
                _unitOfWork.Complete();
            }
        }

        public async Task ToggleDeleteAsync(int id)
        {
            var country = await _unitOfWork.Countries.GetById(id);
            if (country != null)
            {
                country.IsDeleted = !country.IsDeleted;
                country.LastUpdatedOn = DateTime.UtcNow;
                _unitOfWork.Countries.Update(country);
                _unitOfWork.Complete();
            }
        }
    }
}
