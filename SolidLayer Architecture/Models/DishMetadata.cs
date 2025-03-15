using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using Swipe2TryCore.Models;

namespace SolidLayer_Architecture.Models
{
    // Apply this metadata to the Dish class from Swipe2TryCore
    [ModelMetadataType(typeof(DishMetadata))]
    public partial class DishMetadataApplier : Swipe2TryCore.Models.Dish
    {
        // This is a wrapper class that extends Swipe2TryCore.Models.Dish
    }

    public class DishMetadata
    {
        [Required(ErrorMessage = "Dish ID is required")]
        public string? DishID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Photo { get; set; }

        [Display(Name = "Health Factor")]
        public string? HealthFactor { get; set; }

        // Mark collections as not required for validation
        [BindingBehavior(BindingBehavior.Never)]
        [ValidateNever]
        public ICollection<Swipe2TryCore.Models.Category>? Categories { get; set; }

        [BindingBehavior(BindingBehavior.Never)]
        [ValidateNever]
        public ICollection<Swipe2TryCore.Models.Restaurant>? Restaurants { get; set; }

        [BindingBehavior(BindingBehavior.Never)]
        [ValidateNever]
        public ICollection<Swipe2TryCore.Models.LikeDislike>? LikeDislikes { get; set; }
    }
}
