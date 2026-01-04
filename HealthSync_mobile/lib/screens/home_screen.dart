import 'package:flutter/material.dart';
import '../services/dashboard_service.dart';
import '../models/dashboard_model.dart';
import '../services/goal_service.dart';
import '../models/goal_model.dart';
import 'profile_screen.dart';
import 'workout_history_screen.dart';
import 'nutrition_screen.dart';
import 'goals_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final DashboardService _dashboardService = DashboardService();
  CustomerDashboardDto? _dashboard;
  bool _isLoading = true;
  String? _error;
  int _currentIndex = 0;

  late List<Widget> _tabs;

  // State for goal selection
  int? _selectedGoalId;
  final Map<int, GoalProgressDto> _cachedGoalProgress = {};
  bool _isGoalLoading = false;
  final GoalService _goalService = GoalService();

  @override
  void initState() {
    super.initState();
    _loadDashboard();
    _tabs = [
      const SizedBox(), // Placeholder for Overview
      const WorkoutHistoryScreen(),
      const NutritionScreen(),
      const GoalsScreen(),
    ];
  }

  Future<void> _loadDashboard() async {
    try {
      setState(() {
        _isLoading = true;
        _error = null;
      });

      final dashboard = await _dashboardService.getCustomerDashboard();

      if (mounted) {
        setState(() {
          _dashboard = dashboard;
          _isLoading = false;

          // Initialize selected goal
          if (dashboard.activeGoals.isNotEmpty) {
            _selectedGoalId = dashboard.activeGoals.first.goalId;
            _cachedGoalProgress[_selectedGoalId!] = dashboard.goalProgress;
          }
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _error = e.toString();
          _isLoading = false;
        });
        
        // Nếu là lỗi 403, hiển thị dialog đề nghị đăng xuất
        if (e.toString().contains('403') || e.toString().contains('không có quyền')) {
          _showPermissionError();
        }
      }
    }
  }

  void _showPermissionError() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Lỗi quyền truy cập'),
        content: const Text(
          'Tài khoản của bạn không có quyền truy cập dashboard. '
          'Điều này có thể do:\n'
          '1. Token đăng nhập đã hết hạn\n'
          '2. Quyền chưa được cấp đúng\n\n'
          'Vui lòng đăng xuất và đăng nhập lại để cập nhật quyền.'
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Đóng'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(context);
              // Navigate to profile to logout
              setState(() => _currentIndex = 0);
              Navigator.pushReplacement(
                context,
                MaterialPageRoute(builder: (context) => const ProfileScreen()),
              );
            },
            child: const Text('Đến Trang Cá Nhân'),
          ),
        ],
      ),
    );
  }

  Future<void> _onGoalSelected(int? goalId) async {
    if (goalId == null || goalId == _selectedGoalId) return;

    setState(() {
      _selectedGoalId = goalId;
    });

    if (!_cachedGoalProgress.containsKey(goalId)) {
      await _fetchGoalDetails(goalId);
    }
  }

  Future<void> _fetchGoalDetails(int goalId) async {
    setState(() {
      _isGoalLoading = true;
    });

    try {
      final goal = await _goalService.getGoalById(goalId);
      final progressDto = _calculateGoalProgress(goal);

      if (mounted) {
        setState(() {
          _cachedGoalProgress[goalId] = progressDto;
          _isGoalLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _isGoalLoading = false;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Không thể tải chi tiết mục tiêu: $e')),
        );
      }
    }
  }

  GoalProgressDto _calculateGoalProgress(Goal goal) {
    final records = goal.progressRecords;
    if (records != null) {
      records.sort((a, b) => a.recordDate.compareTo(b.recordDate));
    }
    
    final firstRecord = (records != null && records.isNotEmpty) ? records.first : null;
    final lastRecord = (records != null && records.isNotEmpty) ? records.last : null;

    double startValue = firstRecord?.weightKg ?? firstRecord?.value ?? 0.0;

    if (records == null || records.isEmpty) {
      startValue = goal.targetValue;
    }

    double currentValue = lastRecord?.weightKg ?? lastRecord?.value ?? startValue;
    double targetValue = goal.targetValue;

    double progressAmount = 0;
    double remaining = 0;
    double progressPercentage = 0;

    bool isDecreaseGoal = goal.type.toLowerCase().contains('loss') ||
        goal.type.toLowerCase().contains('giảm') ||
        targetValue < startValue;

    if (isDecreaseGoal) {
      progressAmount = startValue - currentValue;
      remaining = currentValue - targetValue;
      double totalChangeNeeded = startValue - targetValue;
      if (totalChangeNeeded > 0) {
        progressPercentage = (progressAmount / totalChangeNeeded) * 100;
      }
    } else {
      progressAmount = currentValue - startValue;
      remaining = targetValue - currentValue;
      double totalChangeNeeded = targetValue - startValue;
      if (totalChangeNeeded > 0) {
        progressPercentage = (progressAmount / totalChangeNeeded) * 100;
      }
    }

    return GoalProgressDto(
      goalType: goal.type,
      startValue: startValue,
      currentValue: currentValue,
      targetValue: targetValue,
      progress: progressPercentage.clamp(0.0, 100.0),
      progressAmount: progressAmount,
      remaining: remaining,
      status: goal.status,
    );
  }

  @override
  Widget build(BuildContext context) {
    _tabs[0] = _buildOverviewTab();

    return Scaffold(
      backgroundColor: const Color(0xFFD9D7B6),
      body: IndexedStack(
        index: _currentIndex,
        children: _tabs,
      ),
      bottomNavigationBar: Container(
        decoration: BoxDecoration(
          color: const Color(0xFFFDFBD4),
          borderRadius: const BorderRadius.only(
            topLeft: Radius.circular(30),
            topRight: Radius.circular(30),
          ),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.1),
              blurRadius: 10,
              offset: const Offset(0, -5),
            ),
          ],
        ),
        child: ClipRRect(
          borderRadius: const BorderRadius.only(
            topLeft: Radius.circular(30),
            topRight: Radius.circular(30),
          ),
          child: BottomNavigationBar(
            currentIndex: _currentIndex,
            onTap: (index) {
              setState(() {
                _currentIndex = index;
                if (index == 0) {
                  _loadDashboard();
                }
              });
            },
            type: BottomNavigationBarType.fixed,
            backgroundColor: const Color(0xFFFDFBD4),
            selectedItemColor: const Color(0xFFA4C639),
            unselectedItemColor: Colors.grey,
            selectedLabelStyle:
                const TextStyle(fontWeight: FontWeight.bold, fontSize: 12),
            unselectedLabelStyle: const TextStyle(fontSize: 12),
            elevation: 0,
            items: const [
              BottomNavigationBarItem(
                icon: Icon(Icons.dashboard_outlined),
                activeIcon: Icon(Icons.dashboard),
                label: 'Tổng quan',
              ),
              BottomNavigationBarItem(
                icon: Icon(Icons.calendar_today_outlined),
                activeIcon: Icon(Icons.calendar_today),
                label: 'Tập luyện',
              ),
              BottomNavigationBarItem(
                icon: Icon(Icons.restaurant_outlined),
                activeIcon: Icon(Icons.restaurant),
                label: 'Dinh dưỡng',
              ),
              BottomNavigationBarItem(
                icon: Icon(Icons.flag_outlined),
                activeIcon: Icon(Icons.flag),
                label: 'Mục tiêu',
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildOverviewTab() {
    if (_isLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_error != null) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text('Lỗi: $_error'),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: _loadDashboard,
              child: const Text('Thử lại'),
            ),
          ],
        ),
      );
    }

    if (_dashboard == null) {
      return const Center(child: Text('Không có dữ liệu'));
    }

    return SafeArea(
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
                        GestureDetector(
                          onTap: () {
                            Navigator.push(
                              context,
                              MaterialPageRoute(
                                builder: (context) => const ProfileScreen(),
                              ),
                            ).then((_) => _loadDashboard());
                          },
                          child: CircleAvatar(
                            radius: 24,
                            backgroundColor: Colors.grey[300],
                            backgroundImage:
                                _dashboard!.userInfo.avatarUrl.isNotEmpty
                                    ? NetworkImage(_dashboard!.userInfo.avatarUrl)
                                    : null,
                            child: _dashboard!.userInfo.avatarUrl.isEmpty
                                ? const Icon(Icons.person, color: Colors.grey)
                                : null,
                          ),
                        ),
                        const SizedBox(width: 12),
                        Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Chào buổi sáng, ${_dashboard!.userInfo.fullName.split(' ').last}!',
                              style: const TextStyle(
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
                _buildGoalCardSection(),

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
                        'Tập luyện hôm nay',
                        _dashboard!.todayStats.workoutDuration.isNotEmpty
                            ? _dashboard!.todayStats.workoutDuration
                            : '0 min',
                        Icons.fitness_center,
                        const Color(0xFFC5C292),
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 12),

                // Calo card - full width
                SizedBox(
                  width: double.infinity,
                  child: _buildStatCard(
                    'Calo hôm nay',
                    '${_dashboard!.todayStats.caloriesConsumed}/${_dashboard!.todayStats.caloriesTarget}',
                    Icons.local_fire_department,
                    const Color(0xFFC5C292),
                  ),
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
                          setState(() {
                            _currentIndex = 2; // Switch to Nutrition tab
                          });
                        },
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: _buildActionButton(
                        'Ghi buổi tập',
                        Icons.fitness_center,
                        () {
                          setState(() {
                            _currentIndex = 1; // Switch to Workout tab
                          });
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
    );
  }

  Widget _buildGoalCardSection() {
    if (_dashboard == null || _dashboard!.activeGoals.isEmpty) {
      return Card(
        color: const Color(0xFFC5C292),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(32)),
        child: const Padding(
          padding: EdgeInsets.all(20),
          child: Center(child: Text("Chưa có mục tiêu nào đang hoạt động")),
        ),
      );
    }

    GoalProgressDto? currentProgress;

    if (_selectedGoalId != null && _cachedGoalProgress.containsKey(_selectedGoalId)) {
      currentProgress = _cachedGoalProgress[_selectedGoalId];
    } else {
      currentProgress = _dashboard!.goalProgress;
    }

    if (_isGoalLoading) {
      return Container(
        height: 180,
        decoration: BoxDecoration(
          color: const Color(0xFFC5C292),
          borderRadius: BorderRadius.circular(32),
        ),
        child: const Center(child: CircularProgressIndicator()),
      );
    }

    if (currentProgress == null) return const SizedBox();

    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: const Color(0xFFC5C292),
        borderRadius: BorderRadius.circular(32),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.25),
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
              if (_dashboard!.activeGoals.length > 1)
                DropdownButtonHideUnderline(
                  child: DropdownButton<int>(
                    value: _selectedGoalId,
                    dropdownColor: const Color(0xFFFDFBD4),
                    icon: const Icon(Icons.arrow_drop_down, color: Colors.black),
                    style: const TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.w600,
                      color: Colors.black,
                      fontFamily: 'Estedad-VF',
                    ),
                    onChanged: _onGoalSelected,
                    items: _dashboard!.activeGoals.map((GoalSummaryDto goal) {
                      return DropdownMenuItem<int>(
                        value: goal.goalId,
                        child: Text(goal.getTypeDisplay()),
                      );
                    }).toList(),
                  ),
                )
              else
                Text(
                  currentProgress.getGoalTypeDisplay(),
                  style: const TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w600,
                    fontFamily: 'Estedad-VF',
                  ),
                ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Text(
                  'Tiến độ ${currentProgress.progress.toStringAsFixed(0)}%',
                  style: const TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.bold,
                    fontFamily: 'Estedad-VF',
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 4),
          Text(
            'Đã hoàn thành ${currentProgress.progress.toStringAsFixed(0)}%',
            style: TextStyle(
              fontSize: 12,
              color: Colors.grey[700],
              fontFamily: 'Estedad-VF',
            ),
          ),
          const SizedBox(height: 12),
          LinearProgressIndicator(
            value: (currentProgress.progress) / 100,
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
                    '${currentProgress.currentValue.toStringAsFixed(1)}kg',
                    style: const TextStyle(
                      fontSize: 32,
                      fontWeight: FontWeight.bold,
                      fontFamily: 'Estedad-VF',
                    ),
                  ),
                  Text(
                    'Mục tiêu: ${currentProgress.targetValue.toStringAsFixed(1)}kg',
                    style: TextStyle(
                      fontSize: 12,
                      color: Colors.grey[700],
                      fontFamily: 'Estedad-VF',
                    ),
                  ),
                ],
              ),
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Text(
                    currentProgress.goalType.contains('loss') ||
                            currentProgress.goalType.contains('giảm') ||
                            currentProgress.targetValue < currentProgress.startValue
                        ? 'Giảm'
                        : 'Tăng',
                    style: const TextStyle(
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                      color: Color(0xFF8BA655),
                      fontFamily: 'Estedad-VF',
                    ),
                  ),
                  Text(
                    '${currentProgress.progressAmount.abs().toStringAsFixed(1)}kg',
                    style: const TextStyle(
                      fontSize: 24,
                      fontWeight: FontWeight.bold,
                      fontFamily: 'Estedad-VF',
                    ),
                  ),
                  Text(
                    'còn ${currentProgress.remaining.abs().toStringAsFixed(1)}kg nữa',
                    style: TextStyle(
                      fontSize: 12,
                      color: Colors.grey[700],
                      fontFamily: 'Estedad-VF',
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

  Widget _buildStatCard(String title, String value, IconData icon, Color color,
      {String? subtitle}) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: color,
        borderRadius: BorderRadius.circular(32),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.25),
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
              fontFamily: 'Estedad-VF',
            ),
          ),
          const SizedBox(height: 4),
          Text(
            value,
            style: const TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.bold,
              fontFamily: 'Estedad-VF',
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
                  fontFamily: 'Estedad-VF',
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildActionButton(
      String label, IconData icon, VoidCallback onPressed) {
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
              fontFamily: 'Estedad-VF',
            ),
          ),
        ],
      ),
    );
  }
}
