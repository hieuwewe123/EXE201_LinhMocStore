using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;

namespace EXE201_LinhMocStore.Pages.Admin.Order
{
    public class DeleteModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.Order Order { get; set; } = new();

        public DeleteModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _context.Orders.FindAsync(id);
            if (Order == null)
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
            var orderInDb = await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == Order.OrderId);

            if (orderInDb == null)
                return NotFound();

            // Xóa OrderDetail và Payment trước khi xóa Order
            if (orderInDb.OrderDetails != null)
                _context.OrderDetails.RemoveRange(orderInDb.OrderDetails);
            if (orderInDb.Payments != null)
                _context.Payments.RemoveRange(orderInDb.Payments);

            _context.Orders.Remove(orderInDb);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Order/Index");
        }
    }
}
