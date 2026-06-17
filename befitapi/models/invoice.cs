using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace befitapi.models
{
    public class invoice
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        public order Order { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountDue { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } // e.g., Pending, Paid, Cancelled

        [MaxLength(200)]
        public string PaymentMethod { get; set; }

        public DateTime? PaymentDate { get; set; }
    }
}
