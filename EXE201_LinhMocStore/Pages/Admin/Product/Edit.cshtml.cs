using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;

namespace EXE201_LinhMocStore.Pages.Admin.Product
{
    public class EditModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.Product Product { get; set; } = new();

        public List<EXE201_LinhMocStore.Models.Category> Categories { get; set; } = new();

        public EditModel(PhongThuyShopContext context, IWebHostEnvironment env)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Product = await _context.Products.FindAsync(id);
            if (Product == null)
                return NotFound();

            Categories = await _context.Categories.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var productInDb = await _context.Products.FindAsync(Product.ProductId);
            if (productInDb == null)
                return NotFound();

            productInDb.Name = Product.Name;
            productInDb.Price = Product.Price;
            productInDb.Quantity = Product.Quantity;
            productInDb.Description = Product.Description;
            productInDb.Material = Product.Material;
            productInDb.Size = Product.Size;
            productInDb.CategoryId = Product.CategoryId;
            productInDb.Image = Product.Image; // Cập nhật link ảnh

            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Product/Index");
        }
    }
}
