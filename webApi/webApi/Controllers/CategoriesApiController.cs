using Microsoft.AspNetCore.Mvc;
using webApi.Model;
using webApi.Repositories;
using System;
using System.Threading.Tasks;

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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategories(int id, [FromBody] Categories categories)
        {
            try
            {
                if (id != categories.Id)
                {
                    return BadRequest("Categories ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingcategories = await _categoriesRepository.GetCategoriesByIdAsync(id);
                if (existingcategories == null)
                {
                    return NotFound();
                }

                // Cập nhật các thuộc tính của existingcategories
                existingcategories.Name = categories.Name;
                // ... cập nhật các thuộc tính khác ...

                await _categoriesRepository.UpdateCategoriesAsync(existingcategories); // Truyền entity đã được cập nhật
                return NoContent();
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
                var existingCategories = await _categoriesRepository.GetCategoriesByIdAsync(id);
                if (existingCategories == null)
                {
                    return NotFound();
                }

                await _categoriesRepository.DeleteCategoriesAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
