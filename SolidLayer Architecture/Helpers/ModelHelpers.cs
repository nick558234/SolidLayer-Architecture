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
        public static User CreateUser()
        {
            return new User
            {
                UserID = string.Empty,
                Name = string.Empty,
                Email = string.Empty,
                Password = string.Empty,
                RoleID = string.Empty,
                Role = new Role { RoleID = string.Empty, RoleName = string.Empty }
            };
        }
        
        /// <summary>
        /// Initialize a Category object with default values
        /// </summary>
        public static Category CreateCategory()
        {
            return new Category
            {
                CategoryID = string.Empty,
                CategoryName = string.Empty
            };
        }
        
        /// <summary>
        /// Initialize a Restaurant object with default values
        /// </summary>
        public static Restaurant CreateRestaurant()
        {
            return new Restaurant
            {
                RestaurantID = string.Empty,
                Name = string.Empty,
                Location = string.Empty,
                UserID = string.Empty,
                User = CreateUser(),
                Categories = new List<Category>(),
                Dishes = new List<Dish>()
            };
        }
        
        /// <summary>
        /// Initialize a Dish with default values
        /// </summary>
        public static Dish CreateDish()
        {
            return new Dish
            {
                DishID = $"d{DateTime.Now.Ticks % 900000 + 100000}",  // Generate unique ID
                Name = string.Empty,
                Description = string.Empty,
                Photo = string.Empty,
                HealthFactor = string.Empty,
                Categories = new List<Category>(),
                Restaurants = new List<Restaurant>(),
                LikeDislikes = new List<LikeDislike>()
            };
        }
        
        /// <summary>
        /// Initialize a LikeDislike object with default values
        /// </summary>
        public static LikeDislike CreateLikeDislike()
        {
            return new LikeDislike
            {
                LikeDislikeID = $"l{DateTime.Now.Ticks % 900000 + 100000}",
                UserID = string.Empty,
                DishID = string.Empty,
                IsLike = false,
                User = new User(),
                Dish = new Dish()
            };
        }
    }
}
