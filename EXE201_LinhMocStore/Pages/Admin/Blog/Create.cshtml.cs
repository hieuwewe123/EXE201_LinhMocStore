using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;

namespace EXE201_LinhMocStore.Pages.Admin.Blog
{
    public class CreateModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.Blog Blog { get; set; } = new();

        public CreateModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            Blog.CreatedAt = DateTime.Now;
            _context.Blogs.Add(Blog);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Blog/Index");
        }
    }
}
