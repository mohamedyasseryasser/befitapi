using System.ComponentModel.DataAnnotations;

namespace befitapi.dto
{

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
    public class CreateRoleDto
    {
        public string RoleName { get; set; }
    }
}
