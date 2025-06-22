using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;

namespace EXE201_LinhMocStore.Pages.Admin.User
{
    public class EditModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        [BindProperty]
        public Models.User UserModel { get; set; } = new();

        public EditModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            UserModel = await _context.Users.FindAsync(id);
            if (UserModel == null)
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
            var userInDb = await _context.Users.FindAsync(UserModel.UserId);
            if (userInDb == null)
                return NotFound();

            userInDb.NormalName = UserModel.NormalName;
            userInDb.Email = UserModel.Email;
            userInDb.Username = UserModel.Username;
            userInDb.PasswordHash = UserModel.PasswordHash;
            userInDb.Address = UserModel.Address;
            userInDb.PhoneNumber = UserModel.PhoneNumber;
            userInDb.Gender = UserModel.Gender;
            userInDb.BirthDate = UserModel.BirthDate;
            userInDb.Status = UserModel.Status;

            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/User/Index");
        }
    }
}
