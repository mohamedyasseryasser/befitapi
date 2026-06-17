using befitapi.dto;
using befitapi.Interfaces;
using befitapi.models;
using befitapi.services.interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace befitapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly UserManager<Applicationuser> _userManager;

        public CartController(ICartRepository cartRepository, IProductRepository productRepository, UserManager<Applicationuser> userManager)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _userManager = userManager;
        }

        private async Task<string> GetCurrentUserId()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id;
        }

        [HttpGet]
        public async Task<ActionResult<CartDto>> GetUserCart()
        {
            var userId = await GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                // Create a new cart if one doesn't exist for the user
                cart = new Cart { UserId = userId };
                await _cartRepository.AddAsync(cart);
            }

            var cartDto = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CartItems = cart.CartItems?.Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Quantity = ci.Quantity,
                    Price = ci.Price
                }).ToList()
            };

            return Ok(cartDto);
        }

        [HttpPost("add-item")]
        public async Task<IActionResult> AddItemToCart([FromBody] AddCartItemDto addCartItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = await GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var product = await _productRepository.GetByIdAsync(addCartItemDto.ProductId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _cartRepository.AddAsync(cart);
            }

            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == addCartItemDto.ProductId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += addCartItemDto.Quantity;
                existingCartItem.Price = product.Price * existingCartItem.Quantity; // Update price based on new quantity
                await _cartRepository.UpdateAsync(cart); // Update the cart to save changes to cart item
            }
            else
            {
                var cartItem = new Cartitems
                {
                    ProductId = addCartItemDto.ProductId,
                    CartId = cart.Id,
                    Quantity = addCartItemDto.Quantity,
                    Price = product.Price * addCartItemDto.Quantity
                };
                cart.CartItems.Add(cartItem);
                await _cartRepository.UpdateAsync(cart); // Update the cart to add new cart item
            }

            return Ok(new { Message = "Item added to cart successfully." });
        }

        [HttpPut("update-item-quantity/{productId}")]
        public async Task<IActionResult> UpdateCartItemQuantity(int productId, [FromQuery] int quantity)
        {
            var userId = await GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                return NotFound("Cart not found.");
            }

            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (existingCartItem == null)
            {
                return NotFound("Cart item not found.");
            }

            if (quantity <= 0)
            {
                cart.CartItems.Remove(existingCartItem);
            }
            else
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }
                existingCartItem.Quantity = quantity;
                existingCartItem.Price = product.Price * quantity;
            }
            await _cartRepository.UpdateAsync(cart);

            return NoContent();
        }

        [HttpDelete("remove-item/{productId}")]
        public async Task<IActionResult> RemoveItemFromCart(int productId)
        {
            var userId = await GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                return NotFound("Cart not found.");
            }

            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (existingCartItem == null)
            {
                return NotFound("Cart item not found.");
            }

            cart.CartItems.Remove(existingCartItem);
            await _cartRepository.UpdateAsync(cart);

            return NoContent();
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = await GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                return NotFound("Cart not found.");
            }

            cart.CartItems.Clear();
            await _cartRepository.UpdateAsync(cart);

            return NoContent();
        }
    }
}
