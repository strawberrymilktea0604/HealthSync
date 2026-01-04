import 'package:flutter/material.dart';
import '../models/goal_model.dart';
import '../services/goal_service.dart';
import 'create_goal_screen.dart';
import 'goal_details_screen.dart';

class GoalsScreen extends StatefulWidget {
  const GoalsScreen({super.key});

  @override
  State<GoalsScreen> createState() => _GoalsScreenState();
}

class _GoalsScreenState extends State<GoalsScreen> with SingleTickerProviderStateMixin {
  final GoalService _goalService = GoalService();
  List<Goal> _goals = [];
  bool _isLoading = true;
  late TabController _tabController;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
    _loadGoals();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<void> _loadGoals() async {
    try {
      setState(() => _isLoading = true);
      final goals = await _goalService.getGoals();
      setState(() {
        _goals = goals;
        _isLoading = false;
      });
    } catch (e) {
      setState(() => _isLoading = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Không thể tải mục tiêu: $e')),
        );
      }
    }
  }

  List<Goal> get _activeGoals => _goals.where((g) => 
      g.status == 'in_progress' || g.status == 'not_started' || g.status == 'active'
  ).toList();
  
  List<Goal> get _completedGoals => _goals.where((g) => g.status == 'completed').toList();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFD9D7B6),
      appBar: AppBar(
        backgroundColor: const Color(0xFFD9D7B6),
        elevation: 0,
        title: const Text(
          'Mục tiêu của tôi',
          style: TextStyle(
            fontFamily: 'Estedad-VF',
            color: Colors.black,
            fontSize: 24,
            fontWeight: FontWeight.bold,
          ),
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.search, color: Colors.black),
            onPressed: () {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Tính năng tìm kiếm đang được phát triển!')),
              );
            },
          ),
        ],
        bottom: TabBar(
          controller: _tabController,
          indicatorColor: const Color(0xFFA4C639),
          indicatorWeight: 3,
          labelColor: Colors.black,
          labelStyle: const TextStyle(fontFamily: 'Estedad-VF', fontWeight: FontWeight.bold),
          unselectedLabelColor: Colors.black54,
          unselectedLabelStyle: const TextStyle(fontFamily: 'Estedad-VF'),
          tabs: const [
            Tab(text: 'Đang thực hiện'),
            Tab(text: 'Đã hoàn thành'),
          ],
        ),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : TabBarView(
              controller: _tabController,
              children: [
                _buildGoalsList(_activeGoals, true),
                _buildGoalsList(_completedGoals, false),
              ],
            ),
      floatingActionButton: FloatingActionButton(
        heroTag: 'goals_fab',
        onPressed: () async {
          final result = await Navigator.push(
            context,
            MaterialPageRoute(builder: (context) => const CreateGoalScreen()),
          );
          if (result == true) {
            _loadGoals();
          }
        },
        backgroundColor: const Color(0xFF2d2d2d),
        foregroundColor: const Color(0xFFFDFBD4),
        child: const Icon(Icons.add),
      ),
    );
  }

  Widget _buildGoalsList(List<Goal> goals, bool isActive) {
    if (goals.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              isActive ? Icons.track_changes : Icons.check_circle_outline,
              size: 80,
              color: Colors.black.withOpacity(0.2),
            ),
            const SizedBox(height: 16),
            Text(
              isActive ? 'Chưa có mục tiêu nào' : 'Chưa hoàn thành mục tiêu nào',
              style: TextStyle(
                fontFamily: 'Estedad-VF',
                fontSize: 18,
                fontWeight: FontWeight.w600,
                color: Colors.grey[700],
              ),
            ),
            const SizedBox(height: 8),
            Text(
              isActive ? 'Hãy tạo mục tiêu đầu tiên của bạn' : '',
              style: TextStyle(
                fontFamily: 'Estedad-VF',
                fontSize: 14,
                color: Colors.grey[600],
              ),
            ),
          ],
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _loadGoals,
      child: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: goals.length,
        itemBuilder: (context, index) {
          final goal = goals[index];
          return _buildGoalCard(goal);
        },
      ),
    );
  }

  Widget _buildGoalCard(Goal goal) {
    final progress = goal.calculateProgress();
    final currentValue = goal.getCurrentValue();

    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      decoration: BoxDecoration(
        color: const Color(0xFFC5C292),
        borderRadius: BorderRadius.circular(32),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.1),
            blurRadius: 10,
            offset: const Offset(0, 5),
          ),
        ],
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: () async {
            final result = await Navigator.push(
              context,
              MaterialPageRoute(
                builder: (context) => GoalDetailsScreen(goalId: goal.goalId),
              ),
            );
            if (result == true) {
              _loadGoals();
            }
          },
          borderRadius: BorderRadius.circular(32),
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Header
                Row(
                  children: [
                    Container(
                      width: 48,
                      height: 48,
                      decoration: BoxDecoration(
                        color: const Color(0xFFFDFBD4),
                        borderRadius: BorderRadius.circular(16),
                      ),
                      child: const Icon(
                        Icons.track_changes,
                        color: Color(0xFF8BA655),
                        size: 28,
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            goal.getTypeDisplay(),
                            style: const TextStyle(
                              fontFamily: 'Estedad-VF',
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                              color: Colors.black87,
                            ),
                          ),
                          Text(
                            'Mục tiêu: ${goal.targetValue.toStringAsFixed(1)} kg',
                            style: TextStyle(
                              fontFamily: 'Estedad-VF',
                              fontSize: 14,
                              color: Colors.black.withOpacity(0.6),
                            ),
                          ),
                        ],
                      ),
                    ),
                    Icon(
                      Icons.chevron_right,
                      color: Colors.black.withOpacity(0.3),
                    ),
                  ],
                ),

                if (currentValue != null) ...[
                  const SizedBox(height: 20),
                  // Progress
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text(
                        '${currentValue.toStringAsFixed(1)} kg',
                        style: const TextStyle(
                          fontFamily: 'Estedad-VF',
                          fontSize: 24,
                          fontWeight: FontWeight.bold,
                          color: Colors.black87,
                        ),
                      ),
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 12,
                          vertical: 6,
                        ),
                        decoration: BoxDecoration(
                          color: const Color(0xFFFDFBD4),
                          borderRadius: BorderRadius.circular(20),
                        ),
                        child: Row(
                          children: [
                            Icon(
                              progress >= 100 
                                  ? Icons.check_circle 
                                  : Icons.trending_up,
                              size: 16,
                              color: const Color(0xFF8BA655),
                            ),
                            const SizedBox(width: 4),
                            Text(
                              '${progress.toStringAsFixed(0)}%',
                              style: const TextStyle(
                                fontFamily: 'Estedad-VF',
                                fontSize: 14,
                                fontWeight: FontWeight.w600,
                                color: Color(0xFF8BA655),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),
                  // Progress Bar
                  ClipRRect(
                    borderRadius: BorderRadius.circular(10),
                    child: LinearProgressIndicator(
                      value: progress / 100,
                      backgroundColor: const Color(0xFFFDFBD4),
                      valueColor: const AlwaysStoppedAnimation<Color>(
                        Color(0xFF8BA655),
                      ),
                      minHeight: 12,
                    ),
                  ),
                ],

                const SizedBox(height: 16),
                // Footer info
                Row(
                  children: [
                    const Icon(Icons.calendar_today, size: 14, color: Colors.black54),
                    const SizedBox(width: 4),
                    Text(
                      '${_formatDate(goal.startDate)}${goal.endDate != null ? ' - ${_formatDate(goal.endDate!)}' : ''}',
                      style: const TextStyle(
                        fontFamily: 'Estedad-VF',
                        fontSize: 12,
                        color: Colors.black54,
                      ),
                    ),
                    const Spacer(),
                    const Icon(Icons.show_chart, size: 14, color: Colors.black54),
                    const SizedBox(width: 4),
                    Text(
                      '${goal.progressRecords.length} bản ghi',
                      style: const TextStyle(
                        fontFamily: 'Estedad-VF',
                        fontSize: 12,
                        color: Colors.black54,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  String _formatDate(DateTime date) {
    return '${date.day}/${date.month}/${date.year}';
  }
}
