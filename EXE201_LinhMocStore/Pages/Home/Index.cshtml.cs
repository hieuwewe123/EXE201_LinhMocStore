using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace EXE201_LinhMocStore.Pages.Home
{
    public class IndexModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        public IndexModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public List<Product> LatestProducts { get; set; }
        public List<Product> FeaturedProducts { get; set; }
        public List<Blog> FeaturedBlogs { get; set; }

        [TempData]
        public string Message { get; set; }

        public async Task OnGetAsync()
        {
            // Lấy sản phẩm mới nhất
            LatestProducts = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.ProductId)
                .Take(3)
                .ToListAsync();

            // Lấy sản phẩm nổi bật (có thể thêm trường IsFeatured vào database sau)
            FeaturedProducts = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Price)
                .Take(3)
                .ToListAsync();

            // Lấy bài viết nổi bật
            FeaturedBlogs = await _context.Blogs
                .Where(b => b.IsActive && b.IsFeatured)
                .OrderByDescending(b => b.CreatedAt)
                .Take(3)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddToCartAjaxAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
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

        public class AddToCartRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
