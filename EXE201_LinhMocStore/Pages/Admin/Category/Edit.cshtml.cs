using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;

namespace EXE201_LinhMocStore.Pages.Admin.Category
{
    public class EditModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.Category Category { get; set; } = new();

        public EditModel(PhongThuyShopContext context)
        {
            _context = context;
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
            var catInDb = await _context.Categories.FindAsync(Category.CategoryId);
            if (catInDb == null)
                return NotFound();

            catInDb.Name = Category.Name;
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Category/Index");
        }
    }
}
