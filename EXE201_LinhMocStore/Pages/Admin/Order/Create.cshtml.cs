using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;

namespace EXE201_LinhMocStore.Pages.Admin.Order
{
    public class CreateModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.Order Order { get; set; } = new();

        public List<Models.User> Users { get; set; } = new();

        public CreateModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Users = await _context.Users.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Users = await _context.Users.ToListAsync();
                return Page();
            }

            _context.Orders.Add(Order);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Order/Index");
        }
    }
}
