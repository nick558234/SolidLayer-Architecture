using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Services
{
    public interface ILikeDislikeService
    {
        void AddPreference(string userId, string dishId, bool isLike);
        IEnumerable<LikeDislike> GetUserPreferences(string userId);
        IEnumerable<LikeDislike> GetDishPreferences(string dishId);
        int GetLikeCount(string dishId);
        int GetDislikeCount(string dishId);
    }
}
