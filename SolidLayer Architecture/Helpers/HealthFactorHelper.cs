namespace SolidLayer_Architecture.Helpers
{
    public static class HealthFactorHelper
    {
        // Define health factor mappings
        private static readonly Dictionary<string, int> StringToIntMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "Very Healthy", 100 },
            { "Healthy", 75 },
            { "Moderate", 50 },
            { "Less Healthy", 25 },
            { "Unhealthy", 0 }
        };

        private static readonly Dictionary<int, string> IntToStringMap = new Dictionary<int, string>
        {
            { 100, "Very Healthy" },
            { 75, "Healthy" },
            { 50, "Moderate" },
            { 25, "Less Healthy" },
            { 0, "Unhealthy" }
        };

        /// <summary>
        /// Convert string health factor to integer value (0-100)
        /// </summary>
        public static int ToInt(string healthFactorString)
        {
            if (string.IsNullOrEmpty(healthFactorString))
                return 50; // Default to moderate

            // If it's already a number, parse it
            if (int.TryParse(healthFactorString, out int value))
                return value;

            // Try to match with our defined strings
            foreach (var kvp in StringToIntMap)
            {
                if (healthFactorString.ToLower().Contains(kvp.Key.ToLower()))
                    return kvp.Value;
            }

            // Default fallback based on keywords
            if (healthFactorString.ToLower().Contains("very") && 
                healthFactorString.ToLower().Contains("health"))
                return 100;
            if (healthFactorString.ToLower().Contains("health"))
                return 75;
            if (healthFactorString.ToLower().Contains("moderate"))
                return 50;
            if (healthFactorString.ToLower().Contains("less"))
                return 25;
            if (healthFactorString.ToLower().Contains("unhealthy"))
                return 0;

            return 50; // Default value if no match
        }

        /// <summary>
        /// Convert integer value to string health factor
        /// </summary>
        public static string ToString(int healthFactorInt)
        {
            // Find exact match
            if (IntToStringMap.TryGetValue(healthFactorInt, out string? result))
                return result;

            // Find closest match
            if (healthFactorInt >= 90) return "Very Healthy";
            if (healthFactorInt >= 70) return "Healthy";
            if (healthFactorInt >= 40) return "Moderate";
            if (healthFactorInt >= 20) return "Less Healthy";
            return "Unhealthy";
        }

        /// <summary>
        /// Gets a CSS class for styling based on health factor
        /// </summary>
        public static string GetCssClass(string healthFactorString)
        {
            int value = ToInt(healthFactorString);
            
            if (value >= 75) return "bg-success";
            if (value >= 50) return "bg-warning";
            return "bg-danger";
        }

        /// <summary>
        /// Get all available health factor options
        /// </summary>
        public static IEnumerable<string> GetAllOptions()
        {
            return StringToIntMap.Keys;
        }
    }
}
