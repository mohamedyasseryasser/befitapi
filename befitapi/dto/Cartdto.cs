using System.ComponentModel.DataAnnotations;

namespace befitapi.dto
{
    public class CartDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ICollection<CartItemDto> CartItems { get; set; }
    }
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
    public class AddCartItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
