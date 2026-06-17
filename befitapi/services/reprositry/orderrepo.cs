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
    public class OrderRepository : GenericRepository<order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _dbSet
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Invoice)
                .ToListAsync();
        }
    }
}

