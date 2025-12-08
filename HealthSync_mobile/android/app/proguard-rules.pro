# Nội dung file android/app/proguard-rules.pro

# Giữ lại các class của Flutter để không bị lỗi
-keep class io.flutter.app.** { *; }
-keep class io.flutter.plugin.** { *; }
-keep class io.flutter.util.** { *; }
-keep class io.flutter.view.** { *; }
-keep class io.flutter.** { *; }
-keep class io.flutter.plugins.** { *; }

# Nếu bạn dùng thư viện Google Maps, Firebase... có thể cần thêm luật riêng của họ vào đây.