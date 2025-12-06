import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../models/dashboard_model.dart';

class DashboardService {
  static const String baseUrl = 'http://10.0.2.2:5274/api';

  Future<CustomerDashboard> getCustomerDashboard() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('token');

    if (token == null) {
      throw Exception('No authentication token found');
    }

    final response = await http.get(
      Uri.parse('$baseUrl/dashboard/customer'),
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      },
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return CustomerDashboard.fromJson(data);
    } else {
      final error = jsonDecode(response.body);
      throw Exception(error['message'] ?? 'Failed to load dashboard');
    }
  }
}
