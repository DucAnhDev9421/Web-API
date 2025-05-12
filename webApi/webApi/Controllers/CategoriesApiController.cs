using Microsoft.AspNetCore.Mvc;
using webApi.Repositories;
using System;
using System.Threading.Tasks;
using webApi.Model.CategoryModel;

namespace webApi.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesApiController : ControllerBase
    {
        private readonly ICategoriesRepository _categoriesRepository;

        public CategoriesApiController(ICategoriesRepository categoriesRepository)
        {
            _categoriesRepository = categoriesRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _categoriesRepository.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoriesById(int id)
        {
            try
            {
                var categories = await _categoriesRepository.GetCategoriesByIdAsync(id);
                if (categories == null)
                {
                    return NotFound();
                }
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategories([FromBody] Categories categories)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var newCategory = new Categories
                {
                    Name = categories.Name,
                    // Gán tất cả các thuộc tính khác
                };

                await _categoriesRepository.AddCategoriesAsync(newCategory);
                return CreatedAtAction(nameof(GetCategoriesById), new { id = newCategory.Id }, newCategory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("batch")]
        public async Task<IActionResult> CreateCategoriesBatch([FromBody] List<Categories> categories)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (categories == null || !categories.Any())
                {
                    return BadRequest("No categories provided");
                }

                // Validate each category
                foreach (var category in categories)
                {
                    if (string.IsNullOrWhiteSpace(category.Name))
                    {
                        return BadRequest($"Category name cannot be empty");
                    }
                }

                await _categoriesRepository.AddCategoriesBatchAsync(categories);
                return Ok(new { message = $"{categories.Count} categories created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategories(int id, [FromBody] Categories categories)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid category ID");
                }

                if (categories == null)
                {
                    return BadRequest("Category data is required");
                }

                if (string.IsNullOrWhiteSpace(categories.Name))
                {
                    return BadRequest("Category name cannot be empty");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingCategory = await _categoriesRepository.GetCategoriesByIdAsync(id);
                if (existingCategory == null)
                {
                    return NotFound($"Category with ID {id} not found");
                }

                // Update category properties
                existingCategory.Name = categories.Name.Trim();

                await _categoriesRepository.UpdateCategoriesAsync(existingCategory);
                return Ok(new { 
                    message = "Category updated successfully",
                    category = existingCategory
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategories(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid category ID");
                }

                var existingCategory = await _categoriesRepository.GetCategoriesByIdAsync(id);
                if (existingCategory == null)
                {
                    return NotFound($"Category with ID {id} not found");
                }

                await _categoriesRepository.DeleteCategoriesAsync(id);
                return Ok(new { message = $"Category '{existingCategory.Name}' deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
