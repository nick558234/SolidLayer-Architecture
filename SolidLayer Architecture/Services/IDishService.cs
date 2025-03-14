using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Services
{
    public interface IDishService
    {
        IEnumerable<Dish> GetAllDishes();
        Dish? GetDishById(string id);
        void AddDish(Dish dish);
        void UpdateDish(Dish dish);
        void DeleteDish(string id);
        IEnumerable<Dish> SearchDishes(string searchTerm);
    }
}
