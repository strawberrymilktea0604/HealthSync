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
      backgroundColor: const Color(0xFFD9D7B6),
      appBar: AppBar(
        backgroundColor: const Color(0xFFD9D7B6),
        elevation: 0,
        title: const Text(
          'Lịch sử Luyện tập',
          style: TextStyle(
            fontFamily: 'Estedad-VF',
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
        heroTag: 'workout_history_fab',
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
        backgroundColor: const Color(0xFF2d2d2d),
        foregroundColor: const Color(0xFFFDFBD4),
        icon: const Icon(Icons.add),
        label: const Text(
          'Thêm Bài Tập',
          style: TextStyle(fontFamily: 'Estedad-VF', fontWeight: FontWeight.bold),
        ),
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
              color: Colors.black.withOpacity(0.2),
            ),
            const SizedBox(height: 24),
            const Text(
              'Chưa có buổi tập nào',
              style: TextStyle(
                fontFamily: 'Estedad-VF',
                fontSize: 22,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 12),
            Text(
              'Bắt đầu ghi lại hành trình\nluyện tập của bạn ngay hôm nay!',
              textAlign: TextAlign.center,
              style: TextStyle(
                fontFamily: 'Estedad-VF',
                fontSize: 16,
                color: Colors.grey[700],
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
                backgroundColor: const Color(0xFF2d2d2d),
                foregroundColor: const Color(0xFFFDFBD4),
                padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 16),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(30),
                ),
              ),
              icon: const Icon(Icons.add),
              label: const Text(
                'Tạo buổi tập đầu tiên',
                style: TextStyle(fontFamily: 'Estedad-VF', fontSize: 16, fontWeight: FontWeight.bold),
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
                      fontFamily: 'Estedad-VF',
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

  Future<void> _confirmDeleteWorkout(int workoutLogId) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Xác nhận xóa'),
        content: const Text('Bạn có chắc muốn xóa buổi tập này?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Hủy'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            style: TextButton.styleFrom(foregroundColor: Colors.red),
            child: const Text('Xóa'),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      await _deleteWorkout(workoutLogId);
    }
  }

  Future<void> _deleteWorkout(int workoutLogId) async {
    try {
      await _workoutService.deleteWorkoutLog(workoutLogId);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Đã xóa buổi tập')),
        );
        _loadWorkoutLogs();
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Không thể xóa: $e')),
        );
      }
    }
  }

  Widget _buildWorkoutCard(WorkoutLog log) {
    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      padding: const EdgeInsets.all(20),
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
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Row(
                children: [
                  Icon(Icons.fitness_center, size: 20, color: Color(0xFF2d2d2d)),
                  SizedBox(width: 8),
                  Text(
                    'Buổi tập',
                    style: TextStyle(
                      fontFamily: 'Estedad-VF',
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                      color: Color(0xFF2d2d2d),
                    ),
                  ),
                ],
              ),
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                    decoration: BoxDecoration(
                      color: const Color(0xFFFDFBD4),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Row(
                      children: [
                        const Icon(Icons.schedule, size: 14, color: Colors.black87),
                        const SizedBox(width: 4),
                        Text(
                          '${log.durationMin} phút',
                          style: const TextStyle(
                            fontFamily: 'Estedad-VF',
                            color: Colors.black87,
                            fontSize: 14,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(width: 8),
                  IconButton(
                    icon: const Icon(Icons.delete_outline, color: Colors.red, size: 20),
                    onPressed: () => _confirmDeleteWorkout(log.workoutLogId),
                    padding: EdgeInsets.zero,
                    constraints: const BoxConstraints(),
                  ),
                ],
              ),
            ],
          ),
          if (log.notes != null && log.notes!.isNotEmpty) ...[
            const SizedBox(height: 12),
            Text(
              log.notes!,
              style: TextStyle(
                fontFamily: 'Estedad-VF',
                color: Colors.black.withOpacity(0.7),
                fontSize: 14,
                fontStyle: FontStyle.italic,
              ),
            ),
          ],
          const SizedBox(height: 16),
          ...log.exerciseSessions.map((session) => Padding(
                padding: const EdgeInsets.only(bottom: 8),
                child: Container(
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: const Color(0xFFFDFBD4).withOpacity(0.6),
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(color: Colors.white.withOpacity(0.5)),
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
                                fontFamily: 'Estedad-VF',
                                fontWeight: FontWeight.w700,
                                fontSize: 16,
                                color: Colors.black87,
                              ),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              '${session.sets} sets × ${session.reps} reps'
                              '${session.weightKg > 0 ? ' @ ${session.weightKg}kg' : ''}',
                              style: TextStyle(
                                fontFamily: 'Estedad-VF',
                                color: Colors.black.withOpacity(0.6),
                                fontSize: 14,
                                fontWeight: FontWeight.w500,
                              ),
                            ),
                          ],
                        ),
                      ),
                      if (session.restSec != null)
                        Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: 10,
                            vertical: 6,
                          ),
                          decoration: BoxDecoration(
                            color: Colors.white,
                            borderRadius: BorderRadius.circular(12),
                          ),
                          child: Text(
                            'Nghỉ: ${session.restSec}s',
                            style: const TextStyle(
                              fontFamily: 'Estedad-VF',
                              fontSize: 12,
                              fontWeight: FontWeight.w600,
                              color: Color(0xFF8BA655),
                            ),
                          ),
                        ),
                    ],
                  ),
                ),
              )),
        ],
      ),
    );
  }
}
