using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EXE201_LinhMocStore.Models;
using Microsoft.AspNetCore.Http; // Thêm dòng này để dùng Session
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        public async Task<IActionResult> OnGetAsync(int id)
        {
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

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int productId, int quantity)
        {
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
    }
}
