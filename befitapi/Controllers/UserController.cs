using befitapi.dto;
using befitapi.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace befitapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly UserManager<Applicationuser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<Applicationuser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [HttpPost("createrole")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto model)
        {
            if (string.IsNullOrEmpty(model.RoleName))
                return BadRequest("Role name is required");

            var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
            if (roleExists)
                return BadRequest("Role already exists");

            var result = await _roleManager.CreateAsync(new IdentityRole(model.RoleName));

            if (result.Succeeded)
                return Ok(new { Message = "Role created successfully" });

            return BadRequest(result.Errors);
        }
        [HttpGet("getusers")]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult<IEnumerable<Applicationuser>>> GetUsers()
        {
            var users = _userManager.Users.ToList();
            return Ok(users);
        }

        [HttpGet("getuserbyid/{id}")]
        [Authorize(Roles = "Admin,Seller,Customer")]
        public async Task<ActionResult<Applicationuser>> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

            if (!currentUserRoles.Contains("Admin") && currentUser.Id != id)
            {
                return Forbid();
            }

            return Ok(user);
        }

        [HttpPost("createuser")]
        [Authorize]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDto model, [FromQuery] string role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new Applicationuser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,
                City = model.City,
                Country = model.Country,
                PostalCode = model.PostalCode
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(role) && await _roleManager.RoleExistsAsync(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "Customer"); // Default role
                }
                return Ok(new { Message = "User created successfully" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPut("updateuser/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] Applicationuser model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Address = model.Address;
            user.City = model.City;
            user.Country = model.Country;
            user.PostalCode = model.PostalCode;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        [HttpDelete("deleteuser/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("{userId}/assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(string userId, [FromQuery] string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return BadRequest($"Role {roleName} does not exist.");
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                return Ok(new { Message = $"Role {roleName} assigned to user {user.Email} successfully." });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("{userId}/remove-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole(string userId, [FromQuery] string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return BadRequest($"Role {roleName} does not exist.");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                return Ok(new { Message = $"Role {roleName} removed from user {user.Email} successfully." });
            }

            return BadRequest(result.Errors);
        }
    }
}
