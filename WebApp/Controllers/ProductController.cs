using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IFileService _fileService;

        public ProductController(
            ApplicationDbContext context,
            IWebHostEnvironment hostEnvironment,
            IFileService fileService)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _fileService = fileService;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ToListAsync();
            return View(products);
        }

        // GET: Product/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,CategoryId")] Product product, List<IFormFile> images)
        {
            ModelState.Remove("Category"); // Remove validation for navigation property
            ModelState.Remove("ProductImages"); // Remove validation for collection
            ModelState.Remove("MainImage"); // Remove validation for computed property

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();

                if (images != null && images.Any())
                {
                    bool isFirst = true;
                    foreach (var image in images)
                    {
                        if (image.Length > 0)
                        {
                            var fileName = await _fileService.SaveFileAsync(image, _hostEnvironment.WebRootPath);
                            var productImage = new ProductImage
                            {
                                ProductId = product.Id,
                                FileName = image.FileName,
                                FilePath = fileName,
                                IsMain = isFirst
                            };
                            _context.ProductImages.Add(productImage);
                            isFirst = false;
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Product/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,CategoryId")] Product product, List<IFormFile> images)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Category"); // Remove validation for navigation property
            ModelState.Remove("ProductImages"); // Remove validation for collection
            ModelState.Remove("MainImage"); // Remove validation for computed property

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    if (images != null && images.Any())
                    {
                        foreach (var image in images)
                        {
                            if (image.Length > 0)
                            {
                                var fileName = await _fileService.SaveFileAsync(image, _hostEnvironment.WebRootPath);
                                var productImage = new ProductImage
                                {
                                    ProductId = product.Id,
                                    FileName = image.FileName,
                                    FilePath = fileName,
                                    IsMain = !await _context.ProductImages.AnyAsync(pi => pi.ProductId == product.Id)
                                };
                                _context.ProductImages.Add(productImage);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Product/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                foreach (var image in product.ProductImages)
                {
                    _fileService.DeleteFile(image.FilePath, _hostEnvironment.WebRootPath);
                }

                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // POST: Product/DeleteImage/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image == null)
            {
                return NotFound();
            }

            var wasMainImage = image.IsMain;
            var productId = image.ProductId;

            _fileService.DeleteFile(image.FilePath, _hostEnvironment.WebRootPath);
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            if (wasMainImage)
            {
                var newMainImage = await _context.ProductImages
                    .FirstOrDefaultAsync(pi => pi.ProductId == productId);
                if (newMainImage != null)
                {
                    newMainImage.IsMain = true;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        // POST: Product/SetMainImage/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetMainImage(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image == null)
            {
                return NotFound();
            }

            var currentMainImage = await _context.ProductImages
                .FirstOrDefaultAsync(pi => pi.ProductId == image.ProductId && pi.IsMain);
            if (currentMainImage != null)
            {
                currentMainImage.IsMain = false;
            }

            image.IsMain = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = image.ProductId });
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
} 