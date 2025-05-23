using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;

namespace EXE201_LinhMocStore.Pages.Admin.Category
{
    public class IndexModel : PageModel
    {
        private readonly PhongThuyShopContext _context;
        public List<Models.Category> Categories { get; set; } = new();

        public IndexModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories.ToListAsync();
        }
    }
}
