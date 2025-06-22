using EXE201_LinhMocStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EXE201_LinhMocStore.Pages.UserSite
{
    public class MyOrdersModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        public MyOrdersModel(PhongThuyShopContext context)
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

        public List<Order> Orders { get; set; } = new List<Order>();
        public string Message { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            // Kiểm tra đăng nhập
            if (!IsUserLoggedIn)
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
                return RedirectToPage("/Login/Login", new { returnUrl = returnUrl });
            }

            // Lấy tất cả đơn hàng của user hiện tại
            Orders = await _context.Orders
                .Where(o => o.UserId == CurrentUserId.Value)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Load order details cho mỗi order
            foreach (var order in Orders)
            {
                var orderDetails = await _context.OrderDetails
                    .Where(od => od.OrderId == order.OrderId)
                    .Include(od => od.Product)
                    .ToListAsync();
                
                ViewData[$"OrderDetails_{order.OrderId}"] = orderDetails;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCancelOrderAsync(int orderId)
        {
            if (!IsUserLoggedIn)
            {
                return RedirectToPage("/Login/Login");
            }

            try
            {
                // Kiểm tra đơn hàng có tồn tại và thuộc về user hiện tại không
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == CurrentUserId.Value);

                if (order == null)
                {
                    TempData["Message"] = "Không tìm thấy đơn hàng.";
                    return RedirectToPage();
                }

                // Chỉ cho phép hủy đơn hàng ở trạng thái PendingPayment hoặc AwaitingConfirmation
                if (order.Status != OrderStatus.PendingPayment && order.Status != OrderStatus.AwaitingConfirmation)
                {
                    TempData["Message"] = "Không thể hủy đơn hàng ở trạng thái này.";
                    return RedirectToPage();
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Lấy order details để hoàn trả số lượng sản phẩm
                    var orderDetails = await _context.OrderDetails
                        .Where(od => od.OrderId == orderId)
                        .Include(od => od.Product)
                        .ToListAsync();

                    // Hoàn trả số lượng sản phẩm về kho
                    foreach (var detail in orderDetails)
                    {
                        if (detail.Product != null)
                        {
                            detail.Product.Quantity += detail.Quantity;
                            _context.Products.Update(detail.Product);
                        }
                    }

                    // Cập nhật trạng thái đơn hàng thành Cancelled
                    order.Status = OrderStatus.Cancelled;
                    _context.Orders.Update(order);

                    // Cập nhật payment nếu có
                    var payment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.OrderId == orderId);
                    
                    if (payment != null)
                    {
                        payment.isCompleted = false;
                        _context.Payments.Update(payment);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Message"] = "Đã hủy đơn hàng thành công.";
                    return RedirectToPage();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Message"] = "Có lỗi xảy ra khi hủy đơn hàng. Vui lòng thử lại.";
                    return RedirectToPage();
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi hủy đơn hàng. Vui lòng thử lại.";
                return RedirectToPage();
            }
        }
    }
} 