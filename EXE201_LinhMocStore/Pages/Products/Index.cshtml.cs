using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EXE201_LinhMocStore.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EXE201_LinhMocStore.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly PhongThuyShopContext _context;
        private const int PageSize = 9;

        public IndexModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        public IList<Product> Products { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string> Materials { get; set; }
        public List<string> SelectedMaterials { get; set; }
        public string SortBy { get; set; }
        public string QueryString { get; set; }

        public async Task OnGetAsync(
            string searchTerm,
            decimal? minPrice,
            decimal? maxPrice,
            List<string> materials,
            string sortBy = "name_asc",
            int page = 1)
        {
            // Store filter parameters
            SearchTerm = searchTerm;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            SelectedMaterials = materials;
            SortBy = sortBy;
            CurrentPage = page;

            // Get all unique materials for filter
            Materials = await _context.Products
                .Where(p => !string.IsNullOrEmpty(p.Material))
                .Select(p => p.Material)
                .Distinct()
                .ToListAsync();

            // Build query
            var query = _context.Products.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || 
                                       (p.Description != null && p.Description.Contains(searchTerm)));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (materials != null && materials.Any())
            {
                query = query.Where(p => materials.Contains(p.Material));
            }

            // Apply sorting
            query = sortBy switch
            {
                "name_desc" => query.OrderByDescending(p => p.Name),
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Name), // Default: name_asc
            };

            // Get total count for pagination
            var totalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            // Ensure current page is valid
            CurrentPage = Math.Max(1, Math.Min(CurrentPage, TotalPages));

            // Get paginated results
            Products = await query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Build query string for pagination links
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(searchTerm)) queryParams.Add($"searchTerm={searchTerm}");
            if (minPrice.HasValue) queryParams.Add($"minPrice={minPrice}");
            if (maxPrice.HasValue) queryParams.Add($"maxPrice={maxPrice}");
            if (materials != null && materials.Any()) queryParams.AddRange(materials.Select(m => $"materials={m}"));
            if (!string.IsNullOrEmpty(sortBy)) queryParams.Add($"sortBy={sortBy}");
            
            QueryString = queryParams.Any() ? "&" + string.Join("&", queryParams) : "";
        }
    }
} 