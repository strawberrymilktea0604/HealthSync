import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
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

  // Update user profile
  Future<void> updateProfile({
    required String fullName,
    required DateTime dob,
    required String gender,
    required double heightCm,
    required double weightKg,
    required String activityLevel,
  }) async {
    // Get token from shared preferences
    final prefs = await SharedPreferences.getInstance();
    final userJson = prefs.getString('user');
    if (userJson == null) throw Exception('User not authenticated');

    final userData = jsonDecode(userJson);
    final token = userData['token'];

    final response = await http.put(
      Uri.parse('$baseUrl/user/profile'),
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
      body: jsonEncode({
        'fullName': fullName,
        'dob': dob.toIso8601String(),
        'gender': gender,
        'heightCm': heightCm,
        'weightKg': weightKg,
        'activityLevel': activityLevel,
      }),
    );

    if (response.statusCode != 200) {
      final error = jsonDecode(response.body);
      throw Exception(error['Error'] ?? 'Failed to update profile');
    }
  }

  // Set password for Google OAuth users
  Future<void> setPassword({
    required int userId,
    required String password,
  }) async {
    final response = await http.post(
      Uri.parse('$baseUrl/auth/set-password'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'userId': userId,
        'password': password,
      }),
    );

    if (response.statusCode != 200) {
      final error = jsonDecode(response.body);
      throw Exception(error['Error'] ?? 'Failed to set password');
    }
  }

  // Generic GET method
  Future<dynamic> get(String endpoint, {Map<String, dynamic>? queryParameters}) async {
    final prefs = await SharedPreferences.getInstance();
    final userJson = prefs.getString('user');
    String? token;
    
    if (userJson != null) {
      final userData = jsonDecode(userJson);
      token = userData['token'];
    }

    final uri = queryParameters != null
        ? Uri.parse('$baseUrl$endpoint').replace(queryParameters: queryParameters.map((key, value) => MapEntry(key, value.toString())))
        : Uri.parse('$baseUrl$endpoint');

    final headers = {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };

    final response = await http.get(uri, headers: headers);

    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    } else {
      final error = jsonDecode(response.body);
      throw Exception(error['Error'] ?? 'Request failed');
    }
  }

  // Generic POST method
  Future<dynamic> post(String endpoint, {Map<String, dynamic>? body}) async {
    final prefs = await SharedPreferences.getInstance();
    final userJson = prefs.getString('user');
    String? token;
    
    if (userJson != null) {
      final userData = jsonDecode(userJson);
      token = userData['token'];
    }

    final headers = {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };

    final response = await http.post(
      Uri.parse('$baseUrl$endpoint'),
      headers: headers,
      body: body != null ? jsonEncode(body) : null,
    );

    if (response.statusCode == 200 || response.statusCode == 201) {
      return jsonDecode(response.body);
    } else {
      final error = jsonDecode(response.body);
      throw Exception(error['Error'] ?? 'Request failed');
    }
  }

  // Generic DELETE method
  Future<void> delete(String endpoint) async {
    final prefs = await SharedPreferences.getInstance();
    final userJson = prefs.getString('user');
    String? token;
    
    if (userJson != null) {
      final userData = jsonDecode(userJson);
      token = userData['token'];
    }

    final headers = {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };

    final response = await http.delete(
      Uri.parse('$baseUrl$endpoint'),
      headers: headers,
    );

    if (response.statusCode != 200 && response.statusCode != 204) {
      final error = jsonDecode(response.body);
      throw Exception(error['Error'] ?? 'Delete request failed');
    }
  }
}
