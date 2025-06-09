using Microsoft.EntityFrameworkCore;
using EXE201_LinhMocStore.Models;
using EXE201_LinhMocStore.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add session and IHttpContextAccessor
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // thời gian timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<BankSettings>(builder.Configuration.GetSection("Bank"));
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<BankSettings>>().Value);
builder.Services.AddSingleton<BankSettings>(sp =>
    new BankSettings(builder.Configuration));

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
