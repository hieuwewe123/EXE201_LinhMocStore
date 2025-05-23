using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;

namespace EXE201_LinhMocStore.Pages.Admin.Category
{
    public class CreateModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.Category Category { get; set; } = new();

        public CreateModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Categories.Add(Category);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Category/Index");
        }
    }
}
