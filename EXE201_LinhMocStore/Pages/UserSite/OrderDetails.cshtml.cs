using EXE201_LinhMocStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EXE201_LinhMocStore.Pages.UserSite
{
    public class OrderDetailsModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        public OrderDetailsModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        // Lấy UserId từ session
        private int? CurrentUserId
        {
            get => HttpContext.Session.GetInt32("UserId");
        }

        // Kiểm tra người dùng đã đăng nhập chưa
        private bool IsUserLoggedIn
        {
            get => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username")) && CurrentUserId.HasValue;
        }

        public Order Order { get; set; } = new Order();
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public Payment? Payment { get; set; }
        public bool isLoggedIn { get; set; }

        public async Task<IActionResult> OnGetAsync(int orderId)
        {
            // Kiểm tra đăng nhập
            if (!IsUserLoggedIn)
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
                return RedirectToPage("/Login/Login", new { returnUrl = returnUrl });
            }

            // Lấy thông tin đơn hàng
            Order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (Order == null)
            {
                return NotFound();
            }

            // Kiểm tra xem đơn hàng có thuộc về user hiện tại không
            if (Order.UserId != CurrentUserId.Value)
            {
                return RedirectToPage("/Error", new { message = "Bạn không có quyền truy cập đơn hàng này." });
            }

            // Lấy order details
            OrderDetails = await _context.OrderDetails
                .Where(od => od.OrderId == orderId)
                .Include(od => od.Product)
                .ToListAsync();

            // Lấy thông tin payment
            Payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            // Set login status
            isLoggedIn = true; // User đã đăng nhập mới vào được trang này

            return Page();
        }

        public async Task<IActionResult> OnPostGetCartCountAsync()
        {
            if (!IsUserLoggedIn)
            {
                return new JsonResult(new { count = 0 });
            }

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == CurrentUserId.Value);
            if (cart == null)
            {
                return new JsonResult(new { count = 0 });
            }

            var cartItemCount = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId)
                .SumAsync(ci => ci.Quantity);

            return new JsonResult(new { count = cartItemCount });
        }
    }
} 