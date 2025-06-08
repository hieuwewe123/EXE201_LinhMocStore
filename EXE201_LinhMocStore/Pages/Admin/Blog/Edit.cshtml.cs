using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;

namespace EXE201_LinhMocStore.Pages.Admin.Blog
{
    public class EditModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.Blog Blog { get; set; } = new();

        public EditModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Blog = await _context.Blogs.FindAsync(id);
            if (Blog == null)
                return NotFound();
            return Page();
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

        public async Task<IActionResult> OnPostAsync()
        {
            var blogInDb = await _context.Blogs.FindAsync(Blog.BlogId);
            if (blogInDb == null)
                return NotFound();

            blogInDb.Title = Blog.Title;
            blogInDb.Content = Blog.Content;
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Blog/Index");
        }
    }
}
