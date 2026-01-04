import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import '../widgets/custom_text_field.dart';
import '../widgets/custom_button.dart';
import 'home_screen.dart';

class CompleteProfileScreen extends StatefulWidget {
  const CompleteProfileScreen({super.key});

  @override
  State<CompleteProfileScreen> createState() => _CompleteProfileScreenState();
}

class _CompleteProfileScreenState extends State<CompleteProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  final _fullNameController = TextEditingController();
  final _heightController = TextEditingController();
  final _weightController = TextEditingController();
  DateTime? _selectedDate;
  String _selectedGender = 'Male';
  String _selectedActivityLevel = 'Moderate';
  bool _isLoading = false;

  final List<String> _genders = ['Male', 'Female', 'Other'];
  final List<String> _activityLevels = [
    'Sedentary',
    'Light',
    'Moderate',
    'Active',
    'VeryActive'
  ];

  @override
  void dispose() {
    _fullNameController.dispose();
    _heightController.dispose();
    _weightController.dispose();
    super.dispose();
  }

  Future<void> _selectDate(BuildContext context) async {
    final DateTime? picked = await showDatePicker(
      context: context,
      initialDate: DateTime.now().subtract(const Duration(days: 365 * 25)),
      firstDate: DateTime(1900),
      lastDate: DateTime.now(),
    );
    if (picked != null && picked != _selectedDate) {
      setState(() {
        _selectedDate = picked;
      });
    }
  }

  Future<void> _updateProfile() async {
    if (!_formKey.currentState!.validate()) return;

    if (_selectedDate == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Vui lòng chọn ngày sinh')),
      );
      return;
    }

    setState(() => _isLoading = true);

    try {
      final authProvider = Provider.of<AuthProvider>(context, listen: false);
      final height = double.tryParse(_heightController.text);
      final weight = double.tryParse(_weightController.text);

      if (height == null || height <= 0 || height > 300) {
        throw Exception('Chiều cao không hợp lệ');
      }

      if (weight == null || weight <= 0 || weight > 500) {
        throw Exception('Cân nặng không hợp lệ');
      }

      await authProvider.updateProfile(
        fullName: _fullNameController.text.trim(),
        dob: _selectedDate!,
        gender: _selectedGender,
        heightCm: height,
        weightKg: weight,
        activityLevel: _selectedActivityLevel,
      );

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Cập nhật hồ sơ thành công!')),
        );
        Navigator.of(context).pushAndRemoveUntil(
          MaterialPageRoute(builder: (context) => const HomeScreen()),
          (route) => false,
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Lỗi: ${e.toString()}')),
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
      backgroundColor: const Color(0xFFD9D7B6),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24.0),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const SizedBox(height: 40),
                const Text(
                  'Hoàn thiện hồ sơ',
                  style: TextStyle(
                    fontSize: 28,
                    fontWeight: FontWeight.bold,
                    color: Colors.black87,
                  ),
                ),
                const SizedBox(height: 8),
                const Text(
                  'Vui lòng điền đầy đủ thông tin để tiếp tục sử dụng ứng dụng',
                  style: TextStyle(
                    fontSize: 16,
                    color: Colors.black54,
                  ),
                ),
                const SizedBox(height: 40),

                // Full Name
                CustomTextField(
                  controller: _fullNameController,
                  label: 'Họ và tên *',
                  hint: 'Nhập họ và tên',
                  validator: (value) {
                    if (value == null || value.trim().isEmpty) {
                      return 'Vui lòng nhập họ tên';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 20),

                // Date of Birth
                const Text(
                  'Ngày sinh *',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w500,
                    color: Colors.black87,
                  ),
                ),
                const SizedBox(height: 8),
                InkWell(
                  onTap: () => _selectDate(context),
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: Colors.grey.shade300),
                    ),
                    child: Row(
                      children: [
                        Expanded(
                          child: Text(
                            _selectedDate == null
                                ? 'Chọn ngày sinh'
                                : '${_selectedDate!.day}/${_selectedDate!.month}/${_selectedDate!.year}',
                            style: TextStyle(
                              color: _selectedDate == null ? Colors.grey : Colors.black,
                              fontSize: 16,
                            ),
                          ),
                        ),
                        const Icon(Icons.calendar_today, color: Colors.grey),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 20),

                // Gender
                const Text(
                  'Giới tính *',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w500,
                    color: Colors.black87,
                  ),
                ),
                const SizedBox(height: 8),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 16),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: Colors.grey.shade300),
                  ),
                  child: DropdownButtonHideUnderline(
                    child: DropdownButton<String>(
                      value: _selectedGender,
                      isExpanded: true,
                      items: _genders.map((gender) {
                        return DropdownMenuItem(
                          value: gender,
                          child: Text(gender == 'Male' ? 'Nam' : gender == 'Female' ? 'Nữ' : 'Khác'),
                        );
                      }).toList(),
                      onChanged: (value) {
                        setState(() => _selectedGender = value!);
                      },
                    ),
                  ),
                ),
                const SizedBox(height: 20),

                // Height and Weight
                Row(
                  children: [
                    Expanded(
                      child: CustomTextField(
                        controller: _heightController,
                        label: 'Chiều cao (cm) *',
                        hint: '170',
                        keyboardType: TextInputType.number,
                        validator: (value) {
                          if (value == null || value.isEmpty) {
                            return 'Bắt buộc';
                          }
                          final height = double.tryParse(value);
                          if (height == null || height <= 0 || height > 300) {
                            return 'Không hợp lệ';
                          }
                          return null;
                        },
                      ),
                    ),
                    const SizedBox(width: 16),
                    Expanded(
                      child: CustomTextField(
                        controller: _weightController,
                        label: 'Cân nặng (kg) *',
                        hint: '70',
                        keyboardType: TextInputType.number,
                        validator: (value) {
                          if (value == null || value.isEmpty) {
                            return 'Bắt buộc';
                          }
                          final weight = double.tryParse(value);
                          if (weight == null || weight <= 0 || weight > 500) {
                            return 'Không hợp lệ';
                          }
                          return null;
                        },
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 20),

                // Activity Level
                const Text(
                  'Mức độ hoạt động',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w500,
                    color: Colors.black87,
                  ),
                ),
                const SizedBox(height: 8),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 16),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: Colors.grey.shade300),
                  ),
                  child: DropdownButtonHideUnderline(
                    child: DropdownButton<String>(
                      value: _selectedActivityLevel,
                      isExpanded: true,
                      items: _activityLevels.map((level) {
                        String displayText;
                        switch (level) {
                          case 'Sedentary':
                            displayText = 'Ít vận động';
                            break;
                          case 'Light':
                            displayText = 'Vận động nhẹ';
                            break;
                          case 'Moderate':
                            displayText = 'Vận động vừa phải';
                            break;
                          case 'Active':
                            displayText = 'Vận động nhiều';
                            break;
                          case 'VeryActive':
                            displayText = 'Vận động rất nhiều';
                            break;
                          default:
                            displayText = level;
                        }
                        return DropdownMenuItem(
                          value: level,
                          child: Text(displayText),
                        );
                      }).toList(),
                      onChanged: (value) {
                        setState(() => _selectedActivityLevel = value!);
                      },
                    ),
                  ),
                ),
                const SizedBox(height: 40),

                // Submit Button
                CustomButton(
                  text: _isLoading ? 'Đang cập nhật...' : 'Hoàn thành',
                  onPressed: _isLoading ? () {} : _updateProfile,
                  isLoading: _isLoading,
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}