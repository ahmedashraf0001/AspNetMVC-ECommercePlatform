using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories;
using E_Commerce_Platform.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Platform.Services
{
    public class ProductService
    {
        private readonly ProductRepository Repository;
        private readonly ReviewService reviewService;
        private readonly CategoryRepository categoryRepository;

        public ProductService(ProductRepository repository, ReviewService reviewService, CategoryRepository categoryRepository)
        {
            Repository = repository;
            this.reviewService = reviewService;
            this.categoryRepository = categoryRepository;
        }

        public async Task<List<Product>> GetProductsAsync(bool withDeletes, bool withIncludes = true)
        {
            return await Repository.GetProductsAsync(withDeletes, withIncludes);
        }

        public async Task<List<Category>> GetCategories()
        {
            return await Repository.GetCategories();
        }

        public async Task<Product> GetProductByIdAsync(int productId, bool withDeletes, bool withIncludes = true)
        {
            return await Repository.GetProductByIdAsync(productId, withDeletes, withIncludes);
        }

        public async Task<ProductReviewViewModel> LoadProductPageAsync(int productId, string userId)
        {
            var product = await Repository.GetProductByIdAsync(productId, withDeletes: false, withIncludes: true);
            var reviews = (await reviewService.GetReviewsByProductIdAsync(productId)).OrderByDescending(e => e.ReviewDate).ToList();

            var likedReviewIds = string.IsNullOrEmpty(userId)
                ? new List<int>()
                : await reviewService.GetLikedReviewsByUserAsync(productId, userId);

            var model = new ProductReviewViewModel
            {
                Product = product,
                quantity = product.Stock,
                Reviews = reviews,
                avgRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0,
                RelatedProducts = await Repository.TakeProductsByCategories(product.CategoryId, withDeletes: false, 4, withIncludes: false),
                LikedReviewsIds = likedReviewIds
            };

            return model;
        }

        public async Task<int> CountProductsAsync(bool withDeletes = false)
        {
            return await Repository.CountProductsAsync(withDeletes);
        }

        public async Task<ShopViewModel> LoadProductPageAsync(int page = 1, int pageSize = 12, string search = "", string sortBy = "")
        {
            IQueryable<Product> query = Repository.GetProductQuery();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search) || p.Category.Name.Contains(search));
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortBy switch
                {
                    "Name" => query.OrderBy(p => p.Name),
                    "Price" => query.OrderBy(p => p.Price),
                    "Description" => query.OrderBy(u => u.Description),
                    "Stock" => query.OrderBy(u => u.Stock),
                    "Category" => query.OrderBy(u => u.Category.Name),
                    _ => query
                };
            }

            int totalProducts = await query.CountAsync();
            List<ProductViewModel> pagedProducts = await ApplyPagination(query, page, pageSize)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    IsDeleted = p.IsDeleted,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            var viewModel = new ShopViewModel
            {
                Products = pagedProducts,
                TotalPages = CalculateTotalPages(totalProducts, pageSize),
                CurrentPage = page,
                TotalProducts = totalProducts,
                SearchQuery = search,
                SortBy = sortBy,
                Productcategories = await categoryRepository.GetAllCategoriesAsync()
            };
            return viewModel;
        }

        public IQueryable<Product> ApplyPagination(IQueryable<Product> query, int page, int pageSize)
        {
            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        private int CalculateTotalPages(int totalProducts, int pageSize)
        {
            return (int)Math.Ceiling(totalProducts / (double)pageSize);
        }

        public async Task<List<Product>> BestSellingProducts(int n)
        {
            var products = await Repository.GetProductsAsync(withDeletes: true);
            var bestSellingProduct = products
                .OrderByDescending(p => p.OrderDetails?
                    .Where(o => o.Order?.Transaction == null || o.Order.Transaction.transactionStatus != TransStatus.Refunded)
                    .Sum(o => o.Quantity * o.Product.Price) ?? 0)
                .Take(n)
                .ToList();
            return bestSellingProduct;
        }

        public async Task<List<TopProductViewModel>> GetTopProductsAsync(int n)
        {
            var products = await Repository.GetProductsAsync(withDeletes: true);

            var topProducts = products
                .Select(p => new TopProductViewModel
                {
                    Name = p.Name,
                    Sales = p.OrderDetails?
                        .Where(o => o.Order?.Transaction == null || o.Order.Transaction.transactionStatus != TransStatus.Refunded)
                        .Sum(o => o.Quantity * o.Product.Price) ?? 0
                })
                .OrderByDescending(p => p.Sales)
                .Take(n)
                .ToList();

            return topProducts;
        }

        public async Task<IdentityResult> AddProductAsync(Product product, IFormFile ImageFile)
        {
            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Products");
                    if (!Directory.Exists(uploadsfolder))
                    {
                        Directory.CreateDirectory(uploadsfolder);
                    }

                    var uniquename = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);

                    var filepath = Path.Combine(uploadsfolder, uniquename);

                    using (var stream = new FileStream(filepath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    product.ImageUrl = "/images/Products/" + uniquename;
                }
                await Repository.AddAsync(product);
                await Repository.SaveAsync();
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error Adding Product" });
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateProductAsync(Product product, IFormFile ImageFile, string ExistingImageUrl)
        {
            try
            {
                if (product == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Product Not Found" });
                }
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Products");
                    if (!Directory.Exists(uploadsfolder))
                    {
                        Directory.CreateDirectory(uploadsfolder);
                    }

                    var uniquename = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);

                    var filepath = Path.Combine(uploadsfolder, uniquename);

                    using (var stream = new FileStream(filepath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    product.ImageUrl = "/images/Products/" + uniquename;
                }
                else if (!string.IsNullOrEmpty(ExistingImageUrl))
                {
                    product.ImageUrl = ExistingImageUrl;
                }
                else
                {
                    product.ImageUrl = "/images/main/Users/default.jpeg";
                }
                Repository.Update(product);
                await Repository.SaveAsync();
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error Updating Product" });
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> ToggleDisableProductAsync(int productId)
        {
            try
            {
                var product = await Repository.GetProductByIdAsync(productId, withDeletes: true);
                if (product == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Product not found." });
                }
                if (product.IsDeleted == false)
                {
                    Repository.Disable(product);
                }
                else
                {
                    Repository.Enable(product);
                }
                await Repository.SaveAsync();
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error Deleting Product" });
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveProductAsync(int productId)
        {
            try
            {
                var product = await Repository.GetProductByIdAsync(productId, withDeletes: false);
                if (product == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Product not found." });
                }
                Repository.Remove(product);
                await Repository.SaveAsync();
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error Deleting Product" });
            }
            return IdentityResult.Success;
        }
    }
}