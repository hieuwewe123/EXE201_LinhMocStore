using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EXE201_LinhMocStore.Models;
using System.ComponentModel.DataAnnotations;

namespace EXE201_LinhMocStore.Pages.Profile
{
    public class IndexModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        public IndexModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public User? CurrentUser { get; set; }
        public List<Order> UserOrders { get; set; } = new List<Order>();
        public List<Order> RecentOrders { get; set; } = new();
        public bool isLoggedIn { get; set; }

        [BindProperty]
        public ProfileInputModel ProfileInput { get; set; } = new();

        [BindProperty]
        public PasswordInputModel PasswordInput { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login/Login");
            }

            CurrentUser = await _context.Users.FindAsync(userId.Value);
            if (CurrentUser == null)
            {
                return RedirectToPage("/Login/Login");
            }

            // Load user orders if not admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                UserOrders = await _context.Orders
                    .Where(o => o.UserId == userId.Value)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10) // Show last 10 orders
                    .ToListAsync();
            }

            // Load recent orders for non-admin users
            if (userRole != "Admin")
            {
                RecentOrders = await _context.Orders
                    .Where(o => o.UserId == userId.Value)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync();
            }

            // Set login status
            isLoggedIn = true; // User đã đăng nhập mới vào được trang này

            // Pre-populate form
            ProfileInput.Username = CurrentUser.Username ?? "";
            ProfileInput.Email = CurrentUser.Email ?? "";
            ProfileInput.NormalName = CurrentUser.NormalName ?? "";
            ProfileInput.Phone = CurrentUser.PhoneNumber ?? "";
            ProfileInput.Address = CurrentUser.Address ?? "";

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login/Login");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                ErrorMessage = "Không tìm thấy thông tin người dùng.";
                return Page();
            }

            // Check if email is already taken by another user
            if (!string.IsNullOrEmpty(ProfileInput.Email) && 
                ProfileInput.Email != user.Email &&
                await _context.Users.AnyAsync(u => u.Email == ProfileInput.Email && u.UserId != userId.Value))
            {
                ErrorMessage = "Email này đã được sử dụng bởi tài khoản khác.";
                return Page();
            }

            // Update user information
            user.Email = ProfileInput.Email;
            user.NormalName = ProfileInput.NormalName;
            user.PhoneNumber = ProfileInput.Phone;
            user.Address = ProfileInput.Address;

            await _context.SaveChangesAsync();

            // Update session if name changed
            if (user.NormalName != HttpContext.Session.GetString("Username"))
            {
                HttpContext.Session.SetString("Username", user.NormalName ?? user.Username ?? "");
            }

            SuccessMessage = "Cập nhật thông tin thành công!";
            
            // Reload current user data
            CurrentUser = user;
            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Login/Login");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                ErrorMessage = "Không tìm thấy thông tin người dùng.";
                return Page();
            }

            // Validate current password
            if (user.PasswordHash != PasswordInput.CurrentPassword)
            {
                ErrorMessage = "Mật khẩu hiện tại không đúng.";
                return Page();
            }

            // Validate new password
            if (PasswordInput.NewPassword != PasswordInput.ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không khớp.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(PasswordInput.NewPassword) || PasswordInput.NewPassword.Length < 6)
            {
                ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return Page();
            }

            // Update password
            user.PasswordHash = PasswordInput.NewPassword;
            await _context.SaveChangesAsync();

            SuccessMessage = "Đổi mật khẩu thành công!";
            
            // Clear password form
            PasswordInput = new PasswordInputModel();
            
            // Reload current user data
            CurrentUser = user;
            return Page();
        }

        public async Task<IActionResult> OnPostGetCartCountAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return new JsonResult(new { count = 0 });
            }

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId.Value);
            if (cart == null)
            {
                return new JsonResult(new { count = 0 });
            }

            var cartItemCount = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId)
                .SumAsync(ci => ci.Quantity);

            return new JsonResult(new { count = cartItemCount });
        }

        public class ProfileInputModel
        {
            [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email là bắt buộc")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Họ và tên là bắt buộc")]
            [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
            public string NormalName { get; set; } = string.Empty;

            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            public string? Phone { get; set; }

            [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
            public string? Address { get; set; }
        }

        public class PasswordInputModel
        {
            [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
            public string CurrentPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
            public string NewPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
            [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
} 