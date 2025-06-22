using Microsoft.AspNetCore.Mvc.RazorPages;
using EXE201_LinhMocStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace EXE201_LinhMocStore.Pages.Admin.Order
{
    public class IndexModel : PageModel
    {
        private readonly PhongThuyShopContext _context;
        public List<Models.Order> Orders { get; set; } = new();

        // Thống kê
        public int TotalOrders { get; set; }
        public int PendingPaymentOrders { get; set; }
        public int AwaitingConfirmationOrders { get; set; }
        public int CompletedOrders { get; set; }

        // Bộ lọc
        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "";
        [BindProperty(SupportsGet = true)]
        public DateTime? DateFrom { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? DateTo { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = "";

        // Phân trang
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public IndexModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            // Tính toán thống kê
            TotalOrders = await _context.Orders.CountAsync();
            PendingPaymentOrders = await _context.Orders.CountAsync(o => o.Status == "PendingPayment");
            AwaitingConfirmationOrders = await _context.Orders.CountAsync(o => o.Status == "AwaitingConfirmation");
            CompletedOrders = await _context.Orders.CountAsync(o => o.Status == "Delivered");

            // Xây dựng query với bộ lọc
            var query = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                query = query.Where(o => o.Status == StatusFilter);
            }

            // Lọc theo ngày
            if (DateFrom.HasValue)
            {
                query = query.Where(o => o.OrderDate.Date >= DateFrom.Value.Date);
            }
            if (DateTo.HasValue)
            {
                query = query.Where(o => o.OrderDate.Date <= DateTo.Value.Date);
            }

            // Tìm kiếm
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var searchTerm = SearchTerm.ToLower();
                query = query.Where(o => 
                    o.User.NormalName.ToLower().Contains(searchTerm) ||
                    o.User.Email.ToLower().Contains(searchTerm) ||
                    o.ReceiverName.ToLower().Contains(searchTerm) ||
                    o.ReceiverPhone.Contains(searchTerm) ||
                    o.ShippingAddress.ToLower().Contains(searchTerm)
                );
            }

            // Tính toán phân trang
            var totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            // Đảm bảo CurrentPage hợp lệ
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            // Lấy dữ liệu với phân trang
            Orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }
    }
}
