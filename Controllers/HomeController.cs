using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using eShopEnhanced.Models;
using eShopEnhanced.Repositories;
using eShopEnhanced.ViewModels;

namespace eShopEnhanced.Controllers;

public class HomeController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IActionResult> Index()
    {
        HomeViewModel homeViewModel = new HomeViewModel(
            await _productRepository.GetPicksOfTheWeekAsync(),
            await _categoryRepository.GetAllCategoriesAsync());

        return View(homeViewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
