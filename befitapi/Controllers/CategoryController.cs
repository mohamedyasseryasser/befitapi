using befitapi.dto;
using befitapi.Interfaces;
using befitapi.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace befitapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet("getallcategories")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var categoryDtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                isvalid=c.isvalid,
                Name = c.Name,
                Description = c.Description
            });
            return Ok(categoryDtos);
        }

        [HttpGet("getcategory/{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                isvalid = category.isvalid,
                Name = category.Name,
                Description = category.Description
            };
            return Ok(categoryDto);
        }

        [HttpPost("createcategory")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = new Category
            {
                Name = createCategoryDto.Name,
                Description = createCategoryDto.Description,
                isvalid=true
            };

            await _categoryRepository.AddAsync(category);

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,isvalid=category.isvalid
            };

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, categoryDto);
        }

        [HttpPut("updatecategory/{id}")]
       // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            if (id != updateCategoryDto.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            category.isvalid = updateCategoryDto.isvalid;
            category.Name = updateCategoryDto.Name;
            category.Description = updateCategoryDto.Description;

            await _categoryRepository.UpdateAsync(category);

            return NoContent();
        }

        [HttpDelete("deletecategory/{id}")]
       // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            category.isvalid = false;
            await _categoryRepository.UpdateAsync(category);
           // await _categoryRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}
