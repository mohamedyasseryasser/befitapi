using System.ComponentModel.DataAnnotations;

namespace befitapi.dto
{
    public class CreateProductDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Stock { get; set; }

        public IFormFile ImageFile { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}
