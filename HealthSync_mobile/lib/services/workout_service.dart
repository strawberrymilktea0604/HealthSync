import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../models/workout_model.dart';
import 'network_service.dart';

class WorkoutService {
  static const String baseUrl = 'http://10.0.2.2:5274/api';

  Future<String?> _getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('token');
  }

  Future<Map<String, String>> _getHeaders() async {
    final token = await _getToken();
    return {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
  }

  // Lấy danh sách bài tập
  Future<List<Exercise>> getExercises({
    String? muscleGroup,
    String? difficulty,
    String? search,
  }) async {
    // Check mạng trước khi gọi API
    if (!(await NetworkService.isConnected())) {
      throw Exception('Không có kết nối Internet. Vui lòng kiểm tra lại đường truyền.');
    }

    try {
      final queryParams = <String, String>{};
      if (muscleGroup != null) queryParams['muscleGroup'] = muscleGroup;
      if (difficulty != null) queryParams['difficulty'] = difficulty;
      if (search != null) queryParams['search'] = search;

      final uri = Uri.parse('$baseUrl/workout/exercises')
          .replace(queryParameters: queryParams.isNotEmpty ? queryParams : null);

      final response = await http.get(uri, headers: await _getHeaders());

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => Exercise.fromJson(json)).toList();
      } else {
        throw Exception('Không thể tải danh sách bài tập');
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

  // Lấy lịch sử luyện tập
  Future<List<WorkoutLog>> getWorkoutLogs({
    DateTime? startDate,
    DateTime? endDate,
  }) async {
    final queryParams = <String, String>{};
    if (startDate != null) {
      queryParams['startDate'] = startDate.toIso8601String();
    }
    if (endDate != null) {
      queryParams['endDate'] = endDate.toIso8601String();
    }

    final uri = Uri.parse('$baseUrl/workout/workout-logs')
        .replace(queryParameters: queryParams.isNotEmpty ? queryParams : null);

    final response = await http.get(uri, headers: await _getHeaders());

    if (response.statusCode == 200) {
      final List<dynamic> data = jsonDecode(response.body);
      return data.map((json) => WorkoutLog.fromJson(json)).toList();
    } else {
      throw Exception('Không thể tải lịch sử luyện tập');
    }
  }

  // Tạo nhật ký luyện tập mới
  Future<int> createWorkoutLog(CreateWorkoutLog workoutLog) async {
    final response = await http.post(
      Uri.parse('$baseUrl/workout/workout-logs'),
      headers: await _getHeaders(),
      body: jsonEncode(workoutLog.toJson()),
    );

    if (response.statusCode == 201) {
      final data = jsonDecode(response.body);
      return data['workoutLogId'];
    } else {
      throw Exception('Không thể lưu nhật ký luyện tập');
    }
  }
}
