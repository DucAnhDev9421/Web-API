using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webApi.Repositories;

namespace webApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlideController : ControllerBase
    {
        private readonly ISlideRepository _slideRepository;

        public SlideController(ISlideRepository slideRepository)
        {
            _slideRepository = slideRepository;
        }

        // GET: api/slide
        [HttpGet]
        public async Task<ActionResult<List<SlideDto>>> GetSlides([FromQuery] bool? isActive)
        {
            try
            {
                var slides = await _slideRepository.GetSlidesAsync(isActive);
                return Ok(slides);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/slide/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SlideDto>> GetSlide(int id)
        {
            try
            {
                var slide = await _slideRepository.GetSlideByIdAsync(id);
                if (slide == null)
                {
                    return NotFound($"Slide with ID {id} not found");
                }
                return Ok(slide);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/slide
        [HttpPost]
        public async Task<ActionResult<SlideDto>> CreateSlide(CreateSlideDto slideDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var slide = await _slideRepository.CreateSlideAsync(slideDto);
                return CreatedAtAction(nameof(GetSlide), new { id = slide.Id }, slide);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/slide/{id}

        [HttpPut("{id}")]
        public async Task<ActionResult<SlideDto>> UpdateSlide(int id, UpdateSlideDto slideDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var slide = await _slideRepository.UpdateSlideAsync(id, slideDto);
                if (slide == null)
                {
                    return NotFound($"Slide with ID {id} not found");
                }
                return Ok(slide);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/slide/{id}

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSlide(int id)
        {
            try
            {
                var result = await _slideRepository.DeleteSlideAsync(id);
                if (!result)
                {
                    return NotFound($"Slide with ID {id} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PATCH: api/slide/{id}/status

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateSlideStatus(int id, [FromBody] bool isActive)
        {
            try
            {
                var result = await _slideRepository.UpdateSlideStatusAsync(id, isActive);
                if (!result)
                {
                    return NotFound($"Slide with ID {id} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PATCH: api/slide/{id}/order

        [HttpPatch("{id}/order")]
        public async Task<IActionResult> UpdateSlideOrder(int id, [FromBody] int newOrder)
        {
            try
            {
                var result = await _slideRepository.UpdateSlideOrderAsync(id, newOrder);
                if (!result)
                {
                    return NotFound($"Slide with ID {id} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/slide/reorder

        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderSlides([FromBody] List<int> slideIds)
        {
            try
            {
                if (slideIds == null || !slideIds.Any())
                {
                    return BadRequest("Slide IDs list cannot be empty");
                }

                var result = await _slideRepository.ReorderSlidesAsync(slideIds);
                if (!result)
                {
                    return BadRequest("Invalid slide IDs provided");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 