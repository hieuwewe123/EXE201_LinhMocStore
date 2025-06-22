using EXE201_LinhMocStore.Models;
using EXE201_LinhMocStore.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;

namespace EXE201_LinhMocStore.Pages.UserSite
{
    public class CartModel : PageModel
    {
        private readonly PhongThuyShopContext _context;

        public CartModel(PhongThuyShopContext context)
        {
            _context = context;
        }

        // Lấy UserId từ session
        private int? CurrentUserId
        {
            get => HttpContext.Session.GetInt32("UserId");
        }

        // Kiểm tra người dùng đã đăng nhập chưa
        private bool IsUserLoggedIn
        {
            get => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username")) && CurrentUserId.HasValue;
        }

        public List<CartItem> CartProducts { get; set; } = new List<CartItem>();

        [BindProperty]
        public List<int> SelectedIds { get; set; } = new List<int>();

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public int Quantity { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool isLoggedIn { get; set; }
        public User? CurrentUser { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Kiểm tra đăng nhập
            if (!IsUserLoggedIn)
            {
                // Lưu URL hiện tại để chuyển hướng sau khi đăng nhập
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
                return RedirectToPage("/Login/Login", new { returnUrl = returnUrl });
            }

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == CurrentUserId.Value);

            if (cart == null)
            {
                CartProducts = new List<CartItem>();
                return Page();
            }

            CartProducts = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId)
                .Include(ci => ci.Product)
                .ToListAsync();

            // Lấy thông tin user hiện tại
            CurrentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == CurrentUserId.Value);

            // Set login status
            isLoggedIn = true; // User đã đăng nhập mới vào được trang này

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync()
        {
            if (!IsUserLoggedIn)
            {
                return new JsonResult(new { success = false, message = "Vui lòng đăng nhập để thực hiện thao tác này." });
            }
            var cartItem = await _context.CartItems.FindAsync(Id);
            if (cartItem == null)
                return new JsonResult(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.CartId == cartItem.CartId);
            if (cart?.UserId != CurrentUserId.Value)
            {
                return new JsonResult(new { success = false, message = "Không có quyền thực hiện thao tác này." });
            }
            // Kiểm tra tồn kho
            var product = await _context.Products.FindAsync(cartItem.ProductId);
            if (product == null || Quantity > product.Quantity)
            {
                return new JsonResult(new { success = false, message = $"Sản phẩm chỉ còn {product?.Quantity ?? 0} sản phẩm!" });
            }
            cartItem.Quantity = Quantity;
            await _context.SaveChangesAsync();
            
            // Tính tổng số lượng giỏ hàng
            var totalCartItems = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId)
                .SumAsync(ci => ci.Quantity);
            
            return new JsonResult(new { 
                success = true, 
                quantity = cartItem.Quantity,
                totalCartItems = totalCartItems
            });
        }

        public async Task<IActionResult> OnPostDeleteSingleAsync(int id)
        {
            if (!IsUserLoggedIn)
            {
                return new JsonResult(new { success = false, message = "Vui lòng đăng nhập để thực hiện thao tác này." });
            }
            
            // Try to get id from query string if not passed as parameter
            if (id == 0)
            {
                var idFromQuery = Request.Query["id"].ToString();
                if (!int.TryParse(idFromQuery, out id))
                {
                    return new JsonResult(new { success = false, message = "ID sản phẩm không hợp lệ." });
                }
            }
            
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                var cart = await _context.Carts.FirstOrDefaultAsync(c => c.CartId == cartItem.CartId);
                if (cart?.UserId == CurrentUserId.Value)
                {
                    _context.CartItems.Remove(cartItem);
                    await _context.SaveChangesAsync();
                    
                    // Tính tổng số lượng giỏ hàng sau khi xóa
                    var totalCartItems = await _context.CartItems
                        .Where(ci => ci.CartId == cart.CartId)
                        .SumAsync(ci => ci.Quantity);
                    
                    return new JsonResult(new { 
                        success = true,
                        totalCartItems = totalCartItems
                    });
                }
            }
            return new JsonResult(new { success = false, message = "Không tìm thấy sản phẩm hoặc không có quyền xóa." });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Kiểm tra đăng nhập
            if (!IsUserLoggedIn)
            {
                return RedirectToPage("/Login/Login", new { returnUrl = "/UserSite/Cart" });
            }

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == CurrentUserId.Value);

            if (cart == null)
            {
                CartProducts = new List<CartItem>();
                return Page();
            }

            List<CartItem> SelectedProducts = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => SelectedIds.Contains(c.CartItemId) && c.CartId == cart.CartId)
                .ToListAsync();

            foreach (var item in SelectedProducts)
            {
                if (item.Product.Quantity < item.Quantity)
                {
                    Message = $"Sản phẩm \"{item.Product.Name}\" không đủ hàng trong kho.";

                    // Gọi lại logic như OnGetAsync
                    CartProducts = await _context.CartItems
                        .Where(ci => ci.CartId == cart.CartId)
                        .Include(ci => ci.Product)
                        .ToListAsync();

                    return Page();
                }
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                // Check if there's already a pending order for this user
                var existingOrder = await _context.Orders
                    .FirstOrDefaultAsync(o => o.UserId == CurrentUserId && o.Status == OrderStatus.PendingPayment);
                
                if (existingOrder != null)
                {
                    Message = "Bạn đã có đơn hàng đang chờ thanh toán. Vui lòng hoàn tất đơn hàng hiện tại trước khi tạo đơn hàng mới.";
                    
                    // Reload cart products for display
                    CartProducts = await _context.CartItems
                        .Where(ci => ci.CartId == cart.CartId)
                        .Include(ci => ci.Product)
                        .ToListAsync();
                    
                    return Page();
                }
                
                // Nếu không có lỗi hết hàng thì xử lý tiếp đơn hàng
                decimal TotalAmount = 0;
                foreach (var item in SelectedProducts)
                {
                    item.Product.Quantity -= item.Quantity;
                    TotalAmount += item.Quantity * item.Product.Price;
                    _context.Products.Update(item.Product);
                }

                string TransferInfo = Commons.GenerateBankInfo(6);

                Order newOrder = new Order
                {
                    UserId = CurrentUserId,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.PendingPayment,
                    CreatedAt = DateTime.Now
                };

                _context.Orders.Add(newOrder);
                
                // Save the order first to get the generated ID
                await _context.SaveChangesAsync();

                Payment payment = new Payment
                {
                    OrderId = newOrder.OrderId,
                    Price = TotalAmount,
                    TransactionCode = TransferInfo,
                    CreatedAt = DateTime.Now,
                    isCompleted = false
                };
                _context.Payments.Add(payment);

                List<OrderDetail> orderDetails = SelectedProducts.Select(item => new OrderDetail
                {
                    OrderId = newOrder.OrderId,
                    ProductId = item.Product.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                }).ToList();

                _context.OrderDetails.AddRange(orderDetails);
                
                // Save Payment, OrderDetails and update Products
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToPage("/UserSite/Checkout", new { OrderId = newOrder.OrderId });
            }
            catch (DbUpdateException dbEx)
            {
                // Reload cart products for display
                CartProducts = await _context.CartItems
                    .Where(ci => ci.CartId == cart.CartId)
                    .Include(ci => ci.Product)
                    .ToListAsync();
                
                var innerException = dbEx.InnerException;
                var errorMessage = "Có lỗi xảy ra khi lưu đơn hàng.";
                
                if (innerException != null)
                {
                    if (innerException.Message.Contains("duplicate key") || innerException.Message.Contains("PRIMARY KEY"))
                    {
                        errorMessage = "Đơn hàng đã tồn tại. Vui lòng thử lại.";
                    }
                    else if (innerException.Message.Contains("foreign key"))
                    {
                        errorMessage = "Dữ liệu không hợp lệ. Vui lòng thử lại.";
                    }
                }
                
                Message = errorMessage;
                return Page();
            }
            catch (Exception ex)
            {
                // Reload cart products for display
                CartProducts = await _context.CartItems
                    .Where(ci => ci.CartId == cart.CartId)
                    .Include(ci => ci.Product)
                    .ToListAsync();
                
                Message = "Có lỗi xảy ra khi tạo đơn hàng. Vui lòng thử lại.";
                return Page();
            }
        }

        // Handler đồng bộ giỏ hàng từ localStorage sau khi đăng nhập
        public async Task<IActionResult> OnPostSyncAsync()
        {
            if (!IsUserLoggedIn)
            {
                return new JsonResult(new { success = false, message = "Chưa đăng nhập" });
            }
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var localCart = JsonSerializer.Deserialize<List<LocalCartItem>>(body);
            if (localCart == null || !localCart.Any())
                return new JsonResult(new { success = true });

            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == CurrentUserId.Value);
            if (cart == null)
            {
                cart = new Cart { UserId = CurrentUserId.Value };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
                cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == CurrentUserId.Value);
            }

            foreach (var item in localCart)
            {
                var product = await _context.Products.FindAsync(item.id);
                if (product == null || product.Quantity < 1) continue;
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == item.id);
                int addQty = Math.Min(item.quantity, product.Quantity);
                if (cartItem != null)
                {
                    cartItem.Quantity = Math.Min(cartItem.Quantity + addQty, product.Quantity);
                }
                else
                {
                    cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = item.id,
                        Quantity = addQty
                    };
                    _context.CartItems.Add(cartItem);
                }
            }
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }

        public class LocalCartItem
        {
            public int id { get; set; }
            public string name { get; set; }
            public int price { get; set; }
            public string image { get; set; }
            public int quantity { get; set; }
            public int stock { get; set; }
        }

        public class CartTotalRequest
        {
            public List<CartTotalItem> Items { get; set; } = new();
        }
        public class CartTotalItem
        {
            public int Id { get; set; }
            public int Quantity { get; set; }
        }

        public class UpdateItemRequest : CartTotalItem
        {
            public List<int> SelectedIds { get; set; } = new();
        }

        public async Task<IActionResult> OnPostUpdateAndGetTotalsAsync()
        {
            if (!IsUserLoggedIn)
            {
                return new JsonResult(new { success = false, message = "Vui lòng đăng nhập." });
            }

            UpdateItemRequest request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<UpdateItemRequest>(Request.Body);
            }
            catch
            {
                return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ." });
            }


            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.CartItemId == request.Id);

            if (cartItem == null)
            {
                return new JsonResult(new { success = false, message = "Sản phẩm không có trong giỏ." });
            }

            var cart = await _context.Carts.FindAsync(cartItem.CartId);
            if (cart.UserId != CurrentUserId)
            {
                return new JsonResult(new { success = false, message = "Không có quyền." });
            }

            if (request.Quantity < 1)
            {
                request.Quantity = 1;
            }

            if (cartItem.Product.Quantity < request.Quantity)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = $"Sản phẩm '{cartItem.Product.Name}' chỉ còn {cartItem.Product.Quantity} trong kho.",
                    newQuantity = cartItem.Product.Quantity,
                    stock = cartItem.Product.Quantity
                });
            }

            cartItem.Quantity = request.Quantity;
            await _context.SaveChangesAsync();

            var selectedItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.CartId && request.SelectedIds.Contains(ci.CartItemId))
                .ToListAsync();

            decimal grandTotal = 0;
            foreach (var item in selectedItems)
            {
                // Ensure the updated item's quantity is reflected in the total calculation
                if (item.CartItemId == cartItem.CartItemId)
                {
                    grandTotal += (decimal)request.Quantity * item.Product.Price;
                }
                else
                {
                    grandTotal += (decimal)item.Quantity * item.Product.Price;
                }
            }


            return new JsonResult(new
            {
                success = true,
                newQuantity = cartItem.Quantity,
                itemTotal = (decimal)cartItem.Quantity * cartItem.Product.Price,
                grandTotal = grandTotal,
                stock = cartItem.Product.Quantity,
                totalCartItems = await _context.CartItems
                    .Where(ci => ci.CartId == cart.CartId)
                    .SumAsync(ci => ci.Quantity)
            });
        }

        public async Task<IActionResult> OnPostGetTotalAsync()
        {
            if (!IsUserLoggedIn)
            {
                return new JsonResult(new { success = false, message = "Vui lòng đăng nhập." });
            }

            CartTotalRequest request;
            try
            {
                 request = await JsonSerializer.DeserializeAsync<CartTotalRequest>(Request.Body);
            }
            catch
            {
                return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ." });
            }


            if (request.Items == null || !request.Items.Any())
            {
                return new JsonResult(new { success = true, subtotal = 0, grandTotal = 0, warnings = new List<string>() });
            }

            var cartItemIds = request.Items.Select(i => i.Id).ToList();
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == CurrentUserId.Value);
            if (cart == null)
            {
                return new JsonResult(new { success = false, message = "Không tìm thấy giỏ hàng" });
            }

            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.CartId && cartItemIds.Contains(ci.CartItemId))
                .ToDictionaryAsync(ci => ci.CartItemId, ci => ci);

            decimal grandTotal = 0;
            foreach (var item in request.Items)
            {
                if (cartItems.TryGetValue(item.Id, out var cartItem))
                {
                    grandTotal += (decimal)item.Quantity * cartItem.Product.Price;
                }
            }

            return new JsonResult(new
            {
                success = true,
                grandTotal
            });
        }

        public class CheckoutRequest
        {
            public List<CartTotalItem> Items { get; set; } = new();
            public AddressInfo AddressInfo { get; set; } = new();
        }

        public class AddressInfo
        {
            public string ReceiverName { get; set; } = string.Empty;
            public string ReceiverPhone { get; set; } = string.Empty;
            public string ShippingAddress { get; set; } = string.Empty;
            public string DeliveryNote { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnPostCheckoutAjaxAsync()
        {
            if (!IsUserLoggedIn)
            {
                return new JsonResult(new { success = false, message = "Vui lòng đăng nhập." });
            }
            
            CheckoutRequest request;
            try
            {
                // Enable buffering để có thể đọc request body nhiều lần
                Request.EnableBuffering();
                
                // Đọc request body để debug
                Request.Body.Position = 0;
                using var reader = new StreamReader(Request.Body, leaveOpen: true);
                var requestBody = await reader.ReadToEndAsync();
                Console.WriteLine($"CheckoutAjax request body: {requestBody}");
                
                // Reset position để deserialize
                Request.Body.Position = 0;
                request = await JsonSerializer.DeserializeAsync<CheckoutRequest>(Request.Body);
                
                if (request == null)
                {
                    return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ." });
                }
                
                Console.WriteLine($"Deserialized request: Items count = {request.Items?.Count ?? 0}");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON deserialization error: {jsonEx.Message}");
                return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Request parsing error: {ex.Message}");
                return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            if (request.Items == null || !request.Items.Any())
            {
                return new JsonResult(new { success = false, message = "Vui lòng chọn sản phẩm để thanh toán." });
            }

            // Validation thông tin địa chỉ
            if (request.AddressInfo != null)
            {
                if (string.IsNullOrWhiteSpace(request.AddressInfo.ReceiverName))
                {
                    return new JsonResult(new { success = false, message = "Vui lòng nhập tên người nhận." });
                }
                
                if (string.IsNullOrWhiteSpace(request.AddressInfo.ReceiverPhone))
                {
                    return new JsonResult(new { success = false, message = "Vui lòng nhập số điện thoại người nhận." });
                }
                
                if (string.IsNullOrWhiteSpace(request.AddressInfo.ShippingAddress))
                {
                    return new JsonResult(new { success = false, message = "Vui lòng nhập địa chỉ giao hàng." });
                }
            }
            else
            {
                // Kiểm tra thông tin địa chỉ hiện tại của user
                if (string.IsNullOrWhiteSpace(CurrentUser?.Address))
                {
                    return new JsonResult(new { success = false, message = "Vui lòng cập nhật địa chỉ giao hàng trong hồ sơ hoặc nhập địa chỉ mới." });
                }
            }

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == CurrentUserId.Value);

            if (cart == null)
            {
                return new JsonResult(new { success = false, message = "Không tìm thấy giỏ hàng của bạn." });
            }

            var selectedCartItemIds = request.Items.Select(i => i.Id).ToList();
            Console.WriteLine($"Selected cart item IDs: {string.Join(", ", selectedCartItemIds)}");
            
            var selectedProducts = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => selectedCartItemIds.Contains(c.CartItemId) && c.CartId == cart.CartId)
                .ToListAsync();
            
            Console.WriteLine($"Found {selectedProducts.Count} selected products");
            
            if (!selectedProducts.Any())
            {
                return new JsonResult(new { success = false, message = "Không tìm thấy sản phẩm đã chọn." });
            }
            
            // Thay vì ToDictionary, dùng GroupBy để cộng dồn quantity nếu có trùng Id
            var requestItemsDict = request.Items
                .GroupBy(i => i.Id)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            Console.WriteLine("Request items breakdown:");
            foreach (var item in request.Items)
            {
                Console.WriteLine($"  Item ID: {item.Id}, Quantity: {item.Quantity}");
            }
            
            Console.WriteLine("Grouped request items:");
            foreach (var kvp in requestItemsDict)
            {
                Console.WriteLine($"  CartItem ID: {kvp.Key}, Total Quantity: {kvp.Value}");
            }

            foreach (var item in selectedProducts)
            {
                var requestedQuantity = requestItemsDict[item.CartItemId];
                var availableQuantity = item.Product.Quantity;
                
                Console.WriteLine($"Product: {item.Product.Name}, Requested: {requestedQuantity}, Available: {availableQuantity}");
                
                if (availableQuantity < requestedQuantity)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = $"Sản phẩm \"{item.Product.Name}\" không đủ hàng trong kho (chỉ còn {availableQuantity}). Vui lòng cập nhật lại giỏ hàng."
                    });
                }
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                // Check if there's already a pending order for this user
                var existingOrder = await _context.Orders
                    .FirstOrDefaultAsync(o => o.UserId == CurrentUserId && o.Status == OrderStatus.PendingPayment);
                
                if (existingOrder != null)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Bạn đã có đơn hàng đang chờ thanh toán. Vui lòng hoàn tất đơn hàng hiện tại trước khi tạo đơn hàng mới."
                    });
                }
                
                // Create Order first
                decimal totalAmount = 0;
                foreach (var item in selectedProducts)
                {
                    // Cập nhật số lượng sản phẩm trong kho
                    item.Product.Quantity -= requestItemsDict[item.CartItemId];
                    totalAmount += (decimal)requestItemsDict[item.CartItemId] * item.Product.Price;
                    _context.Products.Update(item.Product);
                }
                
                string transferInfo = Commons.GenerateBankInfo(6);

                Order newOrder = new Order
                {
                    UserId = CurrentUserId,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.PendingPayment,
                    CreatedAt = DateTime.Now,
                    // Thêm thông tin địa chỉ giao hàng
                    ReceiverName = request.AddressInfo?.ReceiverName ?? CurrentUser?.NormalName ?? "",
                    ReceiverPhone = request.AddressInfo?.ReceiverPhone ?? CurrentUser?.PhoneNumber ?? "",
                    ShippingAddress = request.AddressInfo?.ShippingAddress ?? CurrentUser?.Address ?? "",
                    DeliveryNote = request.AddressInfo?.DeliveryNote ?? ""
                };
                _context.Orders.Add(newOrder);
                
                // Save the order first to get the generated ID
                await _context.SaveChangesAsync();
                
                Payment payment = new Payment
                {
                    OrderId = newOrder.OrderId,
                    Price = totalAmount,
                    TransactionCode = transferInfo,
                    CreatedAt = DateTime.Now,
                    isCompleted = false
                };
                _context.Payments.Add(payment);

                List<OrderDetail> orderDetails = selectedProducts.Select(item => new OrderDetail
                {
                    OrderId = newOrder.OrderId,
                    ProductId = item.Product.ProductId,
                    Quantity = requestItemsDict[item.CartItemId],
                    Price = item.Product.Price
                }).ToList();
                _context.OrderDetails.AddRange(orderDetails);
                
                // Xóa các sản phẩm đã chọn khỏi giỏ hàng
                _context.CartItems.RemoveRange(selectedProducts);
                
                // Save Payment, OrderDetails, Products và xóa CartItems
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                Console.WriteLine($"CheckoutAjax success: OrderId={newOrder.OrderId}, TotalAmount={totalAmount}");
                
                return new JsonResult(new
                {
                    success = true,
                    redirectUrl = $"/UserSite/Checkout?OrderId={newOrder.OrderId}"
                });
            }
            catch (DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException;
                var errorMessage = "Có lỗi xảy ra khi lưu đơn hàng.";
                
                Console.WriteLine($"Database update error: {dbEx.Message}");
                if (innerException != null)
                {
                    Console.WriteLine($"Inner exception: {innerException.Message}");
                    
                    if (innerException.Message.Contains("duplicate key") || innerException.Message.Contains("PRIMARY KEY"))
                    {
                        errorMessage = "Đơn hàng đã tồn tại. Vui lòng thử lại.";
                    }
                    else if (innerException.Message.Contains("foreign key"))
                    {
                        errorMessage = "Dữ liệu không hợp lệ. Vui lòng thử lại.";
                    }
                    else if (innerException.Message.Contains("constraint"))
                    {
                        errorMessage = "Dữ liệu không đáp ứng yêu cầu. Vui lòng thử lại.";
                    }
                }
                
                return new JsonResult(new
                {
                    success = false,
                    message = errorMessage
                });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"CheckoutAjax error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return new JsonResult(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tạo đơn hàng. Vui lòng thử lại."
                });
            }
        }

        public async Task<IActionResult> OnPostGetCartCountAsync()
        {
            if (!IsUserLoggedIn)
            {
                return new JsonResult(new { count = 0 });
            }

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == CurrentUserId.Value);
            if (cart == null)
            {
                return new JsonResult(new { count = 0 });
            }

            var cartItemCount = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId)
                .SumAsync(ci => ci.Quantity);

            return new JsonResult(new { count = cartItemCount });
        }
    }
}
