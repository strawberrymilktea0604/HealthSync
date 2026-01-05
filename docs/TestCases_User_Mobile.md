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
| TC-M-GOAL-013 | Khóa nút Cập nhật cho goal not_started | 1. Xem goal có status "not_started"<br>2. Kiểm tra FAB và nút "Thêm mới" | N/A | FAB không hiển thị, nút "Thêm mới" bị disable (màu xám) | | | |
| TC-M-GOAL-014 | Khóa nút Cập nhật cho goal completed | 1. Xem goal có status "completed"<br>2. Kiểm tra FAB và nút "Thêm mới" | N/A | FAB không hiển thị, nút "Thêm mới" bị disable | | | |
| TC-M-GOAL-015 | Cho phép Cập nhật goal in_progress | 1. Xem goal có status "in_progress"<br>2. Kiểm tra FAB và nút "Thêm mới" | N/A | FAB hiển thị và active, nút "Thêm mới" màu xanh và có thể nhấn | | | |
| TC-M-GOAL-016 | Filter goals theo tab Đang thực hiện | 1. Trong Goals Screen<br>2. Tab "Đang thực hiện" được chọn | N/A | Chỉ hiển thị goals có status "in_progress" hoặc "not_started" | | | |
| TC-M-GOAL-017 | Filter goals theo tab Đã hoàn thành | 1. Nhấn tab "Đã hoàn thành" | N/A | Chỉ hiển thị goals có status "completed" | | | |

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

### Chức năng 6.4: Xem Lịch Sử Dinh Dưỡng

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-NUT-013 | Mở Nutrition History từ icon | 1. Trong Nutrition Screen<br>2. Nhấn icon History trên AppBar | N/A | Chuyển đến Nutrition History Screen | | | |
| TC-M-NUT-014 | Xem lịch sử 7 ngày | 1. Trong Nutrition History<br>2. Tab "7 ngày" được chọn | N/A | Hiển thị: Thống kê trung bình, Biểu đồ Calories, Biểu đồ Macros, Danh sách chi tiết 7 ngày | | | |
| TC-M-NUT-015 | Xem lịch sử 30 ngày | 1. Nhấn tab "30 ngày" | N/A | Hiển thị dữ liệu 30 ngày, biểu đồ cập nhật | | | |
| TC-M-NUT-016 | Hiển thị thống kê trung bình | 1. Xem Nutrition History | N/A | Hiển thị đúng: Calories TB, Protein TB, Carbs TB, Fat TB dạng cards | | | |
| TC-M-NUT-017 | Hiển thị biểu đồ Calories theo ngày | 1. Scroll xuống phần biểu đồ | N/A | Line Chart hiển thị calories theo từng ngày, có tooltip khi nhấn vào điểm | | | |
| TC-M-NUT-018 | Hiển thị biểu đồ Macros theo ngày | 1. Xem biểu đồ Macros | N/A | Bar Chart hiển thị Protein/Carbs/Fat theo ngày với màu sắc khác nhau | | | |
| TC-M-NUT-019 | Xem chi tiết ngày trong lịch sử | 1. Scroll xuống danh sách "Chi tiết theo ngày"<br>2. Xem thông tin từng ngày | N/A | Hiển thị: Ngày, Số món ăn, Total Calories, Macros dạng list items | | | |
| TC-M-NUT-020 | Lịch sử trống | 1. User mới chưa có dữ liệu<br>2. Vào Nutrition History | N/A | Hiển thị empty state với icon và text "Chưa có dữ liệu dinh dưỡng" | | | |
| TC-M-NUT-021 | Pull to refresh lịch sử | 1. Trong Nutrition History<br>2. Kéo xuống để refresh | N/A | Loading indicator hiện, dữ liệu được tải lại | | | |
| TC-M-NUT-022 | Chuyển đổi giữa tabs | 1. Nhấn tab "7 ngày"<br>2. Nhấn tab "30 ngày" | N/A | Chuyển đổi mượt mà, dữ liệu cập nhật không bị lag | | | |

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

## Module 8: AI Chatbot HealthSync (Mobile)

> **Mô tả:** AI Chatbot trên mobile là trợ lý sức khỏe thông minh, cung cấp tư vấn cá nhân hóa 100% dựa trên dữ liệu sức khỏe thực của người dùng.

### Chức năng 8.1: Giao Diện Chat Screen

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-AI-001 | Truy cập Chat Screen | 1. Nhấn icon Chat trên Bottom Navigation | N/A | Chat Screen hiển thị với: Header "HealthBot 💪", vùng tin nhắn, input TextField, nút gửi | | | **Screen: ChatScreen** |
| TC-M-AI-002 | Hiển thị welcome message | 1. Mở Chat Screen lần đầu (chưa có history) | N/A | Hiển thị welcome message từ AI: "Xin chào! Tôi là HealthBot, trợ lý sức khỏe của bạn" | | | |
| TC-M-AI-003 | Load chat history | 1. Mở Chat Screen khi đã có lịch sử | N/A | Gọi API GET /api/Chat/history, hiển thị loading, sau đó render messages | | | **API: GET /api/Chat/history** |
| TC-M-AI-004 | Hiển thị messages đúng vị trí | 1. Xem chat có nhiều messages | N/A | User messages bên phải (bubble màu xanh/đen), AI messages bên trái (bubble trắng/xám) | | | |
| TC-M-AI-005 | Scroll lịch sử chat | 1. Có nhiều messages<br>2. Scroll lên xem tin cũ | N/A | ListView scroll mượt, tin nhắn cũ hiển thị đúng | | | |
| TC-M-AI-006 | Auto scroll khi có tin mới | 1. Đang ở cuối danh sách<br>2. AI trả lời tin mới | N/A | Tự động scroll xuống tin mới nhất | | | |

### Chức năng 8.2: Gửi Tin Nhắn

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-AI-007 | Gửi tin nhắn cơ bản | 1. Nhập tin nhắn vào TextField<br>2. Nhấn nút gửi | Message: "Xin chào" | User message hiển thị ngay, gọi API POST /api/Chat/ask | | | **API: POST /api/Chat/ask** |
| TC-M-AI-008 | Gửi tin nhắn bằng keyboard | 1. Nhập tin nhắn<br>2. Nhấn Done/Enter trên keyboard | Message: "Xin chào" | Tin nhắn được gửi, keyboard ẩn | | | |
| TC-M-AI-009 | Validate tin nhắn trống | 1. Để trống TextField<br>2. Nhấn gửi | Message: "" | Nút gửi bị disable hoặc không có action | | | |
| TC-M-AI-010 | Nhận phản hồi từ AI | 1. Sau khi gửi tin nhắn<br>2. Chờ response | N/A | AI response xuất hiện bên trái với animation | | | |
| TC-M-AI-011 | Hiển thị loading indicator | 1. Gửi tin nhắn<br>2. Quan sát khi đang chờ AI | N/A | Hiển thị typing indicator (3 dots bounce) hoặc CircularProgressIndicator | | | |
| TC-M-AI-012 | Keyboard không che input | 1. Mở keyboard<br>2. Kiểm tra input visibility | N/A | Input TextField vẫn visible, không bị keyboard che | | | Flutter resizeToAvoidBottomInset |

### Chức năng 8.3: Tư Vấn Dinh Dưỡng (Nutrition Advice)

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-AI-NUT-001 | Hỏi gợi ý bữa sáng | 1. Gửi câu hỏi | "Tôi nên ăn gì cho bữa sáng?" | AI gợi ý bữa sáng phù hợp với mục tiêu user | | | |
| TC-M-AI-NUT-002 | Hỏi gợi ý bữa trưa | 1. Gửi câu hỏi | "Gợi ý bữa trưa cho tôi" | AI gợi ý món ăn cân bằng dinh dưỡng | | | |
| TC-M-AI-NUT-003 | Hỏi gợi ý bữa tối | 1. Gửi câu hỏi | "Bữa tối nên ăn gì?" | AI gợi ý bữa tối nhẹ, phù hợp | | | |
| TC-M-AI-NUT-004 | Hỏi về calories | 1. Gửi câu hỏi | "Tôi cần bao nhiêu calo mỗi ngày?" | AI tính TDEE dựa trên profile và mục tiêu | | | Context: Profile |
| TC-M-AI-NUT-005 | Hỏi về protein | 1. Gửi câu hỏi | "Tôi cần bao nhiêu protein?" | AI tính dựa trên cân nặng và mục tiêu | | | |
| TC-M-AI-NUT-006 | Hỏi thực phẩm giàu protein | 1. Gửi câu hỏi | "Thực phẩm nào giàu protein?" | AI liệt kê thực phẩm với gram protein | | | |
| TC-M-AI-NUT-007 | Hỏi về chế độ ăn kiêng | 1. Gửi câu hỏi | "Chế độ ăn kiêng nào tốt để giảm cân?" | AI gợi ý phù hợp với profile | | | |
| TC-M-AI-NUT-008 | Đánh giá dinh dưỡng hôm nay | 1. Gửi câu hỏi | "Hôm nay tôi ăn như vậy có đủ không?" | AI phân tích từ nutrition logs | | | Context: Nutrition logs |

### Chức năng 8.4: Tư Vấn Luyện Tập (Workout Advice)

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-AI-WORK-001 | Hỏi bài tập cho người mới | 1. Gửi câu hỏi | "Tôi mới bắt đầu tập, nên tập gì?" | AI gợi ý bài tập beginner | | | |
| TC-M-AI-WORK-002 | Hỏi bài tập cho ngực | 1. Gửi câu hỏi | "Bài tập nào tốt cho cơ ngực?" | AI liệt kê bài chest với sets x reps | | | |
| TC-M-AI-WORK-003 | Hỏi bài tập giảm mỡ | 1. Gửi câu hỏi | "Bài tập nào giúp giảm mỡ?" | AI giải thích và gợi ý cardio + strength | | | |
| TC-M-AI-WORK-004 | Hỏi lịch tập trong tuần | 1. Gửi câu hỏi | "Gợi ý lịch tập cho tôi" | AI đưa lịch split phù hợp | | | |
| TC-M-AI-WORK-005 | Hỏi về cardio | 1. Gửi câu hỏi | "Tôi nên chạy bao lâu?" | AI gợi ý thời gian cardio | | | |
| TC-M-AI-WORK-006 | Đánh giá buổi tập | 1. Gửi câu hỏi | "Buổi tập hôm nay của tôi có tốt không?" | AI phân tích từ workout logs | | | Context: Workout logs |

### Chức năng 8.5: Tư Vấn Sức Khỏe Cá Nhân

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-AI-HEALTH-001 | Hỏi về BMI | 1. Gửi câu hỏi | "BMI của tôi là bao nhiêu?" | AI tính BMI từ profile, phân loại | | | Context: Profile |
| TC-M-AI-HEALTH-002 | Hỏi về BMR | 1. Gửi câu hỏi | "BMR của tôi bao nhiêu?" | AI tính BMR và giải thích | | | |
| TC-M-AI-HEALTH-003 | Hỏi về tiến độ mục tiêu | 1. Gửi câu hỏi | "Tôi tiến triển thế nào?" | AI phân tích goal progress | | | Context: Goal |
| TC-M-AI-HEALTH-004 | Hỏi cân nặng lý tưởng | 1. Gửi câu hỏi | "Cân nặng lý tưởng của tôi?" | AI tính từ chiều cao | | | |
| TC-M-AI-HEALTH-005 | Hỏi thời gian đạt mục tiêu | 1. Gửi câu hỏi | "Bao lâu tôi đạt mục tiêu?" | AI ước tính dựa trên tốc độ an toàn | | | |
| TC-M-AI-HEALTH-006 | Tổng kết tuần | 1. Gửi câu hỏi | "Tuần này tôi tập và ăn thế nào?" | AI tổng hợp 7 ngày gần nhất | | | Context: 7-day logs |

### Chức năng 8.6: Edge Cases & Error Handling

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-M-AI-EDGE-001 | Câu hỏi không liên quan | 1. Gửi câu hỏi ngoài lề | "Thời tiết hôm nay?" | AI trả lời lịch sự, gợi ý hỏi về sức khỏe | | | |
| TC-M-AI-EDGE-002 | Tin nhắn chứa emoji | 1. Gửi tin có emoji | "Tôi muốn tập 💪" | AI xử lý đúng | | | |
| TC-M-AI-EDGE-003 | Tin nhắn rất dài | 1. Gửi tin 500+ ký tự | Tin nhắn dài | AI xử lý đúng, không crash | | | |
| TC-M-AI-EDGE-004 | API timeout | 1. Server chậm > 30s | N/A | Hiển thị thông báo timeout | | | |
| TC-M-AI-EDGE-005 | Mất mạng khi gửi | 1. Tắt WiFi<br>2. Gửi tin | N/A | SnackBar hiển thị lỗi network | | | |
| TC-M-AI-EDGE-006 | Token hết hạn | 1. Token expired<br>2. Gửi tin | N/A | Redirect về đăng nhập | | | |

### Chức năng 8.7: Câu Mẫu Thử Nghiệm (Mobile)

> **Mục đích:** Các câu mẫu để QA Team test AI Chatbot trên mobile

#### 8.7.1 Câu Mẫu Dinh Dưỡng

| STT | Câu Hỏi Mẫu | Kỳ Vọng | Ghi Chú |
|-----|-------------|---------|---------|
| 1 | "Hôm nay tôi nên ăn gì?" | Gợi ý bữa ăn phù hợp mục tiêu | |
| 2 | "Tôi đang giảm cân, nên ăn bao nhiêu calo?" | Tính TDEE - deficit | Context: Goal |
| 3 | "Snack healthy là gì?" | Liệt kê snack ít calo | |
| 4 | "Protein shake có cần không?" | Giải thích supplement | |
| 5 | "Tôi ăn chay, lấy protein từ đâu?" | Gợi ý protein thực vật | |

#### 8.7.2 Câu Mẫu Luyện Tập

| STT | Câu Hỏi Mẫu | Kỳ Vọng | Ghi Chú |
|-----|-------------|---------|---------|
| 1 | "Tập gym hay yoga tốt hơn?" | So sánh ưu nhược điểm | |
| 2 | "Bài tập tại nhà không dụng cụ" | Bodyweight exercises | |
| 3 | "Tôi đau cơ sau tập, có sao không?" | Giải thích DOMS | |
| 4 | "Warm up có cần thiết không?" | Giải thích tầm quan trọng | |
| 5 | "Tập bao lâu mới thấy kết quả?" | Realistic expectations | |

#### 8.7.3 Câu Mẫu Sức Khỏe

| STT | Câu Hỏi Mẫu | Kỳ Vọng | Ghi Chú |
|-----|-------------|---------|---------|
| 1 | "Tôi có thừa cân không?" | Phân tích BMI | |
| 2 | "Giảm cân nhanh có tốt không?" | Khuyến cáo tốc độ an toàn | |
| 3 | "Tôi hay mệt mỏi, làm sao?" | Gợi ý kiểm tra dinh dưỡng, ngủ | |
| 4 | "Uống bao nhiêu nước?" | Tính theo cân nặng | |
| 5 | "Ngủ bao nhiêu tiếng đủ?" | 7-9 tiếng/đêm | |

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
| Quản lý Mục tiêu | 17 | | | |
| Theo dõi Dinh dưỡng | 22 | | | |
| Theo dõi Bài tập | 15 | | | |
| **AI Chatbot HealthSync** | **51** | | | **MỚI HOÀN TOÀN** |
| Tạo mật khẩu Google | 4 | | | |
| Đăng xuất | 3 | | | |
| Kiểm thử đặc thù Mobile | 14 | | | |
| Quản lý Hồ sơ cá nhân | 8 | | | |
| **TỔNG** | **180** | | | **+51 test cases AI Chatbot** |

---

# TỔNG KẾT TOÀN BỘ TEST CASE

| Phần | Số lượng Test Case |
|------|-------------------|
| Phần 1: Admin (Web) | 59 |
| Phần 2: User Web | 175 |
| Phần 3: User Mobile | 180 |
| **TỔNG CỘNG** | **414** |

---

*Ghi chú: Tài liệu này có thể được mở rộng thêm các test case cho các tính năng mới hoặc các trường hợp edge case khác.*
