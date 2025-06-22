using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EXE201_LinhMocStore.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace EXE201_LinhMocStore.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly PhongThuyShopContext _context;
        private readonly ILogger<IndexModel> _logger;
        private const int PageSize = 9;

        public IndexModel(PhongThuyShopContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IList<Product> Products { get; set; }

        [BindProperty(SupportsGet = true, Name = "p")]
        public int p { get; set; } = 1;
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string> Materials { get; set; }
        public List<string> SelectedMaterials { get; set; }
        public string SortBy { get; set; }
        public string CurrentSort { get; set; } = "name_asc";
        public string CurrentFilter { get; set; } = "";
        public bool isLoggedIn { get; set; }
        public bool isAdmin { get; set; }
        public string QueryString { get; set; }

        public async Task OnGetAsync(
            string searchTerm,
            decimal? minPrice,
            decimal? maxPrice,
            List<string> materials,
            string sortBy = "name_asc")
        {
            _logger.LogInformation("--- Start OnGetAsync Product Index ---");
            _logger.LogInformation("Input parameters: searchTerm={SearchTerm}, minPrice={MinPrice}, maxPrice={MaxPrice}, sortBy={SortBy}, page={Page}", 
                searchTerm, minPrice, maxPrice, sortBy, p);

            // Store filter parameters
            SearchTerm = searchTerm;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            SelectedMaterials = materials;
            SortBy = sortBy;

            // Get all unique materials for filter
            Materials = await _context.Products
                .Where(p => !string.IsNullOrEmpty(p.Material))
                .Select(p => p.Material)
                .Distinct()
                .ToListAsync();

            // Build query
            var query = _context.Products.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || 
                                       (p.Description != null && p.Description.Contains(searchTerm)));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (materials != null && materials.Any())
            {
                query = query.Where(p => materials.Contains(p.Material));
            }

            // Apply sorting
            query = sortBy switch
            {
                "name_desc" => query.OrderByDescending(p => p.Name),
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Name), // Default: name_asc
            };

            // Get total count for pagination
            var totalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            _logger.LogInformation("Pagination: TotalItems={TotalItems}, PageSize={PageSize}, Calculated TotalPages={TotalPages}", 
                totalItems, PageSize, TotalPages);

            // Ensure current page is valid
            p = Math.Max(1, Math.Min(p, TotalPages));
            _logger.LogInformation("Pagination: Validated CurrentPage={CurrentPage}", p);

            // Get paginated results
            int itemsToSkip = (p - 1) * PageSize;
            _logger.LogInformation("Pagination: Skipping {ItemsToSkip} items for page {CurrentPage}.", itemsToSkip, p);
            
            Products = await query
                .Skip(itemsToSkip)
                .Take(PageSize)
                .ToListAsync();
            
            _logger.LogInformation("Pagination: Fetched {ProductCount} products for page {CurrentPage}.", Products.Count, p);

            // Create query string for pagination, keeping all filters
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(searchTerm)) queryParams.Add($"searchTerm={searchTerm}");
            if (minPrice.HasValue) queryParams.Add($"minPrice={minPrice}");
            if (maxPrice.HasValue) queryParams.Add($"maxPrice={maxPrice}");
            if (SelectedMaterials != null && SelectedMaterials.Any())
            {
                foreach (var material in SelectedMaterials)
                {
                    queryParams.Add($"materials={System.Net.WebUtility.UrlEncode(material)}");
                }
            }
            if (!string.IsNullOrEmpty(sortBy)) queryParams.Add($"sortBy={sortBy}");
            
            QueryString = queryParams.Any() ? "&" + string.Join("&", queryParams) : "";

            // Set login status
            var username = HttpContext.Session.GetString("Username");
            isLoggedIn = !string.IsNullOrEmpty(username);

            var userRole = HttpContext.Session.GetString("UserRole");
            isAdmin = userRole == "Admin";
            
            _logger.LogInformation("--- End OnGetAsync Product Index ---");
        }

        public async Task<IActionResult> OnPostAddToCartAjaxAsync()
        {
            // Kiểm tra quyền admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Admin")
            {
                return new JsonResult(new { success = false, message = "Admin không thể thêm sản phẩm vào giỏ hàng." });
            }

            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return new JsonResult(new { success = false, message = "Bạn cần đăng nhập để thêm vào giỏ hàng." });
            }

            using var reader = new System.IO.StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            try
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<AddToCartRequest>(body);
                if (data == null)
                    return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ." });

                var product = await _context.Products.FindAsync(data.ProductId);
                if (product == null)
                {
                    return new JsonResult(new { success = false, message = "Sản phẩm không tồn tại." });
                }
                if (data.Quantity <= 0 || data.Quantity > product.Quantity)
                {
                    return new JsonResult(new { success = false, message = $"Số lượng không hợp lệ. Chỉ còn {product.Quantity} sản phẩm." });
                }

                var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
                if (cart == null)
                {
                    cart = new Cart { UserId = userId.Value };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == data.ProductId);
                if (cartItem != null)
                {
                    // If item exists, just add one quantity
                    cartItem.Quantity += 1; // Add 1 unit from product list page
                    if (cartItem.Quantity > product.Quantity)
                    {
                        return new JsonResult(new { success = false, message = $"Số lượng vượt quá số lượng có sẵn. Chỉ còn {product.Quantity} sản phẩm." });
                    }
                }
                else
                {
                    cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = data.ProductId,
                        Quantity = data.Quantity // Should be 1 from product list
                    };
                    _context.CartItems.Add(cartItem);
                }
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng!" });
            }
            catch (System.Exception ex)
            {
                return new JsonResult(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        public class AddToCartRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        public async Task<IActionResult> OnPostGetCartCountAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return new JsonResult(new { count = 0 });
            }

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId.Value);
            if (cart == null)
            {
                return new JsonResult(new { count = 0 });
            }

            var cartItemCount = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId)
                .SumAsync(ci => ci.Quantity);

            return new JsonResult(new { count = cartItemCount });
        }
    }
} 