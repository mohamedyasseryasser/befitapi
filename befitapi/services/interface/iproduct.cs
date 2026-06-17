using befitapi.models;
using befitapi.services.interfaces;
using befitapi.models;

namespace befitapi.Interfaces
{
    public interface IProductRepository : IGenericRepository<product>
    {
        public Task<IEnumerable<product>> GetProductsByCategoryAsync(int categoryId);

    }
}
