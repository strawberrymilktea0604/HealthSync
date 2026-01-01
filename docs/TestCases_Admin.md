# Tài Liệu Test Case - Dự Án HealthSync

## Thông Tin Tài Liệu

| Thông tin | Chi tiết |
|-----------|----------|
| **Dự án** | HealthSync - Ứng dụng theo dõi sức khỏe |
| **Phiên bản** | 1.0 |
| **Ngày tạo** | 02/01/2026 |
| **Người tạo** | QA Team |
| **Loại kiểm thử** | Functional Testing / System Testing |
| **Nền tảng** | Web (React), Mobile (React Native) |

---

## Phạm Vi Kiểm Thử

Tài liệu này tập trung vào **kiểm thử chức năng (Functional Testing)** trên giao diện người dùng (Frontend). Các test case được viết theo luồng workflow của ứng dụng, bắt đầu từ luồng **Admin**.

---

# PHẦN 1: LUỒNG ADMIN (WEB)

---

## Module 1: Xác Thực Admin (Authentication)

### Chức năng 1.1: Đăng nhập Admin

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-AUTH-001 | Đăng nhập Admin với thông tin hợp lệ | 1. Mở trình duyệt và truy cập URL đăng nhập<br>2. Nhập email admin<br>3. Nhập mật khẩu đúng<br>4. Nhấn nút "Đăng nhập" | Email: admin@healthsync.com<br>Password: Admin@123 | Hệ thống chuyển hướng đến trang Admin Dashboard, hiển thị menu sidebar với các chức năng Admin | | | |
| TC-AUTH-002 | Đăng nhập Admin với sai mật khẩu | 1. Mở trình duyệt và truy cập URL đăng nhập<br>2. Nhập email admin hợp lệ<br>3. Nhập sai mật khẩu<br>4. Nhấn nút "Đăng nhập" | Email: admin@healthsync.com<br>Password: wrongpass | Hiển thị thông báo lỗi "Sai email hoặc mật khẩu", không chuyển trang | | | |
| TC-AUTH-003 | Đăng nhập Admin với email không tồn tại | 1. Mở trình duyệt và truy cập URL đăng nhập<br>2. Nhập email không tồn tại<br>3. Nhập mật khẩu bất kỳ<br>4. Nhấn nút "Đăng nhập" | Email: notexist@test.com<br>Password: 123456 | Hiển thị thông báo lỗi "Sai email hoặc mật khẩu" | | | |
| TC-AUTH-004 | Đăng nhập Admin để trống email | 1. Truy cập URL đăng nhập<br>2. Để trống email<br>3. Nhập mật khẩu<br>4. Nhấn nút "Đăng nhập" | Email: (trống)<br>Password: Admin@123 | Hiển thị thông báo lỗi validation "Email là bắt buộc" | | | |
| TC-AUTH-005 | Đăng nhập Admin để trống mật khẩu | 1. Truy cập URL đăng nhập<br>2. Nhập email<br>3. Để trống mật khẩu<br>4. Nhấn nút "Đăng nhập" | Email: admin@healthsync.com<br>Password: (trống) | Hiển thị thông báo lỗi validation "Mật khẩu là bắt buộc" | | | |
| TC-AUTH-006 | Đăng nhập với email format sai | 1. Truy cập URL đăng nhập<br>2. Nhập email không hợp lệ<br>3. Nhập mật khẩu<br>4. Nhấn nút "Đăng nhập" | Email: invalid-email<br>Password: Admin@123 | Hiển thị thông báo lỗi "Email không hợp lệ" | | | |
| TC-AUTH-007 | Đăng nhập với tài khoản bị khóa (Inactive) | 1. Truy cập URL đăng nhập<br>2. Nhập email của tài khoản đã bị Admin khóa<br>3. Nhập mật khẩu đúng<br>4. Nhấn nút "Đăng nhập" | Email: locked_user@test.com<br>Password: User@123 | Hiển thị thông báo "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên" | | | |
| TC-AUTH-008 | Kiểm tra Remember Me checkbox | 1. Đăng nhập thành công với checkbox "Ghi nhớ" được tích<br>2. Đóng trình duyệt<br>3. Mở lại trình duyệt và truy cập ứng dụng | Email: admin@healthsync.com<br>Password: Admin@123<br>Remember Me: ✓ | Hệ thống giữ phiên đăng nhập, không yêu cầu đăng nhập lại | | | |
| TC-AUTH-009 | Đăng nhập bằng Google (OAuth) | 1. Truy cập URL đăng nhập<br>2. Nhấn nút "Đăng nhập với Google"<br>3. Chọn tài khoản Google<br>4. Cấp quyền cho ứng dụng | Tài khoản Google hợp lệ | Hệ thống xác thực thành công và chuyển hướng đến Dashboard tương ứng với role | | | |

### Chức năng 1.2: Đăng xuất Admin

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-AUTH-010 | Đăng xuất thành công | 1. Đăng nhập với tài khoản Admin<br>2. Nhấn vào avatar/menu góc phải<br>3. Nhấn nút "Đăng xuất" | N/A | Hệ thống chuyển hướng về trang đăng nhập, session được xóa | | | |
| TC-AUTH-011 | Truy cập trang Admin sau khi đăng xuất | 1. Đăng xuất thành công<br>2. Nhập trực tiếp URL trang Admin Dashboard vào trình duyệt | URL: /admin/dashboard | Hệ thống chuyển hướng về trang đăng nhập | | | |

---

## Module 2: Admin Dashboard

### Chức năng 2.1: Xem Dashboard Tổng Quan

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-DASH-001 | Hiển thị Dashboard Admin | 1. Đăng nhập với tài khoản Admin<br>2. Hệ thống tự động chuyển đến hoặc nhấn menu "Dashboard" | N/A | Trang Dashboard hiển thị với các thống kê: Tổng số người dùng, Tổng số bài tập, Tổng số món ăn, và các biểu đồ thống kê | | | |
| TC-DASH-002 | Kiểm tra hiển thị số liệu thống kê người dùng | 1. Đăng nhập Admin<br>2. Xem Dashboard<br>3. Kiểm tra số "Tổng người dùng" | N/A | Số liệu "Tổng người dùng" hiển thị đúng với số lượng user trong database | | | |
| TC-DASH-003 | Kiểm tra biểu đồ người dùng mới theo thời gian | 1. Đăng nhập Admin<br>2. Xem Dashboard<br>3. Kiểm tra biểu đồ "Người dùng mới" | N/A | Biểu đồ hiển thị đúng xu hướng đăng ký người dùng theo ngày/tuần/tháng | | | |
| TC-DASH-004 | Responsive Dashboard trên mobile | 1. Đăng nhập Admin trên trình duyệt mobile<br>2. Xem Dashboard | Viewport: 375px width | Dashboard hiển thị responsive, các card thống kê xếp dọc, biểu đồ scale phù hợp | | | |
| TC-DASH-005 | Kiểm tra thống kê Workout | 1. Đăng nhập Admin<br>2. Xem Dashboard<br>3. Kiểm tra section "Workout Statistics" | N/A | Hiển thị đúng số lượng bài tập đã hoàn thành, tổng thời gian tập luyện | | | |
| TC-DASH-006 | Kiểm tra thống kê Dinh dưỡng | 1. Đăng nhập Admin<br>2. Xem Dashboard<br>3. Kiểm tra section "Nutrition Statistics" | N/A | Hiển thị đúng số bữa ăn đã ghi nhận, calo trung bình | | | |
| TC-DASH-007 | Kiểm tra thống kê Mục tiêu | 1. Đăng nhập Admin<br>2. Xem Dashboard<br>3. Kiểm tra section "Goal Statistics" | N/A | Hiển thị đúng số mục tiêu đang hoạt động, đã hoàn thành | | | |

---

## Module 3: Quản Lý Người Dùng (User Management)

### Chức năng 3.1: Xem Danh Sách Người Dùng

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-USER-001 | Hiển thị danh sách người dùng | 1. Đăng nhập Admin<br>2. Nhấn menu "Quản lý người dùng" | N/A | Bảng danh sách người dùng hiển thị với các cột: ID, Avatar, Tên, Email, Role, Trạng thái, Ngày tham gia, Hành động | | | |
| TC-USER-002 | Phân trang danh sách người dùng | 1. Đăng nhập Admin<br>2. Vào trang Quản lý người dùng<br>3. Nhấn nút chuyển trang (Next/Previous) | N/A | Hệ thống chuyển đúng trang, hiển thị đúng số lượng user trên mỗi trang (mặc định 10) | | | |
| TC-USER-003 | Thay đổi số dòng hiển thị mỗi trang | 1. Đăng nhập Admin<br>2. Vào trang Quản lý người dùng<br>3. Chọn dropdown "Rows per page" = 25 | Rows per page: 25 | Bảng hiển thị 25 dòng trên mỗi trang | | | |
| TC-USER-004 | Sắp xếp theo tên (A-Z) | 1. Đăng nhập Admin<br>2. Vào trang Quản lý người dùng<br>3. Nhấn vào header cột "Tên" | N/A | Danh sách được sắp xếp theo tên từ A đến Z | | | |
| TC-USER-005 | Sắp xếp theo tên (Z-A) | 1. Đăng nhập Admin<br>2. Vào trang Quản lý người dùng<br>3. Nhấn vào header cột "Tên" 2 lần | N/A | Danh sách được sắp xếp theo tên từ Z đến A | | | |
| TC-USER-006 | Sắp xếp theo Role | 1. Đăng nhập Admin<br>2. Vào trang Quản lý người dùng<br>3. Nhấn vào header cột "Role" | N/A | Danh sách được sắp xếp theo Role (Admin > Customer hoặc ngược lại) | | | |
| TC-USER-007 | Sắp xếp theo Trạng thái | 1. Đăng nhập Admin<br>2. Vào trang Quản lý người dùng<br>3. Nhấn vào header cột "Trạng thái" | N/A | Danh sách được sắp xếp theo trạng thái (Active/Inactive) | | | |
| TC-USER-008 | Sắp xếp theo Ngày tham gia | 1. Đăng nhập Admin<br>2. Vào trang Quản lý người dùng<br>3. Nhấn vào header cột "Ngày tham gia" | N/A | Danh sách được sắp xếp theo ngày tham gia (mới nhất/cũ nhất) | | | |
| TC-USER-009 | Tìm kiếm người dùng theo tên | 1. Đăng nhập Admin<br>2. Vào trang Quản lý người dùng<br>3. Nhập từ khóa vào ô tìm kiếm | Keyword: "Nguyen" | Danh sách chỉ hiển thị những người dùng có tên chứa "Nguyen" | | | |
| TC-USER-010 | Tìm kiếm người dùng theo email | 1. Đăng nhập Admin<br>2. Vào trang Quản lý người dùng<br>3. Nhập email vào ô tìm kiếm | Keyword: "test@gmail.com" | Danh sách hiển thị người dùng có email chứa từ khóa | | | |

### Chức năng 3.2: Thêm Người Dùng Mới

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-USER-011 | Thêm người dùng mới với dữ liệu hợp lệ | 1. Đăng nhập Admin<br>2. Vào trang Quản lý người dùng<br>3. Nhấn nút "Thêm người dùng"<br>4. Điền đầy đủ thông tin hợp lệ<br>5. Nhấn "Lưu" | FirstName: John<br>LastName: Doe<br>Email: newuser@test.com<br>Password: User@123<br>Role: Customer | Hiển thị thông báo "Tạo người dùng thành công", người dùng mới xuất hiện trong danh sách | | | |
| TC-USER-012 | Thêm người dùng với email đã tồn tại | 1. Đăng nhập Admin<br>2. Nhấn "Thêm người dùng"<br>3. Nhập email đã tồn tại trong hệ thống<br>4. Nhấn "Lưu" | Email: admin@healthsync.com (đã tồn tại) | Hiển thị thông báo lỗi "Email đã được sử dụng" | | | |
| TC-USER-013 | Thêm người dùng thiếu trường bắt buộc | 1. Đăng nhập Admin<br>2. Nhấn "Thêm người dùng"<br>3. Để trống trường "Email"<br>4. Nhấn "Lưu" | Email: (trống) | Hiển thị thông báo lỗi validation "Email là bắt buộc" | | | |
| TC-USER-014 | Thêm người dùng với mật khẩu yếu | 1. Đăng nhập Admin<br>2. Nhấn "Thêm người dùng"<br>3. Nhập mật khẩu không đủ mạnh<br>4. Nhấn "Lưu" | Password: 123456 | Hiển thị thông báo lỗi "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt" | | | |
| TC-USER-015 | Hủy thêm người dùng | 1. Đăng nhập Admin<br>2. Nhấn "Thêm người dùng"<br>3. Điền một số thông tin<br>4. Nhấn "Hủy" | N/A | Dialog đóng lại, không có người dùng mới được tạo | | | |

### Chức năng 3.3: Chỉnh Sửa Thông Tin Người Dùng

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-USER-016 | Chỉnh sửa tên người dùng | 1. Đăng nhập Admin<br>2. Vào trang Quản lý người dùng<br>3. Nhấn nút "Sửa" trên một user<br>4. Thay đổi FirstName<br>5. Nhấn "Lưu" | FirstName: "Updated Name" | Hiển thị thông báo "Cập nhật thành công", tên trong danh sách được cập nhật | | | |
| TC-USER-017 | Thay đổi Role người dùng | 1. Đăng nhập Admin<br>2. Nhấn "Sửa" trên một user có role Customer<br>3. Đổi role thành Admin<br>4. Nhấn "Lưu" | Role: Admin | Role được cập nhật thành Admin, user có thể truy cập các chức năng Admin | | | |
| TC-USER-018 | Cập nhật Avatar người dùng | 1. Đăng nhập Admin<br>2. Nhấn nút "Đổi Avatar" trên một user<br>3. Chọn file ảnh hợp lệ<br>4. Xác nhận upload | File: avatar.jpg (< 5MB) | Avatar được cập nhật, hiển thị ảnh mới trong danh sách | | | |
| TC-USER-019 | Upload Avatar với file không phải ảnh | 1. Đăng nhập Admin<br>2. Nhấn nút "Đổi Avatar"<br>3. Chọn file PDF<br>4. Xác nhận | File: document.pdf | Hiển thị lỗi "Chỉ chấp nhận file ảnh (JPG, PNG, GIF)" | | | |
| TC-USER-020 | Upload Avatar vượt quá dung lượng | 1. Đăng nhập Admin<br>2. Nhấn nút "Đổi Avatar"<br>3. Chọn file ảnh lớn hơn giới hạn | File: large_image.jpg (> 5MB) | Hiển thị lỗi "Dung lượng file không được vượt quá 5MB" | | | |

### Chức năng 3.4: Đổi Mật Khẩu Người Dùng

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-USER-021 | Đổi mật khẩu người dùng | 1. Đăng nhập Admin<br>2. Nhấn nút "Đổi mật khẩu" trên một user<br>3. Nhập mật khẩu mới hợp lệ<br>4. Xác nhận mật khẩu<br>5. Nhấn "Lưu" | NewPassword: NewPass@123<br>ConfirmPassword: NewPass@123 | Hiển thị thông báo "Đổi mật khẩu thành công" | | | |
| TC-USER-022 | Đổi mật khẩu không khớp | 1. Đăng nhập Admin<br>2. Nhấn nút "Đổi mật khẩu"<br>3. Nhập mật khẩu mới<br>4. Nhập xác nhận mật khẩu khác<br>5. Nhấn "Lưu" | NewPassword: Pass@123<br>ConfirmPassword: Different@123 | Hiển thị lỗi "Mật khẩu xác nhận không khớp" | | | |
| TC-USER-023 | Đổi mật khẩu với yêu cầu không đạt | 1. Đăng nhập Admin<br>2. Nhấn nút "Đổi mật khẩu"<br>3. Nhập mật khẩu mới quá ngắn<br>4. Nhấn "Lưu" | NewPassword: 1234 | Hiển thị lỗi validation về yêu cầu mật khẩu | | | |

### Chức năng 3.5: Khóa/Mở Khóa Tài Khoản

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-USER-024 | Khóa tài khoản người dùng | 1. Đăng nhập Admin<br>2. Tìm một user đang Active<br>3. Nhấn nút "Khóa tài khoản" (Toggle Status)<br>4. Xác nhận | User ID: [user có trạng thái Active] | Trạng thái chuyển thành "Inactive", user không thể đăng nhập | | | |
| TC-USER-025 | Mở khóa tài khoản người dùng | 1. Đăng nhập Admin<br>2. Tìm một user đang Inactive<br>3. Nhấn nút "Mở khóa tài khoản"<br>4. Xác nhận | User ID: [user có trạng thái Inactive] | Trạng thái chuyển thành "Active", user có thể đăng nhập lại | | | |
| TC-USER-026 | Admin không thể tự khóa tài khoản mình | 1. Đăng nhập Admin<br>2. Tìm chính tài khoản Admin đang đăng nhập<br>3. Thử nhấn nút "Khóa tài khoản" | N/A | Nút bị disable hoặc hiển thị cảnh báo "Không thể khóa tài khoản của chính mình" | | | |

### Chức năng 3.6: Xóa Người Dùng

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-USER-027 | Xóa người dùng | 1. Đăng nhập Admin<br>2. Nhấn nút "Xóa" trên một user<br>3. Xác nhận trong dialog | User ID: [user cần xóa] | Hiển thị thông báo "Xóa người dùng thành công", user biến mất khỏi danh sách | | | |
| TC-USER-028 | Hủy xóa người dùng | 1. Đăng nhập Admin<br>2. Nhấn nút "Xóa" trên một user<br>3. Nhấn "Hủy" trong dialog xác nhận | N/A | Dialog đóng lại, user vẫn còn trong danh sách | | | |

---

## Module 4: Quản Lý Bài Tập (Exercise Management)

### Chức năng 4.1: Xem Danh Sách Bài Tập

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-EXER-001 | Hiển thị danh sách bài tập | 1. Đăng nhập Admin<br>2. Nhấn menu "Quản lý Bài tập" hoặc "Content Library" | N/A | Bảng danh sách hiển thị với các cột: Hình ảnh, Tên bài tập, Nhóm cơ, Độ khó, Mô tả, Hành động | | | |
| TC-EXER-002 | Phân trang danh sách bài tập | 1. Đăng nhập Admin<br>2. Vào trang Quản lý Bài tập<br>3. Chuyển trang | N/A | Hệ thống phân trang chính xác | | | |

### Chức năng 4.2: Thêm Bài Tập Mới

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-EXER-003 | Thêm bài tập với dữ liệu hợp lệ | 1. Đăng nhập Admin<br>2. Nhấn nút "Thêm bài tập"<br>3. Điền đầy đủ thông tin<br>4. Upload hình ảnh<br>5. Nhấn "Lưu" | Name: Push Up<br>MuscleGroup: Chest<br>Difficulty: Beginner<br>Description: Basic push up exercise<br>CaloriesBurnedPerMinute: 7 | Thông báo "Tạo bài tập thành công", bài tập mới xuất hiện trong danh sách | | | |
| TC-EXER-004 | Thêm bài tập thiếu tên | 1. Đăng nhập Admin<br>2. Nhấn "Thêm bài tập"<br>3. Để trống trường "Tên"<br>4. Nhấn "Lưu" | Name: (trống) | Hiển thị lỗi validation "Tên bài tập là bắt buộc" | | | |
| TC-EXER-005 | Thêm bài tập với hình ảnh | 1. Đăng nhập Admin<br>2. Nhấn "Thêm bài tập"<br>3. Điền thông tin<br>4. Upload file ảnh hợp lệ<br>5. Nhấn "Lưu" | ImageFile: exercise.jpg | Bài tập được tạo với hình ảnh hiển thị trong danh sách | | | |

### Chức năng 4.3: Chỉnh Sửa Bài Tập

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-EXER-006 | Chỉnh sửa thông tin bài tập | 1. Đăng nhập Admin<br>2. Nhấn nút "Sửa" trên bài tập<br>3. Thay đổi tên và mô tả<br>4. Nhấn "Lưu" | Name: "Updated Push Up"<br>Description: "Updated description" | Thông báo "Cập nhật thành công", thông tin được cập nhật trong danh sách | | | |
| TC-EXER-007 | Thay đổi hình ảnh bài tập | 1. Đăng nhập Admin<br>2. Nhấn nút "Đổi ảnh" trên bài tập<br>3. Upload ảnh mới<br>4. Xác nhận | ImageFile: new_exercise.jpg | Hình ảnh được cập nhật trong danh sách | | | |

### Chức năng 4.4: Xóa Bài Tập

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-EXER-008 | Xóa bài tập | 1. Đăng nhập Admin<br>2. Nhấn nút "Xóa" trên bài tập<br>3. Xác nhận trong dialog | Exercise ID: [bài tập cần xóa] | Thông báo "Xóa bài tập thành công", bài tập biến mất khỏi danh sách | | | |
| TC-EXER-009 | Hủy xóa bài tập | 1. Đăng nhập Admin<br>2. Nhấn nút "Xóa"<br>3. Nhấn "Hủy" trong dialog | N/A | Dialog đóng, bài tập vẫn còn trong danh sách | | | |

---

## Module 5: Quản Lý Món Ăn (Food Management)

### Chức năng 5.1: Xem Danh Sách Món Ăn

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-FOOD-001 | Hiển thị danh sách món ăn | 1. Đăng nhập Admin<br>2. Nhấn menu "Quản lý Món ăn" hoặc tab "Foods" trong Content Library | N/A | Bảng danh sách hiển thị: Hình ảnh, Tên món, Serving Size, Calories, Macros (P/C/F), Hành động | | | |
| TC-FOOD-002 | Phân trang danh sách món ăn | 1. Đăng nhập Admin<br>2. Vào trang Quản lý Món ăn<br>3. Chuyển trang | N/A | Hệ thống phân trang chính xác | | | |

### Chức năng 5.2: Thêm Món Ăn Mới

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-FOOD-003 | Thêm món ăn với dữ liệu hợp lệ | 1. Đăng nhập Admin<br>2. Nhấn nút "Thêm món ăn"<br>3. Điền đầy đủ thông tin dinh dưỡng<br>4. Upload hình ảnh<br>5. Nhấn "Lưu" | Name: Chicken Breast<br>ServingSize: 100g<br>Calories: 165<br>Protein: 31<br>Carbs: 0<br>Fat: 3.6 | Thông báo "Tạo món ăn thành công", món ăn xuất hiện trong danh sách | | | |
| TC-FOOD-004 | Thêm món ăn thiếu tên | 1. Đăng nhập Admin<br>2. Nhấn "Thêm món ăn"<br>3. Để trống trường "Tên"<br>4. Nhấn "Lưu" | Name: (trống) | Hiển thị lỗi "Tên món ăn là bắt buộc" | | | |
| TC-FOOD-005 | Thêm món ăn với calories âm | 1. Đăng nhập Admin<br>2. Nhấn "Thêm món ăn"<br>3. Nhập calories = -100<br>4. Nhấn "Lưu" | Calories: -100 | Hiển thị lỗi "Calories phải là số dương" | | | |

### Chức năng 5.3: Chỉnh Sửa Món Ăn

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-FOOD-006 | Chỉnh sửa thông tin món ăn | 1. Đăng nhập Admin<br>2. Nhấn nút "Sửa" trên món ăn<br>3. Thay đổi thông tin dinh dưỡng<br>4. Nhấn "Lưu" | Calories: 180<br>Protein: 35 | Thông báo "Cập nhật thành công", thông tin được cập nhật | | | |
| TC-FOOD-007 | Thay đổi hình ảnh món ăn | 1. Đăng nhập Admin<br>2. Nhấn nút "Đổi ảnh"<br>3. Upload ảnh mới | ImageFile: new_food.jpg | Hình ảnh được cập nhật | | | |

### Chức năng 5.4: Xóa Món Ăn

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-FOOD-008 | Xóa món ăn | 1. Đăng nhập Admin<br>2. Nhấn nút "Xóa" trên món ăn<br>3. Xác nhận | Food ID: [món ăn cần xóa] | Thông báo "Xóa thành công", món ăn biến mất khỏi danh sách | | | |
| TC-FOOD-009 | Hủy xóa món ăn | 1. Nhấn "Xóa"<br>2. Nhấn "Hủy" trong dialog | N/A | Dialog đóng, món ăn vẫn còn | | | |

---

## Module 6: Content Library (Thư Viện Nội Dung Tổng Hợp)

### Chức năng 6.1: Chuyển đổi giữa Exercises và Foods

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-LIB-001 | Chuyển tab từ Exercises sang Foods | 1. Đăng nhập Admin<br>2. Vào Content Library<br>3. Nhấn tab "Foods" | N/A | Bảng chuyển sang hiển thị danh sách món ăn | | | |
| TC-LIB-002 | Chuyển tab từ Foods sang Exercises | 1. Đăng nhập Admin<br>2. Vào Content Library (đang ở tab Foods)<br>3. Nhấn tab "Exercises" | N/A | Bảng chuyển sang hiển thị danh sách bài tập | | | |

---

## Module 7: Phân Quyền Admin (Permission Control)

### Chức năng 7.1: Kiểm Tra Phân Quyền

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-PERM-001 | User thường không thể truy cập trang Admin | 1. Đăng nhập với tài khoản Customer<br>2. Nhập trực tiếp URL /admin/dashboard | Tài khoản: customer@test.com | Hệ thống chuyển hướng về trang Dashboard User hoặc hiển thị 403 Forbidden | | | |
| TC-PERM-002 | Admin có thể truy cập tất cả chức năng Admin | 1. Đăng nhập với tài khoản Admin<br>2. Truy cập lần lượt các trang: Dashboard, User Management, Content Library | Tài khoản: admin@healthsync.com | Tất cả các trang hiển thị đầy đủ, không bị chặn | | | |
| TC-PERM-003 | Ẩn nút xóa với user không có quyền | 1. Đăng nhập với tài khoản có role giới hạn (nếu có)<br>2. Vào trang User Management | N/A | Các nút "Xóa" bị ẩn hoặc disable nếu user không có quyền DELETE | | | |

---

# TỔNG KẾT TEST CASE ADMIN

| Module | Số lượng Test Case | Pass | Fail | Pending |
|--------|-------------------|------|------|---------|
| Xác thực Admin | 11 | | | |
| Admin Dashboard | 7 | | | |
| Quản lý Người dùng | 18 | | | |
| Quản lý Bài tập | 9 | | | |
| Quản lý Món ăn | 9 | | | |
| Content Library | 2 | | | |
| Phân quyền | 3 | | | |
| **TỔNG** | **59** | | | |

---

*Ghi chú: Tài liệu này sẽ được cập nhật thêm phần User (Customer) luồng sau khi hoàn thành test luồng Admin.*
