using befitapi.dto;
using befitapi.Interfaces;
using befitapi.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace befitapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IImageRepository _imageRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, IImageRepository imageRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _imageRepository = imageRepository;
        }

        [HttpGet("getallproducts")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _productRepository.GetAllAsync();
            var productDtos = new List<ProductDto>();
            foreach (var product in products)
            {
                var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                productDtos.Add(new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    ImageUrl = product.ImageUrl,
                    CategoryId = product.CategoryId,
                    CategoryName = category?.Name
                });
            }
            return Ok(productDtos);
        }

        [HttpGet("getproductbyid/{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = category?.Name
            };
            return Ok(productDto);
        }

        [HttpGet("getproductbycategory/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
        {
            var products = await _productRepository.GetProductsByCategoryAsync(categoryId);
            var productDtos = new List<ProductDto>();
            foreach (var product in products)
            {
                var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                productDtos.Add(new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    ImageUrl = product.ImageUrl,
                    CategoryId = product.CategoryId,
                    CategoryName = category?.Name
                });
            }
            return Ok(productDtos);
        }

        [HttpGet("searchproduct")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts([FromQuery] string query)
        {
            var products = await _productRepository.FindAsync(p => p.Name.Contains(query) || p.Description.Contains(query));
            var productDtos = new List<ProductDto>();
            foreach (var product in products)
            {
                var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                productDtos.Add(new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    ImageUrl = product.ImageUrl,
                    CategoryId = product.CategoryId,
                    CategoryName = category?.Name
                });
            }
            return Ok(productDtos);
        }

        [HttpPost("createproduct")]
        [Authorize(Roles = "Admin,Seller")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = new product
            {
                Name = createProductDto.Name,
                Description = createProductDto.Description,
                Price = createProductDto.Price,
                Stock = createProductDto.Stock,
                CategoryId = createProductDto.CategoryId
            };

            try
            {
                product.ImageUrl = await _imageRepository.UploadImageAsync(createProductDto.ImageFile, "product-images");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            await _productRepository.AddAsync(product);

            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = category?.Name
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
        }
        [HttpPut("updateproduct/{id}")]
         [Authorize(Roles = "Admin,Seller")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto updateProductDto)
        {
            if (id != updateProductDto.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            product.Name = updateProductDto.Name;
            product.Description = updateProductDto.Description;
            product.Price = updateProductDto.Price;
            product.Stock = updateProductDto.Stock;
            product.CategoryId = updateProductDto.CategoryId;

            try
            {
                // إذا تم رفع صورة جديدة يتم استبدال القديمة بها
                if (updateProductDto.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        _imageRepository.DeleteImage(product.ImageUrl);
                    }

                    product.ImageUrl = await _imageRepository.UploadImageAsync(
                        updateProductDto.ImageFile,
                        "product-images");
                }
                // حذف الصورة فقط (إذا كان ImageUrl يسمح بـ null في قاعدة البيانات)
                else if (updateProductDto.RemoveImage)
                {
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        _imageRepository.DeleteImage(product.ImageUrl);
                    }

                    return BadRequest(new
                    {
                        message = "لا يمكن حذف الصورة بدون رفع صورة بديلة لأن ImageUrl لا يقبل NULL."
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            await _productRepository.UpdateAsync(product);

            return NoContent();
        }

        [HttpDelete("deleteproduct/{id}")]
          [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                _imageRepository.DeleteImage(product.ImageUrl);
            }
            await _productRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}
