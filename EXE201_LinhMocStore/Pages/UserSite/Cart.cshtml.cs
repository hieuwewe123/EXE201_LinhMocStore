using EXE201_LinhMocStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EXE201_LinhMocStore.Pages.UserSite
{
    public class CartModel : PageModel
    {
        private readonly PhongThuyShopContext _context;
        private readonly User CurrentUser;

        public CartModel(PhongThuyShopContext context)
        {
            _context = context;
            CurrentUser = new User { UserId = 2 };
        }
        public List<CartItem> CartProducts { get; set; } = new List<CartItem>();

        [BindProperty]
        public List<int> SelectedIds { get; set; } = new List<int>();

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public int Quantity { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Cart card = await _context.Carts.Where(c=>c.UserId == CurrentUser.UserId).FirstOrDefaultAsync();
            CartProducts = await _context.CartItems
                .Where(ci => ci.CartId == card.CartId)
                .Include(ci => ci.Product)
                .ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync()
        {
            var cartItem = await _context.CartItems.FindAsync(Id);
            if (cartItem == null) return NotFound();

            cartItem.Quantity = Quantity;
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostDeleteSingleAsync(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckoutAsync()
        {
            // Lấy các CartId của user
            var userCartIds = await _context.Carts
                .Where(c => c.UserId == CurrentUser.UserId)
                .Select(c => c.CartId)
                .ToListAsync();

            // Lấy CartItems được chọn
            var selectedItems = await _context.CartItems
                .Where(ci => ci.CartId != null && userCartIds.Contains(ci.CartId.Value) && SelectedIds.Contains(ci.CartItemId))
                .Include(ci => ci.Product)
                .ToListAsync();

            if (!selectedItems.Any())
            {
                ModelState.AddModelError(string.Empty, "Bạn chưa chọn sản phẩm để thanh toán.");

                // Load lại CartProducts để hiển thị lại trang
                CartProducts = await _context.CartItems
                    .Where(ci => ci.CartId != null && userCartIds.Contains(ci.CartId.Value))
                    .Include(ci => ci.Product)
                    .ToListAsync();

                return Page();
            }

            // TODO: Xử lý thanh toán với selectedItems

            return RedirectToPage("/UserSite/Checkout");
        }
    }
}