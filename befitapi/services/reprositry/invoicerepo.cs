using befitapi.Data;
using befitapi.Interfaces;
using befitapi.models;
using befitapi.services.reprositry;
using befitapi.Data;
using befitapi.Interfaces;
using befitapi.models;
using Microsoft.EntityFrameworkCore;

namespace BefitAPI.Repositories
{
    public class InvoiceRepository : GenericRepository<invoice>, IInvoiceRepository
    {
        public InvoiceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<invoice> GetInvoiceByOrderIdAsync(int orderId)
        {
            return await _dbSet.FirstOrDefaultAsync(i => i.OrderId == orderId);
        }
    }
}