using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace befitapi.models
{
    public class order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public Applicationuser User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } // e.g., Pending, Processing, Shipped, Delivered, Cancelled

        public ICollection<orderitems> OrderItems { get; set; }
        public invoice Invoice { get; set; }
    }
}
