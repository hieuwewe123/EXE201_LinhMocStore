using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace EXE201_LinhMocStore.Pages.ForgotPassword
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly PhongThuyShopContext _context;
        private readonly IConfiguration _config;

        public ForgotPasswordModel(PhongThuyShopContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Input.Username))
            {
                ErrorMessage = "Vui lòng nhập username.";
                return Page();
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == Input.Username);
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                ErrorMessage = "Không tìm thấy tài khoản hoặc tài khoản chưa có email.";
                return Page();
            }

            // Sinh mã xác nhận
            var code = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("ResetCode", code);
            HttpContext.Session.SetString("ResetUser", user.Username);

            // Gửi email
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("LinhMocStore", _config["Smtp:User"]));
            message.To.Add(new MailboxAddress("", user.Email));
            message.Subject = "Mã xác nhận đặt lại mật khẩu";
            message.Body = new TextPart("plain") { Text = $"Mã xác nhận của bạn là: {code}" };

            using (var client = new SmtpClient())
            {
                client.Connect(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]), false);
                client.Authenticate(_config["Smtp:User"], _config["Smtp:Pass"]);
                client.Send(message);
                client.Disconnect(true);
            }

            SuccessMessage = "Mã xác nhận đã được gửi tới email của bạn. Vui lòng kiểm tra email!";
            // Có thể chuyển hướng sang trang nhập mã xác nhận nếu muốn
            // return RedirectToPage("/ForgotPassword/VerifyCode");
            return Page();
        }

        public class InputModel
        {
            public string Username { get; set; } = string.Empty;
        }
    }
}
