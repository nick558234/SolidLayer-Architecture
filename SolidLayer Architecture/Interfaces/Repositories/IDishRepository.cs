using Swipe2TryCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolidLayer_Architecture.Interfaces.Repositories
{
    public interface IDishRepository
    {
        Task<IEnumerable<Dish>> GetAllDishesAsync();
        Task<Dish> GetDishByIdAsync(string id);
        Task<Dish> CreateDishAsync(Dish dish);
        Task<bool> UpdateDishAsync(Dish dish);
        Task<bool> DeleteDishAsync(string id);
    }
}