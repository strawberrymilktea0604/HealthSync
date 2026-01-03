# Tài Liệu Test Case - Luồng User (Web)

## Thông Tin Tài Liệu

| Thông tin | Chi tiết |
|-----------|----------|
| **Dự án** | HealthSync - Ứng dụng theo dõi sức khỏe |
| **Phiên bản** | 1.0 |
| **Ngày tạo** | 02/01/2026 |
| **Người tạo** | QA Team |
| **Loại kiểm thử** | Functional Testing / System Testing |
| **Nền tảng** | Web (React + Vite) |

---

## Phạm Vi Kiểm Thử

Tài liệu này tập trung vào **kiểm thử chức năng (Functional Testing)** cho luồng **User/Customer** trên nền tảng **Web**. Các test case được viết theo workflow từ đăng ký → hoàn thiện profile → sử dụng các chức năng chính.

---

# PHẦN 2: LUỒNG USER/CUSTOMER (WEB)

---

## Module 1: Xác Thực Người Dùng (Authentication)

### Chức năng 1.1: Đăng Ký Tài Khoản

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-REG-001 | Đăng ký với thông tin hợp lệ | 1. Truy cập trang đăng ký<br>2. Nhập email hợp lệ<br>3. Nhập mật khẩu đủ mạnh<br>4. Nhập xác nhận mật khẩu<br>5. Nhấn "Sign up" | Email: newuser@test.com<br>Password: User@12345<br>Confirm: User@12345 | Hệ thống gửi mã xác thực đến email, chuyển đến form nhập mã xác thực | | | |
| TC-REG-002 | Xác thực email với mã đúng | 1. Sau khi nhận mã xác thực từ email<br>2. Nhập mã 6 số vào form<br>3. Nhấn "Sign up" | Verification Code: 123456 (mã đúng) | Đăng ký thành công, hiển thị trang "Đăng ký thành công", chuyển đến Complete Profile | | | |
| TC-REG-003 | Xác thực email với mã sai | 1. Nhập mã xác thực sai<br>2. Nhấn "Sign up" | Verification Code: 000000 (mã sai) | Hiển thị lỗi "Mã xác thực không đúng" | | | |
| TC-REG-004 | Đăng ký với email đã tồn tại | 1. Truy cập trang đăng ký<br>2. Nhập email đã có trong hệ thống<br>3. Nhấn "Sign up" | Email: admin@healthsync.com | Hiển thị lỗi "Email đã được sử dụng" | | | |
| TC-REG-005 | Đăng ký với mật khẩu không khớp | 1. Nhập email<br>2. Nhập mật khẩu<br>3. Nhập xác nhận mật khẩu khác<br>4. Nhấn "Sign up" | Password: User@123<br>Confirm: Different@123 | Hiển thị lỗi "Mật khẩu không khớp" | | | |
| TC-REG-006 | Đăng ký với mật khẩu yếu | 1. Nhập email<br>2. Nhập mật khẩu < 8 ký tự<br>3. Nhấn "Sign up" | Password: 1234567 | Hiển thị lỗi "Mật khẩu phải có ít nhất 8 ký tự" | | | |
| TC-REG-007 | Đăng ký với email format sai | 1. Nhập email không hợp lệ<br>2. Nhấn "Sign up" | Email: invalid-email | Form không cho phép submit hoặc hiển thị lỗi validation | | | |
| TC-REG-008 | Gửi lại mã xác thực | 1. Đang ở bước nhập mã xác thực<br>2. Nhấn "Resend code" | N/A | Mã mới được gửi đến email, hiển thị thông báo "Mã xác thực đã được gửi lại" | | | |
| TC-REG-009 | Quay lại từ form nhập mã | 1. Đang ở bước nhập mã xác thực<br>2. Nhấn nút "Back" | N/A | Quay lại form nhập email/password, dữ liệu vẫn được giữ | | | |

### Chức năng 1.2: Đăng Nhập

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-LOGIN-001 | Đăng nhập với thông tin hợp lệ | 1. Truy cập trang đăng nhập<br>2. Nhập email<br>3. Nhập mật khẩu<br>4. Nhấn "Login" | Email: user@test.com<br>Password: User@123 | Đăng nhập thành công, chuyển đến Dashboard người dùng | | | |
| TC-LOGIN-002 | Đăng nhập với sai mật khẩu | 1. Nhập email đúng<br>2. Nhập mật khẩu sai<br>3. Nhấn "Login" | Email: user@test.com<br>Password: wrongpass | Hiển thị thông báo "Sai email hoặc mật khẩu" | | | |
| TC-LOGIN-003 | Đăng nhập với email không tồn tại | 1. Nhập email không tồn tại<br>2. Nhấn "Login" | Email: notexist@test.com | Hiển thị thông báo "Sai email hoặc mật khẩu" | | | |
| TC-LOGIN-004 | Đăng nhập để trống email | 1. Để trống email<br>2. Nhập mật khẩu<br>3. Nhấn "Login" | Email: (trống) | Form không cho submit hoặc hiển thị lỗi validation | | | |
| TC-LOGIN-005 | Đăng nhập để trống mật khẩu | 1. Nhập email<br>2. Để trống mật khẩu<br>3. Nhấn "Login" | Password: (trống) | Form không cho submit hoặc hiển thị lỗi validation | | | |
| TC-LOGIN-006 | Đăng nhập với tài khoản bị khóa | 1. Nhập email của tài khoản đã bị Admin khóa<br>2. Nhập mật khẩu đúng<br>3. Nhấn "Login" | Email: locked_user@test.com | Hiển thị thông báo "Tài khoản đã bị khóa" | | | |
| TC-LOGIN-007 | Đăng nhập bằng Google | 1. Nhấn nút "Sign in with Google"<br>2. Chọn tài khoản Google<br>3. Cấp quyền | Tài khoản Google hợp lệ | Đăng nhập thành công, nếu lần đầu thì chuyển đến Complete Profile, ngược lại đến Dashboard | | | |
| TC-LOGIN-008 | Đăng nhập Google với email đã có tài khoản | 1. Nhấn "Sign in with Google"<br>2. Chọn tài khoản Google có email đã đăng ký bằng form | Email đã tồn tại | Liên kết tài khoản hoặc hiển thị thông báo phù hợp | | | |
| TC-LOGIN-009 | Hiển thị/Ẩn mật khẩu | 1. Nhập mật khẩu<br>2. Nhấn icon "eye" để hiện mật khẩu<br>3. Nhấn lại để ẩn | N/A | Mật khẩu chuyển đổi giữa hiển thị text và dots | | | |

### Chức năng 1.3: Quên Mật Khẩu

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-FORGOT-001 | Yêu cầu reset mật khẩu với email hợp lệ | 1. Từ trang đăng nhập, nhấn "Forgot password?"<br>2. Nhập email đã đăng ký<br>3. Nhấn "Gửi mã" | Email: user@test.com | Mã xác thực được gửi đến email, chuyển đến form nhập mã | | | |
| TC-FORGOT-002 | Yêu cầu reset với email không tồn tại | 1. Nhấn "Forgot password?"<br>2. Nhập email không có trong hệ thống<br>3. Nhấn "Gửi mã" | Email: notexist@test.com | Hiển thị lỗi "Email không tồn tại trong hệ thống" | | | |
| TC-FORGOT-003 | Xác thực mã reset và đổi mật khẩu | 1. Nhập mã xác thực đúng<br>2. Nhập mật khẩu mới<br>3. Xác nhận mật khẩu mới<br>4. Nhấn "Xác nhận" | Code: 123456<br>NewPassword: NewUser@123<br>Confirm: NewUser@123 | Mật khẩu được đổi thành công, chuyển đến trang thông báo thành công | | | |
| TC-FORGOT-004 | Đổi mật khẩu với mã sai | 1. Nhập mã xác thực sai<br>2. Nhấn "Xác nhận" | Code: 000000 | Hiển thị lỗi "Mã xác thực không đúng" | | | |
| TC-FORGOT-005 | Đổi mật khẩu mới không khớp | 1. Nhập mã đúng<br>2. Nhập mật khẩu mới<br>3. Nhập xác nhận không khớp | NewPassword: Pass@123<br>Confirm: Different@123 | Hiển thị lỗi "Mật khẩu không khớp" | | | |

---

## Module 2: Hoàn Thiện Hồ Sơ (Complete Profile)

### Chức năng 2.1: Nhập Thông Tin Cá Nhân

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-PROFILE-001 | Hoàn thiện profile với dữ liệu hợp lệ | 1. Sau đăng ký, hệ thống chuyển đến Complete Profile<br>2. Nhập họ tên<br>3. Chọn giới tính<br>4. Nhập ngày sinh<br>5. Nhập chiều cao, cân nặng<br>6. Chọn mức độ hoạt động<br>7. Nhấn "Lưu" | FirstName: John<br>LastName: Doe<br>Gender: Male<br>DOB: 1990-01-15<br>Height: 175<br>Weight: 70<br>ActivityLevel: Moderate | Profile được lưu thành công, chuyển đến Dashboard | | | |
| TC-PROFILE-002 | Hoàn thiện profile thiếu trường bắt buộc | 1. Để trống trường "Họ"<br>2. Nhấn "Lưu" | FirstName: (trống) | Hiển thị lỗi "Vui lòng nhập đầy đủ thông tin" | | | |
| TC-PROFILE-003 | Nhập chiều cao không hợp lệ | 1. Nhập chiều cao âm hoặc quá lớn<br>2. Nhấn "Lưu" | Height: -10 hoặc Height: 500 | Hiển thị lỗi validation "Chiều cao không hợp lệ" | | | |
| TC-PROFILE-004 | Nhập cân nặng không hợp lệ | 1. Nhập cân nặng âm hoặc quá lớn<br>2. Nhấn "Lưu" | Weight: -5 hoặc Weight: 1000 | Hiển thị lỗi validation "Cân nặng không hợp lệ" | | | |
| TC-PROFILE-005 | Nhập ngày sinh trong tương lai | 1. Chọn ngày sinh trong tương lai<br>2. Nhấn "Lưu" | DOB: 2030-01-01 | Hiển thị lỗi "Ngày sinh không hợp lệ" | | | |
| TC-PROFILE-006 | Upload avatar (nếu có) | 1. Nhấn vào avatar<br>2. Chọn file ảnh<br>3. Xác nhận | File: avatar.jpg (< 5MB) | Avatar được upload và hiển thị | | | |

### Chức năng 2.2: Cập Nhật Hồ Sơ (Profile Settings)

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-PROFILE-007 | Cập nhật thông tin cá nhân | 1. Vào trang Profile/Settings<br>2. Thay đổi cân nặng, chiều cao, activity level<br>3. Nhấn "Lưu thay đổi" | Weight: 72kg<br>ActivityLevel: Active | Thông tin được cập nhật thành công, hiển thị thông báo thành công | | | |
| TC-PROFILE-008 | Cập nhật avatar từ trang settings | 1. Vào trang Profile<br>2. Nhấn vào ảnh đại diện<br>3. Upload ảnh mới | File: new_avatar.png | Avatar mới được cập nhật ngay lập tức | | | |
| TC-PROFILE-009 | Hủy thay đổi | 1. Vào trang Profile<br>2. Thay đổi thông tin nhưng không lưu<br>3. Nhấn "Back" hoặc chuyển trang | N/A | Thông tin không bị thay đổi trong cơ sở dữ liệu | | | |
| TC-PROFILE-010 | Validate dữ liệu khi cập nhật | 1. Nhập cân nặng âm<br>2. Nhấn "Lưu" | Weight: -5 | Hiển thị lỗi validation, không cho phép lưu | | | |

---

## Module 3: Dashboard Người Dùng

### Chức năng 3.1: Hiển Thị Dashboard

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-DASH-001 | Hiển thị Dashboard sau đăng nhập | 1. Đăng nhập thành công<br>2. Hệ thống chuyển đến Dashboard | N/A | Dashboard hiển thị với: Thông tin người dùng, Thống kê calories, Tiến độ mục tiêu, Lịch sử bài tập gần đây | | | |
| TC-DASH-002 | Hiển thị thống kê BMI | 1. Xem Dashboard<br>2. Kiểm tra chỉ số BMI | N/A | BMI được tính đúng dựa trên chiều cao và cân nặng hiện tại | | | |
| TC-DASH-003 | Hiển thị tiến độ mục tiêu | 1. Xem Dashboard<br>2. Kiểm tra section "Mục tiêu" | N/A | Hiển thị đúng mục tiêu đang hoạt động với % tiến độ | | | |
| TC-DASH-004 | Hiển thị thống kê dinh dưỡng hôm nay | 1. Xem Dashboard<br>2. Kiểm tra calories/macros hôm nay | N/A | Hiển thị tổng calories đã ăn, protein, carbs, fat | | | |
| TC-DASH-005 | Responsive Dashboard trên tablet | 1. Mở Dashboard trên màn hình tablet<br>2. Kiểm tra layout | Viewport: 768px | Layout responsive, không bị vỡ giao diện | | | |
| TC-DASH-006 | Responsive Dashboard trên mobile | 1. Mở Dashboard trên màn hình mobile<br>2. Kiểm tra layout | Viewport: 375px | Layout responsive, các card xếp dọc | | | |

---

## Module 4: Quản Lý Mục Tiêu (Goals)

### Chức năng 4.1: Xem Danh Sách Mục Tiêu

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-GOAL-001 | Xem danh sách mục tiêu | 1. Đăng nhập<br>2. Vào trang "Goals" hoặc "Mục tiêu" | N/A | Hiển thị danh sách các mục tiêu của người dùng với: Loại mục tiêu, Giá trị mục tiêu, Tiến độ, Ngày tạo/deadline | | | |
| TC-GOAL-002 | Hiển thị trạng thái mục tiêu đang hoạt động | 1. Xem danh sách mục tiêu<br>2. Kiểm tra mục tiêu có trạng thái Active | N/A | Mục tiêu Active có badge hoặc màu sắc khác biệt | | | |
| TC-GOAL-003 | Hiển thị trạng thái mục tiêu đã hoàn thành | 1. Xem danh sách mục tiêu<br>2. Kiểm tra mục tiêu đã hoàn thành | N/A | Mục tiêu Completed có badge "Hoàn thành" | | | |

### Chức năng 4.2: Tạo Mục Tiêu Mới

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-GOAL-004 | Tạo mục tiêu giảm cân | 1. Nhấn "Tạo mục tiêu mới"<br>2. Chọn loại: Weight Loss<br>3. Nhập cân nặng mục tiêu<br>4. Chọn deadline<br>5. Nhấn "Lưu" | GoalType: WeightLoss<br>TargetValue: 65kg<br>Deadline: 2026-06-01 | Mục tiêu được tạo thành công, hiển thị trong danh sách | | | |
| TC-GOAL-005 | Tạo mục tiêu tăng cân | 1. Nhấn "Tạo mục tiêu mới"<br>2. Chọn loại: Weight Gain<br>3. Nhập cân nặng mục tiêu<br>4. Nhấn "Lưu" | GoalType: WeightGain<br>TargetValue: 75kg | Mục tiêu được tạo thành công | | | |
| TC-GOAL-006 | Tạo mục tiêu calories hàng ngày | 1. Tạo mục tiêu calories<br>2. Nhập số calories mục tiêu | GoalType: DailyCalories<br>TargetValue: 2000 | Mục tiêu calories được tạo thành công | | | |
| TC-GOAL-007 | Tạo mục tiêu với giá trị không hợp lệ | 1. Nhấn "Tạo mục tiêu"<br>2. Nhập giá trị âm<br>3. Nhấn "Lưu" | TargetValue: -10 | Hiển thị lỗi validation "Giá trị phải là số dương" | | | |
| TC-GOAL-008 | Tạo mục tiêu với deadline trong quá khứ | 1. Tạo mục tiêu<br>2. Chọn deadline là ngày đã qua | Deadline: 2020-01-01 | Hiển thị lỗi "Deadline phải là ngày trong tương lai" | | | |

### Chức năng 4.3: Xem Chi Tiết & Theo Dõi Tiến Độ

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-GOAL-009 | Xem chi tiết mục tiêu | 1. Trong danh sách mục tiêu<br>2. Nhấn vào một mục tiêu | N/A | Trang chi tiết hiển thị: Biểu đồ tiến độ, Lịch sử cập nhật, Thống kê | | | |
| TC-GOAL-010 | Thêm progress mới | 1. Trong trang chi tiết mục tiêu<br>2. Nhấn "Thêm tiến độ"<br>3. Nhập giá trị mới<br>4. Nhấn "Lưu" | CurrentValue: 68kg (cho mục tiêu giảm cân) | Progress được thêm, biểu đồ cập nhật | | | |
| TC-GOAL-011 | Hiển thị biểu đồ tiến độ | 1. Xem chi tiết mục tiêu có nhiều progress<br>2. Kiểm tra biểu đồ Area/Line chart | N/A | Biểu đồ hiển thị đúng xu hướng tiến độ theo thời gian | | | |

---

## Module 5: Theo Dõi Dinh Dưỡng (Nutrition)

### Chức năng 5.1: Xem Nhật Ký Dinh Dưỡng

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-NUT-001 | Xem nhật ký dinh dưỡng hôm nay | 1. Vào trang "Dinh dưỡng" hoặc "Nutrition"<br>2. Xem tab/ngày hôm nay | N/A | Hiển thị các bữa ăn đã ghi nhận hôm nay, chia theo: Breakfast, Lunch, Dinner, Snacks | | | |
| TC-NUT-002 | Xem tổng calories và macros | 1. Xem trang Nutrition<br>2. Kiểm tra thống kê tổng | N/A | Hiển thị: Total Calories, Protein, Carbs, Fat của ngày | | | |
| TC-NUT-003 | Chọn ngày khác để xem | 1. Nhấn vào date picker<br>2. Chọn ngày khác | Date: 2026-01-01 | Nhật ký dinh dưỡng của ngày được chọn hiển thị | | | |

### Chức năng 5.2: Thêm Món Ăn

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-NUT-004 | Tìm kiếm món ăn | 1. Trong trang Nutrition<br>2. Nhấn "Thêm món ăn"<br>3. Nhập từ khóa tìm kiếm | Search: "Chicken" | Danh sách món ăn chứa từ "Chicken" hiển thị | | | |
| TC-NUT-005 | Thêm món ăn vào bữa sáng | 1. Tìm kiếm món ăn<br>2. Chọn món từ kết quả<br>3. Chọn bữa ăn: Breakfast<br>4. Nhập số lượng/serving<br>5. Nhấn "Thêm" | MealType: Breakfast<br>FoodItem: Chicken Breast<br>Quantity: 1 serving | Món ăn được thêm vào bữa sáng, cập nhật tổng calories | | | |
| TC-NUT-006 | Thêm món ăn vào bữa trưa | 1. Tương tự TC-NUT-005<br>2. Chọn MealType: Lunch | MealType: Lunch | Món ăn được thêm vào bữa trưa | | | |
| TC-NUT-007 | Thêm món ăn với số lượng nhiều | 1. Chọn món ăn<br>2. Nhập quantity = 3<br>3. Thêm | Quantity: 3 | Calories được nhân với 3, hiển thị đúng | | | |
| TC-NUT-008 | Tìm kiếm không có kết quả | 1. Nhập từ khóa không tồn tại | Search: "xyz123abc" | Hiển thị "Không tìm thấy món ăn" | | | |

### Chức năng 5.3: Xóa Món Ăn

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-NUT-009 | Xóa món ăn khỏi nhật ký | 1. Trong nhật ký dinh dưỡng<br>2. Nhấn nút "Xóa" bên cạnh một món<br>3. Xác nhận | FoodEntry để xóa | Món ăn bị xóa, tổng calories được cập nhật | | | |

---

## Module 6: Theo Dõi Bài Tập (Workout)

### Chức năng 6.1: Xem Lịch Sử Bài Tập

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-WORK-001 | Xem lịch sử bài tập | 1. Vào trang "Workout History" hoặc "Lịch sử luyện tập" | N/A | Hiển thị danh sách các buổi tập đã ghi nhận, nhóm theo ngày | | | |
| TC-WORK-002 | Xem chi tiết buổi tập | 1. Nhấn vào một buổi tập trong lịch sử | N/A | Hiển thị chi tiết: Các bài tập, Sets x Reps, Trọng lượng, Thời gian nghỉ | | | |
| TC-WORK-003 | Hiển thị thông tin trống khi chưa có buổi tập | 1. Đăng nhập user mới (chưa có buổi tập)<br>2. Vào Workout History | N/A | Hiển thị thông báo "Chưa có buổi tập nào" với nút "Tạo buổi tập đầu tiên" | | | |

### Chức năng 6.2: Tạo Buổi Tập Mới

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-WORK-004 | Tạo buổi tập mới | 1. Nhấn "Thêm buổi tập" hoặc "Create Workout"<br>2. Chọn ngày<br>3. Thêm các bài tập từ thư viện<br>4. Nhập sets, reps, weight cho mỗi bài<br>5. Nhập thời gian tổng<br>6. Nhấn "Lưu" | WorkoutDate: Today<br>Exercises: Push Up (3x15), Squat (4x12)<br>Duration: 45 mins | Buổi tập được tạo thành công, xuất hiện trong lịch sử | | | |
| TC-WORK-005 | Tìm kiếm bài tập theo tên | 1. Trong form tạo buổi tập<br>2. Tìm kiếm bài tập | Search: "Push" | Hiển thị các bài tập có tên chứa "Push" | | | |
| TC-WORK-006 | Lọc bài tập theo nhóm cơ | 1. Chọn filter: Muscle Group = Chest | Filter: Chest | Chỉ hiển thị bài tập cho ngực | | | |
| TC-WORK-007 | Lọc bài tập theo độ khó | 1. Chọn filter: Difficulty = Beginner | Filter: Beginner | Chỉ hiển thị bài tập dành cho người mới | | | |
| TC-WORK-008 | Thêm bài tập vào buổi tập | 1. Tìm bài tập<br>2. Nhấn nút "+" hoặc "Add"<br>3. Bài tập được thêm vào workout | Exercise: Push Up | Bài tập xuất hiện trong danh sách exercises của buổi tập | | | |
| TC-WORK-009 | Xóa bài tập khỏi buổi tập | 1. Trong form tạo workout<br>2. Nhấn nút "Xóa" bên cạnh bài tập đã thêm | N/A | Bài tập bị xóa khỏi danh sách | | | |
| TC-WORK-010 | Lưu buổi tập không có bài tập nào | 1. Không thêm bài tập nào<br>2. Nhấn "Lưu" | Exercises: (trống) | Hiển thị lỗi "Vui lòng thêm ít nhất một bài tập" | | | |
| TC-WORK-011 | Nhập sets/reps không hợp lệ | 1. Thêm bài tập<br>2. Nhập sets = 0 hoặc reps = -1 | Sets: 0 hoặc Reps: -1 | Hiển thị lỗi validation | | | |

---

## Module 7: AI Chat (HealthBot)

### Chức năng 7.1: Trò Chuyện Với AI

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-CHAT-001 | Mở trang Chat | 1. Đăng nhập<br>2. Vào trang "Chat" hoặc "HealthBot" | N/A | Giao diện chat hiển thị với header "HealthBot", vùng tin nhắn và input | | | |
| TC-CHAT-002 | Gửi tin nhắn đơn giản | 1. Nhập tin nhắn vào ô input<br>2. Nhấn nút "Gửi" hoặc Enter | Message: "Xin chào" | Tin nhắn user hiển thị bên phải, AI trả lời hiển thị bên trái | | | |
| TC-CHAT-003 | Hỏi về dinh dưỡng | 1. Gửi câu hỏi về dinh dưỡng | Message: "Tôi nên ăn gì để giảm cân?" | AI trả lời với thông tin dinh dưỡng phù hợp | | | |
| TC-CHAT-004 | Hỏi về bài tập | 1. Gửi câu hỏi về luyện tập | Message: "Bài tập nào tốt cho cơ ngực?" | AI trả lời với gợi ý bài tập | | | |
| TC-CHAT-005 | Hỏi về sức khỏe cá nhân | 1. Gửi câu hỏi về BMI, cân nặng | Message: "BMI của tôi có bình thường không?" | AI phân tích dựa trên dữ liệu user và trả lời | | | |
| TC-CHAT-006 | Gửi tin nhắn trống | 1. Để trống ô input<br>2. Nhấn nút "Gửi" | Message: (trống) | Nút gửi bị disable hoặc không có action | | | |
| TC-CHAT-007 | Xem lịch sử chat | 1. Đóng trình duyệt<br>2. Mở lại và vào trang Chat | N/A | Lịch sử chat trước đó được load và hiển thị | | | |
| TC-CHAT-008 | Refresh lịch sử chat | 1. Nhấn nút "Refresh" trên header chat | N/A | Lịch sử chat được load lại từ server | | | |
| TC-CHAT-009 | Hiển thị đang xử lý khi AI trả lời | 1. Gửi tin nhắn<br>2. Quan sát khi chờ AI | N/A | Hiển thị animation "..." hoặc "Đang nhập..." khi AI đang xử lý | | | |

---

## Module 8: Đăng Xuất & Bảo Mật

### Chức năng 8.1: Đăng Xuất

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-LOGOUT-001 | Đăng xuất thành công | 1. Nhấn vào avatar/menu người dùng<br>2. Nhấn "Đăng xuất" | N/A | Hệ thống chuyển về trang đăng nhập, session được xóa | | | |
| TC-LOGOUT-002 | Truy cập trang sau khi đăng xuất | 1. Đăng xuất<br>2. Nhập URL Dashboard trực tiếp | URL: /dashboard | Hệ thống chuyển hướng về trang đăng nhập | | | |
| TC-LOGOUT-003 | Session hết hạn | 1. Để app idle trong thời gian dài<br>2. Thực hiện một action | N/A | Hệ thống yêu cầu đăng nhập lại | | | |

---

# TỔNG KẾT TEST CASE USER WEB

| Module | Số lượng Test Case | Pass | Fail | Pending |
|--------|-------------------|------|------|---------|
| Xác thực (Đăng ký, Đăng nhập, Quên MK) | 23 | | | |
| Hoàn thiện hồ sơ & Cập nhật | 10 | | | |
| Dashboard | 6 | | | |
| Quản lý Mục tiêu | 11 | | | |
| Theo dõi Dinh dưỡng | 9 | | | |
| Theo dõi Bài tập | 11 | | | |
| AI Chat | 9 | | | |
| Đăng xuất & Bảo mật | 3 | | | |
| **TỔNG** | **82** | | | |

---

*Ghi chú: Tiếp theo là Test Case cho luồng User trên Mobile (Flutter)*
