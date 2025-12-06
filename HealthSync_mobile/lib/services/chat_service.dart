import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/chat_message.dart';
import '../helpers/api_config.dart';
import '../helpers/auth_helper.dart';

class ChatService {
  static const String baseUrl = ApiConfig.baseUrl;

  Future<ChatResponse> sendMessage(String question) async {
    try {
      final token = await AuthHelper.getToken();
      if (token == null) {
        throw Exception('Authentication required');
      }

      final response = await http.post(
        Uri.parse('$baseUrl/api/Chat/ask'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
        body: jsonEncode({'question': question}),
      );

      if (response.statusCode == 200) {
        return ChatResponse.fromJson(jsonDecode(response.body));
      } else if (response.statusCode == 401) {
        throw Exception('Phiên đăng nhập đã hết hạn');
      } else {
        throw Exception('Không thể kết nối với AI. Vui lòng thử lại.');
      }
    } catch (e) {
      throw Exception('Lỗi: ${e.toString()}');
    }
  }

  Future<List<ChatMessage>> getChatHistory({int pageSize = 20, int pageNumber = 1}) async {
    try {
      final token = await AuthHelper.getToken();
      if (token == null) {
        throw Exception('Authentication required');
      }

      final response = await http.get(
        Uri.parse('$baseUrl/api/Chat/history?pageSize=$pageSize&pageNumber=$pageNumber'),
        headers: {
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => ChatMessage.fromJson(json)).toList();
      } else {
        throw Exception('Không thể tải lịch sử chat');
      }
    } catch (e) {
      throw Exception('Lỗi: ${e.toString()}');
    }
  }
}
