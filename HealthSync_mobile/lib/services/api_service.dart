import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/user_model.dart';

class ApiService {
  // Sử dụng 10.0.2.2 cho Android emulator để trỏ đến localhost của máy host
  static const String baseUrl = 'http://10.0.2.2:5274/api';
  
  // Đăng nhập
  Future<User> login(String email, String password) async {
    final response = await http.post(
      Uri.parse('$baseUrl/auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'email': email,
        'password': password,
      }),
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return User.fromJson(data);
    } else {
      final error = jsonDecode(response.body);
      throw Exception(error['Error'] ?? 'Login failed');
    }
  }

  // Gửi mã xác thực
  Future<void> sendVerificationCode(String email) async {
    final response = await http.post(
      Uri.parse('$baseUrl/auth/send-verification-code'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'email': email}),
    );

    if (response.statusCode != 200) {
      final error = jsonDecode(response.body);
      throw Exception(error['Error'] ?? 'Failed to send verification code');
    }
  }

  // Đăng ký
  Future<User> register({
    required String email,
    required String password,
    required String verificationCode,
  }) async {
    final response = await http.post(
      Uri.parse('$baseUrl/auth/register'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'email': email,
        'password': password,
        'verificationCode': verificationCode,
      }),
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return User.fromJson(data);
    } else {
      final error = jsonDecode(response.body);
      throw Exception(error['Error'] ?? 'Registration failed');
    }
  }

  // Google Login Mobile
  Future<User> googleLoginMobile(String idToken) async {
    final response = await http.post(
      Uri.parse('$baseUrl/auth/google/mobile'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'idToken': idToken,
      }),
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return User.fromJson(data);
    } else {
      final error = jsonDecode(response.body);
      throw Exception(error['Error'] ?? 'Google login failed');
    }
  }

  // Get Google Android Client ID from backend
  Future<String> getGoogleAndroidClientId() async {
    final response = await http.get(
      Uri.parse('$baseUrl/auth/google/android-client-id'),
      headers: {'Content-Type': 'application/json'},
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return data['ClientId'] ?? data['clientId'] ?? '';
    } else {
      throw Exception('Failed to get Android Client ID');
    }
  }
}
