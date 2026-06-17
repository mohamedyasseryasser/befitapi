
using befitapi.models;
 
namespace befitapi.services.interfaces
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart> GetCartByUserIdAsync(string userId);
    }
}