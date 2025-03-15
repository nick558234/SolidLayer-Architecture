using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Models
{
    /// <summary>
    /// Helper methods to properly initialize models with required properties
    /// to avoid null reference warnings/exceptions
    /// </summary>
    public static class ModelInitializers
    {
        /// <summary>
        /// Initialize a Dish object with default values for required properties
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
                // Use explicit instantiation of the Swipe2TryCore collections
                Categories = new List<Swipe2TryCore.Models.Category>(),
                Restaurants = new List<Swipe2TryCore.Models.Restaurant>(),
                LikeDislikes = new List<Swipe2TryCore.Models.LikeDislike>()
            };
        }

        /// <summary>
        /// Initialize a LikeDislike object with default values for required properties
        /// </summary>
        public static Swipe2TryCore.Models.LikeDislike CreateLikeDislike()
        {
            return new Swipe2TryCore.Models.LikeDislike
            {
                LikeDislikeID = $"l{DateTime.Now.Ticks % 900000 + 100000}",
                UserID = string.Empty,
                DishID = string.Empty,
                IsLike = false,
                User = null!, // Using null-forgiving operator for navigation properties
                Dish = null!  // Navigation properties will be populated when needed
            };
        }

        /// <summary>
        /// Initialize default values for page models to avoid null warnings
        /// </summary>
        public static void InitializePageProperties<T>(T pageModel) where T : class
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string) && p.CanWrite);
            
            foreach (var property in properties)
            {
                if (property.GetValue(pageModel) == null)
                {
                    property.SetValue(pageModel, string.Empty);
                }
            }
        }
    }
}
