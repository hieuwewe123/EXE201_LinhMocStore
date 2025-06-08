using Microsoft.EntityFrameworkCore;
using EXE201_LinhMocStore.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add session and IHttpContextAccessor
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// Thêm DbContext
builder.Services.AddDbContext<PhongThuyShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("value")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session before authorization
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
