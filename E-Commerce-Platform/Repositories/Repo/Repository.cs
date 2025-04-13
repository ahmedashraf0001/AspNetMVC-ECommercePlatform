using E_Commerce_Platform.EF;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Platform.Repositories.Repo
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppContextDB _context;
        private readonly DbSet<T> _dbSet;

        public Repository(AppContextDB context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public virtual async Task<T> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public virtual async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }
    }
}