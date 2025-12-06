import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../models/workout_model.dart';
import '../services/workout_service.dart';
import 'create_workout_screen.dart';

class WorkoutHistoryScreen extends StatefulWidget {
  const WorkoutHistoryScreen({super.key});

  @override
  State<WorkoutHistoryScreen> createState() => _WorkoutHistoryScreenState();
}

class _WorkoutHistoryScreenState extends State<WorkoutHistoryScreen> {
  final WorkoutService _workoutService = WorkoutService();
  List<WorkoutLog> _workoutLogs = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadWorkoutLogs();
  }

  Future<void> _loadWorkoutLogs() async {
    try {
      setState(() => _isLoading = true);
      final logs = await _workoutService.getWorkoutLogs();
      setState(() {
        _workoutLogs = logs..sort((a, b) => b.workoutDate.compareTo(a.workoutDate));
        _isLoading = false;
      });
    } catch (e) {
      setState(() => _isLoading = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Không thể tải lịch sử: $e')),
        );
      }
    }
  }

  String _formatDate(DateTime date) {
    final now = DateTime.now();
    final today = DateTime(now.year, now.month, now.day);
    final yesterday = today.subtract(const Duration(days: 1));
    final logDate = DateTime(date.year, date.month, date.day);

    if (logDate == today) {
      return 'Hôm nay';
    } else if (logDate == yesterday) {
      return 'Hôm qua';
    } else {
      return DateFormat('EEEE, dd/MM/yyyy', 'vi').format(date);
    }
  }

  Map<String, List<WorkoutLog>> _groupByDate(List<WorkoutLog> logs) {
    final Map<String, List<WorkoutLog>> grouped = {};
    for (var log in logs) {
      final dateKey = DateFormat('yyyy-MM-dd').format(log.workoutDate);
      if (!grouped.containsKey(dateKey)) {
        grouped[dateKey] = [];
      }
      grouped[dateKey]!.add(log);
    }
    return grouped;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F1E8),
      appBar: AppBar(
        backgroundColor: const Color(0xFFF5F1E8),
        elevation: 0,
        title: const Text(
          'Lịch sử Luyện tập',
          style: TextStyle(
            color: Colors.black,
            fontSize: 24,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _workoutLogs.isEmpty
              ? _buildEmptyState()
              : RefreshIndicator(
                  onRefresh: _loadWorkoutLogs,
                  child: _buildWorkoutList(),
                ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () async {
          final result = await Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => const CreateWorkoutScreen(),
            ),
          );
          if (result == true) {
            _loadWorkoutLogs();
          }
        },
        backgroundColor: const Color(0xFF2D3E2E),
        icon: const Icon(Icons.add),
        label: const Text('Thêm Bài Tập'),
      ),
    );
  }

  Widget _buildEmptyState() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32.0),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.fitness_center,
              size: 80,
              color: Colors.grey[400],
            ),
            const SizedBox(height: 24),
            const Text(
              'Chưa có buổi tập nào',
              style: TextStyle(
                fontSize: 22,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 12),
            Text(
              'Bắt đầu ghi lại hành trình\nluyện tập của bạn ngay hôm nay!',
              textAlign: TextAlign.center,
              style: TextStyle(
                fontSize: 16,
                color: Colors.grey[600],
              ),
            ),
            const SizedBox(height: 32),
            ElevatedButton.icon(
              onPressed: () async {
                final result = await Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => const CreateWorkoutScreen(),
                  ),
                );
                if (result == true) {
                  _loadWorkoutLogs();
                }
              },
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF2D3E2E),
                padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 16),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(30),
                ),
              ),
              icon: const Icon(Icons.add),
              label: const Text(
                'Tạo buổi tập đầu tiên',
                style: TextStyle(fontSize: 16),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildWorkoutList() {
    final groupedLogs = _groupByDate(_workoutLogs);
    final sortedDates = groupedLogs.keys.toList()
      ..sort((a, b) => b.compareTo(a));

    return ListView.builder(
      padding: const EdgeInsets.all(16),
      itemCount: sortedDates.length,
      itemBuilder: (context, index) {
        final dateKey = sortedDates[index];
        final logs = groupedLogs[dateKey]!;
        final date = DateTime.parse(dateKey);

        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 12),
              child: Row(
                children: [
                  const Icon(Icons.calendar_today, size: 20),
                  const SizedBox(width: 8),
                  Text(
                    _formatDate(date),
                    style: const TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            ),
            ...logs.map((log) => _buildWorkoutCard(log)),
            const SizedBox(height: 8),
          ],
        );
      },
    );
  }

  Widget _buildWorkoutCard(WorkoutLog log) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      color: const Color(0xFFE8DCC4),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
      ),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Row(
                  children: [
                    Icon(Icons.fitness_center, size: 20),
                    SizedBox(width: 8),
                    Text(
                      'Buổi tập',
                      style: TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                ),
                Row(
                  children: [
                    const Icon(Icons.schedule, size: 16, color: Colors.grey),
                    const SizedBox(width: 4),
                    Text(
                      '${log.durationMin} phút',
                      style: const TextStyle(
                        color: Colors.grey,
                        fontSize: 14,
                      ),
                    ),
                  ],
                ),
              ],
            ),
            if (log.notes != null && log.notes!.isNotEmpty) ...[
              const SizedBox(height: 8),
              Text(
                log.notes!,
                style: TextStyle(
                  color: Colors.grey[700],
                  fontSize: 14,
                ),
              ),
            ],
            const SizedBox(height: 12),
            ...log.exerciseSessions.map((session) => Padding(
                  padding: const EdgeInsets.only(bottom: 8),
                  child: Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: const Color(0xFFF5F1E8),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                session.exerciseName,
                                style: const TextStyle(
                                  fontWeight: FontWeight.w600,
                                  fontSize: 15,
                                ),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                '${session.sets} sets × ${session.reps} reps'
                                '${session.weightKg > 0 ? ' @ ${session.weightKg}kg' : ''}',
                                style: TextStyle(
                                  color: Colors.grey[600],
                                  fontSize: 13,
                                ),
                              ),
                            ],
                          ),
                        ),
                        if (session.restSec != null)
                          Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 8,
                              vertical: 4,
                            ),
                            decoration: BoxDecoration(
                              color: Colors.white,
                              borderRadius: BorderRadius.circular(8),
                            ),
                            child: Text(
                              'Nghỉ: ${session.restSec}s',
                              style: const TextStyle(fontSize: 12),
                            ),
                          ),
                      ],
                    ),
                  ),
                )),
          ],
        ),
      ),
    );
  }
}
