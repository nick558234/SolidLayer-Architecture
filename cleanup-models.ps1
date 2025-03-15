# Script to delete redundant model files from SolidLayer Architecture

$modelsDir = ".\SolidLayer Architecture\Models"

# List of redundant model files to delete
$filesToDelete = @(
    "User.cs",
    "Role.cs",
    "Category.cs",
    "DishCategory.cs",
    "DishRestaurant.cs",
    "LikeDislike.cs",
    "Restaurant.cs", 
    "RestaurantCategory.cs",
    "Dish.cs",
    "ModelHelpers.cs"
)

# Check if the directory exists first
if (Test-Path $modelsDir) {
    # Delete each file if it exists
    foreach ($file in $filesToDelete) {
        $filePath = Join-Path -Path $modelsDir -ChildPath $file
        if (Test-Path $filePath) {
            Write-Host "Deleting redundant model: $file"
            Remove-Item -Path $filePath -Force
        } else {
            Write-Host "File does not exist: $file"
        }
    }
    
    Write-Host "Cleanup complete."
} else {
    Write-Host "Models directory not found at: $modelsDir"
}
