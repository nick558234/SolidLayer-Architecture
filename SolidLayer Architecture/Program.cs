using Microsoft.AspNetCore.Mvc;
using SolidLayer_Architecture.Data;
using SolidLayer_Architecture.Repositories;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

var builder = WebApplication.CreateBuilder(args);

// Register database initializer
builder.Services.AddScoped<DatabaseInitializer>();

// Register repositories without EF Core
builder.Services.AddScoped<IRepository<Dish>, DishRepository>();

// Register services
builder.Services.AddScoped<IDishService, DishService>();

// Add Razor Pages services with validation options
builder.Services.AddRazorPages()
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
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

// Initialize the database using our custom DatabaseInitializer
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    dbInitializer.EnsureDatabaseExists();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
