using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.AspNetCore.Http;

using System.Linq;

namespace EXE201_LinhMocStore.Pages.Login
{
    public class LoginModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        public LoginModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == Input.Username);

            if (user != null && Input.Password == user.PasswordHash)
            {
                HttpContext.Session.SetString("Username", user.Username ?? "");
                HttpContext.Session.SetInt32("UserId", user.UserId);
                // Phân quyền dựa trên username
                if (user.Username == "admin")
                    HttpContext.Session.SetString("UserRole", "Admin");
                else
                    HttpContext.Session.SetString("UserRole", "User");

                return RedirectToPage("/Index");
            }
            else
            {
                ErrorMessage = "Invalid username or password.";
                return Page();
            }
        }



        public class InputModel
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
