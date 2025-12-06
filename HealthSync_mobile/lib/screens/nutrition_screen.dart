import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../services/nutrition_service.dart';

class NutritionScreen extends StatefulWidget {
  const NutritionScreen({super.key});

  @override
  State<NutritionScreen> createState() => _NutritionScreenState();
}

class _NutritionScreenState extends State<NutritionScreen> {
  final NutritionService _nutritionService = NutritionService();
  DateTime _selectedDate = DateTime.now();
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
      _loadNutritionLog();
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
      backgroundColor: Color(0xFFF5F3E8),
      appBar: AppBar(
        title: Text('Nhật ký Dinh dưỡng'),
        backgroundColor: Color(0xFFB8C5A0),
        actions: [
          IconButton(
            icon: Icon(Icons.calendar_today),
            onPressed: () async {
              final date = await showDatePicker(
                context: context,
                initialDate: _selectedDate,
                firstDate: DateTime(2020),
                lastDate: DateTime.now(),
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
          ? Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _loadNutritionLog,
              child: SingleChildScrollView(
                physics: AlwaysScrollableScrollPhysics(),
                padding: EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Date selector
                    Container(
                      padding: EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(16),
                      ),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(
                            DateFormat('dd/MM/yyyy').format(_selectedDate),
                            style: TextStyle(
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
                            style: TextStyle(
                              color: Color(0xFFB8C5A0),
                              fontWeight: FontWeight.w500,
                            ),
                          ),
                        ],
                      ),
                    ),
                    SizedBox(height: 16),

                    // Summary card
                    Container(
                      padding: EdgeInsets.all(20),
                      decoration: BoxDecoration(
                        color: Color(0xFFFFF8E1),
                        borderRadius: BorderRadius.circular(16),
                      ),
                      child: Column(
                        children: [
                          Text(
                            'Tổng Calo Hôm Nay',
                            style: TextStyle(
                              fontSize: 16,
                              color: Colors.grey[600],
                            ),
                          ),
                          SizedBox(height: 8),
                          Text(
                            '${_nutritionLog?.totalCalories.toInt() ?? 0}',
                            style: TextStyle(
                              fontSize: 48,
                              fontWeight: FontWeight.bold,
                              color: Color(0xFFFFA726),
                            ),
                          ),
                          Text(
                            'kcal',
                            style: TextStyle(
                              fontSize: 16,
                              color: Colors.grey[600],
                            ),
                          ),
                          SizedBox(height: 20),
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                            children: [
                              _buildNutrientInfo(
                                'Protein',
                                '${_nutritionLog?.proteinG.toInt() ?? 0}g',
                                Color(0xFFEC407A),
                              ),
                              _buildNutrientInfo(
                                'Carbs',
                                '${_nutritionLog?.carbsG.toInt() ?? 0}g',
                                Color(0xFF42A5F5),
                              ),
                              _buildNutrientInfo(
                                'Fat',
                                '${_nutritionLog?.fatG.toInt() ?? 0}g',
                                Color(0xFFFFA726),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                    SizedBox(height: 24),

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
        onPressed: _showAddFoodDialog,
        backgroundColor: Color(0xFFB8C5A0),
        child: Icon(Icons.add),
      ),
    );
  }

  Widget _buildNutrientInfo(String label, String value, Color color) {
    return Column(
      children: [
        Text(
          value,
          style: TextStyle(
            fontSize: 20,
            fontWeight: FontWeight.bold,
            color: color,
          ),
        ),
        SizedBox(height: 4),
        Text(
          label,
          style: TextStyle(
            fontSize: 12,
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
        margin: EdgeInsets.only(bottom: 16),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Padding(
              padding: EdgeInsets.all(16),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Row(
                    children: [
                      Container(
                        width: 8,
                        height: 24,
                        decoration: BoxDecoration(
                          color: _getMealTypeColor(mealType),
                          borderRadius: BorderRadius.circular(4),
                        ),
                      ),
                      SizedBox(width: 12),
                      Text(
                        _getMealTypeLabel(mealType),
                        style: TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                  Text(
                    '${totalCalories.toInt()} kcal',
                    style: TextStyle(
                      fontSize: 14,
                      color: Colors.grey[600],
                      fontWeight: FontWeight.w500,
                    ),
                  ),
                ],
              ),
            ),
            Divider(height: 1),
            ...entries.map((entry) => _buildFoodEntryItem(entry)),
          ],
        ),
      );
    }).toList();
  }

  Widget _buildFoodEntryItem(FoodEntry entry) {
    return ListTile(
      contentPadding: EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      title: Text(
        entry.foodItemName,
        style: TextStyle(fontWeight: FontWeight.w500),
      ),
      subtitle: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(height: 4),
          Text('Số lượng: ${entry.quantity.toStringAsFixed(1)}'),
          Text(
            'P: ${entry.proteinG.toInt()}g | C: ${entry.carbsG.toInt()}g | F: ${entry.fatG.toInt()}g',
            style: TextStyle(fontSize: 12, color: Colors.grey[600]),
          ),
        ],
      ),
      trailing: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            '${entry.caloriesKcal.toInt()} kcal',
            style: TextStyle(
              fontWeight: FontWeight.bold,
              color: Color(0xFFFFA726),
            ),
          ),
          SizedBox(width: 8),
          IconButton(
            icon: Icon(Icons.delete_outline, color: Colors.red),
            onPressed: () => _deleteFoodEntry(entry.foodEntryId),
          ),
        ],
      ),
    );
  }

  Widget _buildEmptyState() {
    return Container(
      padding: EdgeInsets.all(48),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        children: [
          Icon(Icons.restaurant_menu, size: 64, color: Colors.grey[400]),
          SizedBox(height: 16),
          Text(
            'Chưa có món ăn nào',
            style: TextStyle(
              fontSize: 18,
              color: Colors.grey[600],
            ),
          ),
          SizedBox(height: 8),
          Text(
            'Nhấn nút + để thêm món ăn',
            style: TextStyle(
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
          decoration: BoxDecoration(
            color: Color(0xFFF5F3E8),
            borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
          ),
          child: Column(
            children: [
              Container(
                margin: EdgeInsets.symmetric(vertical: 12),
                width: 40,
                height: 4,
                decoration: BoxDecoration(
                  color: Colors.grey[300],
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              Padding(
                padding: EdgeInsets.symmetric(horizontal: 16),
                child: Column(
                  children: [
                    Text(
                      'Thêm món ăn',
                      style: TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    SizedBox(height: 16),
                    TextField(
                      controller: _searchController,
                      decoration: InputDecoration(
                        hintText: 'Tìm kiếm món ăn...',
                        prefixIcon: Icon(Icons.search),
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                        filled: true,
                        fillColor: Colors.white,
                      ),
                      onSubmitted: (value) => _loadFoodItems(value),
                    ),
                    SizedBox(height: 12),
                    DropdownButtonFormField<String>(
                      initialValue: _selectedMealType,
                      decoration: InputDecoration(
                        labelText: 'Bữa ăn',
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                        filled: true,
                        fillColor: Colors.white,
                      ),
                      items: [
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
              SizedBox(height: 16),
              Expanded(
                child: _isLoading
                    ? Center(child: CircularProgressIndicator())
                    : ListView.builder(
                        controller: scrollController,
                        padding: EdgeInsets.symmetric(horizontal: 16),
                        itemCount: _foodItems.length,
                        itemBuilder: (context, index) {
                          final item = _foodItems[index];
                          final isSelected = _selectedFood?.foodItemId == item.foodItemId;
                          
                          return Card(
                            margin: EdgeInsets.only(bottom: 12),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(12),
                              side: BorderSide(
                                color: isSelected ? Color(0xFFB8C5A0) : Colors.transparent,
                                width: 2,
                              ),
                            ),
                            child: ListTile(
                              contentPadding: EdgeInsets.all(12),
                              title: Text(
                                item.name,
                                style: TextStyle(fontWeight: FontWeight.w600),
                              ),
                              subtitle: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  SizedBox(height: 4),
                                  Text('${item.servingSize.toInt()} ${item.servingUnit}'),
                                  SizedBox(height: 2),
                                  Text(
                                    'P: ${item.proteinG.toInt()}g | C: ${item.carbsG.toInt()}g | F: ${item.fatG.toInt()}g',
                                    style: TextStyle(fontSize: 12),
                                  ),
                                ],
                              ),
                              trailing: Text(
                                '${item.caloriesKcal.toInt()}\nkcal',
                                textAlign: TextAlign.center,
                                style: TextStyle(
                                  fontWeight: FontWeight.bold,
                                  color: Color(0xFFFFA726),
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
                  padding: EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black12,
                        blurRadius: 8,
                        offset: Offset(0, -2),
                      ),
                    ],
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        'Số lượng',
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                      SizedBox(height: 8),
                      Row(
                        children: [
                          Expanded(
                            child: Slider(
                              value: _quantity,
                              min: 0.1,
                              max: 10.0,
                              divisions: 99,
                              label: _quantity.toStringAsFixed(1),
                              activeColor: Color(0xFFB8C5A0),
                              onChanged: (value) => setState(() => _quantity = value),
                            ),
                          ),
                          Text(
                            _quantity.toStringAsFixed(1),
                            style: TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ],
                      ),
                      Container(
                        padding: EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: Color(0xFFFFF8E1),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Tổng dinh dưỡng:',
                              style: TextStyle(fontWeight: FontWeight.w600),
                            ),
                            SizedBox(height: 4),
                            Text(
                              'Calories: ${(_selectedFood!.caloriesKcal * _quantity).toInt()} kcal',
                            ),
                            Text(
                              'P: ${(_selectedFood!.proteinG * _quantity).toInt()}g | '
                              'C: ${(_selectedFood!.carbsG * _quantity).toInt()}g | '
                              'F: ${(_selectedFood!.fatG * _quantity).toInt()}g',
                            ),
                          ],
                        ),
                      ),
                      SizedBox(height: 12),
                      SizedBox(
                        width: double.infinity,
                        child: ElevatedButton(
                          onPressed: _isAdding ? null : _addFood,
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Color(0xFFB8C5A0),
                            padding: EdgeInsets.symmetric(vertical: 16),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(12),
                            ),
                          ),
                          child: Text(
                            _isAdding ? 'Đang thêm...' : 'Thêm vào nhật ký',
                            style: TextStyle(
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
