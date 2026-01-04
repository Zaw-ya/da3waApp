using Da3wa.Application.Interfaces;
using Da3wa.Application.Interfaces.Repositories;
using Da3wa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Da3wa.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _unitOfWork.Categories.GetQueryable().ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Categories.GetById(id);
        }

        public async Task<Category> CreateAsync(Category category)
        {
            category.CreatedOn = DateTime.Now;
            category.IsDeleted = false;
            var addedCategory = await _unitOfWork.Categories.Add(category);
            _unitOfWork.Complete();
            return addedCategory;
        }

        public async Task UpdateAsync(Category category)
        {
            category.LastUpdatedOn = DateTime.Now;
            _unitOfWork.Categories.Update(category);
            _unitOfWork.Complete();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetById(id);
            if (category != null && !category.IsDeleted)
            {
                category.IsDeleted = true;
                category.LastUpdatedOn = DateTime.Now;
                _unitOfWork.Categories.Update(category);
                _unitOfWork.Complete();
            }
        }

        public async Task ToggleDeleteAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetById(id);
            if (category != null)
            {
                category.IsDeleted = !category.IsDeleted;
                category.LastUpdatedOn = DateTime.Now;
                _unitOfWork.Categories.Update(category);
                _unitOfWork.Complete();
            }
        }
    }
}
