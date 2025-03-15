using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Helpers
{
    /// <summary>
    /// Helper methods to initialize model classes with default values
    /// </summary>
    public static class ModelHelpers
    {
        /// <summary>
        /// Initialize a User object with default values
        /// </summary>
        public static Swipe2TryCore.Models.User CreateUser()
        {
            return new Swipe2TryCore.Models.User
            {
                UserID = string.Empty,
                Name = string.Empty,
                Email = string.Empty,
                Password = string.Empty,
                RoleID = string.Empty,
                Role = new Swipe2TryCore.Models.Role { RoleID = string.Empty, RoleName = string.Empty }
            };
        }
        
        /// <summary>
        /// Initialize a Category object with default values
        /// </summary>
        public static Swipe2TryCore.Models.Category CreateCategory()
        {
            return new Swipe2TryCore.Models.Category
            {
                CategoryID = string.Empty,
                CategoryName = string.Empty
            };
        }
        
        /// <summary>
        /// Initialize a Restaurant object with default values
        /// </summary>
        public static Swipe2TryCore.Models.Restaurant CreateRestaurant()
        {
            return new Swipe2TryCore.Models.Restaurant
            {
                RestaurantID = string.Empty,
                Name = string.Empty,
                Location = string.Empty,
                UserID = string.Empty,
                User = CreateUser(),
                Categories = new List<Swipe2TryCore.Models.Category>(),
                Dishes = new List<Swipe2TryCore.Models.Dish>()
            };
        }
        
        /// <summary>
        /// Initialize a Dish with default values
        /// </summary>
        public static Swipe2TryCore.Models.Dish CreateDish()
        {
            return new Swipe2TryCore.Models.Dish
            {
                DishID = $"d{DateTime.Now.Ticks % 900000 + 100000}",  // Generate unique ID
                Name = string.Empty,
                Description = string.Empty,
                Photo = string.Empty,
                HealthFactor = string.Empty,
                Categories = new List<Swipe2TryCore.Models.Category>(),
                Restaurants = new List<Swipe2TryCore.Models.Restaurant>(),
                LikeDislikes = new List<Swipe2TryCore.Models.LikeDislike>()
            };
        }
        
        /// <summary>
        /// Initialize a LikeDislike object with default values
        /// </summary>
        public static Swipe2TryCore.Models.LikeDislike CreateLikeDislike()
        {
            return new Swipe2TryCore.Models.LikeDislike
            {
                LikeDislikeID = $"l{DateTime.Now.Ticks % 900000 + 100000}",
                UserID = string.Empty,
                DishID = string.Empty,
                IsLike = false,
                User = new Swipe2TryCore.Models.User(),
                Dish = new Swipe2TryCore.Models.Dish()
            };
        }
    }
}
