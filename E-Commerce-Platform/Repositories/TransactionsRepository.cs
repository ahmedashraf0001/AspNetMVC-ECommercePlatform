using E_Commerce_Platform.EF;
using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories.Repo;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Platform.Repositories
{
    public class TransactionsRepository : Repository<Transactions>
    {
        private AppContextDB _context;

        public TransactionsRepository(AppContextDB context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Transactions>> GetAllTransactionsByUserId(string userId)
        {
            return await _context.transactions.Include(e => e.User).Include(e => e.Order).ThenInclude(e => e.OrderDetails).ThenInclude(e => e.Product).IgnoreQueryFilters().Where(t => t.UserId == userId).ToListAsync();
        }

        public async Task<List<Transactions>> GetAllTransactionsByOrderId(int orderId)
        {
            return await _context.transactions.Include(e => e.User).Include(e => e.Order).ThenInclude(e => e.OrderDetails).ThenInclude(e => e.Product).IgnoreQueryFilters().Where(t => t.OrderId == orderId).ToListAsync();
        }

        public async Task<Transactions> GetTransactionAsync(int TransId)
        {
            return await _context.transactions.Include(e => e.User).Include(e => e.Order).ThenInclude(e => e.OrderDetails).ThenInclude(e => e.Product).IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == TransId);
        }

        public async Task<List<Transactions>> GetAllTransactionsAsync()
        {
            return await _context.transactions.Include(e => e.User).Include(e => e.Order).ThenInclude(e => e.OrderDetails).ThenInclude(e => e.Product).IgnoreQueryFilters().ToListAsync();
        }

        public async Task AddTransaction(Transactions order)
        {
            await _context.transactions.AddAsync(order);
            await SaveAsync();
        }
        public IQueryable<Transactions> GetTransactionsQuery()
        {
            return _context.transactions.AsQueryable();
        }
    }
}