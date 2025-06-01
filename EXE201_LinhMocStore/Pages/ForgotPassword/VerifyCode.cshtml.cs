using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using System.Linq;

namespace EXE201_LinhMocStore.Pages.ForgotPassword
{
    public class VerifyCodeModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        public VerifyCodeModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Code { get; set; } = string.Empty;

        [BindProperty]
        public string NewPassword { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            var sessionCode = HttpContext.Session.GetString("ResetCode");
            var username = HttpContext.Session.GetString("ResetUser");

            if (string.IsNullOrEmpty(sessionCode) || string.IsNullOrEmpty(username))
            {
                ErrorMessage = "Phiên xác nhận không hợp lệ.";
                return Page();
            }

            if (Code != sessionCode)
            {
                ErrorMessage = "Mã xác nhận không đúng.";
                return Page();
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                ErrorMessage = "Không tìm thấy tài khoản.";
                return Page();
            }

            user.PasswordHash = NewPassword; // Thực tế nên hash password!
            _context.SaveChanges();

            // Xóa session
            HttpContext.Session.Remove("ResetCode");
            HttpContext.Session.Remove("ResetUser");

            SuccessMessage = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập.";
            return Page();
        }
    }
}
