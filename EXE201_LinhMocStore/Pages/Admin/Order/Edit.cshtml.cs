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
        public Models.Order Order { get; set; } = default!;

        public EditModel(PhongThuyShopContext context)
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

            Order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (Order == null)
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

            var orderInDb = await _context.Orders.FindAsync(Order.OrderId);
            if (orderInDb == null)
                return NotFound();

            // Chỉ cho phép cập nhật status và thông tin địa chỉ giao hàng
            orderInDb.Status = Order.Status;
            orderInDb.ReceiverName = Order.ReceiverName;
            orderInDb.ReceiverPhone = Order.ReceiverPhone;
            orderInDb.ShippingAddress = Order.ShippingAddress;
            orderInDb.DeliveryNote = Order.DeliveryNote;

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Đã cập nhật đơn hàng thành công!";
            return RedirectToPage("/Admin/Order/Index");
        }
    }
}
