import 'dart:math' as math;
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../services/nutrition_service.dart';
import 'nutrition_history_screen.dart';

class NutritionScreen extends StatefulWidget {
  const NutritionScreen({super.key});

  @override
  State<NutritionScreen> createState() => _NutritionScreenState();
}

class _NutritionScreenState extends State<NutritionScreen> {
  final NutritionService _nutritionService = NutritionService();
  DateTime _selectedDate = DateTime(DateTime.now().year, DateTime.now().month, DateTime.now().day);
  NutritionLog? _nutritionLog;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _loadNutritionLog();
  }

  Future<void> _loadNutritionLog() async {
    setState(() => _isLoading = true);
    try {
      final log = await _nutritionService.getNutritionLogByDate(_selectedDate);
      setState(() {
        _nutritionLog = log;
        _isLoading = false;
      });
    } catch (e) {
      setState(() => _isLoading = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Không thể tải nhật ký dinh dưỡng')),
        );
      }
    }
  }

  Future<void> _showAddFoodDialog() async {
    final result = await showModalBottomSheet<bool>(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => AddFoodBottomSheet(date: _selectedDate),
    );

    if (result == true) {
      await _loadNutritionLog();
    }
  }

  Future<void> _deleteFoodEntry(int foodEntryId) async {
    try {
      await _nutritionService.deleteFoodEntry(foodEntryId);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Đã xóa món ăn')),
        );
      }
      _loadNutritionLog();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Không thể xóa món ăn')),
        );
      }
    }
  }

  Map<String, List<FoodEntry>> _groupByMealType(List<FoodEntry> entries) {
    final Map<String, List<FoodEntry>> grouped = {};
    for (var entry in entries) {
      if (!grouped.containsKey(entry.mealType)) {
        grouped[entry.mealType] = [];
      }
      grouped[entry.mealType]!.add(entry);
    }
    return grouped;
  }

  String _getMealTypeLabel(String mealType) {
    const labels = {
      'Breakfast': 'Bữa sáng',
      'Lunch': 'Bữa trưa',
      'Dinner': 'Bữa tối',
      'Snack': 'Ăn vặt',
    };
    return labels[mealType] ?? mealType;
  }

  Color _getMealTypeColor(String mealType) {
    const colors = {
      'Breakfast': Color(0xFFFFA726),
      'Lunch': Color(0xFF66BB6A),
      'Dinner': Color(0xFF42A5F5),
      'Snack': Color(0xFFAB47BC),
    };
    return colors[mealType] ?? Colors.grey;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFD9D7B6),
      appBar: AppBar(
        title: const Text(
          'Nhật ký Dinh dưỡng',
          style: TextStyle(
            fontFamily: 'Estedad-VF',
            fontWeight: FontWeight.bold,
            color: Colors.black,
            fontSize: 24,
          ),
        ),
        backgroundColor: const Color(0xFFD9D7B6),
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.history, color: Colors.black),
            tooltip: 'Lịch sử',
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) => const NutritionHistoryScreen(),
                ),
              );
            },
          ),
          IconButton(
            icon: const Icon(Icons.calendar_today, color: Colors.black),
            onPressed: () async {
              final date = await showDatePicker(
                context: context,
                initialDate: _selectedDate,
                firstDate: DateTime(2020),
                lastDate: DateTime.now(),
                builder: (context, child) {
                  return Theme(
                    data: Theme.of(context).copyWith(
                      colorScheme: const ColorScheme.light(
                        primary: Color(0xFFA4C639),
                        onPrimary: Colors.white,
                        onSurface: Colors.black,
                      ),
                    ),
                    child: child!,
                  );
                },
              );
              if (date != null) {
                setState(() => _selectedDate = date);
                _loadNutritionLog();
              }
            },
          ),
        ],
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _loadNutritionLog,
              child: SingleChildScrollView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Date selector
                    Container(
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: const Color(0xFFFDFBD4),
                        borderRadius: BorderRadius.circular(20),
                        boxShadow: [
                          BoxShadow(
                            color: Colors.black.withOpacity(0.05),
                            blurRadius: 10,
                            offset: const Offset(0, 4),
                          ),
                        ],
                      ),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(
                            DateFormat('dd/MM/yyyy').format(_selectedDate),
                            style: const TextStyle(
                              fontFamily: 'Estedad-VF',
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                          Text(
                            _selectedDate.day == DateTime.now().day &&
                                    _selectedDate.month == DateTime.now().month &&
                                    _selectedDate.year == DateTime.now().year
                                ? 'Hôm nay'
                                : '',
                            style: const TextStyle(
                              fontFamily: 'Estedad-VF',
                              color: Color(0xFF8BA655), // Olive Green
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 16),

                    // Summary card
                    Container(
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
                        children: [
                          const Text(
                            'Tổng Calo Hôm Nay',
                            style: TextStyle(
                              fontFamily: 'Estedad-VF',
                              fontSize: 16,
                              color: Colors.black87,
                              fontWeight: FontWeight.w500,
                            ),
                          ),
                          const SizedBox(height: 8),
                          Row(
                            mainAxisAlignment: MainAxisAlignment.center,
                            crossAxisAlignment: CrossAxisAlignment.baseline,
                            textBaseline: TextBaseline.alphabetic,
                            children: [
                              Text(
                                '${_nutritionLog?.totalCalories.toInt() ?? 0}',
                                style: const TextStyle(
                                  fontFamily: 'Estedad-VF',
                                  fontSize: 48,
                                  fontWeight: FontWeight.bold,
                                  color: Colors.black,
                                ),
                              ),
                              const SizedBox(width: 4),
                              const Text(
                                'kcal',
                                style: TextStyle(
                                  fontFamily: 'Estedad-VF',
                                  fontSize: 16,
                                  color: Colors.black54,
                                ),
                              ),
                            ],
                          ),
                          const SizedBox(height: 20),
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                            children: [
                              _buildCircularNutrientInfo(
                                'Protein',
                                _nutritionLog?.proteinG ?? 0,
                                50, // Target value
                                const Color(0xFFFF6B6B),
                              ),
                              _buildCircularNutrientInfo(
                                'Carbs',
                                _nutritionLog?.carbsG ?? 0,
                                200, // Target value
                                const Color(0xFFFFA726),
                              ),
                              _buildCircularNutrientInfo(
                                'Fat',
                                _nutritionLog?.fatG ?? 0,
                                60, // Target value
                                const Color(0xFF66BB6A),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 24),

                    // Meal sections
                    if (_nutritionLog != null && _nutritionLog!.foodEntries.isNotEmpty)
                      ..._buildMealSections()
                    else
                      _buildEmptyState(),
                  ],
                ),
              ),
            ),
      floatingActionButton: FloatingActionButton(
        heroTag: 'nutrition_fab',
        onPressed: _showAddFoodDialog,
        backgroundColor: const Color(0xFF2d2d2d),
        foregroundColor: const Color(0xFFFDFBD4),
        child: const Icon(Icons.add),
      ),
    );
  }

  Widget _buildNutrientInfo(String label, String value, Color color) {
    return Column(
      children: [
        Text(
          value,
          style: TextStyle(
            fontFamily: 'Estedad-VF',
            fontSize: 20,
            fontWeight: FontWeight.bold,
            color: color,
          ),
        ),
        const SizedBox(height: 4),
        Text(
          label,
          style: const TextStyle(
            fontFamily: 'Estedad-VF',
            fontSize: 12,
            color: Colors.black54,
            fontWeight: FontWeight.w500,
          ),
        ),
      ],
    );
  }

  Widget _buildCircularNutrientInfo(
    String label,
    double currentValue,
    double targetValue,
    Color color,
  ) {
    final progress = (currentValue / targetValue).clamp(0.0, 1.0);
    
    return Column(
      children: [
        SizedBox(
          width: 80,
          height: 80,
          child: Stack(
            alignment: Alignment.center,
            children: [
              // Background circle
              SizedBox(
                width: 80,
                height: 80,
                child: CustomPaint(
                  painter: _CircularProgressPainter(
                    progress: 1.0,
                    color: Colors.black.withOpacity(0.1),
                    strokeWidth: 6,
                  ),
                ),
              ),
              // Progress circle
              SizedBox(
                width: 80,
                height: 80,
                child: CustomPaint(
                  painter: _CircularProgressPainter(
                    progress: progress,
                    color: color,
                    strokeWidth: 6,
                  ),
                ),
              ),
              // Value in center
              Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    currentValue.toStringAsFixed(1),
                    style: TextStyle(
                      fontFamily: 'Estedad-VF',
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                      color: color,
                    ),
                  ),
                  Text(
                    'g',
                    style: TextStyle(
                      fontFamily: 'Estedad-VF',
                      fontSize: 10,
                      color: Colors.grey[600],
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
        const SizedBox(height: 8),
        Text(
          label,
          style: const TextStyle(
            fontFamily: 'Estedad-VF',
            fontSize: 12,
            color: Colors.black87,
            fontWeight: FontWeight.w600,
          ),
        ),
        Text(
          '${(progress * 100).toInt()}%',
          style: TextStyle(
            fontFamily: 'Estedad-VF',
            fontSize: 10,
            color: Colors.grey[600],
          ),
        ),
      ],
    );
  }

  List<Widget> _buildMealSections() {
    final grouped = _groupByMealType(_nutritionLog!.foodEntries);
    final mealOrder = ['Breakfast', 'Lunch', 'Dinner', 'Snack'];
    
    return mealOrder.where((meal) => grouped.containsKey(meal)).map((mealType) {
      final entries = grouped[mealType]!;
      final totalCalories = entries.fold<double>(
        0,
        (sum, entry) => sum + entry.caloriesKcal,
      );

      return Container(
        margin: const EdgeInsets.only(bottom: 16),
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
            Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Row(
                    children: [
                      Container(
                        width: 4,
                        height: 24,
                        decoration: BoxDecoration(
                          color: _getMealTypeColor(mealType),
                          borderRadius: BorderRadius.circular(4),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Text(
                        _getMealTypeLabel(mealType),
                        style: const TextStyle(
                          fontFamily: 'Estedad-VF',
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                  Text(
                    '${totalCalories.toInt()} kcal',
                    style: TextStyle(
                      fontFamily: 'Estedad-VF',
                      fontSize: 14,
                      color: Colors.grey[700],
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
            ),
            const Divider(height: 1, color: Colors.black12),
            ...entries.map((entry) => _buildFoodEntryItem(entry)),
          ],
        ),
      );
    }).toList();
  }

  Widget _buildFoodEntryItem(FoodEntry entry) {
    return ListTile(
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      leading: CircleAvatar(
        radius: 28,
        backgroundColor: const Color(0xFFA4C639).withValues(alpha: 0.2),
        backgroundImage: entry.imageUrl != null
            ? NetworkImage(entry.imageUrl!)
            : null,
        child: entry.imageUrl == null
            ? const Icon(
                Icons.restaurant,
                color: Color(0xFFA4C639),
                size: 26,
              )
            : null,
      ),
      title: Text(
        entry.foodItemName,
        style: const TextStyle(fontFamily: 'Estedad-VF', fontWeight: FontWeight.w600, fontSize: 16),
      ),
      subtitle: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const SizedBox(height: 4),
          Text(
            'Số lượng: ${entry.quantity.toStringAsFixed(1)}',
            style: const TextStyle(fontFamily: 'Estedad-VF', fontSize: 13),  
          ),
          Text(
            'P: ${entry.proteinG.toStringAsFixed(1)}g | C: ${entry.carbsG.toStringAsFixed(1)}g | F: ${entry.fatG.toStringAsFixed(1)}g',
            style: TextStyle(fontFamily: 'Estedad-VF', fontSize: 12, color: Colors.grey[600]),
          ),
        ],
      ),
      trailing: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            '${entry.caloriesKcal.toInt()} kcal',
            style: const TextStyle(
              fontFamily: 'Estedad-VF',
              fontWeight: FontWeight.bold,
              color: Color(0xFFA4C639),
            ),
          ),
          const SizedBox(width: 8),
          IconButton(
            icon: const Icon(Icons.delete_outline, color: Colors.red),
            onPressed: () => _deleteFoodEntry(entry.foodEntryId),
          ),
        ],
      ),
    );
  }

  Widget _buildEmptyState() {
    return Container(
      padding: const EdgeInsets.all(48),
      decoration: BoxDecoration(
       color: const Color(0xFFFDFBD4),
        borderRadius: BorderRadius.circular(32),
      ),
      child: Column(
        children: [
          Icon(Icons.restaurant_menu, size: 64, color: Colors.grey[400]),
          const SizedBox(height: 16),
          Text(
            'Chưa có món ăn nào',
            style: TextStyle(
              fontFamily: 'Estedad-VF',
              fontSize: 18,
              color: Colors.grey[600],
            ),
          ),
          const SizedBox(height: 8),
          Text(
            'Nhấn nút + để thêm món ăn',
            style: TextStyle(
              fontFamily: 'Estedad-VF',
              fontSize: 14,
              color: Colors.grey[500],
            ),
          ),
        ],
      ),
    );
  }
}

class AddFoodBottomSheet extends StatefulWidget {
  final DateTime date;

  const AddFoodBottomSheet({super.key, required this.date});

  @override
  State<AddFoodBottomSheet> createState() => _AddFoodBottomSheetState();
}

class _AddFoodBottomSheetState extends State<AddFoodBottomSheet> {
  final NutritionService _nutritionService = NutritionService();
  final TextEditingController _searchController = TextEditingController();
  List<FoodItem> _foodItems = [];
  FoodItem? _selectedFood;
  String _selectedMealType = 'Breakfast';
  double _quantity = 1.0;
  bool _isLoading = false;
  bool _isAdding = false;

  @override
  void initState() {
    super.initState();
    _loadFoodItems();
  }

  Future<void> _loadFoodItems([String? search]) async {
    setState(() => _isLoading = true);
    try {
      final items = await _nutritionService.getFoodItems(search: search);
      setState(() {
        _foodItems = items;
        _isLoading = false;
      });
    } catch (e) {
      setState(() => _isLoading = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Không thể tải danh sách món ăn')),
        );
      }
    }
  }

  Future<void> _addFood() async {
    if (_selectedFood == null) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Vui lòng chọn món ăn')),
        );
      }
      return;
    }

    setState(() => _isAdding = true);
    try {
      await _nutritionService.addFoodEntry(
        foodItemId: _selectedFood!.foodItemId,
        quantity: _quantity,
        mealType: _selectedMealType,
        logDate: widget.date,
      );
      if (mounted) {
        Navigator.pop(context, true);
      }
    } catch (e) {
      setState(() => _isAdding = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Không thể thêm món ăn')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return DraggableScrollableSheet(
      initialChildSize: 0.9,
      minChildSize: 0.5,
      maxChildSize: 0.95,
      builder: (context, scrollController) {
        return Container(
          decoration: const BoxDecoration(
            color: Color(0xFFD9D7B6),
            borderRadius: BorderRadius.vertical(top: Radius.circular(30)),
          ),
          child: Column(
            children: [
              Container(
                margin: const EdgeInsets.symmetric(vertical: 12),
                width: 40,
                height: 4,
                decoration: BoxDecoration(
                  color: Colors.black.withOpacity(0.2),
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16),
                child: Column(
                  children: [
                    const Text(
                      'Thêm món ăn',
                      style: TextStyle(
                        fontFamily: 'Estedad-VF',
                        fontSize: 20,
                        fontWeight: FontWeight.bold,
                        color: Colors.black,
                      ),
                    ),
                    const SizedBox(height: 16),
                    TextField(
                      controller: _searchController,
                      style: const TextStyle(fontFamily: 'Estedad-VF'),
                      decoration: InputDecoration(
                        hintText: 'Tìm kiếm món ăn...',
                        hintStyle: const TextStyle(fontFamily: 'Estedad-VF'),
                        prefixIcon: const Icon(Icons.search),
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(20),
                          borderSide: BorderSide.none,
                        ),
                        filled: true,
                        fillColor: const Color(0xFFFDFBD4),
                      ),
                      onSubmitted: (value) => _loadFoodItems(value),
                    ),
                    const SizedBox(height: 12),
                    DropdownButtonFormField<String>(
                      value: _selectedMealType,
                      style: const TextStyle(fontFamily: 'Estedad-VF', color: Colors.black),
                      decoration: InputDecoration(
                        labelText: 'Bữa ăn',
                        labelStyle: const TextStyle(fontFamily: 'Estedad-VF'),
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(20),
                          borderSide: BorderSide.none,
                        ),
                        filled: true,
                        fillColor: const Color(0xFFFDFBD4),
                      ),
                      items: const [
                        DropdownMenuItem(value: 'Breakfast', child: Text('Bữa sáng')),
                        DropdownMenuItem(value: 'Lunch', child: Text('Bữa trưa')),
                        DropdownMenuItem(value: 'Dinner', child: Text('Bữa tối')),
                        DropdownMenuItem(value: 'Snack', child: Text('Ăn vặt')),
                      ],
                      onChanged: (value) {
                        if (value != null) {
                          setState(() => _selectedMealType = value);
                        }
                      },
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 16),
              Expanded(
                child: _isLoading
                    ? const Center(child: CircularProgressIndicator())
                    : ListView.builder(
                        controller: scrollController,
                        padding: const EdgeInsets.symmetric(horizontal: 16),
                        itemCount: _foodItems.length,
                        itemBuilder: (context, index) {
                          final item = _foodItems[index];
                          final isSelected = _selectedFood?.foodItemId == item.foodItemId;
                          
                          return Card(
                            margin: const EdgeInsets.only(bottom: 12),
                            color: const Color(0xFFFDFBD4),
                            elevation: 0,
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(20),
                              side: BorderSide(
                                color: isSelected ? const Color(0xFFA4C639) : Colors.transparent,
                                width: 2,
                              ),
                            ),
                            child: ListTile(
                              contentPadding: const EdgeInsets.all(16),
                              leading: CircleAvatar(
                                radius: 30,
                                backgroundColor: const Color(0xFFA4C639).withValues(alpha: 0.2),
                                backgroundImage: item.imageUrl != null
                                    ? NetworkImage(item.imageUrl!)
                                    : null,
                                child: item.imageUrl == null
                                    ? const Icon(
                                        Icons.restaurant,
                                        color: Color(0xFFA4C639),
                                        size: 28,
                                      )
                                    : null,
                              ),
                              title: Text(
                                item.name,
                                style: const TextStyle(fontFamily: 'Estedad-VF', fontWeight: FontWeight.w700),
                              ),
                              subtitle: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  const SizedBox(height: 4),
                                  Text('${item.servingSize.toInt()} ${item.servingUnit}', style: const TextStyle(fontFamily: 'Estedad-VF')),
                                  const SizedBox(height: 2),
                                  Text(
                                    'P: ${item.proteinG.toStringAsFixed(1)}g | C: ${item.carbsG.toStringAsFixed(1)}g | F: ${item.fatG.toStringAsFixed(1)}g',
                                    style: const TextStyle(fontFamily: 'Estedad-VF', fontSize: 12),
                                  ),
                                ],
                              ),
                              trailing: Text(
                                '${item.caloriesKcal.toInt()}\nkcal',
                                textAlign: TextAlign.center,
                                style: const TextStyle(
                                  fontFamily: 'Estedad-VF',
                                  fontWeight: FontWeight.bold,
                                  color: Color(0xFFA4C639),
                                ),
                              ),
                              onTap: () => setState(() => _selectedFood = item),
                            ),
                          );
                        },
                      ),
              ),
              if (_selectedFood != null)
                Container(
                  padding: const EdgeInsets.all(20),
                  decoration: BoxDecoration(
                    color: const Color(0xFFFDFBD4),
                    borderRadius: const BorderRadius.vertical(top: Radius.circular(30)),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withOpacity(0.1),
                        blurRadius: 10,
                        offset: const Offset(0, -5),
                      ),
                    ],
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      const Text(
                        'Số lượng',
                        style: TextStyle(
                          fontFamily: 'Estedad-VF',
                          fontSize: 16,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                      const SizedBox(height: 8),
                      Row(
                        children: [
                          Expanded(
                            child: Slider(
                              value: _quantity,
                              min: 0.1,
                              max: 10.0,
                              divisions: 99,
                              label: _quantity.toStringAsFixed(1),
                              activeColor: const Color(0xFFA4C639),
                              inactiveColor: Colors.grey[300],
                              onChanged: (value) => setState(() => _quantity = value),
                            ),
                          ),
                          Text(
                            _quantity.toStringAsFixed(1),
                            style: const TextStyle(
                              fontFamily: 'Estedad-VF',
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ],
                      ),
                      Container(
                        padding: const EdgeInsets.all(16),
                        decoration: BoxDecoration(
                          color: const Color(0xFFD9D7B6).withOpacity(0.5),
                          borderRadius: BorderRadius.circular(16),
                        ),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const Text(
                              'Tổng dinh dưỡng:',
                              style: TextStyle(fontFamily: 'Estedad-VF', fontWeight: FontWeight.w700),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              'Calories: ${(_selectedFood!.caloriesKcal * _quantity).toStringAsFixed(1)} kcal',
                              style: const TextStyle(fontFamily: 'Estedad-VF'),
                            ),
                            Text(
                              'P: ${(_selectedFood!.proteinG * _quantity).toStringAsFixed(1)}g | '
                              'C: ${(_selectedFood!.carbsG * _quantity).toStringAsFixed(1)}g | '
                              'F: ${(_selectedFood!.fatG * _quantity).toStringAsFixed(1)}g',
                              style: const TextStyle(fontFamily: 'Estedad-VF'),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 20),
                      SizedBox(
                        width: double.infinity,
                        child: ElevatedButton(
                          onPressed: _isAdding ? null : _addFood,
                          style: ElevatedButton.styleFrom(
                            backgroundColor: const Color(0xFF2d2d2d),
                            foregroundColor: const Color(0xFFFDFBD4),
                            padding: const EdgeInsets.symmetric(vertical: 16),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(16),
                            ),
                          ),
                          child: Text(
                            _isAdding ? 'Đang thêm...' : 'Thêm vào nhật ký',
                            style: const TextStyle(
                              fontFamily: 'Estedad-VF',
                              fontSize: 16,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
            ],
          ),
        );
      },
    );
  }
}

// Custom painter for circular progress indicator
class _CircularProgressPainter extends CustomPainter {
  final double progress;
  final Color color;
  final double strokeWidth;

  _CircularProgressPainter({
    required this.progress,
    required this.color,
    required this.strokeWidth,
  });

  @override
  void paint(Canvas canvas, Size size) {
    final center = Offset(size.width / 2, size.height / 2);
    final radius = (size.width - strokeWidth) / 2;
    
    final paint = Paint()
      ..color = color
      ..strokeWidth = strokeWidth
      ..style = PaintingStyle.stroke
      ..strokeCap = StrokeCap.round;

    // Draw arc
    canvas.drawArc(
      Rect.fromCircle(center: center, radius: radius),
      -math.pi / 2, // Start from top
      2 * math.pi * progress, // Sweep angle based on progress
      false,
      paint,
    );
  }

  @override
  bool shouldRepaint(_CircularProgressPainter oldDelegate) {
    return oldDelegate.progress != progress ||
        oldDelegate.color != color ||
        oldDelegate.strokeWidth != strokeWidth;
  }
}
