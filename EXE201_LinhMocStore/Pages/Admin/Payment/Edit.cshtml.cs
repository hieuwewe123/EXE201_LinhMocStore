using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;

namespace EXE201_LinhMocStore.Pages.Admin.Payment
{
    public class EditModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.Payment Payment { get; set; } = new();

        public List<Models.Order> Orders { get; set; } = new();

        public EditModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Payment = await _context.Payments.FindAsync(id);
            if (Payment == null)
                return NotFound();

            Orders = await _context.Orders.Include(o => o.User).ToListAsync();
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
            var paymentInDb = await _context.Payments.FindAsync(Payment.PaymentId);
            if (paymentInDb == null)
                return NotFound();

            paymentInDb.OrderId = Payment.OrderId;
            paymentInDb.Content = Payment.Content;
            paymentInDb.Price = Payment.Price;
            paymentInDb.TransactionCode = Payment.TransactionCode;
            paymentInDb.Status = Payment.Status;

            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Payment/Index");
        }
    }
}
