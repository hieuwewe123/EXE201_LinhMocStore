using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace EXE201_LinhMocStore.Pages.Admin.Payment
{
    public class IndexModel : PageModel
    {
        private readonly PhongThuyShopContext _context;
        public List<Models.Payment> Payments { get; set; } = new();

        public IndexModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            Payments = await _context.Payments
                .Include(p => p.Order)
                .ToListAsync();
            return Page();
        }
    }
}
