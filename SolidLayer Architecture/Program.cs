using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SolidLayer_Architecture.Data;
using SolidLayer_Architecture.Repositories;
using SolidLayer_Architecture.Services;
using Swipe2TryCore.Models;

var builder = WebApplication.CreateBuilder(args);

// Add the database connection to the services container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories - Fix ambiguous reference by using fully qualified type
builder.Services.AddScoped<IRepository<Swipe2TryCore.Models.Dish>, DishRepository>();

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

// Ensure the database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
