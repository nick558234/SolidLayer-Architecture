using Microsoft.AspNetCore.Mvc.RazorPages;
using SolidLayer_Architecture.Models;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Pages.User
{
    public class HealthTip
    {
        public string Title { get; set; } = string.Empty; // Initialize to avoid warning
        public string Description { get; set; } = string.Empty; // Initialize to avoid warning
    }

    public class HealthBreakdown
    {
        public int VeryHealthy { get; set; } = 0;
        public int Healthy { get; set; } = 0;
        public int Moderate { get; set; } = 0;
        public int LessHealthy { get; set; } = 0;
        public int Unhealthy { get; set; } = 0;
    }

    public class HealthDashboardModel : PageModel
    {
        private readonly IDishService _dishService;
        private readonly ILikeDislikeService _likeDislikeService;

        public HealthDashboardModel(IDishService dishService, ILikeDislikeService likeDislikeService)
        {
            _dishService = dishService;
            _likeDislikeService = likeDislikeService;
            
            // Initialize properties to avoid warnings
            HealthScoreColorClass = "bg-secondary"; // Default color
            HealthScoreMessage = "No data available yet";
            
            // For demonstration purposes, we're not setting Title/Description (not used in the view)
            // but we'll initialize them anyway to avoid warnings
            Title = "Health Dashboard";
            Description = "Track your food choices and make healthier decisions.";
        }

        public List<Swipe2TryCore.Models.Dish> LikedDishes { get; set; } = new List<Swipe2TryCore.Models.Dish>();
        public int HealthScore { get; set; }
        public string HealthScoreColorClass { get; set; }
        public string HealthScoreMessage { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public HealthBreakdown HealthBreakdown { get; set; } = new HealthBreakdown();
        public List<HealthTip> HealthTips { get; set; } = new List<HealthTip>();

        // In a real app, this would come from authentication
        private const string UserId = "1";

        public void OnGet()
        {
            // Get the user's liked dishes
            var userPreferences = _likeDislikeService.GetUserPreferences(UserId)
                .Where(ld => ld.IsLike)
                .ToList();

            // Get each dish that was liked
            foreach (var pref in userPreferences)
            {
                var dish = _dishService.GetDishById(pref.DishID);
                if (dish != null)
                {
                    LikedDishes.Add(dish);
                }
            }

            // Calculate health breakdown
            CalculateHealthBreakdown();

            // Calculate health score (0-100)
            CalculateHealthScore();

            // Set health message based on score
            SetHealthMessage();

            // Populate health tips
            PopulateHealthTips();
        }

        private void CalculateHealthBreakdown()
        {
            if (!LikedDishes.Any())
                return;

            int total = LikedDishes.Count;
            
            foreach (var dish in LikedDishes)
            {
                string healthFactor = dish.HealthFactor?.ToLower() ?? "";
                
                if (healthFactor.Contains("very healthy"))
                    HealthBreakdown.VeryHealthy++;
                else if (healthFactor.Contains("healthy"))
                    HealthBreakdown.Healthy++;
                else if (healthFactor.Contains("moderate"))
                    HealthBreakdown.Moderate++;
                else if (healthFactor.Contains("less healthy"))
                    HealthBreakdown.LessHealthy++;
                else if (healthFactor.Contains("unhealthy"))
                    HealthBreakdown.Unhealthy++;
            }

            // Convert to percentages
            if (total > 0)
            {
                HealthBreakdown.VeryHealthy = (int)(HealthBreakdown.VeryHealthy * 100.0 / total);
                HealthBreakdown.Healthy = (int)(HealthBreakdown.Healthy * 100.0 / total);
                HealthBreakdown.Moderate = (int)(HealthBreakdown.Moderate * 100.0 / total);
                HealthBreakdown.LessHealthy = (int)(HealthBreakdown.LessHealthy * 100.0 / total);
                HealthBreakdown.Unhealthy = (int)(HealthBreakdown.Unhealthy * 100.0 / total);
            }
        }

        private void CalculateHealthScore()
        {
            if (!LikedDishes.Any())
            {
                HealthScore = 50; // Neutral score if no liked dishes
                return;
            }

            // Calculate a weighted score:
            // Very Healthy: 100 points
            // Healthy: 75 points
            // Moderate: 50 points
            // Less Healthy: 25 points
            // Unhealthy: 0 points
            int totalPoints = 0;
            int totalItems = 0;

            foreach (var dish in LikedDishes)
            {
                string healthFactor = dish.HealthFactor?.ToLower() ?? "";
                
                if (healthFactor.Contains("very healthy"))
                    totalPoints += 100;
                else if (healthFactor.Contains("healthy"))
                    totalPoints += 75;
                else if (healthFactor.Contains("moderate"))
                    totalPoints += 50;
                else if (healthFactor.Contains("less healthy"))
                    totalPoints += 25;
                else if (healthFactor.Contains("unhealthy"))
                    totalPoints += 0;
                else
                    totalPoints += 50; // Default for unknown
                
                totalItems++;
            }

            HealthScore = totalItems > 0 ? totalPoints / totalItems : 50;

            // Set the health score color class
            if (HealthScore >= 75)
                HealthScoreColorClass = "bg-success";
            else if (HealthScore >= 50)
                HealthScoreColorClass = "bg-warning";
            else
                HealthScoreColorClass = "bg-danger";
        }

        private void SetHealthMessage()
        {
            if (!LikedDishes.Any())
            {
                HealthScoreMessage = "Start liking dishes to see your health score!";
                return;
            }

            if (HealthScore >= 80)
                HealthScoreMessage = "Excellent! You're making very healthy food choices.";
            else if (HealthScore >= 70)
                HealthScoreMessage = "Great job! Your food choices are generally healthy.";
            else if (HealthScore >= 50)
                HealthScoreMessage = "Not bad! Consider adding more healthy options to your diet.";
            else if (HealthScore >= 30)
                HealthScoreMessage = "Needs improvement. Try to choose healthier options more often.";
            else
                HealthScoreMessage = "Your food choices could use significant improvement for better health.";
        }

        private void PopulateHealthTips()
        {
            HealthTips = new List<HealthTip>
            {
                new HealthTip
                {
                    Title = "Add more vegetables",
                    Description = "Try to include at least one vegetable with every meal."
                },
                new HealthTip
                {
                    Title = "Choose whole grains",
                    Description = "Opt for whole grains instead of refined grains for more nutrients and fiber."
                },
                new HealthTip
                {
                    Title = "Stay hydrated",
                    Description = "Drink water throughout the day instead of sugary beverages."
                },
                new HealthTip
                {
                    Title = "Mindful eating",
                    Description = "Slow down and enjoy your meals without distractions."
                },
                new HealthTip
                {
                    Title = "Balanced portions",
                    Description = "Fill half your plate with vegetables, a quarter with protein, and a quarter with starches."
                },
                new HealthTip
                {
                    Title = "Limit processed foods",
                    Description = "Choose fresh, whole foods whenever possible."
                }
            };

            // If the user has poor health habits, add specific tips
            if (HealthScore < 50)
            {
                HealthTips.Add(new HealthTip
                {
                    Title = "Start small",
                    Description = "Make one healthy swap per day to gradually improve your diet."
                });
                
                HealthTips.Add(new HealthTip
                {
                    Title = "Read nutrition labels",
                    Description = "Check labels for hidden sugars and unhealthy fats."
                });
            }
        }
    }
}
