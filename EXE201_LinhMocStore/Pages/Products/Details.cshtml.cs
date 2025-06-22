using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EXE201_LinhMocStore.Models;
using Microsoft.AspNetCore.Http; // Thêm dòng này để dùng Session
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EXE201_LinhMocStore.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        public DetailsModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public Product? Product { get; set; }
        public List<Product>? RelatedProducts { get; set; }
        public string? Message { get; set; }
        public bool isLoggedIn { get; set; }
        public bool isAdmin { get; set; }
        public User? CurrentUser { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Check for TempData messages
            if (TempData["Error"] != null)
            {
                Message = TempData["Error"].ToString();
            }
            else if (TempData["Success"] != null)
            {
                Message = TempData["Success"].ToString();
            }

            Product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (Product == null)
            {
                return NotFound();
            }

            RelatedProducts = await _context.Products
                .Where(p => p.CategoryId == Product.CategoryId && p.ProductId != Product.ProductId)
                .OrderByDescending(p => p.ProductId)
                .Take(3)
                .ToListAsync();

            // Set login status
            var username = HttpContext.Session.GetString("Username");
            isLoggedIn = !string.IsNullOrEmpty(username);
            
            // Set admin status
            var userRole = HttpContext.Session.GetString("UserRole");
            isAdmin = userRole == "Admin";

            // Load current user info if logged in
            if (isLoggedIn && HttpContext.Session.GetInt32("UserId") != null)
            {
                CurrentUser = await _context.Users.FindAsync(HttpContext.Session.GetInt32("UserId"));
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int productId, int quantity)
        {
            // Kiểm tra quyền admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Admin")
            {
                TempData["Error"] = "Admin không thể thêm sản phẩm vào giỏ hàng";
                return RedirectToPage(new { id = productId });
            }

            // 👉 Kiểm tra đăng nhập bằng Session
            var username = HttpContext.Session.GetString("Username");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(username) || userId == null)
            {
                return RedirectToPage("/Login/Login", new { returnUrl = $"/products/details/{productId}" });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            if (quantity <= 0 || quantity > product.Quantity)
            {
                TempData["Error"] = "Số lượng không hợp lệ";
                return RedirectToPage(new { id = productId });
            }

            // Lấy giỏ hàng của người dùng hiện tại
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId.Value };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                if (cartItem.Quantity > product.Quantity)
                {
                    TempData["Error"] = "Số lượng vượt quá số lượng có sẵn";
                    return RedirectToPage(new { id = productId });
                }
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng";
            return RedirectToPage(new { id = productId });
        }

        public async Task<IActionResult> OnPostAddToCartAjaxAsync()
        {
            // Kiểm tra quyền admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Admin")
            {
                return new JsonResult(new { success = false, message = "Admin không thể thêm sản phẩm vào giỏ hàng." });
            }

            var username = HttpContext.Session.GetString("Username");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(username) || userId == null)
            {
                return new JsonResult(new { success = false, message = "Bạn cần đăng nhập để thêm vào giỏ hàng." });
            }

            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            try
            {
                var data = JsonSerializer.Deserialize<AddToCartRequest>(body);
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
                    cartItem.Quantity += data.Quantity;
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
                        Quantity = data.Quantity
                    };
                    _context.CartItems.Add(cartItem);
                }
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng!" });
            }
            catch
            {
                return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ." });
            }
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

        public class AddToCartRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
