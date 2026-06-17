using System.ComponentModel.DataAnnotations;

namespace befitapi.models
{
    public class Cart
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public Applicationuser User { get; set; }

        public ICollection<Cartitems> CartItems { get; set; }
    }
}
