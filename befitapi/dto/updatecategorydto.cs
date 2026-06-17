using System.ComponentModel.DataAnnotations;

namespace befitapi.dto
{
    public class UpdateCategoryDto
    {
        [Required]
        public int Id { get; set; }
        public bool isvalid {  get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
}
