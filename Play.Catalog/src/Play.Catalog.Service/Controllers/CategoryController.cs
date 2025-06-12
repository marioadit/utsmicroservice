using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service;
using Play.Catalog.Service.Entity;
using Play.Universal;
using static Play.Catalog.Service.Dtos;

namespace Play.Catalog
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly IRepository<Category> categoryRepository;

        public CategoryController(IRepository<Category> categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await categoryRepository.GetAllAsync();
            return categories.Select(category => category.AsDto());
        }

        [HttpPost]
        public async Task<ActionResult<Category>> PostAsync(CreateCategoryDto createCategoryDto)
        {
            var category = new Category
            {
                CategoryName = createCategoryDto.CategoryName,
                CreatedDate = DateTime.UtcNow
            };

            await categoryRepository.CreateAsync(category);

            return CreatedAtAction(nameof(GetAsync), new { id = category.Id }, category.AsDto());
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(Guid id, UpdateCategoryDto updateCategoryDto)
        {
            var existingCategory = await categoryRepository.GetAsync(id);
            if (existingCategory is null)
            {
                return NotFound();
            }

            existingCategory.CategoryName = updateCategoryDto.CategoryName;
            await categoryRepository.UpdateAsync(existingCategory);

            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetAsync(Guid id)
        {
            var category = await categoryRepository.GetAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category.AsDto());
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(Guid id)
        {
            var existingCategory = await categoryRepository.GetAsync(id);
            if (existingCategory is null)
            {
                return NotFound();
            }
            await categoryRepository.DeleteAsync(existingCategory.Id);
            return NoContent();
        }
    }
}