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
builder.Services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IShoppingCart, ShoppingCart>(sp => ShoppingCart.GetCart(sp));
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
// Blazor Server: lets Razor components (Components/*.razor) run interactively on the server
builder.Services.AddServerSideBlazor();
builder.Services.AddDbContext<eShopEnhancedDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:eShopEnhancedDbConnection"]);
});

// ASP.NET Core Identity: user accounts stored in our database (via eShopEnhancedDbContext),
// plus ready-made Register / Log in / Log out pages (the "default UI", built as Razor Pages).
// RequireConfirmedAccount = false: no confirmation email needed (we don't send emails yet).
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<eShopEnhancedDbContext>()
    // Puts the user's first and last name into the login cookie (as "claims"), so the navbar
    // can show the initials on every page without a database query. See AppClaimsPrincipalFactory.
    .AddClaimsPrincipalFactory<AppClaimsPrincipalFactory>();

// The Identity pages are Razor Pages, so the app must also serve Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStaticFiles();
app.UseSession();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Format numbers, prices and dates the Greek way for every request: 15,95 €
// el-GR is the only supported culture, so a browser asking for another language still gets el-GR.
var supportedCultures = new[] { "el-GR" };
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("el-GR")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

app.UseRouting();

// Order matters: first find out WHO the visitor is (reads the login cookie)...
app.UseAuthentication();
// ...then decide WHAT they may do ([Authorize] on OrderController). The other way round,
// authorization would always see an anonymous visitor.
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// The live connection (SignalR, at /_blazor) that blazor.server.js uses to talk to the components
app.MapBlazorHub();

// The Identity pages: /Identity/Account/Login, /Identity/Account/Register, ...
app.MapRazorPages();

DbInitializer.Seed(app);

app.Run();
