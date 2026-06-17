using befitapi.Data;
using befitapi.Interfaces;
using befitapi.models;
using befitapi.services.reprositry;
using befitapi.Data;
using befitapi.Interfaces;
using befitapi.models;
using Microsoft.EntityFrameworkCore;

namespace befitapi.Repositories
{
    public class ProductRepository : GenericRepository<product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _dbSet.Where(p => p.CategoryId == categoryId).ToListAsync();
        }
    }
}
