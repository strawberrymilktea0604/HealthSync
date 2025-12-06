import 'package:flutter/material.dart';
import '../services/dashboard_service.dart';
import '../models/dashboard_model.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final DashboardService _dashboardService = DashboardService();
  CustomerDashboard? _dashboard;
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadDashboard();
  }

  Future<void> _loadDashboard() async {
    try {
      setState(() {
        _isLoading = true;
        _error = null;
      });
      
      final dashboard = await _dashboardService.getCustomerDashboard();
      
      setState(() {
        _dashboard = dashboard;
        _isLoading = false;
      });
    } catch (e) {
      setState(() {
        _error = e.toString();
        _isLoading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Scaffold(
        backgroundColor: Color(0xFFD9D7B6),
        body: Center(
          child: CircularProgressIndicator(),
        ),
      );
    }

    if (_error != null) {
      return Scaffold(
        backgroundColor: const Color(0xFFD9D7B6),
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text('Lỗi: $_error'),
              ElevatedButton(
                onPressed: _loadDashboard,
                child: const Text('Thử lại'),
              ),
            ],
          ),
        ),
      );
    }

    if (_dashboard == null) {
      return const Scaffold(
        backgroundColor: Color(0xFFD9D7B6),
        body: Center(
          child: Text('Không có dữ liệu'),
        ),
      );
    }

    return Scaffold(
      backgroundColor: const Color(0xFFD9D7B6),
      body: SafeArea(
        child: RefreshIndicator(
          onRefresh: _loadDashboard,
          child: SingleChildScrollView(
            physics: const AlwaysScrollableScrollPhysics(),
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Header
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Row(
                        children: [
                          CircleAvatar(
                            radius: 24,
                            backgroundColor: Colors.grey[300],
                            backgroundImage: _dashboard!.userInfo.avatarUrl.isNotEmpty
                                ? NetworkImage(_dashboard!.userInfo.avatarUrl)
                                : null,
                            child: _dashboard!.userInfo.avatarUrl.isEmpty
                                ? const Icon(Icons.person, color: Colors.grey)
                                : null,
                          ),
                          const SizedBox(width: 12),
                          Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const Text(
                                'Chào buổi sáng, Mạnh!',
                                style: TextStyle(
                                  fontFamily: 'Estedad-VF',
                                  fontSize: 20,
                                  fontWeight: FontWeight.w700,
                                  color: Colors.black,
                                  height: 1.758,
                                ),
                              ),
                              Text(
                                _dashboard!.userInfo.fullName,
                                style: TextStyle(
                                  fontFamily: 'Estedad-VF',
                                  fontSize: 14,
                                  fontWeight: FontWeight.w400,
                                  color: Colors.grey[600],
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                      IconButton(
                        icon: const Icon(Icons.notifications_outlined),
                        onPressed: () {},
                      ),
                    ],
                  ),
                  
                  const SizedBox(height: 24),
                  
                  // Goal Progress Card
                  if (_dashboard!.goalProgress != null)
                    _buildGoalCard(_dashboard!.goalProgress!, _dashboard!.weightProgress),
                  
                  const SizedBox(height: 16),
                  
                  // Stats Grid
                  Row(
                    children: [
                      Expanded(
                        child: _buildStatCard(
                          'Chuỗi ngày',
                          '${_dashboard!.exerciseStreak.currentStreak} ngày',
                          Icons.calendar_today,
                          const Color(0xFFC5C292),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: _buildStatCard(
                          'Tập luyện tuần này',
                          _dashboard!.todayStats.workoutDuration.isNotEmpty 
                              ? _dashboard!.todayStats.workoutDuration 
                              : '0m',
                          Icons.fitness_center,
                          const Color(0xFFC5C292),
                        ),
                      ),
                    ],
                  ),
                  
                  const SizedBox(height: 12),
                  
                  Row(
                    children: [
                      Expanded(
                        child: _buildStatCard(
                          'Calo hôm nay',
                          '${_dashboard!.todayStats.caloriesConsumed}/${_dashboard!.todayStats.caloriesTarget}',
                          Icons.local_fire_department,
                          const Color(0xFFC5C292),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: _buildStatCard(
                          'Tần độ giảm cân',
                          '${_dashboard!.weightProgress?.currentWeight.toStringAsFixed(1) ?? '0'} kg',
                          Icons.trending_down,
                          const Color(0xFFC5C292),
                          subtitle: _dashboard!.weightProgress != null 
                              ? 'Tháng này -${_dashboard!.weightProgress!.weightLost.toStringAsFixed(1)}kg'
                              : null,
                        ),
                      ),
                    ],
                  ),
                  
                  const SizedBox(height: 24),
                  
                  // Quick Actions
                  Row(
                    children: [
                      Expanded(
                        child: _buildActionButton(
                          'Ghi bữa ăn',
                          Icons.restaurant,
                          () {
                            Navigator.pushNamed(context, '/nutrition');
                          },
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: _buildActionButton(
                          'Ghi buổi tập',
                          Icons.fitness_center,
                          () {
                            Navigator.pushNamed(context, '/workout');
                          },
                        ),
                      ),
                    ],
                  ),
                  
                  const SizedBox(height: 40),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildGoalCard(GoalProgress goalProgress, WeightProgress? weightProgress) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: const Color(0xFFC5C292),
        borderRadius: BorderRadius.circular(32),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.25),
            blurRadius: 4,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Text(
                'Mục tiêu giảm cân',
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.w600,
                ),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Text(
                  'Tiến độ ${weightProgress?.progressPercentage.toStringAsFixed(0) ?? '0'}%',
                  style: const TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 4),
          Text(
            'Đã hoàn thành ${weightProgress?.progressPercentage.toStringAsFixed(0) ?? '0'}%',
            style: TextStyle(
              fontSize: 12,
              color: Colors.grey[700],
            ),
          ),
          const SizedBox(height: 12),
          LinearProgressIndicator(
            value: (weightProgress?.progressPercentage ?? 0) / 100,
            backgroundColor: Colors.grey[300],
            valueColor: const AlwaysStoppedAnimation<Color>(
              Color(0xFF8BA655),
            ),
            minHeight: 8,
            borderRadius: BorderRadius.circular(4),
          ),
          const SizedBox(height: 16),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '${goalProgress.currentValue.toStringAsFixed(1)}kg',
                    style: const TextStyle(
                      fontSize: 32,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  Text(
                    'Mục tiêu: ${goalProgress.targetValue.toStringAsFixed(1)}kg',
                    style: TextStyle(
                      fontSize: 12,
                      color: Colors.grey[700],
                    ),
                  ),
                ],
              ),
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  const Text(
                    'Giảm',
                    style: TextStyle(
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                      color: Color(0xFF8BA655),
                    ),
                  ),
                  Text(
                    '${goalProgress.progress.toStringAsFixed(1)}kg',
                    style: const TextStyle(
                      fontSize: 24,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  Text(
                    'còn ${goalProgress.remaining.toStringAsFixed(1)}kg nữa',
                    style: TextStyle(
                      fontSize: 12,
                      color: Colors.grey[700],
                    ),
                  ),
                ],
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildStatCard(String title, String value, IconData icon, Color color, {String? subtitle}) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: color,
        borderRadius: BorderRadius.circular(32),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.25),
            blurRadius: 4,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, size: 28, color: const Color(0xFF8BA655)),
          const SizedBox(height: 8),
          Text(
            title,
            style: TextStyle(
              fontSize: 12,
              color: Colors.grey[700],
            ),
          ),
          const SizedBox(height: 4),
          Text(
            value,
            style: const TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.bold,
            ),
          ),
          if (subtitle != null)
            Padding(
              padding: const EdgeInsets.only(top: 4),
              child: Text(
                subtitle,
                style: TextStyle(
                  fontSize: 10,
                  color: Colors.grey[600],
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildActionButton(String label, IconData icon, VoidCallback onPressed) {
    return ElevatedButton(
      onPressed: onPressed,
      style: ElevatedButton.styleFrom(
        backgroundColor: const Color(0xFFFDFBD4),
        foregroundColor: Colors.black,
        padding: const EdgeInsets.symmetric(vertical: 20),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(20),
        ),
        elevation: 2,
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(icon, size: 24),
          const SizedBox(width: 8),
          Text(
            label,
            style: const TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }
}
