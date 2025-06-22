using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EXE201_LinhMocStore.Models;

namespace EXE201_LinhMocStore.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        public DashboardModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        // Thống kê tổng quan
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalBlogs { get; set; }

        // Thống kê đơn hàng
        public int PendingPaymentOrders { get; set; }
        public int AwaitingConfirmationOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }

        // Đơn hàng gần đây
        public List<Models.Order> RecentOrders { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToPage("/login");
            }

            // Tính toán thống kê tổng quan
            TotalProducts = await _context.Products.CountAsync();
            TotalOrders = await _context.Orders.CountAsync();
            TotalUsers = await _context.Users.CountAsync();
            TotalBlogs = await _context.Blogs.CountAsync();

            // Tính toán thống kê đơn hàng
            PendingPaymentOrders = await _context.Orders.CountAsync(o => o.Status == "PendingPayment");
            AwaitingConfirmationOrders = await _context.Orders.CountAsync(o => o.Status == "AwaitingConfirmation");
            ShippedOrders = await _context.Orders.CountAsync(o => o.Status == "Shipped");
            DeliveredOrders = await _context.Orders.CountAsync(o => o.Status == "Delivered");

            // Lấy đơn hàng gần đây
            RecentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();

            return Page();
        }
    }
}
