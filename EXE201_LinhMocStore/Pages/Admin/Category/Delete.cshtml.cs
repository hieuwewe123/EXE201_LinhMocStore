using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;

namespace EXE201_LinhMocStore.Pages.Admin.Category
{
    public class DeleteModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.Category Category { get; set; } = new();

        public DeleteModel(PhongThuyShopContext context)
        {
            _context = context;
        }
        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToPage("/Login");
            }
            return Page();
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Category = await _context.Categories.FindAsync(id);
            if (Category == null)
                return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var catInDb = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == Category.CategoryId);

            if (catInDb == null)
                return NotFound();

            // Nếu có sản phẩm thuộc danh mục này, cần xóa hoặc xử lý trước
            if (catInDb.Products != null && catInDb.Products.Any())
            {
                ModelState.AddModelError(string.Empty, "Không thể xóa danh mục vì còn sản phẩm liên quan.");
                return Page();
            }

            _context.Categories.Remove(catInDb);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Category/Index");
        }
    }
}
