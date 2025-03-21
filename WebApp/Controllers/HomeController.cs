using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.IsInRole("Admin"))
        {
            // Load categories with their products count
            ViewBag.Categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();

            // Load featured products (for now, just get the latest 8 products)
            ViewBag.FeaturedProducts = await _context.Products
                .Include(p => p.ProductImages)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToListAsync();
        }

        return View();
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
