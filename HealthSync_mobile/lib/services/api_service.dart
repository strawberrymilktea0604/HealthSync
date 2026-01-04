import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:http_parser/http_parser.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../models/user_model.dart';
import 'network_service.dart';

class ApiService {
  // Sử dụng 10.0.2.2 cho Android emulator để trỏ đến localhost của máy host
  // Port 8080 cho nginx
  static const String baseUrl = 'http://10.0.2.2:8080/api';
  
  // Đăng nhập
  Future<User> login(String email, String password) async {
    // Check mạng trước khi gọi API
    if (!(await NetworkService.isConnected())) {
      throw Exception('Không có kết nối Internet. Vui lòng kiểm tra lại đường truyền.');
    }

    try {
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
    } catch (e) {
      // Nếu là lỗi mạng, throw thông báo cụ thể
      if (e.toString().contains('SocketException') || e.toString().contains('Connection')) {
        throw Exception('Không thể kết nối đến server. Vui lòng thử lại sau.');
      }
      // Nếu là lỗi khác, re-throw
      rethrow;
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

  // Quên mật khẩu - Gửi mã xác thực cho email đã tồn tại
  Future<void> forgotPassword(String email) async {
    final response = await http.post(
      Uri.parse('$baseUrl/auth/forgot-password'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'email': email}),
    );

    if (response.statusCode != 200) {
      if (response.body.isNotEmpty) {
        try {
          final error = jsonDecode(response.body);
          throw Exception(error['Error'] ?? 'Failed to send recovery code');
        } catch (_) {
          throw Exception('Failed to send recovery code (server error)');
        }
      }
      throw Exception('Failed to send recovery code (${response.statusCode})');
    }
  }

  // Xác thực OTP khôi phục mật khẩu
  Future<String> verifyResetOtp(String email, String otp) async {
    final response = await http.post(
      Uri.parse('$baseUrl/auth/verify-reset-otp'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'email': email,
        'otp': otp, 
      }),
    );

    if (response.statusCode != 200) {
      if (response.body.isNotEmpty) {
        try {
          final error = jsonDecode(response.body);
          throw Exception(error['Error'] ?? 'Invalid verification code');
        } catch (_) {
          throw Exception('Invalid verification code (server error)');
        }
      }
      throw Exception('Invalid verification code (${response.statusCode})');
    }

    final data = jsonDecode(response.body);
    return data['token'] ?? data['Token'] ?? '';
  }

  // Đăng ký
  Future<User> register({
    required String email,
    required String password,
    required String verificationCode,
  }) async {
    // Check mạng trước khi gọi API
    if (!(await NetworkService.isConnected())) {
      throw Exception('Không có kết nối Internet. Vui lòng kiểm tra lại đường truyền.');
    }

    try {
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
    } catch (e) {
      // Nếu là lỗi mạng, throw thông báo cụ thể
      if (e.toString().contains('SocketException') || e.toString().contains('Connection')) {
        throw Exception('Không thể kết nối đến server. Vui lòng thử lại sau.');
      }
      // Nếu là lỗi khác, re-throw
      rethrow;
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

  // Get user profile
  Future<Map<String, dynamic>> getProfile() async {
    final prefs = await SharedPreferences.getInstance();
    final userJson = prefs.getString('user');
    if (userJson == null) throw Exception('User not authenticated');

    final userData = jsonDecode(userJson);
    final token = userData['token'];

    final response = await http.get(
      Uri.parse('$baseUrl/UserProfile'),
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
    );

    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    } else {
       if (response.body.isNotEmpty) {
        try {
          final error = jsonDecode(response.body);
          throw Exception(error['Error'] ?? 'Failed to load profile');
        } catch (_) {
          throw Exception('Failed to load profile (server error)');
        }
      }
      throw Exception('Failed to load profile (${response.statusCode})');
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
      Uri.parse('$baseUrl/UserProfile'),
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
      if (response.body.isNotEmpty) {
        try {
          final error = jsonDecode(response.body);
          throw Exception(error['Error'] ?? 'Failed to update profile');
        } catch (_) {
          throw Exception('Failed to update profile (server error)');
        }
      }
      throw Exception('Failed to update profile (${response.statusCode})');
    }
  }

  // Upload Avatar
  Future<String> uploadAvatar(File file) async {
    final prefs = await SharedPreferences.getInstance();
    final userJson = prefs.getString('user');
    if (userJson == null) throw Exception('User not authenticated');

    final userData = jsonDecode(userJson);
    final token = userData['token'];

    var request = http.MultipartRequest(
      'POST',
      Uri.parse('$baseUrl/UserProfile/upload-avatar'),
    );

    request.headers['Authorization'] = 'Bearer $token';
    
    // Determine content type
    MediaType? contentType;
    final path = file.path.toLowerCase();
    if (path.endsWith('.jpg') || path.endsWith('.jpeg')) {
      contentType = MediaType('image', 'jpeg');
    } else if (path.endsWith('.png')) {
      contentType = MediaType('image', 'png');
    } else if (path.endsWith('.gif')) {
      contentType = MediaType('image', 'gif');
    }

    request.files.add(await http.MultipartFile.fromPath(
      'File', 
      file.path,
      contentType: contentType,
    ));

    final streamedResponse = await request.send();
    final response = await http.Response.fromStream(streamedResponse);

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      String avatarUrl = data['AvatarUrl'] ?? data['avatarUrl'] ?? '';
      if (avatarUrl.contains('localhost')) {
        avatarUrl = avatarUrl.replaceFirst('localhost', '10.0.2.2');
      }
      return avatarUrl;
    } else {
      if (response.body.isNotEmpty) {
        try {
          final error = jsonDecode(response.body);
          throw Exception(error['Error'] ?? error['message'] ?? 'Failed to upload avatar: ${response.body}');
        } catch (_) {
           // Return the raw response body as error message if json decode fails
           throw Exception('Failed to upload avatar (${response.statusCode}): ${response.body}');
        }
      }
      throw Exception('Failed to upload avatar (${response.statusCode})');
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

  // Reset password
  Future<void> resetPassword({
    required String token,
    required String newPassword,
  }) async {
    final response = await http.post(
      Uri.parse('$baseUrl/auth/reset-password'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'token': token,
        'newPassword': newPassword,
      }),
    );

    if (response.statusCode != 200) {
      if (response.body.isNotEmpty) {
        try {
          final error = jsonDecode(response.body);
          throw Exception(error['Error'] ?? 'Failed to reset password');
        } catch (_) {
          throw Exception('Failed to reset password (server error)');
        }
      }
      throw Exception('Failed to reset password (${response.statusCode})');
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
