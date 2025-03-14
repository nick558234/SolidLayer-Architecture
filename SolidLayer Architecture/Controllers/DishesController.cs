using Microsoft.AspNetCore.Mvc;
using SolidLayer_Architecture.Services;
using System.Text.Json;

namespace SolidLayer_Architecture.Controllers
{
    [ApiController]
    [Route("api/dishes")]
    public class DishesController : ControllerBase
    {
        private readonly IDishService _dishService;
        private readonly ILikeDislikeService _likeDislikeService;
        private readonly ILogger<DishesController> _logger;

        public DishesController(
            IDishService dishService, 
            ILikeDislikeService likeDislikeService,
            ILogger<DishesController> logger)
        {
            _dishService = dishService;
            _likeDislikeService = likeDislikeService;
            _logger = logger;
        }

        [HttpPost("preference")]
        public IActionResult RecordPreference([FromBody] PreferenceRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.DishId))
                {
                    return BadRequest(new { success = false, message = "Dish ID is required" });
                }

                // For demo purposes, we're using a hardcoded user ID
                // In a real application, this would come from authentication
                string userId = "1"; 

                // Record the preference
                _likeDislikeService.AddPreference(userId, request.DishId, request.IsLike);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording preference");
                return StatusCode(500, new { success = false, message = "An error occurred while recording your preference" });
            }
        }

        public class PreferenceRequest
        {
            public string DishId { get; set; }
            public bool IsLike { get; set; }
        }
    }
}
