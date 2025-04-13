using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories;

namespace E_Commerce_Platform.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository _categoryRepo;

        public CategoryService(CategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepo.GetAllCategoriesAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepo.GetByIdAsync(id);
        }

        public async Task AddCategoryAsync(Category category)
        {
            await _categoryRepo.AddCategoryAsync(category);
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            var existingCategory = await _categoryRepo.GetByIdAsync(category.Id);
            if (existingCategory == null) return false;

            existingCategory.Name = category.Name;
            await _categoryRepo.UpdateAsync(existingCategory);
            return true;
        }

        public async Task<List<Category>> SortAsync(string sortOption)
        {
            return await _categoryRepo.SortAsync(sortOption);
        }

        public async Task<List<Category>> SearchAsync(string keyword)
        {
            return await _categoryRepo.SearchAsync(keyword);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                return await _categoryRepo.DeleteAsync(id);
            }
            catch (KeyNotFoundException ex)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Cannot delete category because it is associated with products. Please reassign or remove products before deleting.");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An unexpected error occurred while deleting the category.", ex);
            }
        }
    }
}