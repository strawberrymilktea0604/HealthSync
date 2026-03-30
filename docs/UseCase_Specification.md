# ĐẶC TẢ USE CASE - HỆ THỐNG HEALTHSYNC

---

## PHẦN A: CHỨC NĂNG DÀNH CHO KHÁCH (Guest)

---

### UC-G01: Xem trang chủ & Giới thiệu

| Thuộc tính | Mô tả |
|------------|-------|
| **Tên Use Case** | Xem trang chủ & Giới thiệu |
| **Mã UC** | UC-G01 |
| **Tác nhân (Actor)** | Khách (Guest) |
| **Mô tả ngắn gọn** | Cho phép người dùng chưa đăng nhập xem thông tin giới thiệu về hệ thống HealthSync, các tính năng chính và hướng dẫn sử dụng. |
| **Điều kiện tiên quyết** | - Người dùng truy cập vào URL của hệ thống (Web hoặc Mobile App). |
| **Điều kiện sau** | - Người dùng có thể xem được nội dung trang chủ và quyết định đăng ký/đăng nhập. |

**Luồng sự kiện chính (Main Flow):**
1. Người dùng truy cập URL trang chủ HealthSync.
2. Hệ thống hiển thị trang Landing Page với các thông tin:
   - Banner giới thiệu hệ thống
   - Các tính năng chính (Quản lý tập luyện, Theo dõi dinh dưỡng, Tư vấn AI, Thiết lập mục tiêu)
   - Nút "Đăng ký" và "Đăng nhập"
3. Người dùng có thể cuộn xem chi tiết các tính năng.
4. Người dùng chọn "Đăng ký" hoặc "Đăng nhập" để tiếp tục.

**Luồng rẽ nhánh / Ngoại lệ:**
- 2a: Nếu người dùng đã đăng nhập, hệ thống tự động chuyển đến Dashboard.

---

## PHẦN B: CHỨC NĂNG ĐĂNG KÝ / ĐĂNG NHẬP

---

### UC-U01: Đăng ký, Đăng nhập

| Thuộc tính | Mô tả |
|------------|-------|
| **Tên Use Case** | Đăng ký, Đăng nhập |
| **Mã UC** | UC-U01 |
| **Tác nhân (Actor)** | Hội viên (User), Khách (Guest) |
| **Mô tả ngắn gọn** | Cho phép người dùng mới tạo tài khoản hoặc đăng nhập để truy cập hệ thống. |
| **Điều kiện tiên quyết** | - Người dùng chưa đăng nhập vào hệ thống.<br>- Đối với "Đăng nhập", người dùng phải có tài khoản đã đăng ký. |
| **Điều kiện sau** | - Người dùng được xác thực thành công và được cấp một phiên làm việc (JWT token).<br>- Hệ thống chuyển hướng người dùng đến trang hoàn thiện hồ sơ (nếu lần đầu) hoặc Dashboard. |

**Luồng sự kiện chính (Main Flow):**

**Luồng Đăng nhập:**
1. Người dùng chọn "Đăng nhập".
2. Hệ thống hiển thị form yêu cầu email và mật khẩu.
3. Người dùng nhập thông tin và nhấn "Đăng nhập".
4. Hệ thống xác thực thông tin qua `AuthController.Login`:
   - Kiểm tra email tồn tại trong bảng `ApplicationUser`
   - So sánh mật khẩu đã hash (SHA256)
   - Kiểm tra tài khoản không bị khóa (`IsActive = true`)
5. Hệ thống tạo JWT token chứa claims: `userId`, `email`, `roles`, `permissions`.
6. Hệ thống trả về `AuthResponse` gồm: token, expiresAt, userId, role, isProfileComplete.
7. Client lưu token vào `localStorage` (Web) hoặc `SharedPreferences` (Mobile).
8. Nếu `isProfileComplete = false`, chuyển đến màn hình "Hoàn thiện hồ sơ".
9. Nếu `isProfileComplete = true`, chuyển đến Dashboard.

**Luồng Đăng ký:**
1. Người dùng chọn "Đăng ký".
2. Hệ thống hiển thị form nhập email.
3. Người dùng nhập email và nhấn "Tiếp tục".
4. Hệ thống gọi `AuthController.SendVerificationEmail`:
   - Kiểm tra email chưa tồn tại
   - Tạo mã OTP 6 ký tự
   - Gửi email chứa mã OTP (qua EmailService)
5. Hệ thống hiển thị form nhập mã OTP và mật khẩu.
6. Người dùng nhập mã OTP, mật khẩu (tối thiểu 8 ký tự) và xác nhận mật khẩu.
7. Hệ thống gọi `AuthController.Register`:
   - Xác thực mã OTP (qua `VerifyEmailCommand`)
   - Hash mật khẩu bằng SHA256
   - Tạo `ApplicationUser` với role "Customer"
   - Tạo `UserProfile` với thông tin mặc định (FullName="", Gender="Unknown", HeightCm=0, WeightKg=0)
8. Đăng ký thành công, hệ thống tự động đăng nhập và cấp JWT token.
9. Chuyển đến màn hình "Hoàn thiện hồ sơ cá nhân".

**Luồng Đăng nhập qua Google (OAuth):**
1. Người dùng chọn "Đăng nhập với Google".
2. **Web:** Hệ thống redirect đến `/api/Auth/google-login`, sau đó đến trang đăng nhập Google.
3. **Mobile:** Ứng dụng sử dụng package `google_sign_in` để lấy ID token.
4. Hệ thống xác thực Google token và kiểm tra email:
   - Nếu email chưa tồn tại: Tạo tài khoản mới, đánh dấu `GoogleRegistered = true`
   - Nếu email đã tồn tại và là tài khoản Google: Đăng nhập bình thường
   - Nếu tài khoản Admin: Từ chối đăng nhập qua Google
5. Nếu `RequiresPassword = true` (lần đầu đăng nhập Google), chuyển đến màn hình tạo mật khẩu.
6. Nếu hồ sơ chưa hoàn thiện, chuyển đến màn hình hoàn thiện hồ sơ.

**Luồng rẽ nhánh / Ngoại lệ:**
- 4a (Đăng nhập): Email không tồn tại hoặc mật khẩu sai → Báo lỗi "Sai email hoặc mật khẩu".
- 4b (Đăng nhập): Tài khoản bị khóa → Báo lỗi "Tài khoản của bạn đã bị khóa".
- 4c (Đăng nhập): Tài khoản đăng ký qua Google → Báo lỗi "Tài khoản này đăng ký qua Google, vui lòng đăng nhập bằng Google".
- 4a (Đăng ký): Email đã tồn tại → Báo lỗi "Email đã tồn tại!".
- 6a (Đăng ký): Mật khẩu < 8 ký tự → Báo lỗi "Mật khẩu phải có ít nhất 8 ký tự".
- 7a (Đăng ký): Mã OTP sai hoặc hết hạn → Báo lỗi "Mã xác thực không hợp lệ!".

---

## PHẦN C: CHỨC NĂNG HỘI VIÊN (User)

---

### UC-U02: Quản lý hồ sơ cá nhân

| Thuộc tính | Mô tả |
|------------|-------|
| **Tên Use Case** | Quản lý hồ sơ cá nhân |
| **Mã UC** | UC-U02 |
| **Tác nhân (Actor)** | Hội viên (User) |
| **Mô tả ngắn gọn** | Cho phép người dùng xem và cập nhật thông tin cá nhân bao gồm họ tên, ngày sinh, giới tính, chiều cao, cân nặng, mức độ vận động và ảnh đại diện. |
| **Điều kiện tiên quyết** | - Người dùng đã đăng nhập vào hệ thống. |
| **Điều kiện sau** | - Thông tin hồ sơ được cập nhật thành công trong bảng `UserProfile`. |

**Luồng sự kiện chính (Main Flow):**

**Luồng Xem hồ sơ:**
1. Người dùng chọn menu "Hồ sơ cá nhân" hoặc icon avatar.
2. Hệ thống gọi `GET /api/UserProfile` với JWT token.
3. Hệ thống trả về `UserProfileDto` chứa: FullName, Dob, Gender, HeightCm, WeightKg, ActivityLevel, AvatarUrl.
4. Hiển thị thông tin hồ sơ trên giao diện.

**Luồng Cập nhật hồ sơ:**
1. Người dùng chọn "Chỉnh sửa hồ sơ".
2. Hệ thống hiển thị form với các trường có thể chỉnh sửa.
3. Người dùng cập nhật thông tin:
   - Họ tên (bắt buộc, không để trống)
   - Ngày sinh (bắt buộc, phải >= 10 tuổi)
   - Giới tính (Male/Female/Other)
   - Chiều cao (0-300 cm)
   - Cân nặng (0-500 kg)
   - Mức độ vận động (Sedentary/Light/Moderate/Active/VeryActive)
4. Người dùng nhấn "Lưu".
5. Hệ thống gọi `PUT /api/UserProfile` với `UpdateProfileRequest`.
6. Hệ thống cập nhật bảng `UserProfile` và trả về thông báo thành công.

**Luồng Thay đổi ảnh đại diện:**
1. Người dùng chọn "Thay đổi ảnh đại diện".
2. Hệ thống mở trình chọn file.
3. Người dùng chọn file ảnh (JPEG/PNG/GIF, tối đa 5MB).
4. Hệ thống gọi `POST /api/UserProfile/upload-avatar` với FormData.
5. Hệ thống upload ảnh lên MinIO storage.
6. Hệ thống cập nhật `AvatarUrl` trong cả `ApplicationUser` và `UserProfile`.
7. Hiển thị ảnh mới.

**Luồng rẽ nhánh / Ngoại lệ:**
- 3a: Ngày sinh không hợp lệ (< 10 tuổi hoặc trong tương lai) → Báo lỗi validation.
- 3b: Chiều cao/Cân nặng ngoài phạm vi → Báo lỗi validation.
- 3a (Avatar): File không phải định dạng ảnh → Báo lỗi "Chỉ chấp nhận file ảnh (JPEG, PNG, GIF)".
- 3b (Avatar): File > 5MB → Báo lỗi "Kích thước file không được vượt quá 5MB".

---

### UC-U03: Tư vấn sức khỏe AI

| Thuộc tính | Mô tả |
|------------|-------|
| **Tên Use Case** | Tư vấn sức khỏe AI |
| **Mã UC** | UC-U03 |
| **Tác nhân (Actor)** | Hội viên (User) |
| **Mô tả ngắn gọn** | Cho phép người dùng trò chuyện với AI HealthSync Coach để nhận tư vấn về sức khỏe, dinh dưỡng và tập luyện dựa trên dữ liệu cá nhân. |
| **Điều kiện tiên quyết** | - Người dùng đã đăng nhập vào hệ thống.<br>- Dịch vụ AI (Groq) đang hoạt động. |
| **Điều kiện sau** | - Người dùng nhận được phản hồi từ AI.<br>- Lịch sử chat được lưu trong bảng `ChatMessage`. |

**Luồng sự kiện chính (Main Flow):**
1. Người dùng truy cập trang Chat AI.
2. Hệ thống gọi `GET /api/Chat/history` để tải lịch sử chat (phân trang, 20 tin nhắn/trang).
3. Hiển thị lịch sử chat hoặc màn hình chat trống.
4. Người dùng nhập câu hỏi vào ô chat (ví dụ: "Hôm nay tôi nên tập gì?").
5. Người dùng nhấn "Gửi".
6. Hệ thống gọi `POST /api/Chat/ask` với `{ question: "..." }`.
7. Backend `ChatWithBotQueryHandler` thực hiện:
   - Thu thập context người dùng:
     - **Profile**: Giới tính, tuổi, chiều cao, cân nặng, BMI, BMR, mức vận động
     - **Mục tiêu hiện tại**: Loại, target, deadline, tiến độ
     - **5 mục tiêu đã hoàn thành** gần nhất
     - **Nhật ký 7 ngày**: Dinh dưỡng (calories, foods) và tập luyện
     - **20 hành động gần nhất** trong hệ thống
     - **40 bài tập và món ăn phổ biến** trong hệ thống
   - Lưu tin nhắn user vào `ChatMessage`
   - Gọi `GroqAiChatService.GetHealthAdviceAsync()` với system prompt cá nhân hóa
   - Lưu phản hồi AI vào `ChatMessage`
8. Trả về `ChatResponseDto` chứa: response, timestamp, messageId.
9. Hiển thị phản hồi AI với format Markdown (hỗ trợ emoji, bullet points).

**Luồng Xem lịch sử chat (Mobile):**
1. Người dùng chọn "Lịch sử chat".
2. Hệ thống nhóm tin nhắn theo phiên (session) - cách nhau > 2 giờ.
3. Hiển thị danh sách các phiên với preview và số tin nhắn.
4. Người dùng chọn một phiên để xem chi tiết.

**Luồng Bắt đầu chat mới:**
1. Người dùng nhấn "Chat mới".
2. Hệ thống xóa giao diện chat hiện tại.
3. Bắt đầu phiên chat mới.

**Luồng rẽ nhánh / Ngoại lệ:**
- 6a: Câu hỏi trống → Báo lỗi "Vui lòng nhập câu hỏi".
- 6b: Mất kết nối mạng → Báo lỗi kết nối và cho phép thử lại.
- 7a: Dịch vụ AI không khả dụng → Báo lỗi "Dịch vụ AI tạm thời không khả dụng".
- 7b: Câu hỏi không liên quan sức khỏe → AI từ chối trả lời lịch sự.

**Đặc điểm AI Response:**
- Trả lời ngắn gọn (100-150 từ)
- Cá nhân hóa dựa trên dữ liệu thực tế
- Đưa ra hành động cụ thể với số liệu
- Sử dụng emoji phù hợp (💪🔥✨)
- KHÔNG đưa ra chẩn đoán y khoa

---

### UC-U04: Thiết lập mục tiêu

| Thuộc tính | Mô tả |
|------------|-------|
| **Tên Use Case** | Thiết lập mục tiêu |
| **Mã UC** | UC-U04 |
| **Tác nhân (Actor)** | Hội viên (User) |
| **Mô tả ngắn gọn** | Cho phép người dùng tạo và quản lý các mục tiêu sức khỏe (giảm cân, tăng cân, tăng cơ, giảm mỡ) với tracking tiến độ. |
| **Điều kiện tiên quyết** | - Người dùng đã đăng nhập.<br>- Người dùng đã hoàn thiện hồ sơ cá nhân (có cân nặng ban đầu). |
| **Điều kiện sau** | - Mục tiêu được tạo/cập nhật trong bảng `Goal`.<br>- Tiến độ được ghi nhận trong bảng `ProgressRecord`. |

**Luồng sự kiện chính (Main Flow):**

**Luồng Xem danh sách mục tiêu:**
1. Người dùng truy cập trang "Mục tiêu".
2. Hệ thống gọi `GET /api/Goals` để lấy tất cả mục tiêu của user.
3. Hiển thị danh sách theo tabs: "Đang thực hiện" / "Đã hoàn thành".
4. Mỗi mục tiêu hiển thị: Loại, Target, Tiến độ %, Ngày bắt đầu/kết thúc.

**Luồng Tạo mục tiêu mới:**
1. Người dùng nhấn "Tạo mục tiêu mới".
2. Hệ thống hiển thị form với:
   - Loại mục tiêu: weight_loss (Giảm cân), weight_gain (Tăng cân), muscle_gain (Tăng cơ), fat_loss (Giảm mỡ)
   - Cân nặng mục tiêu (kg)
   - Ngày bắt đầu (mặc định: hôm nay)
   - Ngày kết thúc (tùy chọn)
   - Ghi chú (tùy chọn)
3. Người dùng điền thông tin và nhấn "Tạo".
4. Hệ thống gọi `POST /api/Goals` với `CreateGoalRequest`.
5. Hệ thống tạo Goal với status = "active".
6. Chuyển về danh sách mục tiêu.

**Luồng Ghi nhận tiến độ:**
1. Người dùng chọn một mục tiêu đang hoạt động.
2. Hệ thống hiển thị chi tiết mục tiêu với biểu đồ tiến độ.
3. Người dùng nhấn "Cập nhật tiến độ".
4. Hệ thống hiển thị form:
   - Ngày ghi nhận
   - Cân nặng hiện tại (kg)
   - Số đo vòng eo (cm) - tùy chọn
   - Ghi chú - tùy chọn
5. Người dùng nhập và nhấn "Lưu".
6. Hệ thống gọi `POST /api/Goals/{goalId}/progress` với `AddProgressRequest`.
7. Hệ thống tính toán tiến độ:
   - Nếu goal giảm: progress = (startValue - currentValue) / (startValue - targetValue) × 100
   - Nếu goal tăng: progress = (currentValue - startValue) / (targetValue - startValue) × 100
8. Cập nhật biểu đồ tiến độ.

**Luồng rẽ nhánh / Ngoại lệ:**
- 3a (Tạo): Target weight không hợp lệ (âm hoặc bằng cân nặng hiện tại) → Báo lỗi.
- 6a (Tiến độ): Mục tiêu không ở trạng thái active/in_progress → Báo lỗi "Không thể cập nhật mục tiêu đã hoàn thành/tạm dừng".

**Các trạng thái mục tiêu:**
- `active`: Đang hoạt động
- `in_progress`: Đang theo dõi
- `completed`: Đã hoàn thành
- `paused`: Tạm dừng

---

### UC-U05: Quản lý tập luyện

| Thuộc tính | Mô tả |
|------------|-------|
| **Tên Use Case** | Quản lý tập luyện |
| **Mã UC** | UC-U05 |
| **Tác nhân (Actor)** | Hội viên (User) |
| **Mô tả ngắn gọn** | Cho phép người dùng ghi nhật ký tập luyện và tra cứu bài tập từ thư viện. |
| **Điều kiện tiên quyết** | - Người dùng đã đăng nhập. |
| **Điều kiện sau** | - Nhật ký tập luyện được lưu trong `WorkoutLog` và `ExerciseSession`. |

**Luồng sự kiện chính (Main Flow):**

**UC-U05a: Ghi nhật ký tập**
1. Người dùng truy cập trang "Ghi buổi tập".
2. Hệ thống gọi `GET /api/Workout/exercises` để lấy danh sách bài tập.
3. Hiển thị form tạo buổi tập:
   - Ngày tập (mặc định: hôm nay)
   - Thời lượng (phút)
   - Danh sách bài tập trong buổi
4. Người dùng chọn bài tập từ thư viện.
5. Với mỗi bài tập, người dùng nhập:
   - Số hiệp (sets): 1-100
   - Số lần/hiệp (reps): 1-1000
   - Tạ (kg): 0-1000
   - Thời gian nghỉ (giây): 0-3600 (tùy chọn)
   - RPE (1-10): Mức độ gắng sức (tùy chọn)
6. Người dùng thêm nhiều bài tập nếu cần.
7. Nhấn "Lưu buổi tập".
8. Hệ thống gọi `POST /api/Workout/workout-logs` với `CreateWorkoutLogDto`.
9. Hệ thống validate:
   - Ngày tập không trong tương lai
   - Thời lượng: 1-1440 phút
   - Ít nhất 1 bài tập
10. Tạo `WorkoutLog` và các `ExerciseSession` liên quan.
11. Hiển thị thông báo thành công.

**UC-U05b: Tra cứu bài tập**
1. Người dùng truy cập trang "Thư viện bài tập".
2. Hệ thống gọi `GET /api/Workout/exercises`.
3. Hiển thị danh sách bài tập với:
   - Ảnh minh họa
   - Tên bài tập
   - Nhóm cơ (Chest, Back, Shoulders, Arms, Legs, Core, Full Body)
   - Độ khó (Beginner, Intermediate, Advanced)
4. Người dùng có thể:
   - Tìm kiếm theo tên/mô tả
   - Lọc theo nhóm cơ
   - Lọc theo độ khó
5. Người dùng chọn bài tập để xem chi tiết (mô tả, thiết bị cần thiết).

**Luồng Xem lịch sử tập luyện:**
1. Người dùng truy cập trang "Lịch sử tập".
2. Hệ thống gọi `GET /api/Workout/workout-logs?startDate=...&endDate=...`.
3. Hiển thị danh sách buổi tập nhóm theo ngày.
4. Người dùng có thể xem chi tiết hoặc xóa buổi tập.

**Luồng rẽ nhánh / Ngoại lệ:**
- 7a: Không có bài tập nào được thêm → Báo lỗi "Vui lòng thêm ít nhất một bài tập".
- 9a: Ngày tập trong tương lai → Báo lỗi validation.
- 5a (Tra cứu): Không tìm thấy bài tập → Hiển thị "Không có kết quả phù hợp".

---

### UC-U06: Theo dõi dinh dưỡng

| Thuộc tính | Mô tả |
|------------|-------|
| **Tên Use Case** | Theo dõi dinh dưỡng |
| **Mã UC** | UC-U06 |
| **Tác nhân (Actor)** | Hội viên (User) |
| **Mô tả ngắn gọn** | Cho phép người dùng ghi nhật ký ăn uống hàng ngày và tra cứu thông tin dinh dưỡng của món ăn. |
| **Điều kiện tiên quyết** | - Người dùng đã đăng nhập. |
| **Điều kiện sau** | - Nhật ký dinh dưỡng được lưu trong `NutritionLog` và `FoodEntry`. |

**Luồng sự kiện chính (Main Flow):**

**UC-U06a: Ghi nhật ký ăn uống**
1. Người dùng truy cập trang "Nhật ký dinh dưỡng".
2. Hệ thống gọi `GET /api/Nutrition/nutrition-logs/{date}` để lấy nhật ký ngày hiện tại.
3. Hiển thị tổng quan:
   - Tổng calories đã tiêu thụ
   - Biểu đồ tròn: Protein, Carbs, Fat
   - Danh sách bữa ăn theo loại (Breakfast, Lunch, Dinner, Snack)
4. Người dùng nhấn "Thêm món ăn" tại bữa mong muốn.
5. Hệ thống hiển thị modal/bottom sheet tìm kiếm món ăn.
6. Người dùng tìm kiếm và chọn món ăn.
7. Hệ thống hiển thị thông tin món ăn:
   - Tên, ảnh
   - Serving size và đơn vị
   - Calories, Protein, Carbs, Fat per serving
8. Người dùng nhập số lượng (quantity).
9. Người dùng nhấn "Thêm".
10. Hệ thống gọi `POST /api/Nutrition/food-entries`:
    ```json
    {
      "foodItemId": 1,
      "quantity": 1.5,
      "mealType": "Lunch",
      "logDate": "2026-01-06"
    }
    ```
11. Hệ thống tính toán dinh dưỡng:
    - calories = foodItem.CaloriesKcal × (quantity / servingSize)
    - protein = foodItem.ProteinG × (quantity / servingSize)
    - carbs = foodItem.CarbsG × (quantity / servingSize)
    - fat = foodItem.FatG × (quantity / servingSize)
12. Tạo `FoodEntry` và cập nhật `TotalCalories` của `NutritionLog`.
13. Refresh giao diện với dữ liệu mới.

**UC-U06b: Tìm kiếm món ăn**
1. Người dùng truy cập trang "Tìm kiếm món ăn".
2. Hệ thống gọi `GET /api/Nutrition/food-items`.
3. Hiển thị danh sách món ăn với bộ lọc:
   - Tìm kiếm theo tên
   - Lọc theo danh mục (Món chính, Món phụ, Ăn vặt)
   - Lọc theo calories (Thấp <200, Trung bình 200-500, Cao >500)
   - Lọc theo protein/carbs
4. Người dùng chọn món để xem chi tiết dinh dưỡng.

**Luồng Xem lịch sử dinh dưỡng:**
1. Người dùng chọn xem lịch sử (7 ngày / 30 ngày).
2. Hệ thống gọi `GET /api/Nutrition/nutrition-logs?startDate=...&endDate=...`.
3. Hiển thị biểu đồ calories theo ngày.
4. Hiển thị thống kê trung bình: Calories, Protein, Carbs, Fat.

**Luồng Xóa food entry:**
1. Người dùng vuốt/nhấn giữ món ăn đã ghi.
2. Xác nhận xóa.
3. Hệ thống gọi `DELETE /api/Nutrition/food-entries/{id}`.
4. Cập nhật tổng calories của ngày.

**Luồng rẽ nhánh / Ngoại lệ:**
- 6a: Không tìm thấy món ăn → Hiển thị "Không có kết quả".
- 8a: Quantity <= 0 → Báo lỗi validation.

**Các loại bữa ăn (MealType):**
- `Breakfast` - Bữa sáng
- `Lunch` - Bữa trưa
- `Dinner` - Bữa tối
- `Snack` - Ăn vặt

---

### UC-U07: Xem Dashboard tổng quan

| Thuộc tính | Mô tả |
|------------|-------|
| **Tên Use Case** | Xem Dashboard tổng quan |
| **Mã UC** | UC-U07 |
| **Tác nhân (Actor)** | Hội viên (User) |
| **Mô tả ngắn gọn** | Cho phép người dùng xem tổng quan về tiến độ sức khỏe, mục tiêu, calories hôm nay và chuỗi ngày tập luyện. |
| **Điều kiện tiên quyết** | - Người dùng đã đăng nhập. |
| **Điều kiện sau** | - Không có thay đổi dữ liệu (chỉ đọc). |

**Luồng sự kiện chính (Main Flow):**
1. Người dùng đăng nhập thành công hoặc truy cập trang chủ.
2. Hệ thống gọi `GET /api/Dashboard/customer`.
3. Backend `GetCustomerDashboardQueryHandler` tổng hợp:
   - Thông tin user (tên, avatar)
   - Mục tiêu đang hoạt động với tiến độ
   - Lịch sử cân nặng (biểu đồ)
   - Stats hôm nay: Calories tiêu thụ, Thời gian tập tuần này
   - Chuỗi ngày tập (streak)
4. Hiển thị Dashboard với:
   - **Header**: Lời chào, avatar
   - **Goal Card**: Tiến độ mục tiêu chính (% hoàn thành)
   - **Stats Cards**:
     - 🔥 Chuỗi ngày tập: X ngày liên tiếp
     - ⏱️ Tập luyện tuần này: X phút
     - 🍽️ Calories hôm nay: X kcal
   - **Weight Chart**: Biểu đồ cân nặng theo thời gian
   - **Quick Actions**: Nút "Ghi bữa ăn", "Ghi buổi tập"
5. Nếu có nhiều mục tiêu, người dùng có thể chuyển đổi qua dropdown.

**Tính toán Streak:**
```
Bắt đầu từ hôm nay (hoặc hôm qua nếu hôm nay chưa tập)
Đếm ngược số ngày liên tiếp có workout log
Dừng khi gặp ngày không có log
```

**Luồng rẽ nhánh / Ngoại lệ:**
- 2a: Chưa có mục tiêu → Hiển thị card gợi ý tạo mục tiêu.
- 2b: Chưa có dữ liệu → Hiển thị trạng thái empty với hướng dẫn bắt đầu.

---

## PHẦN D: CHỨC NĂNG QUẢN TRỊ (Admin)

---

### UC-A01: Dashboard thống kê

| Thuộc tính | Mô tả |
|------------|-------|
| **Tên Use Case** | Dashboard thống kê |
| **Mã UC** | UC-A01 |
| **Tác nhân (Actor)** | Quản trị viên (Admin) |
| **Mô tả ngắn gọn** | Cho phép Admin xem các chỉ số KPI, biểu đồ thống kê và tình trạng hệ thống. |
| **Điều kiện tiên quyết** | - Admin đã đăng nhập.<br>- Admin có quyền `DASHBOARD_ADMIN`. |
| **Điều kiện sau** | - Không có thay đổi dữ liệu (chỉ đọc). |

**Luồng sự kiện chính (Main Flow):**
1. Admin truy cập trang Admin Dashboard.
2. Hệ thống kiểm tra quyền `DASHBOARD_ADMIN`.
3. Hệ thống gọi `GET /api/Admin/Dashboard`.
4. Backend `GetAdminDashboardQueryHandler` tổng hợp:
   - **KPI Stats**:
     - Tổng số users (và growth rate so với tháng trước)
     - Active Users: DAU (Daily) và MAU (Monthly)
     - Content Count: Số bài tập + món ăn
     - AI Usage: Tổng requests, ước tính chi phí
   - **Charts Data**:
     - User Growth: Số user đăng ký 6 tháng gần nhất
     - Goal Success Rate: Tỷ lệ Completed/InProgress/Failed
     - Activity Heatmap: Phân bố workout theo ngày/giờ
   - **Content Insights**:
     - Top 10 bài tập được sử dụng nhiều nhất
     - Top 10 món ăn được ghi nhận nhiều nhất
     - Missed Searches: Từ khóa tìm kiếm không có kết quả
   - **System Health**:
     - Trạng thái Database
     - Trạng thái MinIO Storage
     - Trạng thái AI Service
5. Hiển thị Dashboard với:
   - KPI cards với trend indicators (↑↓)
   - Line chart: User growth
   - Pie chart: Goal success rate
   - List: Activity peak hours
   - Tables: Top content
   - Status panel: System health

**Tính toán chỉ số:**
```
DAU = Max(distinct users có workout log, distinct users có nutrition log) trong 24h qua
MAU = Distinct users có workout log trong 30 ngày qua
Growth Rate = ((current - previous) / previous) × 100%
Goal Completion Rate = (completedGoals / totalGoals) × 100%
```

**Luồng rẽ nhánh / Ngoại lệ:**
- 2a: Không có quyền → Redirect về trang 403 Forbidden.
- 4a: Service không khả dụng → Hiển thị trạng thái "Unavailable" với latency = -1.

---

### UC-A02: Quản lý nội dung (CMS)

| Thuộc tính | Mô tả |
|------------|-------|
| **Tên Use Case** | Quản lý nội dung (CMS) |
| **Mã UC** | UC-A02 |
| **Tác nhân (Actor)** | Quản trị viên (Admin) |
| **Mô tả ngắn gọn** | Cho phép Admin quản lý thư viện bài tập và kho món ăn (CRUD operations). |
| **Điều kiện tiên quyết** | - Admin đã đăng nhập.<br>- Admin có các quyền tương ứng (EXERCISE_*, FOOD_*). |
| **Điều kiện sau** | - Nội dung được tạo/sửa/xóa trong database. |

---

**UC-A02a: Quản lý kho Bài tập**

**Luồng Xem danh sách bài tập:**
1. Admin truy cập menu "Quản lý bài tập".
2. Hệ thống kiểm tra quyền `EXERCISE_READ`.
3. Hệ thống gọi `GET /api/Exercises`.
4. Hiển thị danh sách với phân trang và bộ lọc (nhóm cơ, độ khó, tìm kiếm).

**Luồng Tạo bài tập mới:**
1. Admin nhấn "Thêm bài tập".
2. Hệ thống kiểm tra quyền `EXERCISE_CREATE`.
3. Hiển thị form với các trường:
   - Tên bài tập (bắt buộc)
   - Nhóm cơ: Chest, Back, Shoulders, Arms, Legs, Core, Full Body
   - Độ khó: Beginner, Intermediate, Advanced
   - Thiết bị (tùy chọn)
   - Mô tả (tùy chọn)
4. Admin điền thông tin và nhấn "Lưu".
5. Hệ thống gọi `POST /api/Exercises` với `CreateExerciseDto`.
6. Hiển thị thông báo thành công, refresh danh sách.

**Luồng Upload ảnh bài tập:**
1. Admin chọn bài tập và nhấn "Upload ảnh".
2. Chọn file ảnh (JPEG/PNG/GIF, max 5MB).
3. Hệ thống gọi `PUT /api/Exercises/{id}/upload-image` với FormData.
4. Hệ thống upload lên MinIO bucket `exercises`.
5. Cập nhật `ImageUrl` của Exercise.

**Luồng Sửa bài tập:**
1. Admin chọn bài tập và nhấn "Sửa".
2. Hệ thống kiểm tra quyền `EXERCISE_UPDATE`.
3. Hiển thị form với dữ liệu hiện tại.
4. Admin chỉnh sửa và nhấn "Lưu".
5. Hệ thống gọi `PUT /api/Exercises/{id}`.

**Luồng Xóa bài tập:**
1. Admin chọn bài tập và nhấn "Xóa".
2. Hệ thống kiểm tra quyền `EXERCISE_DELETE`.
3. Hiển thị dialog xác nhận.
4. Admin xác nhận → Hệ thống gọi `DELETE /api/Exercises/{id}`.
5. Hiển thị thông báo thành công, refresh danh sách.

---

**UC-A02b: Quản lý kho Món ăn**

**Luồng Xem danh sách món ăn:**
1. Admin truy cập menu "Quản lý món ăn".
2. Hệ thống kiểm tra quyền `FOOD_READ`.
3. Hệ thống gọi `GET /api/FoodItems`.
4. Hiển thị danh sách với tìm kiếm.

**Luồng Tạo món ăn mới:**
1. Admin nhấn "Thêm món ăn".
2. Hệ thống kiểm tra quyền `FOOD_CREATE`.
3. Hiển thị form với các trường:
   - Tên món ăn (bắt buộc)
   - Serving size (g/ml)
   - Đơn vị serving
   - Calories (kcal)
   - Protein (g)
   - Carbs (g)
   - Fat (g)
4. Admin điền thông tin và nhấn "Lưu".
5. Hệ thống gọi `POST /api/FoodItems` với `CreateFoodItemDto`.
6. Hiển thị thông báo thành công, refresh danh sách.

**Luồng Upload ảnh món ăn:**
1. Admin chọn món ăn và nhấn "Upload ảnh".
2. Chọn file ảnh.
3. Hệ thống gọi `PUT /api/FoodItems/{id}/upload-image` với FormData.
4. Hệ thống upload lên MinIO bucket `foods`.
5. Cập nhật `ImageUrl` của FoodItem.

**Luồng Sửa món ăn:**
1. Admin chọn món ăn và nhấn "Sửa".
2. Hệ thống kiểm tra quyền `FOOD_UPDATE`.
3. Hiển thị form với dữ liệu hiện tại.
4. Admin chỉnh sửa và nhấn "Lưu".
5. Hệ thống gọi `PUT /api/FoodItems/{id}`.

**Luồng Xóa món ăn:**
1. Admin chọn món ăn và nhấn "Xóa".
2. Hệ thống kiểm tra quyền `FOOD_DELETE`.
3. Hiển thị dialog xác nhận.
4. Admin xác nhận → Hệ thống gọi `DELETE /api/FoodItems/{id}`.
5. Hiển thị thông báo thành công.

**Luồng rẽ nhánh / Ngoại lệ:**
- 2a: Không có quyền → Hiển thị thông báo "Bạn không có quyền thực hiện thao tác này".
- 4a (Tạo): Tên trống → Báo lỗi validation.
- 4a (Xóa): Món ăn đang được sử dụng trong FoodEntry → Cảnh báo và cho phép xóa (cascade soft delete).

---

### UC-A03: Quản lý người dùng

| Thuộc tính | Mô tả |
|------------|-------|
| **Tên Use Case** | Quản lý người dùng |
| **Mã UC** | UC-A03 |
| **Tác nhân (Actor)** | Quản trị viên (Admin) |
| **Mô tả ngắn gọn** | Cho phép Admin xem, tạo, sửa, khóa/mở khóa và xóa tài khoản người dùng. |
| **Điều kiện tiên quyết** | - Admin đã đăng nhập.<br>- Admin có quyền `USER_*`. |
| **Điều kiện sau** | - Thông tin người dùng được cập nhật trong bảng `ApplicationUser`. |

**Luồng sự kiện chính (Main Flow):**

**Luồng Xem danh sách người dùng:**
1. Admin truy cập menu "Quản lý người dùng".
2. Hệ thống kiểm tra quyền `USER_READ`.
3. Hệ thống gọi `GET /api/Admin/users?pageNumber=1&pageSize=10`.
4. Hiển thị danh sách với:
   - Phân trang
   - Tìm kiếm theo email/tên
   - Lọc theo role (Admin/Customer)
   - Lọc theo trạng thái (Active/Inactive)
   - Sắp xếp theo các cột

**Luồng Xem chi tiết người dùng:**
1. Admin click vào một user.
2. Hệ thống gọi `GET /api/Admin/users/{id}`.
3. Hiển thị thông tin chi tiết: Profile, Goals, Activity logs.

**Luồng Tạo người dùng mới:**
1. Admin nhấn "Tạo người dùng".
2. Hiển thị form: Email, Mật khẩu, Họ tên, Role.
3. Admin điền và nhấn "Tạo".
4. Hệ thống gọi `POST /api/Admin/users`.
5. Tạo ApplicationUser và UserProfile.

**Luồng Cập nhật role:**
1. Admin chọn user và nhấn "Đổi role".
2. Hệ thống kiểm tra quyền `USER_UPDATE_ROLE`.
3. Chọn role mới (Admin/Customer).
4. Hệ thống gọi `PUT /api/Admin/users/{id}/role`.

**Luồng Khóa/Mở khóa tài khoản:**
1. Admin chọn user và nhấn "Khóa" hoặc "Mở khóa".
2. Hệ thống kiểm tra quyền `USER_BAN`.
3. Hiển thị dialog xác nhận.
4. Hệ thống gọi `PUT /api/Admin/users/{id}/toggle-status`.
5. Cập nhật `IsActive` của ApplicationUser.

**Luồng Xóa người dùng:**
1. Admin chọn user và nhấn "Xóa".
2. Hệ thống kiểm tra quyền `USER_DELETE`.
3. Hiển thị dialog xác nhận nghiêm trọng.
4. Admin xác nhận → Hệ thống gọi `DELETE /api/Admin/users/{id}`.
5. Xóa user và tất cả dữ liệu liên quan (cascade).

**Luồng rẽ nhánh / Ngoại lệ:**
- 4a (Tạo): Email đã tồn tại → Báo lỗi.
- 2a (Khóa): Admin tự khóa chính mình → Từ chối với thông báo.
- 4a (Xóa): Admin tự xóa chính mình → Từ chối với thông báo.

---

## BẢNG TÓM TẮT USE CASE

| Mã UC | Tên Use Case | Actor | Include/Extend |
|-------|--------------|-------|----------------|
| UC-G01 | Xem trang chủ & Giới thiệu | Guest | - |
| UC-U01 | Đăng ký, Đăng nhập | Guest, User | - |
| UC-U02 | Quản lý hồ sơ cá nhân | User | «include» UC-U01 |
| UC-U03 | Tư vấn sức khỏe AI | User | «include» UC-U01 |
| UC-U04 | Thiết lập mục tiêu | User | «include» UC-U01 |
| UC-U05 | Quản lý tập luyện | User | «include» UC-U01, UC-U05a, UC-U05b |
| UC-U05a | Ghi nhật ký tập | User | «include» UC-U05b |
| UC-U05b | Tra cứu bài tập | User | - |
| UC-U06 | Theo dõi dinh dưỡng | User | «include» UC-U01, UC-U06a, UC-U06b |
| UC-U06a | Ghi nhật ký ăn uống | User | «include» UC-U06b |
| UC-U06b | Tìm kiếm món ăn | User | - |
| UC-U07 | Xem Dashboard tổng quan | User | «include» UC-U01 |
| UC-A01 | Dashboard thống kê | Admin | «include» UC-U01 |
| UC-A02 | Quản lý nội dung (CMS) | Admin | «include» UC-U01, UC-A02a, UC-A02b |
| UC-A02a | Quản lý kho Bài tập | Admin | - |
| UC-A02b | Quản lý kho Món ăn | Admin | - |
| UC-A03 | Quản lý người dùng | Admin | «include» UC-U01 |

---

## PHỤ LỤC: THÔNG TIN KỸ THUẬT

### API Base URLs
- **Backend:** `http://localhost:8080/api`
- **Mobile (Android Emulator):** `http://10.0.2.2:8080/api`

### JWT Token Structure
```json
{
  "sub": "userId",
  "email": "user@example.com",
  "jti": "unique-token-id",
  "uid": "userId",
  "role": ["Customer"],
  "Permission": ["EXERCISE_READ", "FOOD_READ", "..."]
}
```

### Permission Codes
| Category | Permissions |
|----------|-------------|
| User | USER_READ, USER_BAN, USER_UPDATE_ROLE, USER_DELETE |
| Exercise | EXERCISE_READ, EXERCISE_CREATE, EXERCISE_UPDATE, EXERCISE_DELETE |
| Food | FOOD_READ, FOOD_CREATE, FOOD_UPDATE, FOOD_DELETE |
| Workout | WORKOUT_LOG_READ, WORKOUT_LOG_CREATE, WORKOUT_LOG_UPDATE, WORKOUT_LOG_DELETE |
| Nutrition | NUTRITION_LOG_READ, NUTRITION_LOG_CREATE, NUTRITION_LOG_UPDATE, NUTRITION_LOG_DELETE |
| Goal | GOAL_READ, GOAL_CREATE, GOAL_UPDATE, GOAL_DELETE |
| Dashboard | DASHBOARD_VIEW, DASHBOARD_ADMIN |
