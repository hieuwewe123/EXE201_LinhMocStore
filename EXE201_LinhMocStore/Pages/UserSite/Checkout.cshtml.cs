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

        // L?y UserId t? session
        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");
        public List<OrderDetail> SelectedProducts { get; set; } = new();
        public BankSettings bankInfo { get; set; }
        public Order Order { get; set; } = new();
        public Payment Payment { get; set; } = new();
        public User? CurrentUser { get; set; }
        public IActionResult OnGet(int orderId)
        {
            Order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            Payment = _context.Payments.FirstOrDefault(o => o.OrderId == orderId);
            SelectedProducts = _context.OrderDetails.Where(o => o.OrderId == orderId)
                .Include(o => o.Product).ToList();
            return Page();
        }

        public IActionResult OnPost(int paymentId, int orderId)
        {
            Payment payment = _context.Payments.FirstOrDefault(p => p.PaymentId == paymentId);
            Order order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);

            payment.isCompleted = true;
            order.Status = OrderStatus.AwaitingConfirmation;

            _context.SaveChanges();
            return RedirectToPage("/Home/Index");
        }
    }
}
