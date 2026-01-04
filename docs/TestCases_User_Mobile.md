# Tài Liệu Test Case - Luồng User (Mobile)

## Thông Tin Tài Liệu

| Thông tin | Chi tiết |
|-----------|----------|
| **Dự án** | HealthSync - Ứng dụng theo dõi sức khỏe |
| **Phiên bản** | 1.0 |
| **Ngày tạo** | 02/01/2026 |
| **Người tạo** | QA Team |
| **Loại kiểm thử** | Functional Testing / System Testing |
| **Nền tảng** | Mobile (Flutter - Android/iOS) |

---

## Phạm Vi Kiểm Thử

Tài liệu này tập trung vào **kiểm thử chức năng (Functional Testing)** cho luồng **User/Customer** trên nền tảng **Mobile** (Flutter). Các test case được viết theo workflow từ Splash Screen → Welcome → đăng ký/đăng nhập → sử dụng các chức năng chính.

---

# PHẦN 3: LUỒNG USER/CUSTOMER (MOBILE - Flutter)

---

## Module 1: Khởi Động Ứng Dụng

### Chức năng 1.1: Splash Screen

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-SPLASH-001 | Hiển thị Splash Screen khi mở app | 1. Mở ứng dụng HealthSync trên điện thoại | N/A | Splash Screen hiển thị với logo HealthSync và animation loading | | | |
| TC-M-SPLASH-002 | Chuyển đến Welcome Screen (chưa đăng nhập) | 1. Mở app lần đầu (chưa đăng nhập) | N/A | Sau 2-3 giây, chuyển đến Welcome Screen | | | |
| TC-M-SPLASH-003 | Chuyển đến Home Screen (đã đăng nhập) | 1. Mở app khi đã có session đăng nhập | Token hợp lệ trong storage | Sau khi verify token, chuyển thẳng đến Home Screen | | | |

### Chức năng 1.2: Welcome Screen

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-WELCOME-001 | Hiển thị Welcome Screen | 1. Mở app lần đầu<br>2. Chờ Splash Screen kết thúc | N/A | Welcome Screen hiển thị với: Logo, Slogan, nút "Sign In", nút "Sign Up", nút "Continue with Google" | | | |
| TC-M-WELCOME-002 | Nhấn nút Sign In | 1. Trong Welcome Screen<br>2. Nhấn nút "Sign In" | N/A | Chuyển đến Sign In Screen | | | |
| TC-M-WELCOME-003 | Nhấn nút Sign Up | 1. Trong Welcome Screen<br>2. Nhấn nút "Sign Up" | N/A | Chuyển đến Sign Up Screen | | | |
| TC-M-WELCOME-004 | Nhấn Continue with Google | 1. Nhấn "Continue with Google" | N/A | Mở Google Sign-In dialog để chọn tài khoản | | | |

---

## Module 2: Xác Thực Người Dùng (Authentication)

### Chức năng 2.1: Đăng Ký Tài Khoản

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-REG-001 | Đăng ký với thông tin hợp lệ | 1. Mở Sign Up Screen<br>2. Nhập email hợp lệ<br>3. Nhập mật khẩu đủ mạnh<br>4. Nhập xác nhận mật khẩu<br>5. Nhấn "Sign Up" | Email: newuser@test.com<br>Password: User@12345<br>Confirm: User@12345 | Hệ thống gửi mã xác thực, chuyển đến Email Verification Screen | | | |
| TC-M-REG-002 | Xác thực OTP đúng | 1. Nhận OTP từ email<br>2. Nhập 6 số OTP vào các ô<br>3. Nhấn "Verify" | OTP: 123456 (mã đúng) | Xác thực thành công, chuyển đến Signup Success Screen | | | |
| TC-M-REG-003 | Xác thực OTP sai | 1. Nhập OTP sai<br>2. Nhấn "Verify" | OTP: 000000 | Hiển thị lỗi "Mã xác thực không đúng", các ô OTP bị reset | | | |
| TC-M-REG-004 | Đăng ký với email đã tồn tại | 1. Nhập email đã có<br>2. Nhấn "Sign Up" | Email: admin@healthsync.com | Hiển thị SnackBar lỗi "Email đã được sử dụng" | | | |
| TC-M-REG-005 | Đăng ký với mật khẩu không khớp | 1. Nhập password và confirm khác nhau<br>2. Nhấn "Sign Up" | Password: User@123<br>Confirm: Different@123 | Hiển thị lỗi "Mật khẩu không khớp" | | | |
| TC-M-REG-006 | Đăng ký với mật khẩu yếu | 1. Nhập mật khẩu < 8 ký tự<br>2. Nhấn "Sign Up" | Password: 1234567 | Hiển thị lỗi "Mật khẩu phải có ít nhất 8 ký tự" | | | |
| TC-M-REG-007 | Hiển thị/Ẩn mật khẩu | 1. Nhập mật khẩu<br>2. Nhấn icon "eye" | N/A | Mật khẩu chuyển đổi giữa hiển thị và ẩn | | | |
| TC-M-REG-008 | Gửi lại OTP | 1. Trong Email Verification Screen<br>2. Nhấn "Resend Code" | N/A | OTP mới được gửi, hiển thị thông báo thành công | | | |
| TC-M-REG-009 | Quay lại từ màn hình xác thực | 1. Trong Email Verification Screen<br>2. Nhấn nút Back | N/A | Quay lại Sign Up Screen | | | |
| TC-M-REG-010 | Nhấn link "Already have an account" | 1. Trong Sign Up Screen<br>2. Nhấn "Already have an account? Sign In" | N/A | Chuyển đến Sign In Screen | | | |

### Chức năng 2.2: Đăng Nhập

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-LOGIN-001 | Đăng nhập với thông tin hợp lệ | 1. Mở Sign In Screen<br>2. Nhập email<br>3. Nhập mật khẩu<br>4. Nhấn "Sign In" | Email: user@test.com<br>Password: User@123 | Đăng nhập thành công, chuyển đến Home Screen | | | |
| TC-M-LOGIN-002 | Đăng nhập với sai mật khẩu | 1. Nhập email đúng<br>2. Nhập mật khẩu sai<br>3. Nhấn "Sign In" | Email: user@test.com<br>Password: wrongpass | Hiển thị SnackBar "Sai email hoặc mật khẩu" | | | |
| TC-M-LOGIN-003 | Đăng nhập với email không tồn tại | 1. Nhập email không có<br>2. Nhấn "Sign In" | Email: notexist@test.com | Hiển thị SnackBar lỗi | | | |
| TC-M-LOGIN-004 | Đăng nhập để trống email | 1. Để trống email<br>2. Nhấn "Sign In" | Email: (trống) | Hiển thị lỗi validation dưới TextField | | | |
| TC-M-LOGIN-005 | Đăng nhập để trống mật khẩu | 1. Để trống mật khẩu<br>2. Nhấn "Sign In" | Password: (trống) | Hiển thị lỗi validation | | | |
| TC-M-LOGIN-006 | Đăng nhập với tài khoản bị khóa | 1. Nhập tài khoản đã bị Admin khóa<br>2. Nhấn "Sign In" | Email: locked@test.com | Hiển thị thông báo "Tài khoản đã bị khóa" | | | |
| TC-M-LOGIN-007 | Đăng nhập bằng Google | 1. Nhấn "Continue with Google"<br>2. Chọn tài khoản Google | Tài khoản Google hợp lệ | Đăng nhập thành công, nếu lần đầu → Complete Profile, ngược lại → Home | | | |
| TC-M-LOGIN-008 | Nhấn link "Don't have account" | 1. Trong Sign In Screen<br>2. Nhấn "Don't have an account? Sign Up" | N/A | Chuyển đến Sign Up Screen | | | |
| TC-M-LOGIN-009 | Nhấn "Forgot Password" | 1. Trong Sign In Screen<br>2. Nhấn "Forgot Password?" | N/A | Chuyển đến Account Recovery Screen | | | |

### Chức năng 2.3: Khôi Phục Tài Khoản (Account Recovery)

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-FORGOT-001 | Yêu cầu reset mật khẩu | 1. Mở Account Recovery Screen<br>2. Nhập email đã đăng ký<br>3. Nhấn "Send Code" | Email: user@test.com | Mã OTP được gửi đến email, chuyển đến Reset Password Screen | | | |
| TC-M-FORGOT-002 | Yêu cầu reset với email không tồn tại | 1. Nhập email không có<br>2. Nhấn "Send Code" | Email: notexist@test.com | Hiển thị lỗi "Email không tồn tại" | | | |
| TC-M-FORGOT-003 | Reset mật khẩu thành công | 1. Nhập OTP đúng<br>2. Nhập mật khẩu mới<br>3. Xác nhận mật khẩu<br>4. Nhấn "Reset Password" | OTP: 123456<br>NewPassword: NewPass@123<br>Confirm: NewPass@123 | Mật khẩu được đổi, hiển thị Password Reset Success Screen | | | |
| TC-M-FORGOT-004 | Reset với OTP sai | 1. Nhập OTP sai<br>2. Nhấn "Reset Password" | OTP: 000000 | Hiển thị lỗi "Mã xác thực không đúng" | | | |
| TC-M-FORGOT-005 | Reset với mật khẩu không khớp | 1. Nhập OTP đúng<br>2. Nhập mật khẩu mới và confirm khác nhau | NewPassword: Pass@123<br>Confirm: Different@123 | Hiển thị lỗi "Mật khẩu không khớp" | | | |

---

## Module 3: Hoàn Thiện Hồ Sơ (Complete Profile)

### Chức năng 3.1: Nhập Thông Tin Cá Nhân

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-PROFILE-001 | Hoàn thiện profile với dữ liệu hợp lệ | 1. Sau đăng ký thành công, chuyển đến Complete Profile<br>2. Nhập họ tên<br>3. Chọn giới tính<br>4. Chọn ngày sinh từ DatePicker<br>5. Nhập chiều cao (cm)<br>6. Nhập cân nặng (kg)<br>7. Chọn mức độ hoạt động<br>8. Nhấn "Save" | FirstName: John<br>LastName: Doe<br>Gender: Male<br>DOB: 01/15/1990<br>Height: 175<br>Weight: 70<br>ActivityLevel: Moderate | Profile được lưu, chuyển đến Home Screen | | | |
| TC-M-PROFILE-002 | Để trống trường bắt buộc | 1. Để trống "First Name"<br>2. Nhấn "Save" | FirstName: (trống) | Hiển thị lỗi validation | | | |
| TC-M-PROFILE-003 | Nhập chiều cao không hợp lệ | 1. Nhập chiều cao quá lớn<br>2. Nhấn "Save" | Height: 500 | Hiển thị lỗi "Chiều cao không hợp lệ" | | | |
| TC-M-PROFILE-004 | Nhập cân nặng không hợp lệ | 1. Nhập cân nặng âm<br>2. Nhấn "Save" | Weight: -10 | Hiển thị lỗi "Cân nặng không hợp lệ" | | | |
| TC-M-PROFILE-005 | Chọn ngày sinh từ DatePicker | 1. Nhấn vào field Date of Birth<br>2. DatePicker hiện lên<br>3. Chọn ngày | DOB: 1990-05-15 | Ngày được chọn hiển thị đúng định dạng | | | |
| TC-M-PROFILE-006 | Chọn mức độ hoạt động | 1. Nhấn vào Dropdown Activity Level<br>2. Chọn "Active" | ActivityLevel: Active | Giá trị được chọn hiển thị trong dropdown | | | |

---

## Module 4: Trang Chủ (Home Screen)

### Chức năng 4.1: Hiển Thị Home

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-HOME-001 | Hiển thị Home Screen sau đăng nhập | 1. Đăng nhập thành công<br>2. Kiểm tra Home Screen | N/A | Home Screen hiển thị với: Greeting (Xin chào + tên), Thống kê BMI, Card dinh dưỡng hôm nay, Card bài tập, Bottom Navigation | | | |
| TC-M-HOME-002 | Hiển thị thống kê BMI | 1. Xem Home Screen<br>2. Kiểm tra chỉ số BMI | N/A | BMI được tính đúng, hiển thị trạng thái (Underweight/Normal/Overweight/Obese) | | | |
| TC-M-HOME-003 | Hiển thị calories hôm nay | 1. Xem Home Screen<br>2. Kiểm tra card Nutrition | N/A | Hiển thị tổng calories đã ăn hôm nay | | | |
| TC-M-HOME-004 | Navigation đến Nutrition từ Home | 1. Trong Home Screen<br>2. Nhấn vào card Nutrition hoặc icon Nutrition trên Bottom Nav | N/A | Chuyển đến Nutrition Screen | | | |
| TC-M-HOME-005 | Navigation đến Goals từ Home | 1. Nhấn icon Goals trên Bottom Nav | N/A | Chuyển đến Goals Screen | | | |
| TC-M-HOME-006 | Navigation đến Workout từ Home | 1. Nhấn vào card Workout hoặc icon Workout | N/A | Chuyển đến Workout History Screen | | | |
| TC-M-HOME-007 | Navigation đến Chat từ Home | 1. Nhấn icon Chat trên Bottom Nav | N/A | Chuyển đến Chat Screen | | | |

### Chức năng 4.2: Bottom Navigation

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-NAV-001 | Chuyển tab giữa các màn hình | 1. Nhấn lần lượt các icon trên Bottom Navigation | N/A | Màn hình tương ứng được hiển thị, icon được highlight | | | |
| TC-M-NAV-002 | Giữ state khi chuyển tab | 1. Vào Nutrition, thêm món ăn<br>2. Chuyển sang Goals<br>3. Quay lại Nutrition | N/A | Dữ liệu vẫn được giữ, không bị reset | | | |

---

## Module 5: Quản Lý Mục Tiêu (Goals)

### Chức năng 5.1: Xem Danh Sách Mục Tiêu

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-GOAL-001 | Xem danh sách mục tiêu | 1. Vào Goals Screen từ Bottom Nav | N/A | Hiển thị danh sách mục tiêu dạng Card với: Loại mục tiêu, Target value, Progress bar, Deadline | | | |
| TC-M-GOAL-002 | Hiển thị mục tiêu trống | 1. User mới chưa có mục tiêu<br>2. Vào Goals Screen | N/A | Hiển thị "Chưa có mục tiêu nào" với nút "Tạo mục tiêu mới" | | | |
| TC-M-GOAL-003 | Scroll danh sách mục tiêu | 1. Có nhiều mục tiêu<br>2. Scroll xuống | N/A | Danh sách scroll mượt, không bị lag | | | |

### Chức năng 5.2: Tạo Mục Tiêu Mới

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-GOAL-004 | Tạo mục tiêu giảm cân | 1. Nhấn FAB "+" hoặc "Tạo mục tiêu"<br>2. Chọn Goal Type: Weight Loss<br>3. Nhập target weight<br>4. Chọn deadline<br>5. Nhấn "Tạo" | GoalType: WeightLoss<br>TargetValue: 65<br>StartDate: Today<br>EndDate: 2026-06-01 | Mục tiêu được tạo, xuất hiện trong danh sách | | | |
| TC-M-GOAL-005 | Tạo mục tiêu tăng cân | 1. Chọn Goal Type: Weight Gain<br>2. Nhập target<br>3. Tạo | GoalType: WeightGain<br>TargetValue: 80 | Mục tiêu được tạo thành công | | | |
| TC-M-GOAL-006 | Tạo mục tiêu calories | 1. Chọn Goal Type: Daily Calories<br>2. Nhập target calories | GoalType: DailyCalories<br>TargetValue: 2500 | Mục tiêu calories được tạo | | | |
| TC-M-GOAL-007 | Tạo mục tiêu với giá trị âm | 1. Nhập target value âm<br>2. Nhấn "Tạo" | TargetValue: -50 | Hiển thị lỗi validation | | | |
| TC-M-GOAL-008 | Chọn StartDate và EndDate | 1. Nhấn vào DatePicker cho StartDate<br>2. Chọn ngày<br>3. Tương tự cho EndDate | StartDate: 2026-01-01<br>EndDate: 2026-06-01 | Ngày được chọn hiển thị đúng | | | |

### Chức năng 5.3: Chi Tiết Mục Tiêu

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-GOAL-009 | Xem chi tiết mục tiêu | 1. Trong Goals Screen<br>2. Nhấn vào một Goal Card | N/A | Chuyển đến Goal Details Screen với: Biểu đồ tiến độ, Lịch sử progress, Thống kê | | | |
| TC-M-GOAL-010 | Thêm progress mới | 1. Trong Goal Details<br>2. Nhấn FAB "+"<br>3. Chuyển đến Add Progress Screen<br>4. Nhập giá trị mới<br>5. Nhấn "Lưu" | CurrentValue: 68 (cho mục tiêu giảm còn 65kg) | Progress được thêm, biểu đồ cập nhật | | | |
| TC-M-GOAL-011 | Xem biểu đồ tiến độ | 1. Xem Goal Details có nhiều progress | N/A | Biểu đồ Line/Area hiển thị đúng xu hướng | | | |
| TC-M-GOAL-012 | Xem thống kê mục tiêu | 1. Scroll xuống trong Goal Details | N/A | Hiển thị: Current Value, Average, Best, Ngày còn lại | | | |

---

## Module 6: Theo Dõi Dinh Dưỡng (Nutrition)

### Chức năng 6.1: Xem Nhật Ký Dinh Dưỡng

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-NUT-001 | Xem Nutrition Screen | 1. Vào Nutrition từ Bottom Nav | N/A | Hiển thị: Bộ chọn ngày, Tổng calories, Macros (P/C/F), Danh sách bữa ăn | | | |
| TC-M-NUT-002 | Xem tổng calories và macros | 1. Xem Nutrition Screen<br>2. Kiểm tra summary section | N/A | Hiển thị đúng: Total Calories, Protein, Carbs, Fat dạng progress bar | | | |
| TC-M-NUT-003 | Chọn ngày khác | 1. Nhấn nút chọn ngày hoặc swipe<br>2. Chọn ngày trong quá khứ | Date: 2026-01-01 | Nhật ký của ngày được chọn hiển thị | | | |
| TC-M-NUT-004 | Xem danh sách bữa ăn | 1. Scroll xuống trong Nutrition Screen | N/A | Hiển thị các bữa: Breakfast, Lunch, Dinner, Snacks với các món đã thêm | | | |

### Chức năng 6.2: Thêm Món Ăn

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-NUT-005 | Mở dialog/screen thêm món ăn | 1. Nhấn FAB "+" hoặc nút "Thêm" bên cạnh bữa ăn | N/A | Hiển thị màn hình/dialog tìm kiếm món ăn | | | |
| TC-M-NUT-006 | Tìm kiếm món ăn | 1. Trong search screen<br>2. Nhập từ khóa | Search: "Chicken" | Danh sách món ăn chứa "Chicken" hiển thị | | | |
| TC-M-NUT-007 | Chọn món ăn từ kết quả | 1. Tìm kiếm món<br>2. Nhấn vào một món trong danh sách | FoodItem: Chicken Breast | Hiển thị chi tiết món: Tên, Serving size, Calories, P/C/F | | | |
| TC-M-NUT-008 | Thêm món vào bữa ăn | 1. Chọn món<br>2. Chọn Meal Type<br>3. Nhập số serving<br>4. Nhấn "Thêm" | MealType: Lunch<br>Servings: 1 | Món được thêm vào bữa trưa, tổng calories cập nhật | | | |
| TC-M-NUT-009 | Thêm món với nhiều serving | 1. Nhập servings = 2<br>2. Thêm | Servings: 2 | Calories được nhân 2 | | | |
| TC-M-NUT-010 | Tìm kiếm không có kết quả | 1. Nhập từ khóa không tồn tại | Search: "xyz123" | Hiển thị "Không tìm thấy món ăn" | | | |

### Chức năng 6.3: Xóa Món Ăn

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-NUT-011 | Xóa món khỏi nhật ký | 1. Trong danh sách bữa ăn<br>2. Swipe left trên món ăn hoặc nhấn nút xóa<br>3. Xác nhận | N/A | Món bị xóa, tổng calories cập nhật | | | |
| TC-M-NUT-012 | Hủy xóa món | 1. Nhấn xóa<br>2. Nhấn "Hủy" trong dialog xác nhận | N/A | Dialog đóng, món vẫn còn | | | |

---

## Module 7: Theo Dõi Bài Tập (Workout)

### Chức năng 7.1: Xem Lịch Sử Bài Tập

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-WORK-001 | Xem Workout History Screen | 1. Vào Workout từ Home hoặc Navigation | N/A | Hiển thị danh sách buổi tập, nhóm theo ngày | | | |
| TC-M-WORK-002 | Hiển thị chi tiết buổi tập | 1. Nhấn vào một buổi tập | N/A | Hiển thị/Expand chi tiết: Các bài tập, Sets x Reps x Weight, Duration | | | |
| TC-M-WORK-003 | Danh sách trống | 1. User mới chưa có buổi tập | N/A | Hiển thị "Chưa có buổi tập" với nút "Tạo buổi tập mới" | | | |

### Chức năng 7.2: Tạo Buổi Tập Mới

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-WORK-004 | Mở Create Workout Screen | 1. Nhấn FAB "+" trong Workout History | N/A | Chuyển đến Create Workout Screen | | | |
| TC-M-WORK-005 | Chọn ngày tập | 1. Trong Create Workout<br>2. Nhấn DatePicker<br>3. Chọn ngày | WorkoutDate: Today | Ngày được chọn hiển thị | | | |
| TC-M-WORK-006 | Tìm kiếm bài tập | 1. Trong phần "Thêm bài tập"<br>2. Nhập từ khóa | Search: "Push" | Hiển thị bài tập có tên chứa "Push" | | | |
| TC-M-WORK-007 | Lọc theo nhóm cơ | 1. Nhấn dropdown/chip "Nhóm cơ"<br>2. Chọn "Chest" | MuscleGroup: Chest | Chỉ hiển thị bài tập cho ngực | | | |
| TC-M-WORK-008 | Lọc theo độ khó | 1. Chọn filter "Độ khó"<br>2. Chọn "Beginner" | Difficulty: Beginner | Chỉ hiển thị bài tập Beginner | | | |
| TC-M-WORK-009 | Thêm bài tập vào workout | 1. Nhấn "+" bên cạnh bài tập | Exercise: Push Up | Bài tập được thêm vào danh sách "Bài tập đã chọn" | | | |
| TC-M-WORK-010 | Nhập sets, reps, weight | 1. Sau khi thêm bài tập<br>2. Nhập Sets, Reps, Weight vào các TextField | Sets: 3<br>Reps: 15<br>Weight: 0 | Giá trị được lưu | | | |
| TC-M-WORK-011 | Nhập thời gian nghỉ | 1. Nhập Rest time | RestSec: 60 | Giá trị được lưu | | | |
| TC-M-WORK-012 | Xóa bài tập khỏi workout | 1. Nhấn nút xóa/swipe bài tập đã thêm | N/A | Bài tập bị xóa khỏi danh sách | | | |
| TC-M-WORK-013 | Nhập thời gian tổng và notes | 1. Nhập Duration<br>2. Nhập Notes | Duration: 45<br>Notes: "Great workout!" | Giá trị được lưu | | | |
| TC-M-WORK-014 | Lưu buổi tập | 1. Đã thêm ít nhất 1 bài tập<br>2. Nhấn "Lưu" | N/A | Buổi tập được lưu, quay lại Workout History với buổi tập mới | | | |
| TC-M-WORK-015 | Lưu buổi tập không có bài | 1. Không thêm bài tập nào<br>2. Nhấn "Lưu" | N/A | Hiển thị lỗi "Vui lòng thêm ít nhất 1 bài tập" | | | |

---

## Module 8: AI Chat (HealthBot)

### Chức năng 8.1: Trò Chuyện Với AI

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-CHAT-001 | Mở Chat Screen | 1. Nhấn icon Chat trên Bottom Navigation | N/A | Chat Screen hiển thị với: Header HealthBot, vùng tin nhắn, input box | | | |
| TC-M-CHAT-002 | Gửi tin nhắn | 1. Nhập tin nhắn vào TextField<br>2. Nhấn nút gửi | Message: "Xin chào" | Tin nhắn user hiển thị bên phải (bubble), AI trả lời bên trái | | | |
| TC-M-CHAT-003 | Hỏi về dinh dưỡng | 1. Gửi câu hỏi về dinh dưỡng | Message: "Tôi nên ăn bao nhiêu protein mỗi ngày?" | AI trả lời với thông tin dinh dưỡng | | | |
| TC-M-CHAT-004 | Hỏi về bài tập | 1. Gửi câu hỏi về workout | Message: "Bài tập nào giúp giảm mỡ bụng?" | AI trả lời với gợi ý bài tập | | | |
| TC-M-CHAT-005 | Hỏi về sức khỏe cá nhân | 1. Hỏi về BMI, cân nặng | Message: "Với chiều cao 175cm và cân nặng 70kg, BMI của tôi là bao nhiêu?" | AI tính và trả lời BMI | | | |
| TC-M-CHAT-006 | Gửi tin nhắn trống | 1. Để trống TextField<br>2. Nhấn gửi | Message: (trống) | Nút gửi bị disable hoặc không có action | | | |
| TC-M-CHAT-007 | Hiển thị loading khi AI đang xử lý | 1. Gửi tin nhắn<br>2. Quan sát | N/A | Hiển thị animation typing (3 dots) hoặc loading indicator | | | |
| TC-M-CHAT-008 | Xem lịch sử chat | 1. Đóng app<br>2. Mở lại, vào Chat | N/A | Lịch sử chat trước đó được load | | | |
| TC-M-CHAT-009 | Scroll lịch sử chat | 1. Có nhiều tin nhắn<br>2. Scroll lên xem tin cũ | N/A | Scroll mượt, tin nhắn cũ hiển thị đúng | | | |
| TC-M-CHAT-010 | Auto scroll khi có tin mới | 1. Đang ở cuối danh sách<br>2. AI trả lời tin mới | N/A | Tự động scroll xuống tin mới nhất | | | |

---

## Module 9: Tạo Mật Khẩu (Google Account)

### Chức năng 9.1: Tạo Mật Khẩu Cho Tài Khoản Google

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-GPASS-001 | Hiển thị Create Password Screen | 1. User đăng nhập bằng Google lần đầu<br>2. Sau Complete Profile, chuyển đến Create Password | N/A | Hiển thị form nhập mật khẩu mới | | | |
| TC-M-GPASS-002 | Tạo mật khẩu thành công | 1. Nhập mật khẩu mới đủ mạnh<br>2. Xác nhận mật khẩu<br>3. Nhấn "Tạo" | Password: User@12345<br>Confirm: User@12345 | Mật khẩu được tạo, có thể đăng nhập bằng email + password | | | |
| TC-M-GPASS-003 | Mật khẩu không khớp | 1. Nhập password và confirm khác nhau | Password: Pass@123<br>Confirm: Different@123 | Hiển thị lỗi | | | |
| TC-M-GPASS-004 | Bỏ qua tạo mật khẩu | 1. Nhấn "Bỏ qua" (nếu có) | N/A | Chuyển đến Home, vẫn có thể đăng nhập bằng Google | | | |

---

## Module 10: Cài Đặt & Đăng Xuất

### Chức năng 10.1: Đăng Xuất

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-LOGOUT-001 | Đăng xuất thành công | 1. Vào Settings/Profile<br>2. Nhấn "Đăng xuất"<br>3. Xác nhận | N/A | Token bị xóa, chuyển về Welcome Screen | | | |
| TC-M-LOGOUT-002 | Hủy đăng xuất | 1. Nhấn "Đăng xuất"<br>2. Nhấn "Hủy" trong dialog xác nhận | N/A | Dialog đóng, vẫn ở trong app | | | |
| TC-M-LOGOUT-003 | Mở app sau khi đăng xuất | 1. Đăng xuất<br>2. Đóng app hoàn toàn<br>3. Mở lại | N/A | Hiển thị Welcome Screen, yêu cầu đăng nhập | | | |

---

## Module 11: Kiểm Thử Đặc Thù Mobile

### Chức năng 11.1: Gesture & Navigation

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-GEST-001 | Vuốt để back (iOS) | 1. Vào màn hình con<br>2. Vuốt từ cạnh trái sang phải | N/A | Quay lại màn hình trước | | | iOS only |
| TC-M-GEST-002 | Nhấn nút Back (Android) | 1. Vào màn hình con<br>2. Nhấn nút Back cứng | N/A | Quay lại màn hình trước | | | Android only |
| TC-M-GEST-003 | Pull to Refresh | 1. Trong danh sách (Goals, Nutrition...)<br>2. Kéo xuống để refresh | N/A | Loading indicator hiện, dữ liệu được tải lại | | | |
| TC-M-GEST-004 | Swipe to Delete | 1. Trong danh sách có thể xóa<br>2. Swipe item sang trái | N/A | Hiển thị nút Delete hoặc xóa trực tiếp | | | |

### Chức năng 11.2: Orientation & Screen Size

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-ORI-001 | Xoay màn hình Portrait → Landscape | 1. Đang ở màn hình bất kỳ<br>2. Xoay điện thoại sang ngang | N/A | Layout responsive, không bị vỡ | | | |
| TC-M-ORI-002 | Xoay màn hình Landscape → Portrait | 1. Đang ở Landscape<br>2. Xoay về Portrait | N/A | Layout trở lại bình thường, dữ liệu không mất | | | |
| TC-M-ORI-003 | Hiển thị trên màn hình nhỏ | 1. Chạy app trên điện thoại có màn hình nhỏ (< 5 inch) | N/A | Tất cả text và button có thể đọc và nhấn được | | | |
| TC-M-ORI-004 | Hiển thị trên tablet | 1. Chạy app trên tablet | N/A | Layout scale hợp lý, không bị quá nhỏ | | | |

### Chức năng 11.3: Offline & Network

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-NET-001 | Mất kết nối khi đang sử dụng | 1. Đang sử dụng app<br>2. Tắt WiFi/Mobile Data | N/A | Hiển thị thông báo "Không có kết nối mạng" | | | |
| TC-M-NET-002 | Gửi request khi offline | 1. Tắt mạng<br>2. Thử submit form (thêm bữa ăn, tạo workout...) | N/A | Hiển thị lỗi network, không crash | | | |
| TC-M-NET-003 | Khôi phục kết nối | 1. Đang offline<br>2. Bật lại mạng | N/A | App tự động retry hoặc cho phép user refresh | | | |

### Chức năng 11.4: Performance

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-PERF-001 | App start time | 1. Đóng app hoàn toàn<br>2. Mở app, đo thời gian đến Home | N/A | Thời gian khởi động < 3 giây | | | |
| TC-M-PERF-002 | Scroll mượt trong danh sách dài | 1. Mở danh sách có nhiều item<br>2. Scroll nhanh | N/A | Frame rate > 55 fps, không lag | | | |
| TC-M-PERF-003 | Memory usage | 1. Sử dụng app 10-15 phút<br>2. Kiểm tra memory | N/A | Không có memory leak, RAM usage ổn định | | | |

---

## Module 12: Quản Lý Hồ Sơ Cá Nhân (User Profile)

### Chức năng 12.1: Xem & Cập Nhật Hồ Sơ

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-UPROF-001 | Truy cập Profile Screen từ Home | 1. Tại Home Screen<br>2. Nhấn vào Avatar ở góc trái trên | N/A | Chuyển đến màn hình Hồ sơ cá nhân | | | |
| TC-M-UPROF-002 | Hiển thị thông tin cá nhân | 1. Mở Profile Screen | N/A | Hiển thị đúng Avatar, Email, Họ tên, Ngày sinh, Giới tính, Chiều cao, Cân nặng, Mức độ hoạt động | | | |
| TC-M-UPROF-003 | Cập nhật thông tin hợp lệ | 1. Sửa đổi thông tin (ví dụ: thay đổi cân nặng)<br>2. Nhấn "Lưu thay đổi" | Weight: 72 | Hiển thị thông báo thành công, dữ liệu được cập nhật trên server | | | |
| TC-M-UPROF-004 | Cập nhật thông tin không hợp lệ | 1. Nhập chiều cao âm hoặc quá lớn<br>2. Nhấn "Lưu thay đổi" | Height: -5 | Hiển thị lỗi validation | | | |
| TC-M-UPROF-005 | Đổi Avatar từ thư viện | 1. Nhấn icon Camera trên Avatar<br>2. Chọn ảnh từ thư viện | Image file | Avatar mới được upload và hiển thị ngay lập tức | | | |
| TC-M-UPROF-006 | Đổi Avatar chụp ảnh mới | 1. Nhấn icon Camera<br>2. Chọn chụp ảnh (nếu hỗ trợ) | Camera capture | Avatar mới được upload và hiển thị | | | |
| TC-M-UPROF-007 | Quay lại Home từ Profile | 1. Nhấn nút Back trên AppBar | N/A | Quay lại Home Screen, Avatar trên Home được cập nhật (nếu có thay đổi) | | | |
| TC-M-UPROF-008 | Đăng xuất từ Profile | 1. Nhấn nút "Đăng xuất"<br>2. Xác nhận | N/A | Token bị xóa, chuyển về màn hình đăng nhập | | | |

---

# TỔNG KẾT TEST CASE USER MOBILE

| Module | Số lượng Test Case | Pass | Fail | Pending |
|--------|-------------------|------|------|---------|
| Khởi động (Splash, Welcome) | 7 | | | |
| Xác thực (Đăng ký, Đăng nhập, Recovery) | 24 | | | |
| Hoàn thiện hồ sơ | 6 | | | |
| Trang chủ & Navigation | 9 | | | |
| Quản lý Mục tiêu | 12 | | | |
| Theo dõi Dinh dưỡng | 12 | | | |
| Theo dõi Bài tập | 15 | | | |
| AI Chat | 10 | | | |
| Tạo mật khẩu Google | 4 | | | |
| Đăng xuất | 3 | | | |
| Kiểm thử đặc thù Mobile | 14 | | | |
| Quản lý Hồ sơ cá nhân | 8 | | | |
| **TỔNG** | **124** | | | |

---

# TỔNG KẾT TOÀN BỘ TEST CASE

| Phần | Số lượng Test Case |
|------|-------------------|
| Phần 1: Admin (Web) | 59 |
| Phần 2: User Web | 78 |
| Phần 3: User Mobile | 124 |
| **TỔNG CỘNG** | **261** |

---

*Ghi chú: Tài liệu này có thể được mở rộng thêm các test case cho các tính năng mới hoặc các trường hợp edge case khác.*
