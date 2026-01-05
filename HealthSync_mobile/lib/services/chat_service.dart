import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../models/chat_message.dart';
import 'network_service.dart';
import '../helpers/navigation_helper.dart';

class ChatService {
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

  Future<ChatResponse> sendMessage(String question) async {
    // Check mạng trước khi gọi API
    if (!(await NetworkService.isConnected())) {
      throw Exception('Không có kết nối Internet. Vui lòng kiểm tra lại đường truyền.');
    }

    try {
      final token = await _getToken();
      if (token == null) {
        throw Exception('Authentication required');
      }

      final response = await http.post(
        Uri.parse('$baseUrl/Chat/ask'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
        body: jsonEncode({'question': question}),
      );

      if (response.statusCode == 200) {
        return ChatResponse.fromJson(jsonDecode(response.body));
      } else if (response.statusCode == 401 || response.statusCode == 403) {
        await handleAuthError();
        throw Exception('Phiên đăng nhập hết hạn hoặc tài khoản bị khóa');
      } else {
        throw Exception('Không thể kết nối với AI. Vui lòng thử lại.');
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

  Future<List<ChatMessage>> getChatHistory({int pageSize = 20, int pageNumber = 1}) async {
    // Check mạng trước khi gọi API
    if (!(await NetworkService.isConnected())) {
      throw Exception('Không có kết nối Internet. Vui lòng kiểm tra lại đường truyền.');
    }

    try {
      final token = await _getToken();
      if (token == null) {
        throw Exception('Authentication required');
      }

      final response = await http.get(
        Uri.parse('$baseUrl/Chat/history?pageSize=$pageSize&pageNumber=$pageNumber'),
        headers: {
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => ChatMessage.fromJson(json)).toList();
      } else if (response.statusCode == 401 || response.statusCode == 403) {
        await handleAuthError();
        throw Exception('Phiên đăng nhập hết hạn hoặc tài khoản bị khóa');
      } else {
        throw Exception('Không thể tải lịch sử chat');
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
}
