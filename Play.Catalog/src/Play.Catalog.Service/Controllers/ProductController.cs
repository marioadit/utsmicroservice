using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Entity;
using Play.Universal;
using static Play.Catalog.Service.Dtos;

namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IRepository<Product> productRepository;
        private readonly IRepository<Category> categoryRepository; // Tambahkan repositori Category untuk validasi CategoryId

        public ProductController(IRepository<Product> productRepository, IRepository<Category> categoryRepository)
        {
            this.productRepository = productRepository;
            this.categoryRepository = categoryRepository; // Inisialisasi repositori Category
        }

        [HttpGet]
        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await productRepository.GetAllAsync();
            return products.Select(product => product.AsDto());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetAsync(Guid id)
        {
            var product = await productRepository.GetAsync(id);
            if (product is null)
            {
                return NotFound();
            }
            return Ok(product.AsDto());
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> PostAsync(CreateProductDto productDto)
        {
            if (productDto.CategoryId == Guid.Empty)
            {
                return BadRequest("CategoryId is required.");
            }

            // Validasi apakah CategoryId yang diberikan ada dalam database
            var category = await categoryRepository.GetAsync(productDto.CategoryId);
            if (category == null)
            {
                return BadRequest("Category not found.");
            }

            var product = new Product
            {
                ProductName = productDto.ProductName,
                CategoryId = productDto.CategoryId,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity,
                Description = productDto.Description,
                CreatedDate = DateTime.UtcNow
            };

            await productRepository.CreateAsync(product);
            return CreatedAtAction(nameof(GetAsync), new { id = product.Id }, product.AsDto());
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(Guid id, UpdateProductDto productDto)
        {
            if (productDto.CategoryId == Guid.Empty)
            {
                return BadRequest("CategoryId is required.");
            }

            // Validasi apakah CategoryId yang diberikan ada dalam database
            var category = await categoryRepository.GetAsync(productDto.CategoryId);
            if (category == null)
            {
                return BadRequest("Category not found.");
            }

            var existingProduct = await productRepository.GetAsync(id);
            if (existingProduct is null)
            {
                return NotFound();
            }

            existingProduct.ProductName = productDto.ProductName;
            existingProduct.CategoryId = productDto.CategoryId;
            existingProduct.Price = productDto.Price;
            existingProduct.StockQuantity = productDto.StockQuantity;
            existingProduct.Description = productDto.Description;

            await productRepository.UpdateAsync(existingProduct);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(Guid id)
        {
            var existingProduct = await productRepository.GetAsync(id);
            if (existingProduct is null)
            {
                return NotFound();
            }
            await productRepository.DeleteAsync(existingProduct.Id);
            return NoContent();
        }
    }
}