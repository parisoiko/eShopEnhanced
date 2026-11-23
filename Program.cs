using eShopEnhanced.Data;
using eShopEnhanced.Services;
using eShopEnhanced.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IShoppingCart, ShoppingCart>(sp => ShoppingCart.GetCart(sp));
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<eShopEnhancedDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:eShopEnhancedDbConnection"]);
});

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

DbInitializer.Seed(app);

app.Run();
