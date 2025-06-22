using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;

namespace EXE201_LinhMocStore.Pages.Admin.Payment
{
    public class DeleteModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.Payment Payment { get; set; } = new();

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

            Payment = await _context.Payments.FindAsync(id);
            if (Payment == null)
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

            var paymentInDb = await _context.Payments.FindAsync(Payment.PaymentId);
            if (paymentInDb == null)
                return NotFound();

            _context.Payments.Remove(paymentInDb);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Payment/Index");
        }
    }
}
