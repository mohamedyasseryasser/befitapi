using befitapi.dto;
using befitapi.Interfaces;
using befitapi.models;
using befitapi.services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace befitapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly UserManager<Applicationuser> _userManager;

        public OrderController(IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IInvoiceRepository invoiceRepository, 
            UserManager<Applicationuser> userManager)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _invoiceRepository = invoiceRepository;
            _userManager = userManager;
        }

        private async Task<string> GetCurrentUserId()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Seller,Customer")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var userId = await GetCurrentUserId();
            var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

            IEnumerable<order> orders;
            if (userRoles.Contains("Admin") || userRoles.Contains("Seller"))
            {
                orders = await _orderRepository.GetAllAsync();
            }
            else
            {
                orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            }

            var orderDtos = new List<OrderDto>();
            foreach (var order in orders)
            {
                var orderItemsDto = new List<OrderItemDto>();
                foreach (var item in order.OrderItems)
                {
                    orderItemsDto.Add(new OrderItemDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });
                }

                var invoiceDto = order.Invoice != null ? new InvoiceDto
                {
                    Id = order.Invoice.Id,
                    OrderId = order.Invoice.OrderId,
                    InvoiceDate = order.Invoice.InvoiceDate,
                    AmountDue = order.Invoice.AmountDue,
                    Status = order.Invoice.Status,
                    PaymentMethod = order.Invoice.PaymentMethod,
                    PaymentDate = order.Invoice.PaymentDate
                } : null;

                orderDtos.Add(new OrderDto
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    OrderItems = orderItemsDto,
                    Invoice = invoiceDto
                });
            }

            return Ok(orderDtos);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Seller,Customer")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var userId = await GetCurrentUserId();
            var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));

            if (!(userRoles.Contains("Admin") || userRoles.Contains("Seller")) && order.UserId != userId)
            {
                return Forbid();
            }

            var orderItemsDto = new List<OrderItemDto>();
            foreach (var item in order.OrderItems)
            {
                orderItemsDto.Add(new OrderItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            var invoiceDto = order.Invoice != null ? new InvoiceDto
            {
                Id = order.Invoice.Id,
                OrderId = order.Invoice.OrderId,
                InvoiceDate = order.Invoice.InvoiceDate,
                AmountDue = order.Invoice.AmountDue,
                Status = order.Invoice.Status,
                PaymentMethod = order.Invoice.PaymentMethod,
                PaymentDate = order.Invoice.PaymentDate
            } : null;

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderItems = orderItemsDto,
                Invoice = invoiceDto
            };

            return Ok(orderDto);
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<OrderDto>> PlaceOrder()
        {
            var userId = await GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.CartItems.Any())
            {
                return BadRequest("Cart is empty.");
            }

            var order = new order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                OrderItems = new List<orderitems>()
            };

            decimal totalAmount = 0;
            foreach (var cartItem in cart.CartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product == null || product.Stock < cartItem.Quantity)
                {
                    return BadRequest($"Product {cartItem.Product.Name} is out of stock or quantity not available.");
                }

                order.OrderItems.Add(new orderitems
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Price
                });
                totalAmount += cartItem.Price;

                // Reduce product stock
                product.Stock -= cartItem.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            order.TotalAmount = totalAmount;
            await _orderRepository.AddAsync(order);

            // Clear the cart after placing the order
            cart.CartItems.Clear();
            await _cartRepository.UpdateAsync(cart);

            // Create invoice
            var invoice = new invoice
            {
                OrderId = order.Id,
                InvoiceDate = DateTime.UtcNow,
                AmountDue = order.TotalAmount,
                Status = "Pending",
                PaymentMethod = "N/A" // This can be updated later
            };
            await _invoiceRepository.AddAsync(invoice);

            var orderItemsDto = new List<OrderItemDto>();
            foreach (var item in order.OrderItems)
            {
                orderItemsDto.Add(new OrderItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = (await _productRepository.GetByIdAsync(item.ProductId)).Name,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            var invoiceDto = new InvoiceDto
            {
                Id = invoice.Id,
                OrderId = invoice.OrderId,
                InvoiceDate = invoice.InvoiceDate,
                AmountDue = invoice.AmountDue,
                Status = invoice.Status,
                PaymentMethod = invoice.PaymentMethod,
                PaymentDate = invoice.PaymentDate
            };

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderItems = orderItemsDto,
                Invoice = invoiceDto
            };

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, orderDto);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromQuery] string status)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            await _orderRepository.UpdateAsync(order);

            return NoContent();
        }
    }
}
