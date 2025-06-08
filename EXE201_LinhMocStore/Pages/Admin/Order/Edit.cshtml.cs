using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;

namespace EXE201_LinhMocStore.Pages.Admin.Order
{
    public class EditModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.Order Order { get; set; } = new();

        public List<Models.User> Users { get; set; } = new();

        public EditModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _context.Orders.FindAsync(id);
            if (Order == null)
                return NotFound();

            Users = await _context.Users.ToListAsync();
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
            var orderInDb = await _context.Orders.FindAsync(Order.OrderId);
            if (orderInDb == null)
                return NotFound();

            orderInDb.UserId = Order.UserId;
            orderInDb.OrderDate = Order.OrderDate;
            orderInDb.Status = Order.Status;
            orderInDb.TotalAmount = Order.TotalAmount;

            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Order/Index");
        }
    }
}
