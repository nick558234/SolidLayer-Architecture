using SolidLayer_Architecture.Repositories;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Services
{
    public class LikeDislikeService : ILikeDislikeService
    {
        private readonly LikeDislikeRepository _likeDislikeRepository;
        private readonly ILogger<LikeDislikeService> _logger;

        public LikeDislikeService(LikeDislikeRepository likeDislikeRepository, ILogger<LikeDislikeService> logger)
        {
            _likeDislikeRepository = likeDislikeRepository;
            _logger = logger;
        }

        public void AddPreference(string userId, string dishId, bool isLike)
        {
            try
            {
                // Check if user already has a preference for this dish
                var existingPreference = _likeDislikeRepository.GetByUserIdAndDishId(userId, dishId).FirstOrDefault();

                if (existingPreference != null)
                {
                    // Update existing preference if it's different
                    if (existingPreference.IsLike != isLike)
                    {
                        existingPreference.IsLike = isLike;
                        _likeDislikeRepository.Update(existingPreference);
                        _logger.LogInformation("Updated preference for user {UserId} on dish {DishId} to {IsLike}", 
                            userId, dishId, isLike);
                    }
                }
                else
                {
                    // Create new preference
                    var newPreference = new LikeDislike
                    {
                        LikeDislikeID = Guid.NewGuid().ToString().Substring(0, 10),
                        UserID = userId,
                        DishID = dishId,
                        IsLike = isLike
                    };
                    
                    _likeDislikeRepository.Insert(newPreference);
                    _logger.LogInformation("Added new preference for user {UserId} on dish {DishId}: {IsLike}", 
                        userId, dishId, isLike);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding preference for user {UserId} on dish {DishId}", userId, dishId);
                throw;
            }
        }

        public IEnumerable<LikeDislike> GetUserPreferences(string userId)
        {
            try
            {
                return _likeDislikeRepository.GetByUserId(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting preferences for user {UserId}", userId);
                return Enumerable.Empty<LikeDislike>();
            }
        }

        public IEnumerable<LikeDislike> GetDishPreferences(string dishId)
        {
            try
            {
                return _likeDislikeRepository.GetByDishId(dishId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting preferences for dish {DishId}", dishId);
                return Enumerable.Empty<LikeDislike>();
            }
        }

        public int GetLikeCount(string dishId)
        {
            try
            {
                return _likeDislikeRepository.GetLikeCount(dishId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting like count for dish {DishId}", dishId);
                return 0;
            }
        }

        public int GetDislikeCount(string dishId)
        {
            try
            {
                return _likeDislikeRepository.GetDislikeCount(dishId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dislike count for dish {DishId}", dishId);
                return 0;
            }
        }
    }
}
