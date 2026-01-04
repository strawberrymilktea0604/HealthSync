# Tài Liệu Test Case - Luồng User (Web) - CẬP NHẬT

## Thông Tin Tài Liệu

| Thông tin | Chi tiết |
|-----------|----------|
| **Dự án** | HealthSync - Ứng dụng theo dõi sức khỏe |
| **Phiên bản** | 1.1 |
| **Ngày cập nhật** | 03/01/2026 |
| **Người cập nhật** | QA Team |
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
| TC-REG-010 | **[MỚI]** Hiển thị trang Register Success | 1. Hoàn tất đăng ký thành công<br>2. Kiểm tra trang RegisterSuccess | N/A | Hiển thị thông báo thành công và nút Continue to Complete Profile | | | **Component: RegisterSuccess.tsx** |

### Chức năng 1.2: Đăng Nhập

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-LOGIN-001 | Đăng nhập với thông tin hợp lệ | 1. Truy cập trang đăng nhập<br>2. Nhập email<br>3. Nhập mật khẩu<br>4. Nhấn "Login" | Email: user@test.com<br>Password: User@123 | Đăng nhập thành công, chuyển đến Dashboard người dùng | | | |
| TC-LOGIN-002 | Đăng nhập với sai mật khẩu | 1. Nhập email đúng<br>2. Nhập mật khẩu sai<br>3. Nhấn "Login" | Email: user@test.com<br>Password: wrongpass | Hiển thị thông báo "Sai email hoặc mật khẩu" | | | |
| TC-LOGIN-003 | Đăng nhập với email không tồn tại | 1. Nhập email không tồn tại<br>2. Nhấn "Login" | Email: notexist@test.com | Hiển thị thông báo "Sai email hoặc mật khẩu" | | | |
| TC-LOGIN-004 | Đăng nhập để trống email | 1. Để trống email<br>2. Nhập mật khẩu<br>3. Nhấn "Login" | Email: (trống) | Form không cho submit hoặc hiển thị lỗi validation | | | |
| TC-LOGIN-005 | Đăng nhập để trống mật khẩu | 1. Nhập email<br>2. Để trống mật khẩu<br>3. Nhấn "Login" | Password: (trống) | Form không cho submit hoặc hiển thị lỗi validation | | | |
| TC-LOGIN-006 | Đăng nhập với tài khoản bị khóa | 1. Nhập email của tài khoản đã bị Admin khóa<br>2. Nhập mật khẩu đúng<br>3. Nhấn "Login" | Email: locked_user@test.com | Hiển thị thông báo "Tài khoản đã bị khóa" | | | |
| TC-LOGIN-007 | Đăng nhập bằng Google | 1. Nhấn nút "Sign in with Google"<br>2. Chọn tài khoản Google<br>3. Cấp quyền | Tài khoản Google hợp lệ | Đăng nhập thành công, nếu lần đầu thì chuyển đến Complete Profile, ngược lại đến Dashboard | | | **Component: GoogleCallback.tsx** |
| TC-LOGIN-008 | Đăng nhập Google với email đã có tài khoản | 1. Nhấn "Sign in with Google"<br>2. Chọn tài khoản Google có email đã đăng ký bằng form | Email đã tồn tại | Liên kết tài khoản hoặc hiển thị thông báo phù hợp | | | |
| TC-LOGIN-009 | Hiển thị/Ẩn mật khẩu | 1. Nhập mật khẩu<br>2. Nhấn icon "eye" để hiện mật khẩu<br>3. Nhấn lại để ẩn | N/A | Mật khẩu chuyển đổi giữa hiển thị text và dots | | | |
| TC-LOGIN-010 | **[MỚI]** Đăng nhập Google yêu cầu set password | 1. Đăng ký Google lần đầu<br>2. Hệ thống redirect đến CreatePasswordForGoogle | N/A | Form tạo password cho tài khoản Google hiển thị | | | **Component: CreatePasswordForGoogle.tsx** |

### Chức năng 1.3: Quên Mật Khẩu

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-FORGOT-001 | Yêu cầu reset mật khẩu với email hợp lệ | 1. Từ trang đăng nhập, nhấn "Forgot password?"<br>2. Nhập email đã đăng ký<br>3. Nhấn "Gửi mã" | Email: user@test.com | Mã xác thực được gửi đến email, chuyển đến form nhập mã | | | **Component: ForgotPassword.tsx** |
| TC-FORGOT-002 | Yêu cầu reset với email không tồn tại | 1. Nhấn "Forgot password?"<br>2. Nhập email không có trong hệ thống<br>3. Nhấn "Gửi mã" | Email: notexist@test.com | Hiển thị lỗi "Email không tồn tại trong hệ thống" | | | |
| TC-FORGOT-003 | **[MỚI]** Xác thực mã OTP reset password | 1. Nhập mã OTP từ email<br>2. Nhấn "Xác nhận" | Code: 123456 | OTP đúng, chuyển đến trang ResetPassword | | | **Component: VerifyPasswordReset.tsx** |
| TC-FORGOT-004 | **[MỚI]** Xác thực OTP sai hoặc hết hạn | 1. Nhập mã OTP sai hoặc đã hết hạn<br>2. Nhấn "Xác nhận" | Code: 000000 | Hiển thị toast error "Mã OTP không hợp lệ hoặc đã hết hạn" | | | **Component: VerifyPasswordReset.tsx** |
| TC-FORGOT-005 | **[MỚI]** Nhập mật khẩu mới | 1. Sau khi OTP hợp lệ<br>2. Nhập mật khẩu mới<br>3. Xác nhận mật khẩu mới<br>4. Nhấn "Đặt lại mật khẩu" | NewPassword: NewUser@123<br>Confirm: NewUser@123 | Mật khẩu được đổi thành công, chuyển đến ResetSuccess page | | | **Component: ResetPassword.tsx** |
| TC-FORGOT-006 | Đổi mật khẩu mới không khớp | 1. Nhập mật khẩu mới<br>2. Nhập xác nhận không khớp | NewPassword: Pass@123<br>Confirm: Different@123 | Hiển thị lỗi "Mật khẩu xác nhận không khớp" | | | |
| TC-FORGOT-007 | **[MỚI]** Hiển thị trang Reset Success | 1. Sau khi reset mật khẩu thành công<br>2. Kiểm tra trang ResetSuccess | N/A | Hiển thị thông báo thành công và nút "Đăng nhập lại" | | | **Component: ResetSuccess.tsx** |
| TC-FORGOT-008 | **[MỚI]** Hiển thị trang Change Password Success | 1. Sau khi thay đổi mật khẩu từ Profile<br>2. Kiểm tra trang ChangePasswordSuccess | N/A | Hiển thị thông báo thành công | | | **Component: ChangePasswordSuccess.tsx** |

---

## Module 2: Hoàn Thiện Hồ Sơ (Complete Profile)

### Chức năng 2.1: Nhập Thông Tin Cá Nhân

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-PROFILE-001 | Hoàn thiện profile với dữ liệu hợp lệ | 1. Sau đăng ký, hệ thống chuyển đến Complete Profile<br>2. Nhập họ tên<br>3. Chọn giới tính<br>4. Nhập ngày sinh<br>5. Nhập chiều cao, cân nặng<br>6. Chọn mức độ hoạt động<br>7. Nhấn "Lưu" | FullName: John Doe<br>Gender: Male<br>DOB: 1990-01-15<br>Height: 175<br>Weight: 70<br>ActivityLevel: Moderate | Profile được lưu thành công, chuyển đến Dashboard | | | **Component: CompleteProfile.tsx** |
| TC-PROFILE-002 | Hoàn thiện profile thiếu trường bắt buộc | 1. Để trống trường "Họ tên"<br>2. Nhấn "Lưu" | FullName: (trống) | Hiển thị lỗi "Vui lòng nhập đầy đủ thông tin" | | | |
| TC-PROFILE-003 | Nhập chiều cao không hợp lệ | 1. Nhập chiều cao âm hoặc quá lớn<br>2. Nhấn "Lưu" | Height: -10 hoặc Height: 500 | Hiển thị lỗi validation "Chiều cao không hợp lệ" | | | |
| TC-PROFILE-004 | Nhập cân nặng không hợp lệ | 1. Nhập cân nặng âm hoặc quá lớn<br>2. Nhấn "Lưu" | Weight: -5 hoặc Weight: 1000 | Hiển thị lỗi validation "Cân nặng không hợp lệ" | | | |
| TC-PROFILE-005 | Nhập ngày sinh trong tương lai | 1. Chọn ngày sinh trong tương lai<br>2. Nhấn "Lưu" | DOB: 2030-01-01 | Hiển thị lỗi "Ngày sinh không hợp lệ" | | | |
| TC-PROFILE-006 | Upload avatar (nếu có) | 1. Nhấn vào avatar<br>2. Chọn file ảnh<br>3. Xác nhận | File: avatar.jpg (< 5MB) | Avatar được upload và hiển thị | | | |

### Chức năng 2.2: Cập Nhật Hồ Sơ (Profile Settings)

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-PROFILE-007 | Cập nhật thông tin cá nhân | 1. Vào trang Profile/Settings<br>2. Thay đổi cân nặng, chiều cao, activity level<br>3. Nhấn "Save Changes" | Weight: 72kg<br>ActivityLevel: Active | Thông tin được cập nhật thành công, hiển thị thông báo thành công | | | **Component: Profile.tsx** |
| TC-PROFILE-008 | Cập nhật avatar từ trang settings | 1. Vào trang Profile<br>2. Nhấn vào ảnh đại diện<br>3. Upload ảnh mới | File: new_avatar.png | Avatar mới được cập nhật ngay lập tức và sync với Header | | | **API: /userprofile/upload-avatar** |
| TC-PROFILE-009 | Hủy thay đổi | 1. Vào trang Profile<br>2. Thay đổi thông tin nhưng không lưu<br>3. Nhấn "Back to Dashboard" | N/A | Thông tin không bị thay đổi trong cơ sở dữ liệu | | | |
| TC-PROFILE-010 | Validate dữ liệu khi cập nhật | 1. Nhập cân nặng âm<br>2. Nhấn "Lưu" | Weight: -5 | Hiển thị lỗi validation, không cho phép lưu | | | |
| TC-PROFILE-011 | **[MỚI]** Kiểm tra fetch profile data | 1. Vào trang Profile<br>2. Kiểm tra data được load | N/A | Tất cả thông tin user được fetch từ API /userprofile và hiển thị đúng | | | **API: GET /userprofile** |
| TC-PROFILE-012 | **[MỚI]** Cập nhật profile không gửi avatarUrl | 1. Cập nhật thông tin (không thay đổi avatar)<br>2. Kiểm tra request payload | N/A | Request PUT /userprofile không chứa field avatarUrl (avatar upload riêng) | | | **API: PUT /userprofile** |

---

## Module 3: Dashboard Người Dùng

### Chức năng 3.1: Hiển Thị Dashboard

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-DASH-001 | Hiển thị Dashboard sau đăng nhập | 1. Đăng nhập thành công<br>2. Hệ thống chuyển đến Dashboard | N/A | Dashboard hiển thị với: Welcome to HealthSync logo, Tiến độ mục tiêu, Biểu đồ cân nặng, Nút Ghi bữa ăn/Ghi buổi tập | | | **Component: Dashboard.tsx** |
| TC-DASH-002 | **[MỚI]** Hiển thị thống kê tiến độ mục tiêu | 1. Xem Dashboard<br>2. Kiểm tra section Goals Progress | N/A | Hiển thị: Mục tiêu chính (giảm Xkg), Tiến độ hiện tại (đã giảm Xkg), Biểu đồ cân nặng | | | |
| TC-DASH-003 | **[MỚI]** Hiển thị biểu đồ cân nặng | 1. Xem Dashboard<br>2. Kiểm tra Weight Chart | N/A | Biểu đồ bar chart hiển thị 7 điểm dữ liệu gần nhất, có tooltip khi hover | | | |
| TC-DASH-004 | **[MỚI]** Hiển thị thống kê workout | 1. Xem Dashboard<br>2. Kiểm tra Workout card | N/A | Hiển thị số phút tập/tuần với badge tròn màu xanh | | | |
| TC-DASH-005 | Responsive Dashboard trên tablet | 1. Mở Dashboard trên màn hình tablet<br>2. Kiểm tra layout | Viewport: 768px | Layout responsive, không bị vỡ giao diện | | | |
| TC-DASH-006 | Responsive Dashboard trên mobile | 1. Mở Dashboard trên màn hình mobile<br>2. Kiểm tra layout | Viewport: 375px | Layout responsive, các card xếp dọc | | | |
| TC-DASH-007 | **[MỚI]** Nút Ghi bữa ăn navigate | 1. Nhấn nút "Ghi bữa ăn"<br>2. Kiểm tra navigation | N/A | Chuyển đến trang /nutrition | | | |
| TC-DASH-008 | **[MỚI]** Nút Ghi buổi tập navigate | 1. Nhấn nút "Ghi buổi tập"<br>2. Kiểm tra navigation | N/A | Chuyển đến trang /create-workout | | | |
| TC-DASH-009 | **[MỚI]** Chat Bot FAB button | 1. Kiểm tra Dashboard<br>2. Nhấn nút chat bot ở góc phải dưới | N/A | Modal chat hiển thị với giao diện chat đầy đủ | | | **Component: Dashboard.tsx - Chat Modal** |
| TC-DASH-010 | **[MỚI]** Fetch dashboard data từ API | 1. Load Dashboard<br>2. Kiểm tra API call | N/A | Gọi API GET /dashboard/customer và hiển thị: userInfo, goalProgress, weightProgress, todayStats | | | **API: GET /dashboard/customer** |
| TC-DASH-011 | **[MỚI]** Header avatar hiển thị | 1. Load Dashboard<br>2. Kiểm tra avatar trong Header | N/A | Avatar user hiển thị tròn hoàn hảo, fetch từ user.avatar hoặc UI Avatars fallback | | | **Component: Header.tsx** |

### Chức năng 3.2: AI Chatbot Assistant

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-CHAT-001 | **[MỚI]** Mở modal chatbot | 1. Ở Dashboard<br>2. Nhấn FAB button chatbot góc phải dưới | N/A | Modal chat hiển thị với animation smooth, header "Assistant" với icon Bot, nút đóng X | | | **Port: 8080** |
| TC-CHAT-002 | **[MỚI]** Load chat history khi mở modal | 1. Mở modal chatbot | N/A | Gọi API GET /api/Chat/history (port 8080), hiển thị loading spinner khi đang fetch | | | **API: GET /api/Chat/history?pageSize=20&pageNumber=1** |
| TC-CHAT-003 | **[MỚI]** Hiển thị chat history | 1. Mở modal chatbot<br>2. Kiểm tra messages hiển thị | N/A | Messages hiển thị theo đúng role (user/assistant), user bên phải với bg đen, assistant bên trái với bg trắng, có avatar và timestamp | | | |
| TC-CHAT-004 | **[MỚI]** Empty state khi chưa có tin nhắn | 1. Mở modal chatbot lần đầu (chưa có lịch sử)<br>2. Kiểm tra empty state | N/A | Hiển thị Bot icon mờ và text "Bắt đầu cuộc trò chuyện" ở giữa | | | |
| TC-CHAT-005 | **[MỚI]** Gửi tin nhắn cho AI | 1. Mở modal chatbot<br>2. Nhập câu hỏi: "Tôi muốn giảm cân"<br>3. Nhấn nút Send hoặc Enter | Question: "Tôi muốn giảm cân" | User message hiển thị ngay lập tức, gọi API POST /api/Chat/ask (port 8080), hiển thị loading spinner trên nút send | | | **API: POST /api/Chat/ask** |
| TC-CHAT-006 | **[MỚI]** Nhận phản hồi từ AI | 1. Sau khi gửi tin nhắn<br>2. Chờ response | N/A | AI response hiển thị dưới user message với avatar Bot, content từ API, timestamp đúng định dạng HH:mm | | | |
| TC-CHAT-007 | **[MỚI]** Auto scroll to bottom | 1. Gửi tin nhắn mới<br>2. Kiểm tra scroll behavior | N/A | Chat tự động scroll xuống message mới nhất với smooth behavior | | | |
| TC-CHAT-008 | **[MỚI]** Disable input khi đang gửi | 1. Gửi tin nhắn<br>2. Kiểm tra trạng thái input và button | N/A | Input và button bị disable khi isSending=true, button hiển thị loading spinner thay vì Send icon | | | |
| TC-CHAT-009 | **[MỚI]** Validate empty message | 1. Không nhập gì<br>2. Nhấn Send | Input: "" (empty) | Button Send bị disable khi input trống hoặc chỉ có whitespace | | | |
| TC-CHAT-010 | **[MỚI]** Error handling khi API fail | 1. Gửi tin nhắn khi backend không available<br>2. Kiểm tra error handling | N/A | Hiển thị error message từ AI: "Xin lỗi, tôi không thể trả lời câu hỏi của bạn lúc này. Vui lòng thử lại sau." | | | |
| TC-CHAT-011 | **[MỚI]** Đóng modal chatbot | 1. Mở modal<br>2. Nhấn nút X hoặc click FAB button lại | N/A | Modal đóng với animation smooth, chat history vẫn được giữ | | | |
| TC-CHAT-012 | **[MỚI]** Responsive chatbot modal | 1. Mở modal trên màn hình khác nhau | Viewport: 1920px, 1366px, 768px | Modal size: 384px width, 32rem height, responsive với màn hình nhỏ | | | |
| TC-CHAT-013 | **[MỚI]** Bearer token authentication | 1. Gửi tin nhắn<br>2. Kiểm tra request header | N/A | Request chứa Authorization header với Bearer token từ localStorage | | | **chatService.ts interceptor** |
| TC-CHAT-014 | **[MỚI]** Format timestamp | 1. Xem tin nhắn trong chat<br>2. Kiểm tra timestamp | CreatedAt: "2026-01-05T10:30:00Z" | Hiển thị format "HH:mm" theo locale vi-VN, ví dụ: "10:30" | | | |
| TC-CHAT-015 | **[MỚI]** Message content với line breaks | 1. AI trả lời với multi-line content<br>2. Kiểm tra hiển thị | Content có \n | Text hiển thị đúng line breaks với whitespace-pre-wrap | | | |
| TC-CHAT-016 | **[MỚI]** Chatbot API base URL | 1. Kiểm tra chatService config | N/A | API_BASE_URL = http://localhost:8080, endpoint = /api/Chat | | | **chatService.ts** |

---

## Module 4: Quản Lý Mục Tiêu (Goals)

### Chức năng 4.1: Xem Danh Sách Mục Tiêu

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-GOAL-001 | Xem danh sách mục tiêu | 1. Đăng nhập<br>2. Vào trang "Goals" hoặc "Mục tiêu" | N/A | Hiển thị danh sách các mục tiêu của người dùng với: Loại mục tiêu, Giá trị mục tiêu, Tiến độ, Ngày tạo/deadline | | | **Component: GoalsPage.tsx** |
| TC-GOAL-002 | Hiển thị trạng thái mục tiêu đang hoạt động | 1. Xem danh sách mục tiêu<br>2. Kiểm tra mục tiêu có trạng thái Active | N/A | Mục tiêu Active có badge "Đang thực hiện" màu xanh | | | |
| TC-GOAL-003 | Hiển thị trạng thái mục tiêu đã hoàn thành | 1. Xem danh sách mục tiêu<br>2. Kiểm tra mục tiêu đã hoàn thành | N/A | Mục tiêu Completed có badge "Hoàn thành" | | | |

### Chức năng 4.2: Tạo Mục Tiêu Mới

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-GOAL-004 | Tạo mục tiêu giảm cân | 1. Nhấn "Tạo mục tiêu mới"<br>2. Chọn loại: weight_loss<br>3. Nhập cân nặng mục tiêu<br>4. Chọn ngày bắt đầu/kết thúc<br>5. Nhấn "Lưu mục tiêu" | GoalType: weight_loss<br>TargetValue: 65kg<br>EndDate: 2026-06-01 | Mục tiêu được tạo thành công, hiển thị trong danh sách | | | **Component: CreateGoalPage.tsx** |
| TC-GOAL-005 | Tạo mục tiêu tăng cân | 1. Nhấn "Tạo mục tiêu mới"<br>2. Chọn loại: weight_gain<br>3. Nhập cân nặng mục tiêu<br>4. Nhấn "Lưu" | GoalType: weight_gain<br>TargetValue: 75kg | Mục tiêu được tạo thành công | | | |
| TC-GOAL-006 | **[MỚI]** Tạo mục tiêu tăng cơ | 1. Tạo mục tiêu<br>2. Chọn loại: muscle_gain | GoalType: muscle_gain | Mục tiêu muscle_gain được tạo | | | |
| TC-GOAL-007 | **[MỚI]** Tạo mục tiêu giảm mỡ | 1. Tạo mục tiêu<br>2. Chọn loại: fat_loss | GoalType: fat_loss | Mục tiêu fat_loss được tạo | | | |
| TC-GOAL-008 | Tạo mục tiêu với giá trị không hợp lệ | 1. Nhấn "Tạo mục tiêu"<br>2. Nhập giá trị <= 0<br>3. Nhấn "Lưu" | TargetValue: 0 hoặc -10 | Hiển thị toast error "Vui lòng điền đầy đủ thông tin" | | | |
| TC-GOAL-009 | Tạo mục tiêu thiếu thông tin | 1. Không chọn loại mục tiêu<br>2. Nhấn "Lưu" | GoalType: (trống) | Hiển thị toast error | | | |
| TC-GOAL-010 | **[MỚI]** Subtitle với logo HealthSync | 1. Mở trang Create Goal<br>2. Kiểm tra subtitle | N/A | Subtitle hiển thị: "Hãy đặt ra 1 mục tiêu và cùng [HealthSync logo] hoàn thiện nhé" | | | |

### Chức năng 4.3: Xem Chi Tiết & Theo Dõi Tiến Độ

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-GOAL-011 | Xem chi tiết mục tiêu | 1. Trong danh sách mục tiêu<br>2. Nhấn "Xem chi tiết" | N/A | Trang chi tiết hiển thị: Biểu đồ tiến độ, Lịch sử cập nhật, Thông tin mục tiêu | | | **Component: GoalDetailsPage.tsx** |
| TC-GOAL-012 | Thêm progress mới | 1. Trong trang chi tiết mục tiêu<br>2. Nhấn "Thêm tiến độ"<br>3. Chọn ngày<br>4. Nhập giá trị<br>5. Nhấn "Lưu tiến độ" | Date: Today<br>CurrentValue: 68kg | Progress được thêm, biểu đồ cập nhật | | | **Component: AddProgressPage.tsx** |
| TC-GOAL-013 | Hiển thị biểu đồ tiến độ | 1. Xem chi tiết mục tiêu có nhiều progress<br>2. Kiểm tra biểu đồ Area chart | N/A | Biểu đồ hiển thị đúng xu hướng tiến độ theo thời gian | | | |
| TC-GOAL-014 | **[MỚI]** Navigate từ Goal Details | 1. Nhấn nút "Back"<br>2. Kiểm tra navigation | N/A | Quay lại trang /goals | | | |

---

## Module 5: Theo Dõi Dinh Dưỡng (Nutrition)

### Chức năng 5.1: Xem Tổng Quan Dinh Dưỡng

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-NUT-001 | **[MỚI]** Xem tổng quan dinh dưỡng | 1. Vào trang "Nutrition"<br>2. Xem overview | N/A | Hiển thị: Circular progress calories, macros (Protein/Carbs/Fat) với % và số liệu | | | **Component: NutritionPage.tsx** |
| TC-NUT-002 | **[MỚI]** Hiển thị target values | 1. Xem Nutrition Overview<br>2. Kiểm tra target | N/A | Target calories và macros được fetch động từ user profile/goals | | | |
| TC-NUT-003 | **[MỚI]** Navigate đến Food Search | 1. Nhấn nút "Tìm kiếm món ăn"<br>2. Kiểm tra navigation | N/A | Chuyển đến /nutrition/food-search | | | |
| TC-NUT-004 | **[MỚI]** Navigate đến Food List | 1. Nhấn nút "Danh sách món ăn"<br>2. Kiểm tra navigation | N/A | Chuyển đến /nutrition/food-list | | | |

### Chức năng 5.2: Tìm Kiếm Món Ăn

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-NUT-005 | **[MỚI]** Tìm kiếm món ăn theo tên | 1. Vào /nutrition/food-search<br>2. Nhập từ khóa tìm kiếm | Search: "Chicken" | Danh sách món ăn chứa "Chicken" hiển thị | | | **Component: FoodSearch.tsx** |
| TC-NUT-006 | **[MỚI]** Filter theo loại món | 1. Chọn filter "Loại món"<br>2. Chọn giá trị | Type: "main" | Chỉ hiển thị món chính | | | |
| TC-NUT-007 | **[MỚI]** Filter theo calories | 1. Chọn filter "Calories"<br>2. Chọn mức | Calories: "low" (< 200) | Chỉ hiển thị món có calories thấp | | | |
| TC-NUT-008 | **[MỚI]** Filter theo protein | 1. Chọn filter "Protein"<br>2. Chọn "Giàu Protein" | Protein: "high" (> 20g) | Chỉ hiển thị món giàu protein | | | |
| TC-NUT-009 | **[MỚI]** Filter theo carbs | 1. Chọn filter "Carbs"<br>2. Chọn mức | Carbs: "low" | Chỉ hiển thị món low carb | | | |
| TC-NUT-010 | Tìm kiếm không có kết quả | 1. Nhập từ khóa không tồn tại | Search: "xyz123abc" | Hiển thị "Không tìm thấy món ăn" | | | |

### Chức năng 5.3: Thêm/Quản Lý Món Ăn

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-NUT-011 | **[MỚI]** Thêm món ăn vào nhật ký | 1. Trong FoodSearch hoặc FoodList<br>2. Nhấn "Thêm" bên cạnh món<br>3. Chọn bữa ăn (Breakfast/Lunch/Dinner/Snack)<br>4. Nhập số lượng serving<br>5. Confirm | MealType: Breakfast<br>Quantity: 1 | Món được thêm vào nutrition diary, calories cập nhật | | | **Component: NutritionPage.tsx** |
| TC-NUT-012 | **[MỚI]** Xem danh sách tất cả món ăn | 1. Navigate đến /nutrition/food-list<br>2. Kiểm tra danh sách | N/A | Hiển thị tất cả món ăn trong database với thông tin calories, macros | | | **Component: FoodList.tsx** |
| TC-NUT-013 | Xóa món ăn khỏi nhật ký | 1. Trong nhật ký dinh dưỡng<br>2. Nhấn nút "Xóa" bên cạnh một món<br>3. Xác nhận | FoodEntry để xóa | Món ăn bị xóa, tổng calories được cập nhật | | | |

---

## Module 6: Theo Dõi Bài Tập (Workout)

### Chức năng 6.1: Xem Lịch Sử Bài Tập

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-WORK-001 | Xem lịch sử bài tập | 1. Vào trang "Workout History" | N/A | Hiển thị danh sách các buổi tập đã ghi nhận, nhóm theo ngày | | | **Component: WorkoutHistoryPage.tsx** |
| TC-WORK-002 | Xem chi tiết buổi tập | 1. Nhấn vào một buổi tập trong lịch sử | N/A | Hiển thị chi tiết: Các bài tập, Sets x Reps, Trọng lượng | | | |
| TC-WORK-003 | Hiển thị trống khi chưa có buổi tập | 1. User mới chưa có workout<br>2. Vào Workout History | N/A | Hiển thị empty state với nút "Tạo buổi tập đầu tiên" | | | |

### Chức năng 6.2: Tạo Buổi Tập Mới

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-WORK-004 | Tạo buổi tập mới | 1. Nhấn "Thêm buổi tập"<br>2. Chọn ngày tập<br>3. Nhập thời gian<br>4. Thêm bài tập<br>5. Nhập sets, reps, weight<br>6. Nhấn "Hoàn tất buổi tập" | WorkoutDate: Today<br>Duration: 45 mins<br>Exercises: Push Up (3x15) | Buổi tập được tạo, chuyển đến /workout-history | | | **Component: CreateWorkoutPage.tsx** |
| TC-WORK-005 | **[MỚI]** Tìm kiếm bài tập trong Exercise Library | 1. Trong form Create Workout<br>2. Nhập từ khóa vào search | Search: "Push" | Danh sách bài tập lọc theo từ khóa | | | |
| TC-WORK-006 | **[MỚI]** Filter bài tập theo nhóm cơ | 1. Chọn filter "Nhóm cơ"<br>2. Chọn "Chest" | MuscleGroup: "Chest" | Chỉ hiển thị bài tập cho ngực | | | |
| TC-WORK-007 | **[MỚI]** Filter bài tập theo độ khó | 1. Chọn filter "Độ khó"<br>2. Chọn "Beginner" | Difficulty: "Beginner" | Chỉ hiển thị bài tập Beginner | | | |
| TC-WORK-008 | **[MỚI]** Fix SelectItem empty value error | 1. Chọn filter "Tất cả"<br>2. Kiểm tra không có error | Filter: "all" | Không có error "SelectItem value cannot be empty string" | | | **Fix: value="all" thay vì value=""** |
| TC-WORK-009 | Thêm bài tập vào buổi tập | 1. Tìm bài tập<br>2. Nhấn nút "+" | Exercise: Push Up | Bài tập xuất hiện trong "Các bài tập đã chọn" | | | |
| TC-WORK-010 | Xóa bài tập khỏi buổi tập | 1. Nhấn icon trash bên cạnh bài tập | N/A | Bài tập bị xóa khỏi danh sách | | | |
| TC-WORK-011 | Lưu buổi tập không có bài tập | 1. Không thêm bài tập<br>2. Nhấn "Hoàn tất" | Exercises: (trống) | Toast error "Vui lòng thêm ít nhất một bài tập" | | | |
| TC-WORK-012 | Nhập sets/reps hợp lệ | 1. Thêm bài tập<br>2. Nhập sets, reps, weight | Sets: 3<br>Reps: 10<br>Weight: 20kg | Dữ liệu được lưu đúng | | | |

### Chức năng 6.3: Thư Viện Bài Tập

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-WORK-013 | **[MỚI]** Xem Exercise Library | 1. Navigate đến /exercise-library<br>2. Xem danh sách | N/A | Hiển thị Featured Exercises và danh sách tất cả exercises | | | **Component: ExerciseLibraryPage.tsx** |
| TC-WORK-014 | **[MỚI]** Search trong Exercise Library | 1. Nhập từ khóa tìm kiếm | Search: "squat" | Lọc exercises theo từ khóa | | | |
| TC-WORK-015 | **[MỚI]** Filter theo muscle group | 1. Chọn filter muscle group | MuscleGroup: "Legs" | Chỉ hiển thị bài tập cho chân | | | |
| TC-WORK-016 | **[MỚI]** Xem chi tiết exercise | 1. Nhấn vào một exercise card<br>2. Xem thông tin | N/A | Hiển thị: Name, Description, Difficulty, MuscleGroup, Video/Image | | | |

---

## Module 7: AI Chat (HealthBot)

### Chức năng 7.1: Trò Chuyện Với AI

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-CHAT-001 | Mở trang Chat | 1. Đăng nhập<br>2. Navigate đến /chat hoặc nhấn chat FAB | N/A | Giao diện chat hiển thị với header "HealthBot 💪", welcome banner, input | | | **Component: ChatScreen.tsx** |
| TC-CHAT-002 | **[MỚI]** Hiển thị welcome banner với logo | 1. Load trang chat<br>2. Kiểm tra welcome banner | N/A | Banner hiển thị: "🤖 Xin chào! Tôi là [HealthSync logo] Bot" | | | |
| TC-CHAT-003 | Gửi tin nhắn đơn giản | 1. Nhập tin nhắn<br>2. Nhấn Send hoặc Enter | Message: "Xin chào" | User message bên phải, AI response bên trái | | | |
| TC-CHAT-004 | Hỏi về dinh dưỡng | 1. Gửi câu hỏi về nutrition | Message: "Tôi nên ăn gì để giảm cân?" | AI trả lời với thông tin dinh dưỡng | | | |
| TC-CHAT-005 | Hỏi về bài tập | 1. Gửi câu hỏi về workout | Message: "Bài tập nào tốt cho cơ ngực?" | AI trả lời với gợi ý bài tập | | | |
| TC-CHAT-006 | Hỏi về sức khỏe cá nhân | 1. Gửi câu hỏi về BMI | Message: "BMI của tôi có bình thường không?" | AI phân tích dựa trên user data | | | |
| TC-CHAT-007 | Gửi tin nhắn trống | 1. Để trống input<br>2. Nhấn Send | Message: (trống) | Button Send bị disabled | | | |
| TC-CHAT-008 | Xem lịch sử chat | 1. Reload trang<br>2. Vào chat lại | N/A | Lịch sử tin nhắn được load từ API /chat/history | | | **API: GET /chat/history** |
| TC-CHAT-009 | Refresh chat history | 1. Nhấn nút Refresh trong header | N/A | Gọi lại API và reload messages | | | |
| TC-CHAT-010 | Hiển thị loading khi AI trả lời | 1. Gửi tin nhắn<br>2. Quan sát | N/A | Hiển thị 3 dots bounce animation "đang nhập..." | | | |
| TC-CHAT-011 | **[MỚI]** Error handling khi send fail | 1. Send message khi offline<br>2. Kiểm tra error | N/A | Alert hiển thị error message | | | |

---

## Module 8: Navigation & UI Components

### Chức năng 8.1: Header Navigation

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-NAV-001 | **[MỚI]** Welcome to HealthSync chung 1 hàng | 1. Load bất kỳ trang nào<br>2. Kiểm tra Header | N/A | Text "Welcome to" và logo HealthSync nằm chung 1 hàng (whitespace-nowrap) | | | **Component: Header.tsx** |
| TC-NAV-002 | **[MỚI]** Thanh search kéo dài | 1. Kiểm tra search bar<br>2. Đo width | N/A | Search bar có max-width: 600px (tăng từ 355px) | | | |
| TC-NAV-003 | **[MỚI]** Avatar trong header | 1. Kiểm tra avatar user<br>2. Hover và click | N/A | Avatar tròn hoàn hảo, hiển thị menu khi click: Profile, Logout | | | |
| TC-NAV-004 | **[MỚI]** Dropdown menu avatar | 1. Click vào avatar/tên user<br>2. Click "Profile" | N/A | Navigate đến /profile | | | |
| TC-NAV-005 | **[MỚI]** Logout từ dropdown | 1. Click avatar<br>2. Click "Logout" | N/A | User logout, navigate về trang login, session cleared | | | |

### Chức năng 8.2: Footer

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-FOOTER-001 | **[MỚI]** Hiển thị footer | 1. Scroll xuống bottom<br>2. Kiểm tra footer | N/A | Footer hiển thị logo HealthSync và copyright text | | | **Component: Footer.tsx** |

### Chức năng 8.3: Trang Không Tồn Tại

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-404-001 | **[MỚI]** Truy cập URL không tồn tại | 1. Navigate đến URL random | URL: /abc123xyz | Hiển thị trang 404 Not Found | | | **Component: NotFound.tsx** |

---

## Module 9: Đăng Xuất & Bảo Mật

### Chức năng 9.1: Đăng Xuất

| Test Case ID | Mô tả | Bước kiểm thử | Dữ liệu đầu vào | Kết quả mong đợi | Kết quả thực tế | Trạng thái | Ghi chú |
|--------------|-------|---------------|-----------------|------------------|-----------------|------------|---------|
| TC-LOGOUT-001 | Đăng xuất thành công | 1. Click avatar<br>2. Click "Logout" | N/A | Chuyển về trang /, session/token xóa khỏi localStorage | | | |
| TC-LOGOUT-002 | Truy cập trang protected sau logout | 1. Logout<br>2. Nhập URL /dashboard | URL: /dashboard | AuthContext redirect về /login | | | |
| TC-LOGOUT-003 | Session hết hạn | 1. Token expires<br>2. Thực hiện action | N/A | API trả về 401, redirect về /login | | | |

---

# TỔNG KẾT TEST CASE USER WEB (CẬP NHẬT)

| Module | Số lượng Test Case | Pass | Fail | Pending | Ghi chú |
|--------|-------------------|------|------|---------|---------|
| Xác thực (Đăng ký, Đăng nhập, Quên MK) | 28 | | | | +5 test cases |
| Hoàn thiện hồ sơ & Cập nhật | 12 | | | | +2 test cases |
| Dashboard | 11 | | | | +5 test cases |
| Quản lý Mục tiêu | 14 | | | | +3 test cases |
| Theo dõi Dinh dưỡng | 13 | | | | +4 test cases (Food Search/List) |
| Theo dõi Bài tập | 16 | | | | +5 test cases (Exercise Library) |
| AI Chat | 11 | | | | +2 test cases |
| Navigation & UI | 6 | | | | **MỚI** |
| Đăng xuất & Bảo mật | 3 | | | | |
| **TỔNG** | **114** | | | | **+32 test cases mới** |

---

## CÁC CHỨC NĂNG ĐÃ BỔ SUNG

### ✅ Chức năng mới được thêm vào testcase:

1. **Authentication Pages:**
   - RegisterSuccess.tsx
   - GoogleCallback.tsx
   - CreatePasswordForGoogle.tsx
   - VerifyPasswordReset.tsx (OTP validation)
   - ResetPassword.tsx
   - ResetSuccess.tsx
   - ChangePasswordSuccess.tsx

2. **Profile Management:**
   - Avatar upload API riêng biệt (/userprofile/upload-avatar)
   - Profile fetch từ API (/userprofile)
   - Update profile không gửi avatarUrl

3. **Dashboard:**
   - Weight progress chart với tooltip
   - Goal progress cards
   - Chat bot FAB button
   - Header avatar circular và fetch đúng

4. **Goals:**
   - muscle_gain và fat_loss goal types
   - CreateGoalPage với subtitle logo
   - AddProgressPage component

5. **Nutrition:**
   - NutritionPage với overview circular progress
   - FoodSearch với filters (Type, Calories, Protein, Carbs)
   - FoodList component
   - Dynamic target values fetch

6. **Workout:**
   - CreateWorkoutPage với dual columns
   - Fix SelectItem empty value error (value="all")
   - ExerciseLibraryPage
   - Search và filter trong exercise library

7. **Chat:**
   - Welcome banner với logo HealthSync
   - Chat history API
   - Error handling

8. **UI Components:**
   - Header: "Welcome to HealthSync" chung 1 hàng
   - Search bar kéo dài (max-width: 600px)
   - Avatar dropdown menu
   - Footer component
   - NotFound page

---

*Ghi chú: Testcase được cập nhật dựa trên source code thực tế từ src/pages/*
