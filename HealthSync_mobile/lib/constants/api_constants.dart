class ApiConstants {
  // Base URL cho API
  // Sử dụng 10.0.2.2 cho Android emulator để trỏ đến localhost của máy host
  // Sử dụng localhost hoặc 127.0.0.1 cho iOS simulator
  // Sử dụng IP thực của máy cho physical device
  static const String baseUrl = 'http://10.0.2.2:5274/api';
  
  // Endpoints
  static const String login = '$baseUrl/auth/login';
  static const String register = '$baseUrl/auth/register';
  static const String sendVerificationCode = '$baseUrl/auth/send-verification-code';
  static const String googleLoginMobile = '$baseUrl/auth/google-login-mobile';
}
