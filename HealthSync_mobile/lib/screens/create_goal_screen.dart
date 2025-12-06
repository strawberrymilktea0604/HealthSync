import 'package:flutter/material.dart';
import '../models/goal_model.dart';
import '../services/goal_service.dart';

class CreateGoalScreen extends StatefulWidget {
  const CreateGoalScreen({super.key});

  @override
  State<CreateGoalScreen> createState() => _CreateGoalScreenState();
}

class _CreateGoalScreenState extends State<CreateGoalScreen> {
  final _formKey = GlobalKey<FormState>();
  final GoalService _goalService = GoalService();
  
  String? _selectedType;
  final TextEditingController _targetValueController = TextEditingController();
  DateTime _startDate = DateTime.now();
  DateTime? _endDate;
  final TextEditingController _notesController = TextEditingController();
  bool _isLoading = false;

  final List<Map<String, String>> _goalTypes = [
    {'value': 'weight_loss', 'label': 'Giảm cân'},
    {'value': 'weight_gain', 'label': 'Tăng cân'},
    {'value': 'muscle_gain', 'label': 'Tăng cơ'},
    {'value': 'fat_loss', 'label': 'Giảm mỡ'},
  ];

  @override
  void dispose() {
    _targetValueController.dispose();
    _notesController.dispose();
    super.dispose();
  }

  Future<void> _selectStartDate() async {
    final DateTime? picked = await showDatePicker(
      context: context,
      initialDate: _startDate,
      firstDate: DateTime(2020),
      lastDate: DateTime(2030),
    );
    if (picked != null && picked != _startDate) {
      setState(() {
        _startDate = picked;
      });
    }
  }

  Future<void> _selectEndDate() async {
    final DateTime? picked = await showDatePicker(
      context: context,
      initialDate: _endDate ?? _startDate.add(const Duration(days: 30)),
      firstDate: _startDate,
      lastDate: DateTime(2030),
    );
    if (picked != null) {
      setState(() {
        _endDate = picked;
      });
    }
  }

  Future<void> _saveGoal() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    if (_selectedType == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Vui lòng chọn loại mục tiêu')),
      );
      return;
    }

    setState(() => _isLoading = true);

    try {
      final request = CreateGoalRequest(
        type: _selectedType!,
        targetValue: double.parse(_targetValueController.text),
        startDate: _startDate,
        endDate: _endDate,
        notes: _notesController.text.isEmpty ? null : _notesController.text,
      );

      await _goalService.createGoal(request);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Đã tạo mục tiêu thành công')),
        );
        Navigator.pop(context, true);
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Không thể tạo mục tiêu: $e')),
        );
      }
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFE8E4D9),
      appBar: AppBar(
        backgroundColor: const Color(0xFFE8E4D9),
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Colors.black87),
          onPressed: () => Navigator.pop(context),
        ),
        title: const Text(
          'Tạo Mục Tiêu Mới',
          style: TextStyle(
            color: Colors.black87,
            fontSize: 20,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      body: SingleChildScrollView(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Goal Type Dropdown
                const Text(
                  'Loại mục tiêu',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w600,
                    color: Colors.black87,
                  ),
                ),
                const SizedBox(height: 8),
                Container(
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(15),
                  ),
                  child: DropdownButtonFormField<String>(
                    initialValue: _selectedType,
                    decoration: const InputDecoration(
                      hintText: 'Chọn loại mục tiêu',
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.all(Radius.circular(15)),
                        borderSide: BorderSide.none,
                      ),
                      contentPadding: EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 16,
                      ),
                    ),
                    items: _goalTypes.map((type) {
                      return DropdownMenuItem<String>(
                        value: type['value'],
                        child: Text(type['label']!),
                      );
                    }).toList(),
                    onChanged: (value) {
                      setState(() {
                        _selectedType = value;
                      });
                    },
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return 'Vui lòng chọn loại mục tiêu';
                      }
                      return null;
                    },
                  ),
                ),

                const SizedBox(height: 20),

                // Target Value
                const Text(
                  'Cân nặng mục tiêu',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w600,
                    color: Colors.black87,
                  ),
                ),
                const SizedBox(height: 8),
                Container(
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(15),
                  ),
                  child: TextFormField(
                    controller: _targetValueController,
                    keyboardType: const TextInputType.numberWithOptions(decimal: true),
                    decoration: const InputDecoration(
                      hintText: 'Ví dụ: 65',
                      suffixText: 'kg',
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.all(Radius.circular(15)),
                        borderSide: BorderSide.none,
                      ),
                      contentPadding: EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 16,
                      ),
                    ),
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return 'Vui lòng nhập cân nặng mục tiêu';
                      }
                      if (double.tryParse(value) == null) {
                        return 'Vui lòng nhập số hợp lệ';
                      }
                      return null;
                    },
                  ),
                ),

                const SizedBox(height: 20),

                // Date Pickers
                Row(
                  children: [
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Text(
                            'Ngày bắt đầu',
                            style: TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.w600,
                              color: Colors.black87,
                            ),
                          ),
                          const SizedBox(height: 8),
                          InkWell(
                            onTap: _selectStartDate,
                            child: Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 16,
                                vertical: 16,
                              ),
                              decoration: BoxDecoration(
                                color: Colors.white,
                                borderRadius: BorderRadius.circular(15),
                              ),
                              child: Row(
                                children: [
                                  const Icon(
                                    Icons.calendar_today,
                                    size: 20,
                                    color: Color(0xFF5FCCB4),
                                  ),
                                  const SizedBox(width: 12),
                                  Text(
                                    '${_startDate.day}/${_startDate.month}/${_startDate.year}',
                                    style: const TextStyle(
                                      fontSize: 14,
                                      color: Colors.black87,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(width: 16),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Text(
                            'Ngày kết thúc',
                            style: TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.w600,
                              color: Colors.black87,
                            ),
                          ),
                          const SizedBox(height: 8),
                          InkWell(
                            onTap: _selectEndDate,
                            child: Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 16,
                                vertical: 16,
                              ),
                              decoration: BoxDecoration(
                                color: Colors.white,
                                borderRadius: BorderRadius.circular(15),
                              ),
                              child: Row(
                                children: [
                                  const Icon(
                                    Icons.calendar_today,
                                    size: 20,
                                    color: Color(0xFF5FCCB4),
                                  ),
                                  const SizedBox(width: 12),
                                  Text(
                                    _endDate != null
                                        ? '${_endDate!.day}/${_endDate!.month}/${_endDate!.year}'
                                        : 'Chọn ngày',
                                    style: TextStyle(
                                      fontSize: 14,
                                      color: _endDate != null
                                          ? Colors.black87
                                          : Colors.grey[500],
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 20),

                // Notes
                const Text(
                  'Ghi chú',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w600,
                    color: Colors.black87,
                  ),
                ),
                const SizedBox(height: 8),
                Container(
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(15),
                  ),
                  child: TextFormField(
                    controller: _notesController,
                    maxLines: 4,
                    decoration: const InputDecoration(
                      hintText: 'Thêm ghi chú về mục tiêu của bạn...',
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.all(Radius.circular(15)),
                        borderSide: BorderSide.none,
                      ),
                      contentPadding: EdgeInsets.all(16),
                    ),
                  ),
                ),

                const SizedBox(height: 32),

                // Save Button
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: _isLoading ? null : _saveGoal,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color(0xFF5FCCB4),
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(15),
                      ),
                    ),
                    child: _isLoading
                        ? const SizedBox(
                            height: 20,
                            width: 20,
                            child: CircularProgressIndicator(
                              color: Colors.white,
                              strokeWidth: 2,
                            ),
                          )
                        : const Text(
                            'Lưu Mục Tiêu',
                            style: TextStyle(
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                              color: Colors.white,
                            ),
                          ),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
