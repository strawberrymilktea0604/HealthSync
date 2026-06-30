# 📋 KỊCH BẢN DEMO - DỰ ÁN HEALTHSYNC

## 📌 Thông Tin Tài Liệu

| Thông tin | Chi tiết |
|-----------|----------|
| **Dự án** | HealthSync - Ứng dụng theo dõi sức khỏe toàn diện |
| **Phiên bản** | 1.0 |
| **Ngày tạo** | 06/01/2026 |
| **Thời lượng demo** | 15-20 phút |
| **Nền tảng** | Web Admin, Web User, Mobile (Flutter) |

---

## 🎯 Mục Tiêu Demo

Trình diễn **luồng end-to-end** của hệ thống HealthSync, thể hiện sự liên kết chặt chẽ giữa 3 nền tảng:
- **Admin** quản lý hệ thống, nội dung và người dùng
- **User Web** sử dụng các tính năng theo dõi sức khỏe trên trình duyệt
- **User Mobile** sử dụng app di động với trải nghiệm native

---

## 👤 Tài Khoản Demo

| Nền tảng | Email | Password | Role | Ghi chú |
|----------|-------|----------|------|---------|
| **Web Admin** | `admin@healthsync.com` | `Admin@123` | Admin | Tài khoản quản trị |
| **Web User** | `demo.web@healthsync.com` | `Demo@123` | Customer | Người dùng web |
| **Mobile** | `demo.mobile@healthsync.com` | `Demo@123` | Customer | Người dùng mobile |

---

## 🎬 LUỒNG DEMO CHI TIẾT

### ⏱️ Timeline Tổng Quan

```
[0:00-3:00]   Phần 1: Web Admin - Giới thiệu hệ thống quản trị
[3:00-9:00]   Phần 2: Web User - Trải nghiệm người dùng trên web
[9:00-15:00]  Phần 3: Mobile - Trải nghiệm app di động + AI Chatbot
[15:00-17:00] Phần 4: Tổng kết & Q&A
```

---

## 📍 PHẦN 1: WEB ADMIN (3 phút)

### 🎯 Mục tiêu: Giới thiệu tổng quan hệ thống quản trị

---

### 1.1 Đăng nhập Admin ⏱️ [0:00-0:30]

**Hành động:**
1. Mở trình duyệt, truy cập: `http://localhost:5173` (hoặc domain deploy)
2. Nhập thông tin đăng nhập:
   - Email: `admin@healthsync.com`
   - Password: `Admin@123`
3. Nhấn **"Login"**

**Điểm nhấn khi demo:**
> 💡 *"Hệ thống HealthSync có phân quyền rõ ràng. Tài khoản Admin sẽ được chuyển hướng đến Dashboard Admin, còn Customer sẽ vào Dashboard cá nhân."*

**Kết quả mong đợi:**
- ✅ Đăng nhập thành công
- ✅ Chuyển đến **Admin Dashboard**
- ✅ Hiển thị sidebar menu với các chức năng Admin

---

### 1.2 Xem Admin Dashboard ⏱️ [0:30-1:30]

**Hành động:**
1. Quan sát các **KPI Cards** ở phần trên:
   - **Total Users**: Tổng số người dùng
   - **Active Users**: Người dùng hoạt động (Monthly/Daily)
   - **Content Library**: Số lượng bài tập + món ăn
   - **AI Usage**: Số request AI + Chi phí ước tính

2. Scroll xuống xem các biểu đồ:
   - **User Growth (6 Months)**: Biểu đồ đường số lượng user mới
   - **Goal Success Rate**: Biểu đồ tròn tỷ lệ đạt mục tiêu

3. Xem thêm:
   - **Activity Peak Hours**: Khung giờ hoạt động cao điểm
   - **Top Content**: Bài tập và món ăn được dùng nhiều nhất
   - **System Health**: Trạng thái các services

**Điểm nhấn khi demo:**
> 💡 *"Dashboard này cho phép Admin có cái nhìn tổng quan về toàn bộ hệ thống trong một màn hình. Mọi dữ liệu đều được cập nhật real-time."*

**Kết quả mong đợi:**
- ✅ Tất cả KPI cards hiển thị số liệu
- ✅ Biểu đồ render đúng với dữ liệu
- ✅ System Health hiển thị trạng thái "Healthy"

---

### 1.3 Quản Lý Người Dùng ⏱️ [1:30-2:15]

**Hành động:**
1. Click menu **"User Management"** hoặc **"Quản lý người dùng"**
2. Quan sát bảng danh sách người dùng với các cột:
   - ID, Avatar, Tên, Email, Role, Trạng thái, Ngày tham gia
3. **Demo tìm kiếm**: Nhập "demo" vào ô search → Kết quả lọc
4. **Demo sắp xếp**: Click vào header cột "Ngày tham gia" → Sắp xếp theo ngày

**Điểm nhấn khi demo:**
> 💡 *"Admin có thể quản lý toàn bộ người dùng: tìm kiếm, lọc, sắp xếp, thêm mới, chỉnh sửa thông tin, khóa/mở khóa tài khoản và thay đổi quyền."*

**Kết quả mong đợi:**
- ✅ Danh sách người dùng hiển thị đầy đủ
- ✅ Tìm kiếm và sắp xếp hoạt động
- ✅ Phân trang hoạt động

---

### 1.4 Quản Lý Content Library ⏱️ [2:15-3:00]

**Hành động:**
1. Click menu **"Content Library"**
2. Mặc định hiển thị tab **Exercises** (Bài tập):
   - Xem danh sách bài tập với hình ảnh, tên, nhóm cơ, độ khó
3. Click tab **"Foods"** (Món ăn):
   - Xem danh sách món ăn với thông tin dinh dưỡng

**Demo thêm nhanh (tùy chọn):**
4. Click **"Add Exercise"** hoặc **"Add Food"**
5. Điền một số thông tin cơ bản → Cancel (không lưu)

**Điểm nhấn khi demo:**
> 💡 *"Admin quản lý thư viện nội dung bao gồm hàng trăm bài tập và món ăn. Tất cả đều có hình ảnh và thông tin chi tiết để user dễ dàng tìm kiếm và sử dụng."*

**Kết quả mong đợi:**
- ✅ Tab Exercises hiển thị danh sách bài tập
- ✅ Tab Foods hiển thị danh sách món ăn
- ✅ Hình ảnh và thông tin hiển thị đúng

---

### 1.5 Đăng Xuất Admin ⏱️ [Cuối phút 3]

**Hành động:**
1. Click vào avatar góc phải trên
2. Click **"Logout"**

**Chuyển tiếp:**
> 🎬 *"Bây giờ chúng ta sẽ chuyển sang trải nghiệm của người dùng trên nền tảng Web..."*

---

## 📍 PHẦN 2: WEB USER (6 phút)

### 🎯 Mục tiêu: Trình diễn luồng sử dụng đầy đủ của người dùng trên web

---

### 2.1 Trang Chủ & Đăng Nhập ⏱️ [3:00-3:30]

**Hành động:**
1. Truy cập lại trang chủ: `http://localhost:5173`
2. Quan sát trang **Home Guest** với:
   - Hero banner "Welcome to HealthSync"
   - Các features highlights
   - Nút **"Get Started"** và **"Sign In"**
3. Click **"Sign In"**
4. Nhập thông tin:
   - Email: `demo.web@healthsync.com`
   - Password: `Demo@123`
5. Nhấn **"Login"**

**Điểm nhấn khi demo:**
> 💡 *"Trang chủ được thiết kế hiện đại, responsive và giới thiệu đầy đủ tính năng của ứng dụng. Người dùng có thể đăng ký, đăng nhập bằng email hoặc Google."*

**Kết quả mong đợi:**
- ✅ Trang Home Guest hiển thị đẹp
- ✅ Đăng nhập thành công
- ✅ Chuyển đến Dashboard người dùng

---

### 2.2 Dashboard Cá Nhân ⏱️ [3:30-4:30]

**Hành động:**
1. Quan sát Dashboard với:
   - **Header**: "Welcome to HealthSync" + Avatar
   - **Goals Progress**: Tiến độ mục tiêu chính
   - **Weight Chart**: Biểu đồ cân nặng 7 ngày
   - **Quick Actions**: Nút "Ghi bữa ăn", "Ghi buổi tập"
   - **Chat FAB**: Nút chat AI ở góc phải dưới

2. Hover vào biểu đồ cân nặng để xem tooltip

**Điểm nhấn khi demo:**
> 💡 *"Dashboard cá nhân cho User thấy ngay tiến độ mục tiêu, có thể nhanh chóng ghi nhật ký bữa ăn hoặc buổi tập. Biểu đồ cân nặng giúp theo dõi xu hướng."*

**Kết quả mong đợi:**
- ✅ Dashboard hiển thị đầy đủ sections
- ✅ Dữ liệu user (tên, avatar) hiển thị đúng
- ✅ Biểu đồ có tooltip khi hover

---

### 2.3 Quản Lý Mục Tiêu ⏱️ [4:30-5:30]

**Hành động:**
1. Click menu **"Goals"** hoặc **"Mục tiêu"** trên sidebar
2. Xem danh sách mục tiêu với:
   - Tab **"Đang thực hiện"**: Mục tiêu active
   - Tab **"Đã hoàn thành"**: Mục tiêu completed
3. Click vào một mục tiêu để xem **Chi tiết**:
   - Biểu đồ tiến độ (Area chart)
   - Thống kê: Current, Target, Progress %
   - Lịch sử cập nhật

**Demo thêm progress (nếu có thời gian):**
4. Click **"Thêm tiến độ"**
5. Nhập cân nặng mới → Lưu

**Điểm nhấn khi demo:**
> 💡 *"Người dùng có thể đặt nhiều loại mục tiêu: giảm cân, tăng cân, tăng cơ, giảm mỡ. Hệ thống tự động tính toán và hiển thị tiến độ bằng biểu đồ trực quan."*

**Kết quả mong đợi:**
- ✅ Danh sách mục tiêu hiển thị đúng
- ✅ Chi tiết mục tiêu có biểu đồ
- ✅ Có thể thêm progress mới

---

### 2.4 Theo Dõi Dinh Dưỡng ⏱️ [5:30-6:30]

**Hành động:**
1. Click menu **"Nutrition"**
2. Xem **Nutrition Overview**:
   - Circular progress: Calories đã nạp / Target
   - Macros: Protein, Carbs, Fat với %
3. Click **"Tìm kiếm món ăn"** hoặc vào Food Search
4. Tìm kiếm: "Chicken" → Kết quả hiển thị
5. Demo các filter:
   - Filter protein: **"Giàu Protein"**
   - Filter calories: **"Thấp (<200)"**

**Demo thêm món (nếu có thời gian):**
6. Click **"+"** bên cạnh một món
7. Chọn bữa: **"Lunch"**, số lượng: **1**
8. Xác nhận → Calories cập nhật

**Điểm nhấn khi demo:**
> 💡 *"Hệ thống có thư viện hàng trăm món ăn với thông tin dinh dưỡng chi tiết. Người dùng dễ dàng tìm kiếm, lọc và thêm vào nhật ký. Mọi thứ được tính toán tự động."*

**Kết quả mong đợi:**
- ✅ Overview hiển thị calories và macros
- ✅ Tìm kiếm và filter hoạt động
- ✅ Có thể thêm món ăn vào nhật ký

---

### 2.5 Theo Dõi Bài Tập ⏱️ [6:30-7:30]

**Hành động:**
1. Click menu **"Workouts"** hoặc từ Dashboard nhấn **"Ghi buổi tập"**
2. Xem **Workout History**:
   - Danh sách buổi tập theo ngày
   - Thông tin: Duration, Exercises count, Calories burned
3. Click **"Thêm buổi tập mới"** hoặc **"+"**
4. Trong Create Workout:
   - Chọn ngày: **Hôm nay**
   - Tìm kiếm bài tập: "Push"
   - Filter nhóm cơ: **"Chest"**
5. Click **"+"** để thêm bài tập "Push Up" vào workout
6. Nhập: Sets = 3, Reps = 15
7. (Không lưu để tiết kiệm thời gian)

**Điểm nhấn khi demo:**
> 💡 *"Giao diện tạo buổi tập được thiết kế 2 cột: bên trái là thư viện bài tập, bên phải là các bài đã chọn. Mỗi bài tập có hình ảnh, hướng dẫn và thông tin calories burned."*

**Kết quả mong đợi:**
- ✅ Workout History hiển thị lịch sử
- ✅ Create Workout có 2 cột
- ✅ Tìm kiếm và filter bài tập hoạt động

---

### 2.6 AI Chatbot trên Web ⏱️ [7:30-9:00]

**Hành động:**
1. Quay lại **Dashboard**
2. Click vào **FAB button** (nút chat) ở góc phải dưới
3. Modal chat hiển thị với:
   - Header "Assistant" + icon Bot
   - Vùng tin nhắn
   - Input box
4. Nhập câu hỏi đầu tiên:
   > "BMI của tôi là bao nhiêu?"
5. Chờ AI trả lời → AI tính toán BMI từ profile user
6. Nhập câu hỏi thứ 2:
   > "Tôi nên ăn gì cho bữa tối hôm nay?"
7. Chờ AI trả lời → AI đưa gợi ý phù hợp với mục tiêu
8. Nhập câu hỏi thứ 3:
   > "Gợi ý bài tập cho tôi"
9. AI đưa ra kế hoạch tập luyện

**Điểm nhấn khi demo:**
> 💡 *"Đây là tính năng đặc biệt của HealthSync: AI Chatbot thông minh sử dụng Groq AI. Điểm khác biệt là AI được cung cấp 100% context từ dữ liệu thực của người dùng: profile, mục tiêu, nutrition logs 7 ngày, workout logs 7 ngày. Nhờ đó, mọi tư vấn đều được CÁ NHÂN HÓA hoàn toàn."*

**Kết quả mong đợi:**
- ✅ Modal chat mở với animation mượt
- ✅ AI trả lời với thông tin cá nhân hóa (BMI, calories target...)
- ✅ Gợi ý phù hợp với mục tiêu user

---

### 2.7 Chuyển Tiếp Sang Mobile ⏱️ [Cuối phút 9]

**Hành động:**
1. Đóng modal chat
2. Click avatar → **"Logout"**

**Chuyển tiếp:**
> 🎬 *"Bây giờ chúng ta sẽ trải nghiệm HealthSync trên nền tảng Mobile, được phát triển bằng Flutter, chạy được trên cả Android và iOS..."*

---

## 📍 PHẦN 3: MOBILE (6 phút)

### 🎯 Mục tiêu: Trình diễn app mobile native với trải nghiệm tối ưu

---

### 3.1 Splash & Welcome Screen ⏱️ [9:00-9:30]

**Hành động:**
1. Mở app HealthSync trên điện thoại/emulator
2. Quan sát **Splash Screen**:
   - Logo HealthSync với animation
3. Tự động chuyển sang **Welcome Screen**:
   - Hero image/illustration
   - Nút **"Sign In"**
   - Nút **"Sign Up"**
   - Nút **"Continue with Google"**

**Điểm nhấn khi demo:**
> 💡 *"App mobile sử dụng Flutter, cho phép build một codebase chạy trên cả Android và iOS. UI/UX được thiết kế native, mượt mà và trực quan."*

**Kết quả mong đợi:**
- ✅ Splash Screen hiển thị đẹp
- ✅ Animation mượt mà
- ✅ Welcome Screen với 3 options

---

### 3.2 Đăng Nhập Mobile ⏱️ [9:30-10:00]

**Hành động:**
1. Nhấn **"Sign In"**
2. Nhập thông tin:
   - Email: `demo.mobile@healthsync.com`
   - Password: `Demo@123`
3. Nhấn **"Sign In"**

**Điểm nhấn khi demo:**
> 💡 *"Lưu ý rằng tài khoản mobile khác với tài khoản web. Mỗi người dùng có thể có dữ liệu khác nhau để so sánh."*

**Kết quả mong đợi:**
- ✅ Đăng nhập thành công
- ✅ Chuyển đến Home Screen

---

### 3.3 Home Screen Mobile ⏱️ [10:00-10:45]

**Hành động:**
1. Quan sát **Home Screen** với:
   - Greeting: "Xin chào, [Tên]!"
   - Card BMI với trạng thái (Normal/Overweight...)
   - Card Calories hôm nay
   - Card Workout hôm nay
2. Quan sát **Bottom Navigation Bar**:
   - Home, Goals, Nutrition, Chat, Profile (hoặc Workout)

**Điểm nhấn khi demo:**
> 💡 *"Home Screen cho cái nhìn nhanh về sức khỏe ngay khi mở app. BMI được tính tự động, và các card cho thấy hoạt động trong ngày."*

**Kết quả mong đợi:**
- ✅ Home Screen hiển thị đầy đủ cards
- ✅ Bottom Navigation hoạt động

---

### 3.4 Goals trên Mobile ⏱️ [10:45-11:30]

**Hành động:**
1. Nhấn tab **"Goals"** trên Bottom Navigation
2. Xem danh sách mục tiêu dạng cards
3. Nhấn vào một mục tiêu để xem **Chi tiết**:
   - Biểu đồ Line/Area
   - Progress percentage
   - Statistics cards
4. (Nếu goal đang active) Nhấn **FAB "+"** để thêm progress

**Điểm nhấn khi demo:**
> 💡 *"Trên mobile, mục tiêu hiển thị dạng card đẹp mắt. Biểu đồ tiến độ sử dụng fl_chart, cho phép touch để xem chi tiết từng điểm dữ liệu."*

**Kết quả mong đợi:**
- ✅ Danh sách goals hiển thị
- ✅ Chi tiết goal có biểu đồ đẹp
- ✅ FAB hoạt động đúng với status

---

### 3.5 Nutrition trên Mobile ⏱️ [11:30-12:15]

**Hành động:**
1. Nhấn tab **"Nutrition"**
2. Xem **Nutrition Screen** với:
   - Bộ chọn ngày (swipe hoặc date picker)
   - Tổng Calories với circular progress
   - Macros breakdown (P/C/F) dạng progress bars
   - Danh sách bữa ăn: Breakfast, Lunch, Dinner, Snacks
3. Nhấn **"+"** để thêm món ăn
4. Tìm kiếm: "Rice"
5. Chọn một món → Chọn bữa → Thêm

**Điểm nhấn khi demo:**
> 💡 *"Giao diện Nutrition trên mobile được tối ưu cho one-handed use. Mọi thao tác đều có thể thực hiện bằng một tay. Pull-to-refresh để cập nhật dữ liệu."*

**Kết quả mong đợi:**
- ✅ Nutrition screen responsive
- ✅ Tìm kiếm và thêm món hoạt động
- ✅ Calories cập nhật real-time

---

### 3.6 Workout trên Mobile ⏱️ [12:15-13:00]

**Hành động:**
1. Navigate đến **Workout History** (từ Home card hoặc menu)
2. Xem danh sách buổi tập theo ngày
3. Nhấn **FAB "+"** để tạo buổi tập mới
4. Trong **Create Workout**:
   - Chọn ngày
   - Tìm kiếm bài tập
   - Filter theo nhóm cơ/độ khó
   - Thêm một bài tập
   - Nhập sets, reps
5. (Không lưu để tiết kiệm thời gian)

**Điểm nhấn khi demo:**
> 💡 *"Create Workout trên mobile có UX khác web: single column, scroll vertical. Mỗi bài tập có hình ảnh minh họa giúp user dễ nhận biết."*

**Kết quả mong đợi:**
- ✅ Workout History hiển thị grouped by date
- ✅ Create Workout có search và filter
- ✅ Thêm bài tập và nhập sets/reps hoạt động

---

### 3.7 AI Chatbot trên Mobile ⏱️ [13:00-14:30] ⭐ HIGHLIGHT

**Hành động:**
1. Nhấn tab **"Chat"** trên Bottom Navigation
2. **Chat Screen** hiển thị với:
   - Header "HealthBot 💪"
   - Welcome message từ AI
   - Input TextField ở dưới
3. Nhập câu hỏi 1:
   > "Tuần này tôi tập và ăn như thế nào?"
4. **AI tổng hợp từ logs 7 ngày** và đưa nhận xét
5. Nhập câu hỏi 2:
   > "Tôi muốn giảm 3kg trong 1 tháng, có khả thi không?"
6. AI phân tích dựa trên:
   - Current weight từ profile
   - Activity level
   - Current nutrition habits
7. Nhập câu hỏi 3:
   > "Gợi ý bữa sáng healthy cho người giảm cân"
8. AI đưa gợi ý cụ thể với calories

**Điểm nhấn khi demo:**
> 💡 *"Đây là HIGHLIGHT của app: AI Chatbot hoàn toàn cá nhân hóa. Backend đã xây dựng một context aggregator, thu thập: Profile (height, weight, gender, DOB, activity level), Goals (current, target), Nutrition Logs 7 ngày, Workout Logs 7 ngày, thậm chí cả User Action Logs. Tất cả được truyền vào prompt của Groq AI. Kết quả: Mỗi câu trả lời đều dựa trên DỮ LIỆU THỰC của chính người dùng đó!"*

**Demo edge case:**
9. Thử hỏi AI ngoài lề:
   > "Thời tiết hôm nay thế nào?"
10. AI trả lời lịch sự và hướng về chủ đề sức khỏe

**Kết quả mong đợi:**
- ✅ Chat history load đúng
- ✅ AI trả lời với context cá nhân hóa
- ✅ Tổng hợp tuần dựa trên logs thực
- ✅ Xử lý edge case ngoài lề

---

### 3.8 Profile & Settings ⏱️ [14:30-15:00]

**Hành động:**
1. Nhấn vào **Avatar** trên Home (hoặc tab Profile)
2. **Profile Screen** hiển thị:
   - Avatar với icon camera để đổi
   - Email (read-only)
   - Thông tin: Name, DOB, Gender, Height, Weight, Activity Level
3. Demo chỉnh sửa nhẹ (nếu cần)
4. Nhấn **"Đăng xuất"**
5. Xác nhận → Quay về Welcome Screen

**Điểm nhấn khi demo:**
> 💡 *"Profile cho phép user cập nhật thông tin bất kỳ lúc nào. Mọi thay đổi về cân nặng, chiều cao sẽ ảnh hưởng đến tính toán BMI, BMR, TDEE và gợi ý của AI Chatbot."*

**Kết quả mong đợi:**
- ✅ Profile hiển thị đầy đủ thông tin
- ✅ Avatar upload hoạt động
- ✅ Logout thành công

---

## 📍 PHẦN 4: TỔNG KẾT (2 phút)

### 4.1 Recap Các Tính Năng ⏱️ [15:00-16:00]

**Trình bày:**

> 🎯 *"Vừa rồi chúng ta đã demo qua toàn bộ hệ thống HealthSync với 3 nền tảng:*
>
> **1. Web Admin:**
> - Dashboard tổng quan với KPI và biểu đồ
> - Quản lý người dùng: CRUD, phân quyền, khóa/mở khóa
> - Quản lý Content Library: Bài tập và Món ăn
>
> **2. Web User:**
> - Dashboard cá nhân với tiến độ mục tiêu
> - Quản lý Goals với biểu đồ tiến độ
> - Theo dõi Nutrition với search, filter, macros
> - Theo dõi Workout với thư viện bài tập
> - AI Chatbot modal cá nhân hóa
>
> **3. Mobile (Flutter):**
> - Native experience cho Android/iOS
> - Tất cả tính năng tương tự Web
> - AI Chatbot full-screen với context 100% từ user data
> - Gesture: Pull-to-refresh, swipe, smooth animations*"

---

### 4.2 Điểm Nổi Bật Kỹ Thuật ⏱️ [16:00-16:30]

**Trình bày:**

> 🔧 *"Về mặt kỹ thuật:*
> - **Backend**: .NET 8, Clean Architecture, CQRS với MediatR
> - **Frontend Web**: React + Vite + TypeScript + Tailwind CSS
> - **Mobile**: Flutter (Dart)
> - **Database**: PostgreSQL + Entity Framework Core
> - **AI**: Groq AI với Llama model
> - **Storage**: MinIO cho avatar/images
> - **Deployment**: Docker Compose, có thể scale horizontal
> - **Test Coverage**: Unit tests + Integration tests*"

---

### 4.3 Q&A ⏱️ [16:30-17:00]

**Script:**
> 📣 *"Đó là toàn bộ demo của dự án HealthSync. Mọi người có câu hỏi gì không ạ?"*

---

## ✅ CHECKLIST TRƯỚC KHI DEMO

### Chuẩn Bị Hệ Thống
- [ ] Backend đang chạy (`docker-compose up -d` hoặc `dotnet run`)
- [ ] Database đã có dữ liệu mẫu (DataSeeder đã chạy)
- [ ] Web frontend đang chạy (`npm run dev`)
- [ ] Mobile emulator/device sẵn sàng
- [ ] Internet ổn định (cho Groq AI)

### Kiểm Tra Tài Khoản
- [ ] `admin@healthsync.com` đăng nhập được
- [ ] `demo.web@healthsync.com` đăng nhập được, có dữ liệu
- [ ] `demo.mobile@healthsync.com` đăng nhập được, có dữ liệu

### Kiểm Tra Dữ Liệu Demo
- [ ] Dashboard Admin có số liệu
- [ ] User có ít nhất 1 mục tiêu đang active
- [ ] User có nutrition logs 7 ngày gần nhất
- [ ] User có workout logs 7 ngày gần nhất
- [ ] AI Chatbot trả lời nhanh (< 5 giây)

### Thiết Bị
- [ ] Laptop/PC chạy web demo
- [ ] Điện thoại/Tablet hoặc Emulator chạy mobile demo
- [ ] Màn hình lớn/Projector để trình chiếu
- [ ] Screen mirroring cho mobile (nếu cần)

---

## 🚨 XỬ LÝ SỰ CỐ

| Sự cố | Cách xử lý |
|-------|-----------|
| API không response | Check `docker-compose logs backend`, restart service |
| Login thất bại | Verify database có user, check password hash |
| AI Chatbot timeout | Check Groq API key, internet connection |
| Biểu đồ không hiển thị | Clear browser cache, check console errors |
| Mobile không kết nối API | Check API base URL trong config, firewall |
| Dữ liệu trống | Chạy lại DataSeeder: `dotnet run --project HealthSync.Presentation -- --seed` |

---

## 📝 GHI CHÚ THÊM

### Câu Hỏi Dự Kiến & Trả Lời

**Q: "AI có thể trả lời sai không?"**
> A: "AI có thể có sai sót như mọi AI khác. Tuy nhiên, vì sử dụng 100% context từ dữ liệu thực của user, độ chính xác cao hơn các chatbot generic. Hệ thống cũng có disclaimer: tư vấn AI không thay thế bác sĩ."

**Q: "Hệ thống có thể scale bao nhiêu user?"**
> A: "Với kiến trúc hiện tại sử dụng Docker, có thể scale horizontal bằng cách tăng replicas. Database PostgreSQL hỗ trợ connection pooling. Estimate: 10,000+ concurrent users với cấu hình phù hợp."

**Q: "Tại sao dùng Groq AI thay vì OpenAI?"**
> A: "Groq có inference speed nhanh hơn nhiều (7x so với GPT-4), chi phí thấp hơn, và model Llama 3 đủ tốt cho use case health advisor."

---

*Tài liệu này được tạo để hướng dẫn demo dự án HealthSync. Vui lòng điều chỉnh nội dung và thời gian phù hợp với yêu cầu thực tế.*
