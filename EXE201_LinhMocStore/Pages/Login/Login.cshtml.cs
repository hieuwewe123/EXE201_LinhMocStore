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
        
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public void OnGet()
        {
            // Nếu user đã đăng nhập, chuyển hướng về trang chủ hoặc returnUrl
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
            {
                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    Response.Redirect(ReturnUrl);
                }
                else
                {
                    Response.Redirect("/");
                }
            }
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

                // Nếu có cartItems trong localStorage, chuyển hướng sang trang trung gian để đồng bộ
                string syncUrl = "/UserSite/Cart?sync=1";
                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    syncUrl += "&returnUrl=" + System.Net.WebUtility.UrlEncode(ReturnUrl);
                }
                return Redirect(syncUrl);
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
