using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SolidLayer_Architecture.Data;
using SolidLayer_Architecture.Repositories;
using SolidLayer_Architecture.Services;
using SolidLayer_Architecture.Tools;
using Swipe2TryCore.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on all network interfaces
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5235); // HTTP port
    serverOptions.ListenAnyIP(7217, configure => configure.UseHttps()); // HTTPS port
});

// Register database initializer
builder.Services.AddScoped<DatabaseInitializer>();
builder.Services.AddScoped<DatabaseCleanupTool>();

// Register repositories without EF Core
builder.Services.AddScoped<IRepository<Dish>, DishRepository>();
builder.Services.AddScoped<LikeDislikeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register services
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddScoped<ILikeDislikeService, LikeDislikeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        
        // Add bypass for diagnostic pages
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                // Don't redirect to login for setup or diagnostic pages
                if (context.Request.Path.StartsWithSegments("/Setup") ||
                    context.Request.Path.StartsWithSegments("/AccessHelp") ||
                    context.Request.Path.StartsWithSegments("/Admin/AdminDiagnostics") ||
                    context.Request.Path.StartsWithSegments("/Admin/UserDiagnostics"))
                {
                    // Skip authentication altogether
                    context.Response.StatusCode = 200;  // OK status
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                // Don't redirect to access denied for setup or diagnostic pages
                if (context.Request.Path.StartsWithSegments("/Setup") ||
                    context.Request.Path.StartsWithSegments("/AccessHelp") ||
                    context.Request.Path.StartsWithSegments("/Admin/AdminDiagnostics") ||
                    context.Request.Path.StartsWithSegments("/Admin/UserDiagnostics"))
                {
                    // Skip authorization altogether
                    context.Response.StatusCode = 200;  // OK status
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireRestaurantOwnerRole", policy => policy.RequireRole("RestaurantOwner", "Admin"));
});

// Add controllers for API endpoints
builder.Services.AddControllers();

// Add Razor Pages services with validation options
builder.Services.AddRazorPages(options => 
{
    // Apply authorization to specific page folders
    options.Conventions.AuthorizeFolder("/Admin", "RequireAdminRole");
    options.Conventions.AuthorizeFolder("/RestaurantOwner", "RequireRestaurantOwnerRole");
})
.AddMvcOptions(options => 
{
    // Configure model binding error messages
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => string.Empty);
})
.AddDataAnnotationsLocalization();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Initialize the database using our custom DatabaseInitializer
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    dbInitializer.EnsureDatabaseExists();
    
    // Ensure user accounts exist
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    // This will initialize the tables and create default roles and admin user
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Enable session middleware
app.UseSession();

// Map endpoints
app.MapRazorPages();
app.MapControllers();

// Add global error handling middleware
app.UseExceptionHandler("/Error");

app.Run();

// Add application information comment for clarity
/* 
 * Swipe2Try Application
 * A web app for discovering and rating food dishes
 * Using Swipe2TryCore models library
 */
