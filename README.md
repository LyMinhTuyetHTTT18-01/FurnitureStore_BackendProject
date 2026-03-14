# Furniture Store Backend Project 🪑

Một ứng dụng web thương mại điện tử chuyên cung cấp nội thất, được xây dựng trên nền tảng **ASP.NET Core MVC**. Dự án tập trung vào trải nghiệm người dùng cao cấp với giao diện hiện đại, hiệu ứng mượt mà và kiến trúc backend vững chắc.

## 🚀 Tính năng chính

- **Quản lý sản phẩm**: Hiển thị danh sách sản phẩm theo danh mục, tìm kiếm và xem chi tiết.
- **Giỏ hàng (Shopping Cart)**: Thêm/xoá sản phẩm, cập nhật số lượng và tính toán tổng tiền thời gian thực.
- **Đặt hàng và Thanh toán**: Quy trình đặt hàng tối ưu hóa, hỗ trợ các bước kiểm tra thông tin giao hàng.
- **Hệ thống Tài khoản**: Đăng ký, Đăng nhập và quản lý hồ sơ người dùng.
- **Theo dõi Đơn hàng**: Khách hàng có thể xem lịch sử và trạng thái các đơn hàng đã đặt.
- **Giao diện UI/UX cao cấp**:
  - Tích hợp hiệu ứng **AOS (Animate On Scroll)** cho các phần tử trang web.
  - Thiết kế Responsive (tương thích mọi thiết bị).
  - Các hiệu ứng hover và micro-interaction tinh tế.
- **Quản trị (Admin Dashboard)**: Quản lý danh mục, sản phẩm và đơn hàng dành cho nhân viên/quản trị viên.

## 🛠 Công nghệ sử dụng

### Backend
- **ASP.NET Core MVC 10.0**
- **Entity Framework Core**: Quản lý cơ sở dữ liệu.
- **Repository Pattern & Unit of Work**: Kiến trúc mã nguồn sạch, dễ bảo trì và mở rộng.
- **SQL Server / SQLite**: Lưu trữ dữ liệu hệ thống.

### Frontend
- **Razor Engine**: Render giao diện phía server.
- **HTML5, CSS3, JavaScript (ES6+)**
- **Bootstrap 5**: Hệ thống Grid và UI Components.
- **Thư viện Plugin**: AOS, jQuery, Owl Carousel, Summernote, Raphael...

## 📂 Cấu trúc mã nguồn

- `FurnitureStoreWeb`: Dự án chính chứa các Controllers, Views và tài nguyên tĩnh (`wwwroot`).
- `FurnitureStoreData`: Chứa Context cơ sở dữ liệu, các Models và lớp Repositories.
- `wwwroot/plugins`: Thư mục chứa các plugin frontend của bên thứ ba.

## 💻 Hướng dẫn cài đặt

1. **Clone dự án**:
   ```bash
   git clone https://github.com/LyMinhTuyetHTTT18-01/FurnitureStore_BackendProject.git
   ```

2. **Cấu hình Cơ sở dữ liệu**:
   - Mở file `appsettings.json` trong dự án `FurnitureStoreWeb`.
   - Cập nhật chuỗi kết nối `DefaultConnection` phù hợp với Server SQL của bạn.

3. **Cấu hình Email (Nếu cần)**:
   - Cập nhật thông tin trong mục `EmailSettings` để sử dụng tính năng gửi mail nếu được kích hoạt.

4. **Chạy Migration**:
   - Sử dụng PMC (Package Manager Console) hoặc Terminal:
   ```bash
   dotnet ef database update
   ```

5. **Khởi chạy**:
   ```bash
   dotnet run
   ```

## 🤝 Đóng góp
Nếu bạn có bất kỳ ý tưởng hoặc đóng góp nào để dự án hoàn thiện hơn, hãy tạo một Pull Request hoặc mở một Issue.

---
*Dự án được thực hiện bởi đội ngũ phát triển FurnitureStore.*
