using EXE201_LinhMocStore.Models;
using EXE201_LinhMocStore.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EXE201_LinhMocStore.Pages.UserSite
{
    public class CartModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        public CartModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        // Lấy UserId từ session
        private int? CurrentUserId
        {
            get => HttpContext.Session.GetInt32("UserId");
        }

        public List<CartItem> CartProducts { get; set; } = new List<CartItem>();

        [BindProperty]
        public List<int> SelectedIds { get; set; } = new List<int>();

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public int Quantity { get; set; }
        public string Message { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (CurrentUserId == null)
                return RedirectToPage("/Login/Login");

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == CurrentUserId.Value);

            if (cart == null)
            {
                CartProducts = new List<CartItem>();
                return Page();
            }

            CartProducts = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId)
                .Include(ci => ci.Product)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync()
        {
            if (CurrentUserId == null)
                return RedirectToPage("/Login/Login");

            var cartItem = await _context.CartItems.FindAsync(Id);
            if (cartItem == null)
                return NotFound();

            cartItem.Quantity = Quantity;
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostDeleteSingleAsync(int id)
        {
            if (CurrentUserId == null)
                return RedirectToPage("/Login/Login");

            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (CurrentUserId == null)
                return RedirectToPage("/Login/Login");

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == CurrentUserId.Value);

            if (cart == null)
            {
                CartProducts = new List<CartItem>();
                return Page();
            }

            List<CartItem> SelectedProducts = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => SelectedIds.Contains(c.CartItemId) && c.CartId == cart.CartId)
                .ToListAsync();

            foreach (var item in SelectedProducts)
            {
                if (item.Product.Quantity < item.Quantity)
                {
                    Message = $"Sản phẩm \"{item.Product.Name}\" không đủ hàng trong kho.";

                    // Gọi lại logic như OnGetAsync
                    CartProducts = await _context.CartItems
                        .Where(ci => ci.CartId == cart.CartId)
                        .Include(ci => ci.Product)
                        .ToListAsync();

                    return Page();
                }
            }

            // Nếu không có lỗi hết hàng thì xử lý tiếp đơn hàng
            decimal TotalAmount = 0;
            foreach (var item in SelectedProducts)
            {
                item.Product.Quantity -= item.Quantity;
                TotalAmount += item.Quantity * item.Product.Price;
                _context.Products.Update(item.Product);
            }

            string TransferInfo = Commons.GenerateBankInfo(6);

            Order newOrder = new Order
            {
                UserId = CurrentUserId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.PendingPayment,
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            Payment payment = new Payment
            {
                OrderId = newOrder.OrderId,
                Price = TotalAmount,
                TransactionCode = TransferInfo,
                CreatedAt = DateTime.Now,
                isCompleted = false
            };
            _context.Payments.Add(payment);

            List<OrderDetail> orderDetails = SelectedProducts.Select(item => new OrderDetail
            {
                OrderId = newOrder.OrderId,
                ProductId = item.Product.ProductId,
                Quantity = item.Quantity,
                Price = item.Product.Price
            }).ToList();

            _context.OrderDetails.AddRange(orderDetails);
            await _context.SaveChangesAsync();

            return RedirectToPage("/UserSite/Checkout", new { OrderId = newOrder.OrderId });
        }
    }
}
