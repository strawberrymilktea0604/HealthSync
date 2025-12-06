import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import 'home_screen.dart';
import 'complete_profile_screen.dart';

class CreatePasswordScreen extends StatefulWidget {
  final String userId;
  final String email;
  final String fullName;

  const CreatePasswordScreen({
    super.key,
    required this.userId,
    required this.email,
    required this.fullName,
  });

  @override
  State<CreatePasswordScreen> createState() => _CreatePasswordScreenState();
}

class _CreatePasswordScreenState extends State<CreatePasswordScreen>
    with SingleTickerProviderStateMixin {
  final TextEditingController _passwordController = TextEditingController();
  final TextEditingController _confirmPasswordController =
      TextEditingController();
  bool _isPasswordVisible = false;
  bool _isConfirmPasswordVisible = false;
  bool _isLoading = false;

  late AnimationController _controller;
  late Animation<double> _headerAnimation;
  late Animation<double> _formAnimation;
  late Animation<double> _buttonAnimation;

  @override
  void initState() {
    super.initState();

    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1200),
    );

    _headerAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(
        parent: _controller,
        curve: const Interval(0.0, 0.3, curve: Curves.easeOut),
      ),
    );

    _formAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(
        parent: _controller,
        curve: const Interval(0.2, 0.6, curve: Curves.easeOut),
      ),
    );

    _buttonAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(
        parent: _controller,
        curve: const Interval(0.5, 1.0, curve: Curves.easeOut),
      ),
    );

    _controller.forward();
  }

  @override
  void dispose() {
    _controller.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    super.dispose();
  }

  Future<void> _handleSubmit() async {
    if (_passwordController.text.isEmpty ||
        _confirmPasswordController.text.isEmpty) {
      _showErrorDialog('Vui lòng nhập đầy đủ thông tin');
      return;
    }

    if (_passwordController.text.length < 8) {
      _showErrorDialog('Mật khẩu phải có ít nhất 8 ký tự');
      return;
    }

    if (_passwordController.text != _confirmPasswordController.text) {
      _showErrorDialog('Mật khẩu không khớp');
      return;
    }

    setState(() {
      _isLoading = true;
    });

    try {
      final authProvider = Provider.of<AuthProvider>(context, listen: false);
      await authProvider.setPassword(
        userId: int.parse(widget.userId),
        password: _passwordController.text,
      );

      if (mounted) {
        // Show success message
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Tạo mật khẩu thành công!'),
            backgroundColor: Colors.green,
            duration: Duration(seconds: 2),
          ),
        );

        // Navigate based on profile completion
        final user = authProvider.user!;
        if (user.isProfileComplete || user.role == 'Admin') {
          Navigator.pushAndRemoveUntil(
            context,
            MaterialPageRoute(builder: (context) => const HomeScreen()),
            (route) => false,
          );
        } else {
          Navigator.pushAndRemoveUntil(
            context,
            MaterialPageRoute(builder: (context) => const CompleteProfileScreen()),
            (route) => false,
          );
        }
      }
    } catch (e) {
      if (mounted) {
        _showErrorDialog(e.toString().replaceAll('Exception: ', ''));
      }
    } finally {
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
      }
    }
  }

  void _showErrorDialog(String message) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('Lỗi'),
          content: Text(message),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('OK'),
            ),
          ],
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    final screenHeight = MediaQuery.of(context).size.height;
    final screenWidth = MediaQuery.of(context).size.width;

    return Scaffold(
      backgroundColor: const Color(0xFFD9D7B6),
      body: SafeArea(
        child: SingleChildScrollView(
          physics: const BouncingScrollPhysics(),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              // Header section with Animation
              FadeTransition(
                opacity: _headerAnimation,
                child: SlideTransition(
                  position: Tween<Offset>(
                    begin: const Offset(0, -0.3),
                    end: Offset.zero,
                  ).animate(
                    CurvedAnimation(
                      parent: _controller,
                      curve: const Interval(0.0, 0.3, curve: Curves.easeOut),
                    ),
                  ),
                  child: Padding(
                    padding: EdgeInsets.symmetric(
                      horizontal: screenWidth * 0.06,
                      vertical: screenHeight * 0.03,
                    ),
                    child: Column(
                      children: [
                        // App Logo
                        Image.asset(
                          'assets/images/logo.png',
                          width: screenWidth * 0.35,
                          height: screenWidth * 0.35,
                          fit: BoxFit.contain,
                        ),

                        SizedBox(height: screenHeight * 0.02),

                        // Title
                        const Text(
                          'Tạo mật khẩu',
                          style: TextStyle(
                            fontSize: 28,
                            fontWeight: FontWeight.bold,
                            color: Colors.black,
                          ),
                        ),

                        SizedBox(height: screenHeight * 0.015),

                        // Subtitle
                        Text(
                          'Xin chào ${widget.fullName}!\nVui lòng tạo mật khẩu cho tài khoản của bạn',
                          textAlign: TextAlign.center,
                          style: const TextStyle(
                            fontSize: 16,
                            color: Colors.black87,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),

              // Form section with Animation
              FadeTransition(
                opacity: _formAnimation,
                child: SlideTransition(
                  position: Tween<Offset>(
                    begin: const Offset(0, 0.3),
                    end: Offset.zero,
                  ).animate(_formAnimation),
                  child: Container(
                    margin: EdgeInsets.symmetric(
                      horizontal: screenWidth * 0.06,
                    ),
                    padding: EdgeInsets.all(screenWidth * 0.06),
                    decoration: BoxDecoration(
                      color: const Color(0xFFFDFBD4),
                      borderRadius: BorderRadius.circular(30),
                    ),
                    child: Column(
                      children: [
                        // Email display (read-only)
                        Container(
                          width: double.infinity,
                          padding: EdgeInsets.symmetric(
                            horizontal: screenWidth * 0.04,
                            vertical: screenHeight * 0.018,
                          ),
                          decoration: BoxDecoration(
                            color: const Color(0xFFE8E6C8),
                            borderRadius: BorderRadius.circular(15),
                            border: Border.all(
                              color: Colors.black.withValues(alpha: 0.1),
                              width: 1,
                            ),
                          ),
                          child: Row(
                            children: [
                              const Icon(Icons.email_outlined, size: 20),
                              const SizedBox(width: 12),
                              Expanded(
                                child: Text(
                                  widget.email,
                                  style: const TextStyle(
                                    fontSize: 16,
                                    color: Colors.black87,
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),

                        SizedBox(height: screenHeight * 0.025),

                        // Password field
                        TextField(
                          controller: _passwordController,
                          obscureText: !_isPasswordVisible,
                          decoration: InputDecoration(
                            hintText: 'Nhập mật khẩu (tối thiểu 8 ký tự)',
                            hintStyle: TextStyle(
                              color: Colors.black.withValues(alpha: 0.4),
                              fontSize: 16,
                            ),
                            filled: true,
                            fillColor: const Color(0xFFD9D7B6),
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(15),
                              borderSide: BorderSide(
                                color: Colors.white.withValues(alpha: 0.3),
                                width: 2,
                              ),
                            ),
                            enabledBorder: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(15),
                              borderSide: BorderSide(
                                color: Colors.white.withValues(alpha: 0.3),
                                width: 2,
                              ),
                            ),
                            focusedBorder: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(15),
                              borderSide: BorderSide(
                                color: Colors.white.withValues(alpha: 0.5),
                                width: 2,
                              ),
                            ),
                            suffixIcon: IconButton(
                              icon: Icon(
                                _isPasswordVisible
                                    ? Icons.visibility
                                    : Icons.visibility_off,
                                color: Colors.black54,
                              ),
                              onPressed: () {
                                setState(() {
                                  _isPasswordVisible = !_isPasswordVisible;
                                });
                              },
                            ),
                          ),
                        ),

                        SizedBox(height: screenHeight * 0.02),

                        // Confirm password field
                        TextField(
                          controller: _confirmPasswordController,
                          obscureText: !_isConfirmPasswordVisible,
                          decoration: InputDecoration(
                            hintText: 'Xác nhận mật khẩu',
                            hintStyle: TextStyle(
                              color: Colors.black.withValues(alpha: 0.4),
                              fontSize: 16,
                            ),
                            filled: true,
                            fillColor: const Color(0xFFD9D7B6),
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(15),
                              borderSide: BorderSide(
                                color: Colors.white.withValues(alpha: 0.3),
                                width: 2,
                              ),
                            ),
                            enabledBorder: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(15),
                              borderSide: BorderSide(
                                color: Colors.white.withValues(alpha: 0.3),
                                width: 2,
                              ),
                            ),
                            focusedBorder: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(15),
                              borderSide: BorderSide(
                                color: Colors.white.withValues(alpha: 0.5),
                                width: 2,
                              ),
                            ),
                            suffixIcon: IconButton(
                              icon: Icon(
                                _isConfirmPasswordVisible
                                    ? Icons.visibility
                                    : Icons.visibility_off,
                                color: Colors.black54,
                              ),
                              onPressed: () {
                                setState(() {
                                  _isConfirmPasswordVisible =
                                      !_isConfirmPasswordVisible;
                                });
                              },
                            ),
                          ),
                        ),

                        SizedBox(height: screenHeight * 0.03),

                        // Submit button with Animation
                        FadeTransition(
                          opacity: _buttonAnimation,
                          child: SlideTransition(
                            position: Tween<Offset>(
                              begin: const Offset(0, 0.3),
                              end: Offset.zero,
                            ).animate(_buttonAnimation),
                            child: SizedBox(
                              width: double.infinity,
                              child: ElevatedButton(
                                onPressed: _isLoading ? null : _handleSubmit,
                                style: ElevatedButton.styleFrom(
                                  backgroundColor: Colors.black,
                                  foregroundColor: Colors.white,
                                  padding: EdgeInsets.symmetric(
                                    vertical: screenHeight * 0.02,
                                  ),
                                  shape: RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(15),
                                  ),
                                  elevation: 2,
                                ),
                                child: _isLoading
                                    ? const SizedBox(
                                        height: 20,
                                        width: 20,
                                        child: CircularProgressIndicator(
                                          strokeWidth: 2,
                                          valueColor:
                                              AlwaysStoppedAnimation<Color>(
                                                  Colors.white),
                                        ),
                                      )
                                    : const Text(
                                        'Tạo mật khẩu',
                                        style: TextStyle(
                                          fontSize: 18,
                                          fontWeight: FontWeight.bold,
                                        ),
                                      ),
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),

              SizedBox(height: screenHeight * 0.03),
            ],
          ),
        ),
      ),
    );
  }
}
