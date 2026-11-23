using eShopEnhanced.Data;
using eShopEnhanced.Models;
using eShopEnhanced.Services;
using eShopEnhanced.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IShoppingCart, ShoppingCart>(sp => ShoppingCart.GetCart(sp));
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
// Blazor Server, used by the live search component (Components/ProductSearch.razor)
builder.Services.AddServerSideBlazor();
builder.Services.AddDbContext<eShopEnhancedDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:eShopEnhancedDbConnection"]);
});

// No account confirmation: the app doesn't send email.
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<eShopEnhancedDbContext>()
    // Adds first/last name claims so the navbar can show initials without a database query.
    .AddClaimsPrincipalFactory<AppClaimsPrincipalFactory>();

// Required by the Identity UI, which is built with Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseSession();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// el-GR is the only supported culture, so prices and dates always render the Greek way (15,95 €),
// whatever language the browser asks for.
var supportedCultures = new[] { "el-GR" };
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("el-GR")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

app.UseRouting();

// Authentication must come before authorization, otherwise [Authorize] always sees an anonymous user.
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// SignalR endpoint (/_blazor) used by blazor.server.js
app.MapBlazorHub();

// Identity UI: /Identity/Account/Login, /Identity/Account/Register, ...
app.MapRazorPages();

DbInitializer.Seed(app);

app.Run();

// Makes the top-level Program class public for WebApplicationFactory<Program> in the integration tests.
public partial class Program { }
