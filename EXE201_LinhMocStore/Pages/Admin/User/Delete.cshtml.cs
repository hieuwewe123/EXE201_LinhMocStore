using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;

namespace EXE201_LinhMocStore.Pages.Admin.User
{
    public class DeleteModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.User User { get; set; } = new();

        public DeleteModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            User = await _context.Users.FindAsync(id);
            if (User == null)
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
            var userInDb = await _context.Users
                .Include(u => u.Carts)
                    .ThenInclude(c => c.CartItems)
                .Include(u => u.Notifications)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderDetails)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.Payments)
                .FirstOrDefaultAsync(u => u.UserId == User.UserId);

            if (userInDb == null)
                return NotFound();

            // Xóa CartItem trong từng Cart
            if (userInDb.Carts != null)
            {
                foreach (var cart in userInDb.Carts)
                {
                    if (cart.CartItems != null)
                        _context.CartItems.RemoveRange(cart.CartItems);
                }
                _context.Carts.RemoveRange(userInDb.Carts);
            }

            // Xóa Notification liên quan
            if (userInDb.Notifications != null)
                _context.Notifications.RemoveRange(userInDb.Notifications);

            // Xóa OrderDetail và Payment trong từng Order
            if (userInDb.Orders != null)
            {
                foreach (var order in userInDb.Orders)
                {
                    if (order.OrderDetails != null)
                        _context.OrderDetails.RemoveRange(order.OrderDetails);
                    if (order.Payments != null)
                        _context.Payments.RemoveRange(order.Payments);
                }
                _context.Orders.RemoveRange(userInDb.Orders);
            }

            // Xóa user
            _context.Users.Remove(userInDb);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/User/Index");
        }
    }
    }
