using Microsoft.EntityFrameworkCore;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<RestaurantCategory> RestaurantCategories { get; set; }
        public DbSet<DishCategory> DishCategories { get; set; }
        public DbSet<LikeDislike> LikeDislikes { get; set; }
        public DbSet<DishRestaurant> DishRestaurants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define composite keys
            modelBuilder.Entity<RestaurantCategory>()
                .HasKey(rc => new { rc.RestaurantID, rc.CategoryID });

            modelBuilder.Entity<DishCategory>()
                .HasKey(dc => new { dc.DishID, dc.CategoryID });

            modelBuilder.Entity<DishRestaurant>()
                .HasKey(dr => new { dr.DishID, dr.RestaurantID });
        }
    }
}
