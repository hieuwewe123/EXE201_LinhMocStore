using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;

namespace EXE201_LinhMocStore.Pages.Admin.Product
{
    public class CreateModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.Product Product { get; set; } = new();

        public List<EXE201_LinhMocStore.Models.Category> Categories { get; set; } = new();

        public CreateModel(PhongThuyShopContext context, IWebHostEnvironment env)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories.ToListAsync();
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
            if (!ModelState.IsValid)
            {
                Categories = await _context.Categories.ToListAsync();
                return Page();
            }

            // Không xử lý ImageFile nữa, chỉ lưu link ảnh từ Product.Image
            _context.Products.Add(Product);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Product/Index");
        }
    }
}
