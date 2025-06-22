using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EXE201_LinhMocStore.Models;

namespace EXE201_LinhMocStore.Pages.Admin.Order
{
    public class DetailsModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        public DetailsModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public Models.Order Order { get; set; } = new();
        public List<OrderDetail> OrderDetails { get; set; } = new();
        public Models.Payment? Payment { get; set; }

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
            {
                return NotFound();
            }

            OrderDetails = await _context.OrderDetails
                .Where(od => od.OrderId == id)
                .Include(od => od.Product)
                .ToListAsync();

            Payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == id);

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string newStatus, string? statusNote)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Lưu trạng thái cũ để so sánh
            var oldStatus = order.Status;
            order.Status = newStatus;

            // Cập nhật thời gian nếu cần
            if (newStatus == "Confirmed")
            {
                // Có thể thêm logic cập nhật thời gian xác nhận
            }
            else if (newStatus == "Shipped")
            {
                // Có thể thêm logic cập nhật thời gian giao hàng
            }
            else if (newStatus == "Delivered")
            {
                // Có thể thêm logic cập nhật thời gian hoàn thành
            }

            await _context.SaveChangesAsync();

            // Có thể thêm logic ghi log hoặc gửi thông báo cho khách hàng
            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng từ '{GetStatusText(oldStatus)}' thành '{GetStatusText(newStatus)}'";

            return RedirectToPage(new { id = id });
        }

        private string GetStatusText(string status)
        {
            return status switch
            {
                "PendingPayment" => "Chờ thanh toán",
                "AwaitingConfirmation" => "Chờ xác nhận",
                "Confirmed" => "Đã xác nhận",
                "Shipped" => "Đang giao",
                "Delivered" => "Đã giao",
                "Cancelled" => "Đã hủy",
                _ => status
            };
        }
    }
} 