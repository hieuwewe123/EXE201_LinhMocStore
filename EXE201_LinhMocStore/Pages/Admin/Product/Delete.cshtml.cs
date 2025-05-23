using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;

namespace EXE201_LinhMocStore.Pages.Admin.Product
{
    public class DeleteModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.Product Product { get; set; } = new();

        public DeleteModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Product = await _context.Products.FindAsync(id);
            if (Product == null)
                return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var productInDb = await _context.Products
                .Include(p => p.CartItems)
                .Include(p => p.OrderDetails)
                .FirstOrDefaultAsync(p => p.ProductId == Product.ProductId);

            if (productInDb == null)
                return NotFound();

            // Xóa các CartItem liên quan
            if (productInDb.CartItems != null)
                _context.CartItems.RemoveRange(productInDb.CartItems);

            // Xóa các OrderDetail liên quan (nếu muốn)
            if (productInDb.OrderDetails != null)
                _context.OrderDetails.RemoveRange(productInDb.OrderDetails);

            // Xóa sản phẩm
            _context.Products.Remove(productInDb);

            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Product/Index");
        }

    }
}
