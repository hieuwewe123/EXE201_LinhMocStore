using System.Security.Claims;
using EXE201_LinhMocStore.Models;
using EXE201_LinhMocStore.Services;
using EXE201_LinhMocStore.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EXE201_LinhMocStore.Pages.UserSite
{
    public class CheckoutModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        public CheckoutModel(PhongThuyShopContext context, BankSettings bankSettings)
        {
            _context = context;
            bankInfo = bankSettings;
        }

        // Lấy UserId từ session
        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        // Kiểm tra người dùng đã đăng nhập chưa
        private bool IsUserLoggedIn
        {
            get => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username")) && CurrentUserId.HasValue;
        }

        public List<OrderDetail> SelectedProducts { get; set; } = new();
        public BankSettings bankInfo { get; set; }
        public Order Order { get; set; } = new();
        public Payment Payment { get; set; } = new();
        public User? CurrentUser { get; set; }

        public IActionResult OnGet(int orderId)
        {
            // Kiểm tra đăng nhập
            if (!IsUserLoggedIn)
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
                return RedirectToPage("/Login/Login", new { returnUrl = returnUrl });
            }

            // Lấy thông tin đơn hàng
            Order = _context.Orders
                .Include(o => o.User)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (Order == null)
            {
                return NotFound();
            }

            // Kiểm tra xem đơn hàng có thuộc về user hiện tại không
            if (Order.UserId != CurrentUserId.Value)
            {
                return RedirectToPage("/Error", new { message = "Bạn không có quyền truy cập đơn hàng này." });
            }

            Payment = _context.Payments.FirstOrDefault(o => o.OrderId == orderId);
            SelectedProducts = _context.OrderDetails
                .Where(o => o.OrderId == orderId)
                .Include(o => o.Product)
                .ToList();

            // Lấy thông tin user hiện tại
            CurrentUser = _context.Users.FirstOrDefault(u => u.UserId == CurrentUserId.Value);

            return Page();
        }

        public IActionResult OnPost(int paymentId, int orderId)
        {
            // Kiểm tra đăng nhập
            if (!IsUserLoggedIn)
            {
                return RedirectToPage("/Login/Login", new { returnUrl = "/UserSite/Cart" });
            }

            // Kiểm tra xem đơn hàng có thuộc về user hiện tại không
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order?.UserId != CurrentUserId.Value)
            {
                return RedirectToPage("/Error", new { message = "Bạn không có quyền thực hiện thao tác này." });
            }

            Payment payment = _context.Payments.FirstOrDefault(p => p.PaymentId == paymentId);
            if (payment == null)
            {
                return NotFound();
            }

            payment.isCompleted = true;
            order.Status = OrderStatus.AwaitingConfirmation;

            _context.SaveChanges();
            return RedirectToPage("/Home/Index");
        }
    }
}
