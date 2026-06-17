using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace befitapi.models
{
    public class Cartitems
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public product Product { get; set; }

        [Required]
        public int CartId { get; set; }
        public Cart Cart { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
