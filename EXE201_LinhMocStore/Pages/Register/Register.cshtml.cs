using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;

namespace EXE201_LinhMocStore.Pages.Register
{
    public class RegisterModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        public RegisterModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Input.Username) || string.IsNullOrWhiteSpace(Input.Password) || string.IsNullOrWhiteSpace(Input.Email))
            {
                ErrorMessage = "Vui lòng nhập đầy đủ thông tin.";
                return Page();
            }

            if (_context.Users.Any(u => u.Username == Input.Username))
            {
                ErrorMessage = "Username đã tồn tại.";
                return Page();
            }

            var user = new User
            {
                Username = Input.Username,
                PasswordHash = Input.Password, // Lưu plain text, demo thôi! Thực tế nên hash password!
                Email = Input.Email
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            SuccessMessage = "Đăng ký thành công! Bạn có thể đăng nhập.";
            return Page();
        }

        public class InputModel
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }
    }
}
