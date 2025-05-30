using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
