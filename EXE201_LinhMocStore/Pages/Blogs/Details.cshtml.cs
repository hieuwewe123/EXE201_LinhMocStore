using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EXE201_LinhMocStore.Models;
using System.Text.RegularExpressions;

namespace EXE201_LinhMocStore.Pages.Blogs
{
    public class DetailsModel : PageModel
    {
        private readonly PhongThuyShopContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DetailsModel(PhongThuyShopContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public Blog Blog { get; set; } = null!;
        public List<Blog> RelatedBlogs { get; set; } = new List<Blog>();
        public int ReadingTime { get; set; }
        public string CurrentUrl { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Get current URL for social sharing
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null)
            {
                CurrentUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
            }

            // Get blog by ID
            Blog = await _context.Blogs
                .Where(b => b.BlogId == id && b.IsActive)
                .FirstOrDefaultAsync();

            if (Blog == null)
            {
                return NotFound();
            }

            // Calculate reading time (average 200 words per minute)
            if (!string.IsNullOrEmpty(Blog.Content))
            {
                var wordCount = Regex.Matches(Blog.Content, @"\b\w+\b").Count;
                ReadingTime = Math.Max(1, wordCount / 200);
            }
            else
            {
                ReadingTime = 1;
            }

            // Get related blogs (same author or similar content)
            RelatedBlogs = await _context.Blogs
                .Where(b => b.BlogId != id && b.IsActive)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToListAsync();

            return Page();
        }
    }
} 