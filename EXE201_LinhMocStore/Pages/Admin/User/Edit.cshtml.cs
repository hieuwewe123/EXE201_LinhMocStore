using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;

namespace EXE201_LinhMocStore.Pages.Admin.User
{
    public class EditModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.User User { get; set; } = new();

        public EditModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            User = await _context.Users.FindAsync(id);
            if (User == null)
                return NotFound();
            return Page();
        }
        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToPage("/Login");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userInDb = await _context.Users.FindAsync(User.UserId);
            if (userInDb == null)
                return NotFound();

            userInDb.NormalName = User.NormalName;
            userInDb.Email = User.Email;
            userInDb.Username = User.Username;
            userInDb.PasswordHash = User.PasswordHash;
            userInDb.Address = User.Address;
            userInDb.PhoneNumber = User.PhoneNumber;
            userInDb.Gender = User.Gender;
            userInDb.BirthDate = User.BirthDate;
            userInDb.Status = User.Status;

            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/User/Index");
        }
    }
}
