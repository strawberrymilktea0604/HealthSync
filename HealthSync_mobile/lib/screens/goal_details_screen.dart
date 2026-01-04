import 'package:flutter/material.dart';
import 'package:fl_chart/fl_chart.dart';
import '../models/goal_model.dart';
import '../services/goal_service.dart';
import 'add_progress_screen.dart';

class GoalDetailsScreen extends StatefulWidget {
  final int goalId;

  const GoalDetailsScreen({
    super.key,
    required this.goalId,
  });

  @override
  State<GoalDetailsScreen> createState() => _GoalDetailsScreenState();
}

class _GoalDetailsScreenState extends State<GoalDetailsScreen> {
  final GoalService _goalService = GoalService();
  Goal? _goal;
  bool _isLoading = true;
  String _selectedTimeRange = 'Tháng';

  @override
  void initState() {
    super.initState();
    _loadGoalDetails();
  }

  Future<void> _loadGoalDetails() async {
    try {
      setState(() => _isLoading = true);
      final goal = await _goalService.getGoalById(widget.goalId);
      setState(() {
        _goal = goal;
        _isLoading = false;
      });
    } catch (e) {
      setState(() => _isLoading = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Không thể tải chi tiết mục tiêu: $e')),
        );
      }
    }
  }

  List<ProgressRecord> _getFilteredRecords() {
    if (_goal == null || _goal!.progressRecords.isEmpty) return [];

    final now = DateTime.now();
    final records = List<ProgressRecord>.from(_goal!.progressRecords)
      ..sort((a, b) => a.recordDate.compareTo(b.recordDate));

    if (_selectedTimeRange == 'Tuần') {
      final weekAgo = now.subtract(const Duration(days: 7));
      return records.where((r) => r.recordDate.isAfter(weekAgo)).toList();
    } else if (_selectedTimeRange == 'Tháng') {
      final monthAgo = now.subtract(const Duration(days: 30));
      return records.where((r) => r.recordDate.isAfter(monthAgo)).toList();
    } else if (_selectedTimeRange == '3 Tháng') {
      final threeMonthsAgo = now.subtract(const Duration(days: 90));
      return records.where((r) => r.recordDate.isAfter(threeMonthsAgo)).toList();
    }
    return records;
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return Scaffold(
        backgroundColor: const Color(0xFFD9D7B6),
        appBar: AppBar(
          backgroundColor: const Color(0xFFD9D7B6),
          elevation: 0,
        ),
        body: const Center(child: CircularProgressIndicator()),
      );
    }

    if (_goal == null) {
      return Scaffold(
        backgroundColor: const Color(0xFFD9D7B6),
        appBar: AppBar(
          backgroundColor: const Color(0xFFD9D7B6),
          elevation: 0,
        ),
        body: const Center(child: Text('Không tìm thấy mục tiêu')),
      );
    }

    final progress = _goal!.calculateProgress();
    final currentValue = _goal!.getCurrentValue();
    final sortedRecords = List<ProgressRecord>.from(_goal!.progressRecords)
      ..sort((a, b) => a.recordDate.compareTo(b.recordDate));

    final startValue = sortedRecords.isNotEmpty ? sortedRecords.first.value : 0.0;
    final change = currentValue != null ? currentValue - startValue : 0.0;
    final remaining = currentValue != null ? _goal!.targetValue - currentValue : _goal!.targetValue;

    return Scaffold(
      backgroundColor: const Color(0xFFD9D7B6),
      body: CustomScrollView(
        slivers: [
          // App Bar with Goal Info
          SliverAppBar(
            expandedHeight: 200,
            pinned: true,
            backgroundColor: const Color(0xFFD9D7B6),
            leading: IconButton(
              icon: const Icon(Icons.arrow_back, color: Colors.black87),
              onPressed: () => Navigator.pop(context, true),
            ),
            flexibleSpace: FlexibleSpaceBar(
              background: Container(
                padding: const EdgeInsets.fromLTRB(16, 80, 16, 16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisAlignment: MainAxisAlignment.end,
                  children: [
                    Text(
                      _goal!.getTypeDisplay(),
                      style: const TextStyle(
                        fontSize: 28,
                        fontWeight: FontWeight.bold,
                        color: Colors.black87,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Mục tiêu: ${_goal!.targetValue.toStringAsFixed(1)} kg',
                      style: const TextStyle(
                        fontSize: 16,
                        color: Colors.black54,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),

          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Stats Cards
                  Row(
                    children: [
                      Expanded(
                        child: _buildStatCard(
                          'Hiện tại',
                          '${currentValue?.toStringAsFixed(1) ?? '0.0'} kg',
                          Icons.trending_flat,
                          const Color(0xFF5FCCB4),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: _buildStatCard(
                          'Thay đổi',
                          '${change >= 0 ? '+' : ''}${change.toStringAsFixed(1)} kg',
                          change < 0 ? Icons.trending_down : Icons.trending_up,
                          change < 0 ? Colors.green : Colors.blue,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),
                  Row(
                    children: [
                      Expanded(
                        child: _buildStatCard(
                          'Còn lại',
                          '${remaining.abs().toStringAsFixed(1)} kg',
                          Icons.flag,
                          Colors.orange,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: _buildStatCard(
                          'Hoàn thành',
                          '${progress.toStringAsFixed(0)}%',
                          Icons.check_circle_outline,
                          const Color(0xFF5FCCB4),
                        ),
                      ),
                    ],
                  ),

                  const SizedBox(height: 24),

                  // Chart Section
                  Container(
                    padding: const EdgeInsets.all(20),
                    decoration: BoxDecoration(
                      color: const Color(0xFFFDFBD4),
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            const Text(
                              'Biểu đồ Cân nặng',
                              style: TextStyle(
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                                color: Colors.black87,
                              ),
                            ),
                            DropdownButton<String>(
                              value: _selectedTimeRange,
                              underline: Container(),
                              items: ['Tuần', 'Tháng', '3 Tháng', 'Tất cả']
                                  .map((range) => DropdownMenuItem(
                                        value: range,
                                        child: Text(range),
                                      ))
                                  .toList(),
                              onChanged: (value) {
                                if (value != null) {
                                  setState(() => _selectedTimeRange = value);
                                }
                              },
                            ),
                          ],
                        ),
                        const SizedBox(height: 20),
                        if (_getFilteredRecords().isNotEmpty)
                          SizedBox(
                            height: 200,
                            child: LineChart(
                              LineChartData(
                                gridData: FlGridData(
                                  show: true,
                                  drawVerticalLine: false,
                                  horizontalInterval: 5,
                                  getDrawingHorizontalLine: (value) {
                                    return FlLine(
                                      color: Colors.grey[300]!,
                                      strokeWidth: 1,
                                    );
                                  },
                                ),
                                titlesData: FlTitlesData(
                                  show: true,
                                  rightTitles: AxisTitles(
                                    sideTitles: SideTitles(showTitles: false),
                                  ),
                                  topTitles: AxisTitles(
                                    sideTitles: SideTitles(showTitles: false),
                                  ),
                                  bottomTitles: AxisTitles(
                                    sideTitles: SideTitles(
                                      showTitles: true,
                                      reservedSize: 30,
                                      interval: 1,
                                      getTitlesWidget: (value, meta) {
                                        final records = _getFilteredRecords();
                                        if (value.toInt() >= 0 &&
                                            value.toInt() < records.length) {
                                          final date = records[value.toInt()].recordDate;
                                          return Padding(
                                            padding: const EdgeInsets.only(top: 8),
                                            child: Text(
                                              '${date.day}/${date.month}',
                                              style: const TextStyle(
                                                fontSize: 10,
                                                color: Colors.grey,
                                              ),
                                            ),
                                          );
                                        }
                                        return const Text('');
                                      },
                                    ),
                                  ),
                                  leftTitles: AxisTitles(
                                    sideTitles: SideTitles(
                                      showTitles: true,
                                      reservedSize: 40,
                                      interval: 5,
                                    ),
                                  ),
                                ),
                                borderData: FlBorderData(show: false),
                                lineBarsData: [
                                  LineChartBarData(
                                    spots: _getFilteredRecords()
                                        .asMap()
                                        .entries
                                        .map((entry) => FlSpot(
                                              entry.key.toDouble(),
                                              entry.value.value,
                                            ))
                                        .toList(),
                                    isCurved: true,
                                    color: const Color(0xFF5FCCB4),
                                    barWidth: 3,
                                    isStrokeCapRound: true,
                                    dotData: FlDotData(
                                      show: true,
                                      getDotPainter: (spot, percent, barData, index) {
                                        return FlDotCirclePainter(
                                          radius: 4,
                                          color: const Color(0xFF5FCCB4),
                                          strokeWidth: 2,
                                          strokeColor: Colors.white,
                                        );
                                      },
                                    ),
                                    belowBarData: BarAreaData(
                                      show: true,
                                      color: const Color(0xFF5FCCB4).withValues(alpha: 0.1),
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          )
                        else
                          const SizedBox(
                            height: 200,
                            child: Center(
                              child: Text(
                                'Chưa có dữ liệu',
                                style: TextStyle(color: Colors.grey),
                              ),
                            ),
                          ),
                      ],
                    ),
                  ),

                  const SizedBox(height: 24),

                  // Progress Records
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text(
                        'Lịch sử tiến độ',
                        style: TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                          color: Colors.black87,
                        ),
                      ),
                      TextButton.icon(
                        onPressed: _goal?.status == 'in_progress'
                            ? () async {
                                final result = await Navigator.push(
                                  context,
                                  MaterialPageRoute(
                                    builder: (context) =>
                                        AddProgressScreen(goalId: widget.goalId),
                                  ),
                                );
                                if (result == true) {
                                  _loadGoalDetails();
                                }
                              }
                            : null,
                        icon: Icon(
                          Icons.add,
                          color: _goal?.status == 'in_progress'
                              ? const Color(0xFF5FCCB4)
                              : Colors.grey,
                          size: 20,
                        ),
                        label: Text(
                          'Thêm mới',
                          style: TextStyle(
                            color: _goal?.status == 'in_progress'
                                ? const Color(0xFF5FCCB4)
                                : Colors.grey,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),

                  if (sortedRecords.isEmpty)
                    Container(
                      padding: const EdgeInsets.all(40),
                      decoration: BoxDecoration(
                        color: const Color(0xFFF5F3ED),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Center(
                        child: Column(
                          children: [
                            Icon(
                              Icons.timeline,
                              size: 60,
                              color: Colors.grey[400],
                            ),
                            const SizedBox(height: 16),
                            Text(
                              'Chưa có bản ghi tiến độ',
                              style: TextStyle(
                                fontSize: 16,
                                color: Colors.grey[600],
                              ),
                            ),
                          ],
                        ),
                      ),
                    )
                  else
                    ...sortedRecords.reversed.map((record) {
                      return Container(
                        margin: const EdgeInsets.only(bottom: 12),
                        padding: const EdgeInsets.all(16),
                        decoration: BoxDecoration(
                          color: const Color(0xFFFDFBD4),
                          borderRadius: BorderRadius.circular(20),
                        ),
                        child: Row(
                          children: [
                            Container(
                              width: 48,
                              height: 48,
                              decoration: BoxDecoration(
                                color: const Color(0xFF5FCCB4).withValues(alpha: 0.2),
                                borderRadius: BorderRadius.circular(12),
                              ),
                              child: const Icon(
                                Icons.fitness_center,
                                color: Color(0xFF5FCCB4),
                              ),
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Row(
                                    children: [
                                      Text(
                                        '${record.value.toStringAsFixed(1)} kg',
                                        style: const TextStyle(
                                          fontSize: 18,
                                          fontWeight: FontWeight.bold,
                                          color: Colors.black87,
                                        ),
                                      ),
                                      if (record.weightKg > 0) ...[
                                        const SizedBox(width: 12),
                                        Text(
                                          'Cân nặng: ${record.weightKg.toStringAsFixed(1)} kg',
                                          style: TextStyle(
                                            fontSize: 12,
                                            color: Colors.grey[600],
                                          ),
                                        ),
                                      ],
                                    ],
                                  ),
                                  const SizedBox(height: 4),
                                  Row(
                                    children: [
                                      Icon(
                                        Icons.calendar_today,
                                        size: 12,
                                        color: Colors.grey[600],
                                      ),
                                      const SizedBox(width: 4),
                                      Text(
                                        '${record.recordDate.day}/${record.recordDate.month}/${record.recordDate.year}',
                                        style: TextStyle(
                                          fontSize: 12,
                                          color: Colors.grey[600],
                                        ),
                                      ),
                                      if (record.waistCm > 0) ...[
                                        const SizedBox(width: 12),
                                        Text(
                                          'Vòng eo: ${record.waistCm.toStringAsFixed(1)} cm',
                                          style: TextStyle(
                                            fontSize: 12,
                                            color: Colors.grey[600],
                                          ),
                                        ),
                                      ],
                                    ],
                                  ),
                                  if (record.notes != null &&
                                      record.notes!.isNotEmpty) ...[
                                    const SizedBox(height: 4),
                                    Text(
                                      record.notes!,
                                      style: TextStyle(
                                        fontSize: 12,
                                        color: Colors.grey[600],
                                        fontStyle: FontStyle.italic,
                                      ),
                                    ),
                                  ],
                                ],
                              ),
                            ),
                          ],
                        ),
                      );
                    }),

                  const SizedBox(height: 100),
                ],
              ),
            ),
          ),
        ],
      ),
      floatingActionButton: (_goal?.status == 'active' || _goal?.status == 'in_progress')
          ? FloatingActionButton.extended(
              onPressed: () async {
                final result = await Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => AddProgressScreen(goalId: widget.goalId),
                  ),
                );
                if (result == true) {
                  _loadGoalDetails();
                }
              },
              backgroundColor: const Color(0xFF5FCCB4),
              icon: const Icon(Icons.add),
              label: const Text('Cập nhật'),
            )
          : null,
    );
  }

  Widget _buildStatCard(String label, String value, IconData icon, Color color) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: const Color(0xFFFDFBD4),
        borderRadius: BorderRadius.circular(15),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(icon, size: 20, color: color),
              const SizedBox(width: 8),
              Text(
                label,
                style: TextStyle(
                  fontSize: 12,
                  color: Colors.grey[600],
                  fontWeight: FontWeight.w500,
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            value,
            style: const TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.bold,
              color: Colors.black87,
            ),
          ),
        ],
      ),
    );
  }
}
