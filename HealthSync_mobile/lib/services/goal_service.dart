import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../models/goal_model.dart';
import '../helpers/navigation_helper.dart';

class GoalService {
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

  // Lấy danh sách mục tiêu
  Future<List<Goal>> getGoals() async {
    try {
      final headers = await _getHeaders();
      
      final response = await http.get(
        Uri.parse('$baseUrl/Goals'),
        headers: headers,
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        
        if (data is Map && data.containsKey('goals')) {
          final goalsJson = data['goals'] as List<dynamic>;
          final goals = goalsJson
              .map((json) => Goal.fromJson(json as Map<String, dynamic>))
              .toList();
          return goals;
        } else {
          return [];
        }
      } else if (response.statusCode == 401 || response.statusCode == 403) {
        await handleAuthError();
        throw Exception('Phiên đăng nhập hết hạn hoặc tài khoản bị khóa');
      } else {
        throw Exception('Failed to load goals: ${response.statusCode} - ${response.body}');
      }
    } catch (e) {
      rethrow;
    }
  }

  // Tạo mục tiêu mới
  Future<Goal> createGoal(CreateGoalRequest request) async {
    final headers = await _getHeaders();
    final response = await http.post(
      Uri.parse('$baseUrl/Goals'),
      headers: headers,
      body: jsonEncode(request.toJson()),
    );

    if (response.statusCode == 201 || response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return Goal.fromJson(data);
    } else if (response.statusCode == 401 || response.statusCode == 403) {
      await handleAuthError();
      throw Exception('Phiên đăng nhập hết hạn hoặc tài khoản bị khóa');
    } else {
      final error = jsonDecode(response.body);
      throw Exception(error['Error'] ?? 'Failed to create goal');
    }
  }

  // Thêm tiến độ cho mục tiêu
  Future<ProgressRecord> addProgress(int goalId, AddProgressRequest request) async {
    final headers = await _getHeaders();
    final response = await http.post(
      Uri.parse('$baseUrl/Goals/$goalId/progress'),
      headers: headers,
      body: jsonEncode(request.toJson()),
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return ProgressRecord.fromJson(data['progressRecord']);
    } else if (response.statusCode == 401 || response.statusCode == 403) {
      await handleAuthError();
      throw Exception('Phiên đăng nhập hết hạn hoặc tài khoản bị khóa');
    } else {
      final error = jsonDecode(response.body);
      throw Exception(error['Error'] ?? 'Failed to add progress');
    }
  }

  // Lấy chi tiết mục tiêu
  Future<Goal> getGoalById(int goalId) async {
    final goals = await getGoals();
    try {
      return goals.firstWhere((g) => g.goalId == goalId);
    } catch (e) {
      throw Exception('Goal not found');
    }
  }
}
