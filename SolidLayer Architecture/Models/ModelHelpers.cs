namespace SolidLayer_Architecture.Models
{
    /// <summary>
    /// Helper methods to initialize local model classes with default values
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
                Categories = new List<RestaurantCategory>(),
                Dishes = new List<Swipe2TryCore.Models.Dish>()
            };
        }
        
        /// <summary>
        /// Initialize a DishCategory object with default values
        /// </summary>
        public static DishCategory CreateDishCategory()
        {
            return new DishCategory
            {
                DishID = string.Empty,
                CategoryID = string.Empty,
                Dish = new Swipe2TryCore.Models.Dish(),
                Category = CreateCategory()
            };
        }
        
        /// <summary>
        /// Initialize a DishRestaurant object with default values
        /// </summary>
        public static DishRestaurant CreateDishRestaurant()
        {
            return new DishRestaurant
            {
                DishID = string.Empty,
                RestaurantID = string.Empty,
                Dish = new Swipe2TryCore.Models.Dish(), 
                Restaurant = CreateRestaurant(),
                Photo = string.Empty,
                Description = string.Empty
            };
        }
        
        /// <summary>
        /// Initialize a LikeDislike object with default values
        /// </summary>
        public static LikeDislike CreateLikeDislike()
        {
            return new LikeDislike
            {
                LikeDislikeID = string.Empty,
                UserID = string.Empty,
                DishID = string.Empty,
                IsLike = false,
                User = CreateUser(),
                Dish = new Swipe2TryCore.Models.Dish()
            };
        }
        
        /// <summary>
        /// Initialize a RestaurantCategory object with default values
        /// </summary>
        public static RestaurantCategory CreateRestaurantCategory()
        {
            return new RestaurantCategory
            {
                RestaurantID = string.Empty,
                CategoryID = string.Empty,
                Restaurant = CreateRestaurant(),
                Category = CreateCategory()
            };
        }
    }
}
