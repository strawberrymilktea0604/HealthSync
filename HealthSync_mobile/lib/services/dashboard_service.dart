import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../models/dashboard_model.dart';
import '../helpers/navigation_helper.dart';
import 'network_service.dart';

class DashboardService {
  static const String baseUrl = 'http://10.0.2.2:8080/api';

  Future<String?> _getToken() async {
    final prefs = await SharedPreferences.getInstance();
    final userJson = prefs.getString('user');
    if (userJson == null) return null;
    try {
      final userData = jsonDecode(userJson);
      return userData['token'];
    } catch (e) {
      return null;
    }
  }

  Future<Map<String, String>> _getHeaders() async {
    final token = await _getToken();
    return {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
  }

  Future<CustomerDashboardDto> getCustomerDashboard() async {
    // Check network trước
    if (!(await NetworkService.isConnected())) {
      throw Exception('Không có kết nối Internet. Vui lòng kiểm tra lại đường truyền.');
    }

    try {
      final headers = await _getHeaders();
      final response = await http.get(
        Uri.parse('$baseUrl/Dashboard/customer'),
        headers: headers,
      );
      
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return CustomerDashboardDto.fromJson(data);
      } else if (response.statusCode == 401 || response.statusCode == 403) {
        await handleAuthError();
        throw Exception('Phiên đăng nhập hết hạn hoặc tài khoản bị khóa');
      } else {
        throw Exception(
            'Không thể tải dashboard: ${response.statusCode} - ${response.body}');
      }
    } catch (e) {
      // Better error handling for network errors
      if (e.toString().contains('SocketException') || 
          e.toString().contains('Connection refused')) {
        throw Exception('Không thể kết nối đến server. Vui lòng đảm bảo backend đang chạy (docker-compose up).');
      }
      rethrow;
    }
  }
}
