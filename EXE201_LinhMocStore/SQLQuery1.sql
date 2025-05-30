-- Tạo database 
CREATE DATABASE PhongThuyShop; GO
USE PhongThuyShop; GO
-- Tạo bảng Category 
CREATE TABLE Category ( CategoryID INT PRIMARY KEY IDENTITY(1,1), Name NVARCHAR(100) NOT NULL );
-- Tạo bảng User 
CREATE TABLE [User] ( UserID INT PRIMARY KEY IDENTITY(1,1), Address NVARCHAR(255), Email NVARCHAR(100), NormalName NVARCHAR(100), EmailConfirmed BIT, PhoneNumber NVARCHAR(20), Gender NVARCHAR(10), BirthDate DATE, Username NVARCHAR(50), PasswordHash NVARCHAR(255), SecurityStamp NVARCHAR(100), ConcurrencyStamp NVARCHAR(100), PhoneNumberConfirmed BIT, TwoFactorEnabled BIT, LockoutEnd DATETIME, LockoutEnabled BIT, AccessFailedCount INT, Status NVARCHAR(20) );
-- Tạo bảng Product 
CREATE TABLE Product ( ProductID INT PRIMARY KEY IDENTITY(1,1), Name NVARCHAR(100) NOT NULL, Price DECIMAL(10, 2) NOT NULL, Quantity INT NOT NULL, Description NVARCHAR(500), Material NVARCHAR(100), Size NVARCHAR(50), Image NVARCHAR(255), CategoryID INT FOREIGN KEY REFERENCES Category(CategoryID) );
-- Tạo bảng Cart (giỏ hàng) 
CREATE TABLE Cart ( CartID INT PRIMARY KEY IDENTITY(1,1), UserID INT FOREIGN KEY REFERENCES UserID, CreatedAt DATETIME NOT NULL DEFAULT GETDATE() );
-- Tạo bảng CartItem (sản phẩm trong giỏ hàng) 
CREATE TABLE CartItem ( CartItemID INT PRIMARY KEY IDENTITY(1,1), CartID INT FOREIGN KEY REFERENCES Cart(CartID), ProductID INT FOREIGN KEY REFERENCES Product(ProductID), Quantity INT NOT NULL );
-- Tạo bảng Order 
CREATE TABLE [Order] ( OrderID INT PRIMARY KEY IDENTITY(1,1), UserID INT FOREIGN KEY REFERENCES UserID, OrderDate DATETIME NOT NULL, Status NVARCHAR(20), TotalAmount DECIMAL(10, 2), CreatedAt DATETIME );
-- Tạo bảng OrderDetails 
CREATE TABLE OrderDetails ( OrderDetailID INT PRIMARY KEY IDENTITY(1,1), OrderID INT FOREIGN KEY REFERENCES OrderID, ProductID INT FOREIGN KEY REFERENCES Product(ProductID), Quantity INT NOT NULL, Price DECIMAL(10, 2) NOT NULL );
-- Tạo bảng Payment 
CREATE TABLE Payment ( PaymentID INT PRIMARY KEY IDENTITY(1,1), OrderID INT FOREIGN KEY REFERENCES OrderID, Content NVARCHAR(255), Price DECIMAL(10, 2), TransactionCode NVARCHAR(50), Status NVARCHAR(20), CreatedAt DATETIME );
-- Tạo bảng Notification 
CREATE TABLE Notification ( NotificationID INT PRIMARY KEY IDENTITY(1,1), UserID INT FOREIGN KEY REFERENCES UserID, Content NVARCHAR(500), Status NVARCHAR(20), CreatedAt DATETIME );
-- Tạo bảng Blog 
CREATE TABLE Blog ( BlogID INT PRIMARY KEY IDENTITY(1,1), Title NVARCHAR(200) NOT NULL, Content NVARCHAR(MAX), CreatedAt DATETIME NOT NULL DEFAULT GETDATE() );

ALTER TABLE Blog
ADD Image nvarchar(255) NULL,
    Author nvarchar(100) NULL,
    Summary nvarchar(500) NULL,
    IsFeatured bit NOT NULL DEFAULT 0,
    IsActive bit NOT NULL DEFAULT 1;
    
-- =========================== -- Insert dữ liệu mẫu -- ===========================
-- Category 
INSERT INTO Category (Name) VALUES (N'Vòng tay trầm hương'), (N'Vòng tay gỗ sưa'), (N'Vòng tay gỗ huyết long');
-- User 
INSERT INTO [User] (Address, Email, NormalName, EmailConfirmed, PhoneNumber, Gender, BirthDate, Username, PasswordHash, Status) VALUES (N'123 Đường A, Q.1, TP.HCM', N'admin@shop.com', N'Admin', 1, N'0909123456', N'Nam', '1990-01-01', N'admin', N'adminhash', N'Active'), (N'456 Đường B, Q.2, TP.HCM', N'khach1@gmail.com', N'Nguyễn Văn A', 1, N'0912345678', N'Nam', '1995-05-10', N'khach1', N'khach1hash', N'Active');
-- Product 
-- Thêm 15 sản phẩm mới vào bảng Product
INSERT INTO Product (Name, Price, Quantity, Description, Material, Size, Image, CategoryID) VALUES
(N'Vòng trầm hương 54 hạt', 1000000, 15, N'Vòng tay trầm hương tự nhiên 54 hạt, mang lại bình an', N'Trầm hương', N'6mm', N'https://pos.nvncdn.com/877529-112891/ps/20220809_eDxSCdVsBejYmeqQkWxz1Ko6.jpg', 1),
(N'Vòng trầm hương chạm khắc', 2000000, 8, N'Vòng tay trầm hương khắc họa tiết tinh xảo', N'Trầm hương', N'10mm', N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTW3VaEVcQawKe7IlaIDQTBwaMxAYJDWqmU4w&s', 1),
(N'Vòng trầm hương mix charm vàng', 2500000, 5, N'Vòng tay trầm hương kết hợp charm vàng 18K', N'Trầm hương', N'8mm', N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTGHXv5kNzsAM5z4yWYlS3xA9s3CL-rR4J7_g&s', 1),
(N'Vòng gỗ sưa trắng 10mm', 1100000, 12, N'Vòng tay gỗ sưa trắng tự nhiên, cao cấp', N'Gỗ sưa', N'10mm', N'https://vonggosua.com/wp-content/uploads/2024/02/Vong-Go-Sua-Quang-Binh-Hat-Tron.jpg', 2),
(N'Vòng gỗ sưa đỏ 8mm', 1300000, 10, N'Vòng tay gỗ sưa đỏ phong thủy, năng lượng mạnh', N'Gỗ sưa', N'8mm', N'https://gosuado.com/Pictures/trang-hat-go-sua2.jpg', 2),
(N'Vòng gỗ sưa mix đá quý', 1800000, 6, N'Vòng tay gỗ sưa kết hợp đá quý tự nhiên', N'Gỗ sưa', N'12mm', N'https://henryjewels.com/wp-content/uploads/2024/09/vong-sua-do-quang-binh-mix-charm-cam-thach-2.jpg', 2),
(N'Vòng huyết long 8mm', 850000, 20, N'Vòng tay gỗ huyết long tự nhiên, hợp mệnh Hỏa', N'Huyết long', N'8mm', N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQcgqhXgCiob2HBy8cGBSVnjQzSx741ckm-3g&s', 3),
(N'Vòng huyết long 12mm', 950000, 10, N'Vòng tay gỗ huyết long cao cấp, năng lượng mạnh', N'Huyết long', N'12mm', N'https://down-vn.img.susercontent.com/file/9558d9bb95611e5be41f4f4927294abe', 3),
(N'Vòng trầm hương cao cấp 10mm', 1700000, 7, N'Vòng tay trầm hương chất lượng cao, mùi thơm dịu', N'Trầm hương', N'10mm', N'https://tramhuongannhien.vn/wp-content/uploads/2024/05/vong-tay-tram-huong-10mm-hat-truc-cao-cap-vt10trcc-2-5.jpg', 1),
(N'Vòng gỗ sưa khắc rồng', 2200000, 4, N'Vòng tay gỗ sưa khắc hình rồng phong thủy', N'Gỗ sưa', N'14mm', N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSikOVDZ2egyMr_l_Mm7VVaQ6ybwDVi3FSlNQ&s', 2),
(N'Vòng huyết long mix bạc', 1200000, 8, N'Vòng tay gỗ huyết long kết hợp charm bạc', N'Huyết long', N'10mm', N'https://vonghuyetlong.com/wp-content/uploads/2024/04/Vong-huyet-long-10ly-mix-ty-huu-bac-1.jpg', 3),
(N'Vòng trầm hương 6mm', 900000, 18, N'Vòng tay trầm hương nhỏ gọn, phù hợp mọi lứa tuổi', N'Trầm hương', N'6mm', N'https://i1.wp.com/viethuongtram.vn/wp-content/uploads/2018/04/vong-tram-sanh-chim-108-hat-1.jpg?fit=2048%2C1262&ssl=1', 1),
(N'Vòng gỗ sưa 16mm', 1500000, 5, N'Vòng tay gỗ sưa kích thước lớn, mạnh mẽ', N'Gỗ sưa', N'16mm', N'https://gosuado.com/Pictures/vong-tay-go-sua-gia-16mm.jpg', 2),
(N'Vòng huyết long 6mm', 700000, 15, N'Vòng tay gỗ huyết long nhỏ gọn, hợp mệnh Thổ', N'Huyết long', N'6mm', N'https://henryjewels.com/wp-content/uploads/2025/01/vong-huyet-long-thau-quang-6mm-108-ava.jpg', 3),
(N'Vòng trầm hương mix đá thạch anh', 1900000, 6, N'Vòng tay trầm hương kết hợp đá thạch anh tím', N'Trầm hương', N'8mm', N'https://product.hstatic.net/1000069526/product/thach-anh-mat-ho-thien-nhien-khac-chu-ommanib_0a7cedf4fbaa4798b5d006f3fc5c4334_master.jpg', 1);-- Cart 
INSERT INTO Cart (UserID) VALUES (2);
-- CartItem
INSERT INTO CartItem (CartID, ProductID, Quantity) VALUES (1, 1, 1), (1, 3, 2);
-- Order 
INSERT INTO [Order] (UserID, OrderDate, Status, TotalAmount, CreatedAt) VALUES (2, GETDATE(), N'Đã thanh toán', 3300000, GETDATE());
-- OrderDetails
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, Price) VALUES (1, 1, 1, 1500000), (1, 3, 2, 1800000);
-- Payment
INSERT INTO Payment (OrderID, Content, Price, TransactionCode, Status, CreatedAt) VALUES (1, N'Thanh toán đơn hàng #1', 3300000, N'TRANS001', N'Thành công', GETDATE());
-- Notification
INSERT INTO Notification (UserID, Content, Status, CreatedAt) VALUES (2, N'Đơn hàng của bạn đã được xác nhận.', N'Chưa đọc', GETDATE());
-- Blog 
INSERT INTO Blog (Title, Content) VALUES (N'Cách chọn vòng phong thủy hợp mệnh', N'Nội dung bài viết về cách chọn vòng phong thủy...');-- Thêm các bài viết blog mẫu
INSERT INTO Blog (Title, Content, CreatedAt, Image, Author, Summary, IsFeatured, IsActive)
VALUES 
(
    N'Ý nghĩa phong thủy của vòng gỗ huyết long',
    N'Vòng gỗ huyết long là một trong những loại vòng phong thủy được ưa chuộng nhất hiện nay. Gỗ huyết long có màu đỏ tự nhiên, tượng trưng cho sự may mắn và tài lộc. Theo phong thủy, vòng gỗ huyết long mang lại nhiều năng lượng tích cực cho người đeo.

Các lợi ích chính của vòng gỗ huyết long:
1. Tăng cường sức khỏe và tuổi thọ
2. Mang lại may mắn trong công việc và tài chính
3. Bảo vệ người đeo khỏi năng lượng tiêu cực
4. Tăng cường sự tự tin và quyết đoán

Cách chọn vòng gỗ huyết long phù hợp:
- Nên chọn vòng có màu đỏ tự nhiên, không bị nhuộm
- Kích thước hạt vừa phải, không quá to hoặc quá nhỏ
- Số lượng hạt nên là số lẻ (9, 11, 13, 15, 17, 19, 21)
- Nên mua từ nguồn uy tín để đảm bảo chất lượng gỗ

Lưu ý khi sử dụng:
- Nên đeo vòng ở tay trái để thu hút năng lượng
- Thường xuyên lau chùi và bảo quản vòng
- Không nên để vòng tiếp xúc với nước hoặc hóa chất
- Nên tháo vòng khi đi ngủ để vòng được "nghỉ ngơi"',
    GETDATE(),
    '/images/blogs/huyet-long.jpg',
    N'Phong Thủy Sư',
    N'Vòng gỗ huyết long là một trong những loại vòng phong thủy được ưa chuộng nhất hiện nay. Bài viết sẽ giúp bạn hiểu rõ về ý nghĩa và cách sử dụng vòng gỗ huyết long trong phong thủy.',
    1,
    1
),
(
    N'Cách phân biệt vòng đá phong thủy thật và giả',
    N'Trên thị trường hiện nay có rất nhiều loại vòng đá phong thủy với chất lượng khác nhau. Việc phân biệt đá thật và giả là rất quan trọng để đảm bảo hiệu quả phong thủy và tránh lãng phí tiền bạc.

Các cách phân biệt vòng đá phong thủy thật:

1. Kiểm tra bằng mắt thường:
- Đá thật thường có vân tự nhiên, không đều
- Màu sắc tự nhiên, không quá rực rỡ
- Có thể có các tạp chất nhỏ bên trong
- Bề mặt có độ bóng tự nhiên

2. Kiểm tra bằng nhiệt:
- Đá thật mát lạnh khi chạm vào
- Giữ nhiệt lâu hơn đá giả
- Không bị nóng lên nhanh khi ma sát

3. Kiểm tra bằng âm thanh:
- Đá thật phát ra âm thanh trong trẻo khi gõ
- Đá giả thường phát ra âm thanh đục

4. Kiểm tra bằng nước:
- Đá thật không bị phai màu khi ngâm nước
- Không bị mất độ bóng sau khi ngâm

Các loại đá phong thủy phổ biến:
1. Thạch anh tím: Tăng cường trí tuệ và sự tập trung
2. Thạch anh hồng: Mang lại tình duyên và hạnh phúc
3. Thạch anh vàng: Thu hút tài lộc và may mắn
4. Mã não: Bảo vệ sức khỏe và xua đuổi tà khí
5. Ngọc bích: Mang lại bình an và thịnh vượng',
    GETDATE(),
    '/images/blogs/da-phong-thuy.jpg',
    N'Chuyên Gia Đá Quý',
    N'Bài viết hướng dẫn chi tiết cách phân biệt vòng đá phong thủy thật và giả, giúp bạn tránh mua phải hàng kém chất lượng và đảm bảo hiệu quả phong thủy.',
    1,
    1
),
(
    N'Hướng dẫn chọn vòng phong thủy theo mệnh',
    N'Theo phong thủy, việc chọn vòng phong thủy phù hợp với mệnh của người đeo là rất quan trọng. Mỗi mệnh sẽ phù hợp với những màu sắc và chất liệu khác nhau.

1. Người mệnh Kim:
- Màu sắc phù hợp: Trắng, vàng, nâu
- Chất liệu: Bạc, vàng, thạch anh trắng
- Tránh màu đỏ, hồng (màu của Hỏa khắc Kim)

2. Người mệnh Mộc:
- Màu sắc phù hợp: Xanh lá, đen
- Chất liệu: Gỗ, ngọc bích, thạch anh đen
- Tránh màu trắng (màu của Kim khắc Mộc)

3. Người mệnh Thủy:
- Màu sắc phù hợp: Đen, xanh dương
- Chất liệu: Thạch anh đen, ngọc lam
- Tránh màu vàng, nâu (màu của Thổ khắc Thủy)

4. Người mệnh Hỏa:
- Màu sắc phù hợp: Đỏ, hồng, tím
- Chất liệu: Thạch anh tím, mã não đỏ
- Tránh màu đen (màu của Thủy khắc Hỏa)

5. Người mệnh Thổ:
- Màu sắc phù hợp: Vàng, nâu, đỏ
- Chất liệu: Thạch anh vàng, mã não nâu
- Tránh màu xanh lá (màu của Mộc khắc Thổ)

Lưu ý khi chọn vòng:
- Nên chọn vòng có số hạt phù hợp với tuổi
- Kích thước hạt vừa phải với cổ tay
- Chất liệu tự nhiên, không bị nhuộm màu
- Nguồn gốc rõ ràng, đảm bảo chất lượng',
    GETDATE(),
    '/images/blogs/vong-theo-menh.jpg',
    N'Phong Thủy Sư',
    N'Bài viết hướng dẫn chi tiết cách chọn vòng phong thủy phù hợp với từng mệnh, giúp bạn tăng cường vận khí và may mắn trong cuộc sống.',
    1,
    1
);