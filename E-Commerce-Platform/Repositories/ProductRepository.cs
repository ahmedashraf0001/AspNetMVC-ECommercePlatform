using E_Commerce_Platform.EF;
using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories.Repo;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Platform.Repositories
{
    public class ProductRepository : Repository<Product>
    {
        private AppContextDB _context;

        public ProductRepository(AppContextDB context) : base(context)
        {
            _context = context;
        }

        public async Task<Product> GetProductByIdAsync(int productId, bool withDeletes, bool withIncludes = true)
        {
            IQueryable<Product> query = _context.products;

            if (withIncludes)
            {
                query = query.Include(e => e.Category)
                             .Include(e => e.OrderDetails)
                             .ThenInclude(e => e.Order)
                             .ThenInclude(e => e.Transaction);
            }

            if (withDeletes)
            {
                query = query.IgnoreQueryFilters();
            }

            return await query.FirstOrDefaultAsync(e => e.Id == productId);
        }

        public async Task<List<Product>> TakeProductsByCategories(int categoryId, bool withDeletes, int take, bool withIncludes)
        {
            IQueryable<Product> query = _context.products;

            if (withDeletes)
                query = query.IgnoreQueryFilters();

            query = query.Where(p => p.CategoryId == categoryId);

            if (withIncludes)
            {
                query = query.Include(p => p.Category)
                             .Include(p => p.OrderDetails)
                             .ThenInclude(od => od.Order)
                             .ThenInclude(o => o.Transaction);
            }

            query = query.AsNoTracking();

            if (take > 0)
                query = query.Take(take);

            return await query.ToListAsync();
        }

        public async Task<int> CountProductsAsync(bool withDeletes)
        {
            if (withDeletes)
            {
                return await _context.products.IgnoreQueryFilters().CountAsync();
            }
            else
            {
                return await _context.products.CountAsync();
            }
        }

        public IQueryable<Product> GetProductQuery()
        {
            return _context.products.AsQueryable();
        }

        public List<Product> ApplyPagination(int page, int pageSize)
        {
            return _context.products.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        public async Task<List<Product>> GetProductsAsync(bool withDeletes, bool withIncludes = true)
        {
            IQueryable<Product> query = _context.products;

            if (withIncludes)
            {
                query = query.Include(e => e.Category)
                             .Include(e => e.OrderDetails)
                             .ThenInclude(e => e.Order)
                             .ThenInclude(e => e.Transaction);
            }

            if (withDeletes)
            {
                query = query.IgnoreQueryFilters();
            }

            return await query.ToListAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId, bool withDeletes, bool withIncludes = true)
        {
            IQueryable<Product> query = _context.products.Where(p => p.CategoryId == categoryId);

            if (withIncludes)
            {
                query = query.Include(e => e.Category)
                             .Include(e => e.OrderDetails)
                             .ThenInclude(e => e.Order)
                             .ThenInclude(e => e.Transaction);
            }

            if (withDeletes)
            {
                query = query.IgnoreQueryFilters();
            }

            return await query.ToListAsync();
        }

        public async Task<List<Category>> GetCategories()
        {
            return await _context.categories.ToListAsync();
        }

        public void Remove(Product entity)
        {
            if (entity != null)
            {
                entity.IsDeleted = true;
                _context.products.Remove(entity);
            }
        }

        public void Disable(Product entity)
        {
            if (entity != null)
            {
                entity.IsDeleted = true;
                _context.products.Update(entity);
            }
        }

        public void Enable(Product entity)
        {
            if (entity != null)
            {
                entity.IsDeleted = false;
                _context.products.Update(entity);
            }
        }
    }
}