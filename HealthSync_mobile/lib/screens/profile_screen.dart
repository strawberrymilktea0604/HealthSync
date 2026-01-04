import 'dart:io';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:image_picker/image_picker.dart';
import 'package:intl/intl.dart';
import '../providers/auth_provider.dart';
import '../screens/sign_in_screen.dart';
import '../widgets/custom_text_field.dart';
import '../widgets/custom_button.dart';
import '../services/api_service.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  final _apiService = ApiService();
  
  late TextEditingController _fullNameController;
  late TextEditingController _heightController;
  late TextEditingController _weightController;
  
  DateTime? _selectedDate;
  String? _selectedGender;
  String? _selectedActivityLevel;
  bool _isLoading = true;
  bool _isSaving = false;
  
  final ImagePicker _picker = ImagePicker();

  final List<String> _genders = ['Male', 'Female', 'Other'];
  final List<String> _activityLevels = [
    'Sedentary',
    'Light',
    'Moderate',
    'Active',
    'VeryActive'
  ];

  @override
  void initState() {
    super.initState();
    _fullNameController = TextEditingController();
    _heightController = TextEditingController();
    _weightController = TextEditingController();
    _loadProfile();
  }

  @override
  void dispose() {
    _fullNameController.dispose();
    _heightController.dispose();
    _weightController.dispose();
    super.dispose();
  }

  Future<void> _loadProfile() async {
    try {
      final profileData = await _apiService.getProfile();
      
      setState(() {
        _fullNameController.text = profileData['fullName'] ?? '';
        if (profileData['dob'] != null) {
          _selectedDate = DateTime.parse(profileData['dob']);
        }
        _selectedGender = profileData['gender'] ?? 'Male';
        _heightController.text = (profileData['heightCm'] ?? '').toString();
        _weightController.text = (profileData['weightKg'] ?? '').toString();
        _selectedActivityLevel = profileData['activityLevel'] ?? 'Moderate';
        _isLoading = false;
      });
    } catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Lỗi tải thông tin: $e')),
        );
      }
    }
  }

  Future<void> _pickImage() async {
    try {
      final XFile? image = await _picker.pickImage(source: ImageSource.gallery);
      if (image != null) {
        final authProvider = Provider.of<AuthProvider>(context, listen: false);
        await authProvider.uploadAvatar(File(image.path));
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Cập nhật avatar thành công!')),
          );
        }
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Lỗi: $e')),
        );
      }
    }
  }

  Future<void> _selectDate(BuildContext context) async {
    final DateTime? picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate ?? DateTime.now().subtract(const Duration(days: 365 * 25)),
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

    setState(() => _isSaving = true);

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
        gender: _selectedGender ?? 'Male',
        heightCm: height,
        weightKg: weight,
        activityLevel: _selectedActivityLevel ?? 'Moderate',
      );

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Cập nhật hồ sơ thành công!')),
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
        setState(() => _isSaving = false);
      }
    }
  }

  Future<void> _logout() async {
    try {
      final authProvider = Provider.of<AuthProvider>(context, listen: false);
      await authProvider.logout();
      if (mounted) {
        Navigator.of(context).pushAndRemoveUntil(
          MaterialPageRoute(builder: (context) => const SignInScreen()),
          (route) => false,
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Lỗi đăng xuất: $e')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final user = Provider.of<AuthProvider>(context).user;

    return Scaffold(
      backgroundColor: const Color(0xFFD9D7B6),
      appBar: AppBar(
        title: const Text('Hồ sơ cá nhân'),
        backgroundColor: Colors.transparent,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Colors.black),
          onPressed: () => Navigator.of(context).pop(),
        ),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : SafeArea(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(24.0),
                child: Form(
                  key: _formKey,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: [
                      // Avatar
                      Center(
                        child: Stack(
                          children: [
                            CircleAvatar(
                              radius: 60,
                              backgroundColor: Colors.grey[300],
                              backgroundImage: (user?.avatarUrl != null && user!.avatarUrl!.isNotEmpty)
                                  ? NetworkImage(user.avatarUrl!)
                                  : null,
                              child: (user?.avatarUrl == null || user!.avatarUrl!.isEmpty)
                                  ? const Icon(Icons.person, size: 60, color: Colors.grey)
                                  : null,
                            ),
                            Positioned(
                              bottom: 0,
                              right: 0,
                              child: CircleAvatar(
                                radius: 20,
                                backgroundColor: Colors.white,
                                child: IconButton(
                                  icon: const Icon(Icons.camera_alt, size: 20),
                                  onPressed: _pickImage,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 16),
                      Text(
                        user?.email ?? '',
                        style: const TextStyle(
                          fontSize: 16,
                          color: Colors.black54,
                        ),
                      ),
                      const SizedBox(height: 32),

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
                      Row(
                        children: [
                          const Text(
                            'Ngày sinh *',
                            style: TextStyle(fontSize: 16, fontWeight: FontWeight.w500),
                          ),
                          const Spacer(),
                          TextButton(
                            onPressed: () => _selectDate(context),
                            child: Text(
                              _selectedDate == null
                                  ? 'Chọn ngày'
                                  : DateFormat('dd/MM/yyyy').format(_selectedDate!),
                              style: const TextStyle(fontSize: 16, color: Colors.blue),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 20),

                      // Gender
                      DropdownButtonFormField<String>(
                        value: _selectedGender,
                        decoration: InputDecoration(
                          labelText: 'Giới tính',
                          filled: true,
                          fillColor: Colors.white,
                          border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                        ),
                        items: _genders.map((gender) {
                          return DropdownMenuItem(
                            value: gender,
                            child: Text(gender == 'Male' ? 'Nam' : gender == 'Female' ? 'Nữ' : 'Khác'),
                          );
                        }).toList(),
                        onChanged: (value) => setState(() => _selectedGender = value),
                      ),
                      const SizedBox(height: 20),

                      // Height & Weight
                      Row(
                        children: [
                          Expanded(
                            child: CustomTextField(
                              controller: _heightController,
                              label: 'Chiều cao (cm)',
                              hint: '170',
                              keyboardType: TextInputType.number,
                            ),
                          ),
                          const SizedBox(width: 16),
                          Expanded(
                            child: CustomTextField(
                              controller: _weightController,
                              label: 'Cân nặng (kg)',
                              hint: '70',
                              keyboardType: TextInputType.number,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 20),

                      // Activity Level
                      DropdownButtonFormField<String>(
                        value: _selectedActivityLevel,
                        decoration: InputDecoration(
                          labelText: 'Mức độ hoạt động',
                          filled: true,
                          fillColor: Colors.white,
                          border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                        ),
                        items: _activityLevels.map((level) {
                           String displayText;
                            switch (level) {
                              case 'Sedentary': displayText = 'Ít vận động'; break;
                              case 'Light': displayText = 'Vận động nhẹ'; break;
                              case 'Moderate': displayText = 'Vận động vừa phải'; break;
                              case 'Active': displayText = 'Vận động nhiều'; break;
                              case 'VeryActive': displayText = 'Vận động rất nhiều'; break;
                              default: displayText = level;
                            }
                          return DropdownMenuItem(
                            value: level,
                            child: Text(displayText),
                          );
                        }).toList(),
                        onChanged: (value) => setState(() => _selectedActivityLevel = value),
                      ),
                      
                      const SizedBox(height: 40),

                      // Save Button
                      CustomButton(
                        text: _isSaving ? 'Đang lưu...' : 'Lưu thay đổi',
                        onPressed: _isSaving ? () {} : _updateProfile,
                        isLoading: _isSaving,
                        backgroundColor: Colors.blueAccent,
                        textColor: Colors.white,
                      ),
                      
                      const SizedBox(height: 20),
                      
                      // Logout Button
                      CustomButton(
                        text: 'Đăng xuất',
                        onPressed: _logout,
                        backgroundColor: Colors.redAccent,
                        textColor: Colors.white,
                      ),
                      const SizedBox(height: 40),
                    ],
                  ),
                ),
              ),
            ),
    );
  }
}
