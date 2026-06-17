using befitapi.Data;
using befitapi.Interfaces;
using befitapi.models;
using befitapi.services.interfaces;
using befitapi.Repositories;
using befitapi.services.reprositry;

namespace befitapi.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}