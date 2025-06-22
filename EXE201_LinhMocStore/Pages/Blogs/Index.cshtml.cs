using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EXE201_LinhMocStore.Models;
using System.Linq;

namespace EXE201_LinhMocStore.Pages.Blogs
{
    public class IndexModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        public IndexModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public List<Blog> Blogs { get; set; } = new List<Blog>();
        public Blog? FeaturedBlog { get; set; }
        public string SearchTerm { get; set; } = "";
        public string Filter { get; set; } = "all";
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public int TotalPages { get; set; }
        public int TotalBlogs { get; set; }

        public async Task<IActionResult> OnGetAsync(string? searchTerm, string? filter, int? page)
        {
            SearchTerm = searchTerm ?? "";
            Filter = filter ?? "all";
            CurrentPage = page ?? 1;

            var query = _context.Blogs.Where(b => b.IsActive);

            // Apply search filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(b => 
                    b.Title.Contains(SearchTerm) || 
                    b.Content.Contains(SearchTerm) || 
                    b.Summary.Contains(SearchTerm) ||
                    b.Author.Contains(SearchTerm));
            }

            // Apply category filter
            switch (Filter)
            {
                case "featured":
                    query = query.Where(b => b.IsFeatured);
                    break;
                case "recent":
                    query = query.OrderByDescending(b => b.CreatedAt);
                    break;
                default:
                    query = query.OrderByDescending(b => b.CreatedAt);
                    break;
            }

            // Get total count for pagination
            TotalBlogs = await query.CountAsync();
            TotalPages = (int)Math.Ceiling((double)TotalBlogs / PageSize);

            // Ensure current page is within valid range
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            // Get featured blog (first featured blog)
            FeaturedBlog = await query.Where(b => b.IsFeatured)
                                    .OrderByDescending(b => b.CreatedAt)
                                    .FirstOrDefaultAsync();

            // Get paginated blogs
            Blogs = await query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }
    }
} 