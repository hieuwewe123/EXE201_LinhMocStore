using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;

namespace EXE201_LinhMocStore.Pages.Admin.Payment
{
    public class CreateModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.Payment Payment { get; set; } = new();

        public List<Models.Order> Orders { get; set; } = new();

        public CreateModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Orders = await _context.Orders.Include(o => o.User).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Orders = await _context.Orders.Include(o => o.User).ToListAsync();
                return Page();
            }

            Payment.CreatedAt = DateTime.Now;
            _context.Payments.Add(Payment);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Payment/Index");
        }
    }
}
