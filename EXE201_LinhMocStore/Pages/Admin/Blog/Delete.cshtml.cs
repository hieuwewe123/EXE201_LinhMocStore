using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;

namespace EXE201_LinhMocStore.Pages.Admin.Blog
{
    public class DeleteModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.Blog Blog { get; set; } = new();

        public DeleteModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            Blog = await _context.Blogs.FindAsync(id);
            if (Blog == null)
                return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            var blogInDb = await _context.Blogs.FindAsync(Blog.BlogId);
            if (blogInDb == null)
                return NotFound();

            _context.Blogs.Remove(blogInDb);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Blog/Index");
        }
    }
}
