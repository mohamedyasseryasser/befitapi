using befitapi.models;
using befitapi.services.interfaces;
using befitapi.models;

namespace befitapi.Interfaces
{
    public interface IOrderRepository : IGenericRepository<order>
    {
        Task<IEnumerable<order>> GetOrdersByUserIdAsync(string userId);
    }
}