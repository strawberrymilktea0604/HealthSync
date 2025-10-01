# Kế hoạch triển khai Giao diện Đăng nhập/Đăng ký

Đây là kế hoạch chi tiết để `code` mode có thể thực hiện.

## Bước 1: Cấu hình và Chuẩn bị tài nguyên

### 1. Cấu trúc thư mục

Tạo cấu trúc thư mục sau trong `HealthSync_mobile/lib/`:

```
lib/
|-- assets/
|   |-- fonts/
|   |-- images/
|   |-- icons/
|-- screens/
|   |-- auth/
|   |   |-- login_screen.dart
|   |   |-- signup_screen.dart
|   |   |-- forgot_password_screen.dart
|   |   |-- email_confirmation_screen.dart
|-- utils/
|   |-- colors.dart
|   |-- app_styles.dart
|-- widgets/
|   |-- custom_button.dart
|   |-- custom_textfield.dart
|-- main.dart
```

### 2. Thêm tài nguyên (Assets)

1.  **Fonts**:
    *   Tạo thư mục `HealthSync_mobile/assets/fonts`.
    *   Thêm các file font `Eras Bold ITC` và `Estedad-VF` vào thư mục này (chờ người dùng cung cấp).
2.  **Images & Icons**:
    *   Tạo thư mục `HealthSync_mobile/assets/images` và `HealthSync_mobile/assets/icons`.
    *   Thêm các hình ảnh và icon được trích xuất từ thiết kế vào các thư mục tương ứng (chờ người dùng cung cấp).
3.  **Cập nhật `pubspec.yaml`**:
    *   Khai báo thư mục `assets/` để Flutter có thể sử dụng.

    ```yaml
    flutter:
      uses-material-design: true
      assets:
        - assets/images/
        - assets/icons/
      fonts:
        - family: ErasBoldITC
          fonts:
            - asset: assets/fonts/Eras-Bold-ITC.ttf # (Tên file có thể khác)
        - family: EstedadVF
          fonts:
            - asset: assets/fonts/Estedad-VF.ttf # (Tên file có thể khác)
    ```

### 3. Định nghĩa Bảng màu và Styles

1.  **`lib/utils/colors.dart`**: Tạo file để định nghĩa các màu sắc sử dụng trong ứng dụng.

    ```dart
    import 'package:flutter/material.dart';

    class AppColors {
      static const Color background = Color(0xFFF3F0D8); // Màu nền chính
      static const Color primaryText = Color(0xFF000000); // Màu chữ chính
      static const Color cardBackground = Color(0xFFFFFDE7); // Màu nền của card
      static const Color buttonPrimary = Color(0xFF707070); // Màu nút chính (Sign in)
      static const Color buttonSecondary = Color(0xFFFFFFFF); // Màu nút phụ (Sign up)
      static const Color textFieldBackground = Color(0xFFE0E0E0); // Màu nền text field
    }
    ```
    *(Lưu ý: Mã màu ở trên là ước tính, cần được điều chỉnh cho chính xác với thiết kế)*

2.  **`lib/utils/app_styles.dart`**: Tạo file để định nghĩa các `TextStyle` tái sử dụng.

    ```dart
    import 'package:flutter/material.dart';
    import 'colors.dart';

    class AppStyles {
      static const TextStyle healthsyncTitle = TextStyle(
        fontFamily: 'ErasBoldITC',
        fontSize: 36, // Cần điều chỉnh
        color: AppColors.primaryText,
      );

      static const TextStyle mainHeading = TextStyle(
        fontFamily: 'EstedadVF',
        fontSize: 28, // Cần điều chỉnh
        fontWeight: FontWeight.bold,
        color: AppColors.primaryText,
      );

      // Thêm các style khác ở đây...
    }
    ```

## Các bước tiếp theo

Sau khi hoàn thành Bước 1, `code` mode sẽ tiếp tục với:
*   **Bước 2**: Xây dựng các widget tái sử dụng (`custom_button.dart`, `custom_textfield.dart`).
*   **Bước 3**: Dựng giao diện cho từng màn hình.
*   **Bước 4**: Thiết lập navigation.
*   **Bước 5**: Hoàn thiện.