using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories;
using E_Commerce_Platform.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Platform.Services
{
    public class TransactionService
    {
        private readonly TransactionsRepository Repository;

        public TransactionService(TransactionsRepository transactionsRepository)
        {
            Repository = transactionsRepository;
        }
        public async Task<TransactionPageViewModel> LoadTransactionPageAsync(
    int page = 1,
    int pageSize = 12,
    string search = "",
    string sortBy = "")
        {
            IQueryable<Transactions> query = Repository.GetTransactionsQuery().Include(e => e.User);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => e.Id.ToString().Contains(search)
                                        || e.User.FullName.ToLower().Contains(search)
                                        || e.OrderId.ToString().Contains(search)
                                        || e.TotalCost.ToString().Contains(search)
                                        || e.paymentMethod.ToString().ToLower().Contains(search)
                                        || e.transactionStatus.ToString().ToLower().Contains(search)
                                        || e.TransactionDate.ToString().Contains(search));
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortBy switch
                {
                    "Id" => query.OrderBy(u => u.Id),
                    "FullName" => query.OrderBy(u => u.User.FullName),
                    "OrderId" => query.OrderBy(u => u.OrderId),
                    "TotalCost" => query.OrderBy(u => u.TotalCost),
                    "PaymentMethod" => query.OrderBy(u => u.paymentMethod),
                    "TransactionDate" => query.OrderBy(u => u.TransactionDate),
                    "Status" => query.OrderBy(u => u.transactionStatus),
                    _ => query.OrderByDescending(u => u.TransactionDate)
                };
            }

            int totalTransactions = await query.CountAsync();
            List<Transactions> pagedTransactions = await ApplyPagination(query, page, pageSize).ToListAsync();

            return new TransactionPageViewModel
            {
                Transactions = pagedTransactions,
                TotalPages = CalculateTotalPages(totalTransactions, pageSize),
                CurrentPage = page,
                TotalTransactions = totalTransactions,
                SearchQuery = search,
                SortBy = sortBy
            };
        }

        public IQueryable<Transactions> ApplyPagination(IQueryable<Transactions> query, int page, int pageSize)
        {
            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        private int CalculateTotalPages(int totalTransactions, int pageSize)
        {
            return (int)Math.Ceiling(totalTransactions / (double)pageSize);
        }

        public async Task<Transactions> GetTransactionAsync(int transId)
        {
            return await Repository.GetTransactionAsync(transId);
        }

        public async Task<List<Transactions>> GetAllTransactionsAsync()
        {
            return await Repository.GetAllTransactionsAsync();
        }
        public async Task<IdentityResult> RefundAsync(int transId)
        {
            try
            {
                Transactions model = await Repository.GetTransactionAsync(transId);
                if (model == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Transaction not found." });
                }
                if (model.transactionStatus == TransStatus.Refunded)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = "Transaction is already refunded."
                    });
                }

                if (model.transactionStatus != TransStatus.Completed)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = "Only completed transactions can be refunded."
                    });
                }
                model.transactionStatus = TransStatus.Refunded;
                foreach (var item in model.Order.OrderDetails)
                {
                    item.Product.Stock += item.Quantity;
                }
                Repository.Update(model);
                await Repository.SaveAsync();
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Error Updating Status!" });
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AddTransactionAsync(Transactions order)
        {
            try
            {
                await Repository.AddTransaction(order);
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error Adding Transaction" });
            }
            return IdentityResult.Success;
        }
    }
}