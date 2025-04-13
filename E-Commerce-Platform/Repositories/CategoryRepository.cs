using E_Commerce_Platform.EF;
using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories.Repo;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace E_Commerce_Platform.Repositories
{
    public class CategoryRepository : Repository<Category>
    {
        private readonly AppContextDB _cntx;

        public CategoryRepository(AppContextDB context) : base(context)
        {
            _cntx = context;
        }

        public async Task<Category> GetCategoryWithProductsAsync(int categoryId)
        {
            return await _cntx.categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _cntx.categories.ToListAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _cntx.categories.Update(category);
            await _cntx.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int categoryId)
        {
            var hasProducts = await _cntx.products.AnyAsync(p => p.CategoryId == categoryId);
            if (hasProducts)
            {
                throw new InvalidOperationException("Cannot delete category because it has associated products.");
            }

            var category = await _cntx.categories.FindAsync(categoryId);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            _cntx.categories.Remove(category);
            await _cntx.SaveChangesAsync();
            return true;
        }

        public async Task AddCategoryAsync(Category category)
        {
            await _cntx.AddAsync(category);
            await _cntx.SaveChangesAsync();
        }

        public async Task<List<Category>> SortAsync(string sortOption)
        {
            var categories = _cntx.categories.AsQueryable();

            categories = sortOption switch
            {
                "Id" => categories.OrderBy(c => c.Id),
                "Name" => categories.OrderBy(c => c.Name),
                _ => categories
            };
            return await categories.ToListAsync();
        }

        public async Task<List<Category>> SearchAsync(string keyword)
        {
            var categories = await (keyword.IsNullOrEmpty() ? _cntx.categories.ToListAsync() : _cntx.categories.Where(c => c.Name.Contains(keyword)).ToListAsync());
            return categories;
        }
    }
}