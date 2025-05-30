using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EXE201_LinhMocStore.Models;
using System.Linq;
using System.Threading.Tasks;

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

            // Lấy sản phẩm liên quan (cùng danh mục)
            RelatedProducts = await _context.Products
                .Where(p => p.CategoryId == Product.CategoryId && p.ProductId != Product.ProductId)
                .OrderByDescending(p => p.ProductId)
                .Take(3)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int productId, int quantity)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = $"/products/details/{productId}" });
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

            // Lấy giỏ hàng của user hiện tại
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // Tạo giỏ hàng mới nếu chưa có
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                // Cập nhật số lượng nếu đã có
                cartItem.Quantity += quantity;
                if (cartItem.Quantity > product.Quantity)
                {
                    TempData["Error"] = "Số lượng vượt quá số lượng có sẵn";
                    return RedirectToPage(new { id = productId });
                }
            }
            else
            {
                // Thêm mới vào giỏ hàng
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