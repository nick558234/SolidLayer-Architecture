using Microsoft.EntityFrameworkCore;
using SolidLayer_Architecture.Data;
using SolidLayer_Architecture.Interfaces.Repositories;
using SolidLayer_Architecture.Repositories;


var builder = WebApplication.CreateBuilder(args);

// Add the database connection to the services container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register DatabaseInitializer as a scoped service
builder.Services.AddScoped<DatabaseInitializer>();
// Register repositories
builder.Services.AddScoped<IDishRepository, DishRepository>();

// ...and so on

// Add Razor Pages services
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();