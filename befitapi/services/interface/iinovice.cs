using befitapi.models;
using befitapi.services.interfaces;
using befitapi.models;

namespace befitapi.Interfaces
{
    public interface IInvoiceRepository : IGenericRepository<invoice>
    {
        Task<invoice> GetInvoiceByOrderIdAsync(int orderId);
    }
}
