import 'dart:math' as math;
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:intl/date_symbol_data_local.dart';
import '../services/nutrition_service.dart';
import 'package:fl_chart/fl_chart.dart';

class NutritionHistoryScreen extends StatefulWidget {
  const NutritionHistoryScreen({super.key});

  @override
  State<NutritionHistoryScreen> createState() => _NutritionHistoryScreenState();
}

class _NutritionHistoryScreenState extends State<NutritionHistoryScreen> {
  final NutritionService _nutritionService = NutritionService();
  List<NutritionLog> _logs = [];
  bool _isLoading = true;
  String _viewMode = 'week'; // 'week' or 'month'

  @override
  void initState() {
    super.initState();
    _initLocale();
  }

  Future<void> _initLocale() async {
    await initializeDateFormatting('vi_VN', null);
    _loadHistory();
  }

  Future<void> _loadHistory() async {
    setState(() => _isLoading = true);
    try {
      final endDate = DateTime.now();
      final startDate = _viewMode == 'week'
          ? endDate.subtract(const Duration(days: 7))
          : endDate.subtract(const Duration(days: 30));

      print('📅 Loading history from $startDate to $endDate');
      
      final logs = await _nutritionService.getNutritionLogs(
        startDate: startDate,
        endDate: endDate,
      );
      
      print('📊 Received ${logs.length} logs');
      
      // Sort by date ascending (oldest first)
      logs.sort((a, b) => a.logDate.compareTo(b.logDate));
      
      setState(() {
        _logs = logs;
        _isLoading = false;
      });
      
      print('✅ History loaded successfully');
    } catch (e, stackTrace) {
      print('❌ Failed to load history: $e');
      print('Stack trace: $stackTrace');
      
      setState(() => _isLoading = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Không thể tải lịch sử: ${e.toString()}'),
            backgroundColor: Colors.red,
            duration: const Duration(seconds: 5),
          ),
        );
      }
    }
  }

  Map<String, double> _calculateAverages() {
    if (_logs.isEmpty) {
      return {'calories': 0, 'protein': 0, 'carbs': 0, 'fat': 0};
    }

    double totalCalories = 0, totalProtein = 0, totalCarbs = 0, totalFat = 0;
    for (var log in _logs) {
      totalCalories += log.totalCalories;
      totalProtein += log.proteinG;
      totalCarbs += log.carbsG;
      totalFat += log.fatG;
    }

    return {
      'calories': totalCalories / _logs.length,
      'protein': totalProtein / _logs.length,
      'carbs': totalCarbs / _logs.length,
      'fat': totalFat / _logs.length,
    };
  }

  @override
  Widget build(BuildContext context) {
    final averages = _calculateAverages();

    return Scaffold(
      backgroundColor: const Color(0xFFD9D7B6),
      appBar: AppBar(
        backgroundColor: const Color(0xFFD9D7B6),
        elevation: 0,
        title: const Text(
          'Lịch sử Dinh dưỡng',
          style: TextStyle(
            fontFamily: 'Estedad-VF',
            color: Colors.black,
            fontSize: 24,
            fontWeight: FontWeight.bold,
          ),
        ),
        actions: [
          // View mode toggles
          Container(
            margin: const EdgeInsets.only(right: 8),
            decoration: BoxDecoration(
              color: const Color(0xFFFDFBD4),
              borderRadius: BorderRadius.circular(20),
            ),
            padding: const EdgeInsets.all(4),
            child: Row(
              children: [
                _buildViewModeButton('7 ngày', 'week'),
                const SizedBox(width: 4),
                _buildViewModeButton('30 ngày', 'month'),
              ],
            ),
          ),
        ],
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _loadHistory,
              child: SingleChildScrollView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Summary cards
                    _buildSummaryCards(averages),
                    const SizedBox(height: 16),
                    
                    // Charts
                    _buildCaloriesChart(),
                    const SizedBox(height: 16),
                    _buildMacrosChart(),
                    const SizedBox(height: 16),
                    
                    // Daily list
                    _buildDailyList(),
                  ],
                ),
              ),
            ),
    );
  }

  Widget _buildViewModeButton(String label, String mode) {
    final isSelected = _viewMode == mode;
    return GestureDetector(
      onTap: () {
        if (_viewMode != mode) {
          setState(() => _viewMode = mode);
          _loadHistory();
        }
      },
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        decoration: BoxDecoration(
          color: isSelected ? const Color(0xFFA4C639) : Colors.transparent,
          borderRadius: BorderRadius.circular(16),
        ),
        child: Text(
          label,
          style: TextStyle(
            fontFamily: 'Estedad-VF',
            fontSize: 13,
            fontWeight: FontWeight.w600,
            color: isSelected ? Colors.white : Colors.black54,
          ),
        ),
      ),
    );
  }

  Widget _buildSummaryCards(Map<String, double> averages) {
    return Row(
      children: [
        Expanded(
          child: _buildSummaryCard(
            'Calories TB',
            averages['calories']!.toInt().toString(),
            'kcal/ngày',
            const Color(0xFFFF6B6B),
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: _buildSummaryCard(
            'Protein TB',
            averages['protein']!.toInt().toString(),
            'g/ngày',
            const Color(0xFFFF6B6B),
          ),
        ),
      ],
    );
  }

  Widget _buildSummaryCard(
      String title, String value, String unit, Color color) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: const Color(0xFFFDFBD4),
        borderRadius: BorderRadius.circular(24),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            title,
            style: const TextStyle(
              fontFamily: 'Estedad-VF',
              fontSize: 12,
              color: Colors.black54,
              fontWeight: FontWeight.w500,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            value,
            style: TextStyle(
              fontFamily: 'Estedad-VF',
              fontSize: 28,
              fontWeight: FontWeight.bold,
              color: color,
            ),
          ),
          Text(
            unit,
            style: const TextStyle(
              fontFamily: 'Estedad-VF',
              fontSize: 11,
              color: Colors.black38,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildCaloriesChart() {
    if (_logs.isEmpty) return const SizedBox.shrink();

    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: const Color(0xFFFDFBD4),
        borderRadius: BorderRadius.circular(24),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Calories theo ngày',
            style: TextStyle(
              fontFamily: 'Estedad-VF',
              fontSize: 16,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 20),
          SizedBox(
            height: 200,
            child: LineChart(
              LineChartData(
                gridData: FlGridData(show: true, drawVerticalLine: false),
                titlesData: FlTitlesData(
                  leftTitles: AxisTitles(
                    sideTitles: SideTitles(
                      showTitles: true,
                      reservedSize: 40,
                      getTitlesWidget: (value, meta) {
                        return Text(
                          value.toInt().toString(),
                          style: const TextStyle(fontSize: 10),
                        );
                      },
                    ),
                  ),
                  bottomTitles: AxisTitles(
                    sideTitles: SideTitles(
                      showTitles: true,
                      getTitlesWidget: (value, meta) {
                        final index = value.toInt();
                        if (index >= 0 && index < _logs.length) {
                          return Padding(
                            padding: const EdgeInsets.only(top: 8),
                            child: Text(
                              DateFormat('dd/MM').format(_logs[index].logDate),
                              style: const TextStyle(fontSize: 10),
                            ),
                          );
                        }
                        return const Text('');
                      },
                    ),
                  ),
                  rightTitles: AxisTitles(sideTitles: SideTitles(showTitles: false)),
                  topTitles: AxisTitles(sideTitles: SideTitles(showTitles: false)),
                ),
                borderData: FlBorderData(show: false),
                lineBarsData: [
                  LineChartBarData(
                    spots: _logs.asMap().entries.map((entry) {
                      return FlSpot(
                        entry.key.toDouble(),
                        entry.value.totalCalories,
                      );
                    }).toList(),
                    isCurved: true,
                    color: const Color(0xFFFF6B6B),
                    barWidth: 3,
                    dotData: FlDotData(show: true),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildMacrosChart() {
    if (_logs.isEmpty) return const SizedBox.shrink();

    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: const Color(0xFFFDFBD4),
        borderRadius: BorderRadius.circular(24),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Row(
            children: [
              Text(
                'Macros theo ngày',
                style: TextStyle(
                  fontFamily: 'Estedad-VF',
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                ),
              ),
              Spacer(),
              // Legend
              Row(
                children: [
                  _MacroLegend(color: Color(0xFFFF6B6B), label: 'P'),
                  SizedBox(width: 8),
                  _MacroLegend(color: Color(0xFFFFA726), label: 'C'),
                  SizedBox(width: 8),
                  _MacroLegend(color: Color(0xFF66BB6A), label: 'F'),
                ],
              ),
            ],
          ),
          const SizedBox(height: 20),
          SizedBox(
            height: 200,
            child: BarChart(
              BarChartData(
                gridData: FlGridData(show: true, drawVerticalLine: false),
                titlesData: FlTitlesData(
                  leftTitles: AxisTitles(
                    sideTitles: SideTitles(
                      showTitles: true,
                      reservedSize: 40,
                      getTitlesWidget: (value, meta) {
                        return Text(
                          value.toInt().toString(),
                          style: const TextStyle(fontSize: 10),
                        );
                      },
                    ),
                  ),
                  bottomTitles: AxisTitles(
                    sideTitles: SideTitles(
                      showTitles: true,
                      getTitlesWidget: (value, meta) {
                        final index = value.toInt();
                        if (index >= 0 && index < _logs.length) {
                          return Padding(
                            padding: const EdgeInsets.only(top: 8),
                            child: Text(
                              DateFormat('dd/MM').format(_logs[index].logDate),
                              style: const TextStyle(fontSize: 10),
                            ),
                          );
                        }
                        return const Text('');
                      },
                    ),
                  ),
                  rightTitles: AxisTitles(sideTitles: SideTitles(showTitles: false)),
                  topTitles: AxisTitles(sideTitles: SideTitles(showTitles: false)),
                ),
                borderData: FlBorderData(show: false),
                barGroups: _logs.asMap().entries.map((entry) {
                  return BarChartGroupData(
                    x: entry.key,
                    barRods: [
                      BarChartRodData(
                        toY: entry.value.proteinG,
                        color: const Color(0xFFFF6B6B),
                        width: 6,
                        borderRadius: const BorderRadius.only(
                          topLeft: Radius.circular(4),
                          topRight: Radius.circular(4),
                        ),
                      ),
                      BarChartRodData(
                        toY: entry.value.carbsG,
                        color: const Color(0xFFFFA726),
                        width: 6,
                        borderRadius: const BorderRadius.only(
                          topLeft: Radius.circular(4),
                          topRight: Radius.circular(4),
                        ),
                      ),
                      BarChartRodData(
                        toY: entry.value.fatG,
                        color: const Color(0xFF66BB6A),
                        width: 6,
                        borderRadius: const BorderRadius.only(
                          topLeft: Radius.circular(4),
                          topRight: Radius.circular(4),
                        ),
                      ),
                    ],
                  );
                }).toList(),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildDailyList() {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: const Color(0xFFFDFBD4),
        borderRadius: BorderRadius.circular(24),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Chi tiết theo ngày',
            style: TextStyle(
              fontFamily: 'Estedad-VF',
              fontSize: 16,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 16),
          if (_logs.isEmpty)
            const Center(
              child: Padding(
                padding: EdgeInsets.all(32),
                child: Text(
                  'Chưa có dữ liệu',
                  style: TextStyle(
                    fontFamily: 'Estedad-VF',
                    color: Colors.grey,
                  ),
                ),
              ),
            )
          else
            ...(_logs.reversed.map((log) => _buildDailyListItem(log))),
        ],
      ),
    );
  }

  Widget _buildDailyListItem(NutritionLog log) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: const Color(0xFFC5C292).withOpacity(0.3),
        borderRadius: BorderRadius.circular(16),
      ),
      child: Row(
        children: [
          // Date
          Container(
            width: 50,
            child: Column(
              children: [
                Text(
                  DateFormat('dd').format(log.logDate),
                  style: const TextStyle(
                    fontFamily: 'Estedad-VF',
                    fontSize: 24,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                Text(
                  DateFormat('MMM').format(log.logDate),
                  style: const TextStyle(
                    fontFamily: 'Estedad-VF',
                    fontSize: 12,
                    color: Colors.black54,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(width: 16),
          // Info
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  DateFormat('EEEE, dd MMMM yyyy', 'vi_VN').format(log.logDate),
                  style: const TextStyle(
                    fontFamily: 'Estedad-VF',
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  '${log.foodEntries.length} món ăn',
                  style: const TextStyle(
                    fontFamily: 'Estedad-VF',
                    fontSize: 12,
                    color: Colors.black54,
                  ),
                ),
              ],
            ),
          ),
          // Calories & Macros
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Text(
                '${log.totalCalories.toInt()}',
                style: const TextStyle(
                  fontFamily: 'Estedad-VF',
                  fontSize: 20,
                  fontWeight: FontWeight.bold,
                  color: Color(0xFFFF6B6B),
                ),
              ),
              const Text(
                'kcal',
                style: TextStyle(
                  fontFamily: 'Estedad-VF',
                  fontSize: 10,
                  color: Colors.black38,
                ),
              ),
              const SizedBox(height: 8),
              Row(
                children: [
                  _buildMacroIndicator(
                      log.proteinG, const Color(0xFFFF6B6B)),
                  const SizedBox(width: 6),
                  _buildMacroIndicator(
                      log.carbsG, const Color(0xFFFFA726)),
                  const SizedBox(width: 6),
                  _buildMacroIndicator(log.fatG, const Color(0xFF66BB6A)),
                ],
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildMacroIndicator(double value, Color color) {
    return Column(
      children: [
        Container(
          width: 6,
          height: 6,
          decoration: BoxDecoration(
            color: color,
            shape: BoxShape.circle,
          ),
        ),
        const SizedBox(height: 2),
        Text(
          value.toStringAsFixed(1),
          style: const TextStyle(
            fontFamily: 'Estedad-VF',
            fontSize: 10,
            fontWeight: FontWeight.w600,
          ),
        ),
      ],
    );
  }
}

class _MacroLegend extends StatelessWidget {
  final Color color;
  final String label;

  const _MacroLegend({required this.color, required this.label});

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Container(
          width: 12,
          height: 12,
          decoration: BoxDecoration(
            color: color,
            borderRadius: BorderRadius.circular(2),
          ),
        ),
        const SizedBox(width: 4),
        Text(
          label,
          style: const TextStyle(
            fontFamily: 'Estedad-VF',
            fontSize: 11,
            fontWeight: FontWeight.w600,
          ),
        ),
      ],
    );
  }
}
