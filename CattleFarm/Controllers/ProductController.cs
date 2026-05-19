using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IFarmService    _farmService;
        private readonly IAuditService   _auditService;
        private const int PageSize = 12;

        public ProductController(IProductService product, IFarmService farm, IAuditService audit)
        { _productService = product; _farmService = farm; _auditService = audit; }

        public async Task<IActionResult> Index(int page = 1, int? farmId = null, string? search = null, ProductCategory? category = null)
        {
            var (items, total) = await _productService.GetPagedAsync(page, PageSize, farmId, search, category);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)PageSize);
            ViewBag.Farms           = await _farmService.GetAllAsync();
            ViewData["FarmId"]      = farmId;
            ViewData["Category"]    = category;
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product is null) return NotFound();
            return View(product);
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create() { ViewBag.Farms = await _farmService.GetAllAsync(); return View(new ProductViewModel()); }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(ProductViewModel vm)
        {
            if (!ModelState.IsValid) { ViewBag.Farms = await _farmService.GetAllAsync(); return View(vm); }
            var product = await _productService.CreateAsync(vm);
            await _auditService.LogActivityAsync(GetUserId(), $"Added product: {product.Name}", "Product", product.Id);
            TempData["SuccessMessage"] = $"Product '{product.Name}' added.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _productService.GetByIdAsync(id);
            if (p is null) return NotFound();
            ViewBag.Farms = await _farmService.GetAllAsync();
            var vm = new ProductViewModel { Id = p.Id, Name = p.Name, Category = p.Category, Description = p.Description, Price = p.Price, StockQuantity = p.StockQuantity, Unit = p.Unit, MinStockLevel = p.MinStockLevel, IsAvailable = p.IsAvailable, IsFeatured = p.IsFeatured, FarmId = p.FarmId, ExistingImagePath = p.ImagePath };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id, ProductViewModel vm)
        {
            if (!ModelState.IsValid) { ViewBag.Farms = await _farmService.GetAllAsync(); return View(vm); }
            await _productService.UpdateAsync(id, vm);
            TempData["SuccessMessage"] = "Product updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Product deleted.";
            return RedirectToAction(nameof(Index));
        }

        private int GetUserId() { var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; return int.TryParse(id, out var p) ? p : 0; }
    }
}
