using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace EXE201_LinhMocStore.Pages.Admin.Order
{
    public class IndexModel : PageModel
    {
        private readonly PhongThuyShopContext _context;
        public List<Models.Order> Orders { get; set; } = new();

        public IndexModel(PhongThuyShopContext context)
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

        public async Task OnGetAsync()
        {
            Orders = await _context.Orders
                .Include(o => o.User)
                .ToListAsync();
        }
    }
}
