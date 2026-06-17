using befitapi.Data;
using befitapi.models;
using befitapi.services.interfaces;
using befitapi.services.reprositry;
using befitapi.Data;
using befitapi.Interfaces;
using befitapi.models;
using Microsoft.EntityFrameworkCore;

namespace befitapi.Repositories
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Cart> GetCartByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }
    }
}