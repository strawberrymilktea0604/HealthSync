import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import 'sign_up_screen.dart';
import 'account_recovery_screen.dart';
import 'home_screen.dart';
import 'create_password_screen.dart';

class SignInScreen extends StatefulWidget {
  const SignInScreen({super.key});

  @override
  State<SignInScreen> createState() => _SignInScreenState();
}

class _SignInScreenState extends State<SignInScreen> with SingleTickerProviderStateMixin {
  final TextEditingController _emailController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();
  bool _isPasswordVisible = false;
  
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
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
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
                        // Welcome back text
                        const Text(
                          'Welcome back!',
                          style: TextStyle(
                            fontSize: 24,
                            fontWeight: FontWeight.bold,
                            color: Colors.black,
                          ),
                        ),
                        
                        SizedBox(height: screenHeight * 0.015),
                        
                        // Logo healthsync
                        Image.asset(
                          'assets/images/logo.png',
                          width: screenWidth * 0.5,
                          height: screenHeight * 0.06,
                          fit: BoxFit.contain,
                        ),
                      ],
                    ),
                  ),
                ),
              ),
              
              SizedBox(height: screenHeight * 0.02),
              
              // Form container với bo góc - FORM GIỮ NGUYÊN, CHỈ ANIMATE NỘI DUNG BÊN TRONG
              Container(
                width: double.infinity,
                constraints: BoxConstraints(
                  minHeight: screenHeight * 0.7, // Chiếm ít nhất 70% chiều cao
                ),
                padding: EdgeInsets.symmetric(
                  horizontal: screenWidth * 0.08,
                  vertical: screenHeight * 0.04,
                ),
                decoration: BoxDecoration(
                  color: const Color(0xFFFDFBD4),
                  borderRadius: const BorderRadius.only(
                    topLeft: Radius.circular(40),
                    topRight: Radius.circular(40),
                  ),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withOpacity(0.15),
                      blurRadius: 30,
                      spreadRadius: 5,
                      offset: const Offset(0, -10), // Shadow hướng lên trên
                    ),
                  ],
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Email field with Animation
                    FadeTransition(
                      opacity: _formAnimation,
                      child: SlideTransition(
                        position: Tween<Offset>(
                          begin: const Offset(0, 0.2),
                          end: Offset.zero,
                        ).animate(
                          CurvedAnimation(
                            parent: _controller,
                            curve: const Interval(0.2, 0.5, curve: Curves.easeOut),
                          ),
                        ),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const Text(
                              'Your email address',
                              style: TextStyle(
                                fontSize: 12,
                                color: Colors.black87,
                                fontWeight: FontWeight.w500,
                              ),
                            ),
                            const SizedBox(height: 8),
                            TextField(
                              controller: _emailController,
                              decoration: InputDecoration(
                                filled: true,
                                fillColor: Colors.white.withOpacity(0.5),
                                border: OutlineInputBorder(
                                  borderRadius: BorderRadius.circular(12),
                                  borderSide: BorderSide.none,
                                ),
                                contentPadding: const EdgeInsets.symmetric(
                                  horizontal: 16,
                                  vertical: 14,
                                ),
                              ),
                              keyboardType: TextInputType.emailAddress,
                            ),
                          ],
                        ),
                      ),
                    ),
                    
                    const SizedBox(height: 20),
                    
                    // Password field with Animation
                    FadeTransition(
                      opacity: _formAnimation,
                      child: SlideTransition(
                        position: Tween<Offset>(
                          begin: const Offset(0, 0.2),
                          end: Offset.zero,
                        ).animate(
                          CurvedAnimation(
                            parent: _controller,
                            curve: const Interval(0.25, 0.55, curve: Curves.easeOut),
                          ),
                        ),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const Text(
                              'Enter a password (min. 8 characters)',
                              style: TextStyle(
                                fontSize: 12,
                                color: Colors.black87,
                                fontWeight: FontWeight.w500,
                              ),
                            ),
                            const SizedBox(height: 8),
                            TextField(
                              controller: _passwordController,
                              obscureText: !_isPasswordVisible,
                              decoration: InputDecoration(
                                filled: true,
                                fillColor: Colors.white.withOpacity(0.5),
                                border: OutlineInputBorder(
                                  borderRadius: BorderRadius.circular(12),
                                  borderSide: BorderSide.none,
                                ),
                                contentPadding: const EdgeInsets.symmetric(
                                  horizontal: 16,
                                  vertical: 14,
                                ),
                                suffixIcon: IconButton(
                                  icon: Icon(
                                    _isPasswordVisible
                                        ? Icons.visibility_off
                                        : Icons.visibility,
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
                          ],
                        ),
                      ),
                    ),
                    
                    const SizedBox(height: 12),
                    
                    // Forgot password with Animation
                    FadeTransition(
                      opacity: _formAnimation,
                      child: Align(
                        alignment: Alignment.centerRight,
                        child: TextButton(
                          onPressed: () {
                            // Navigate to account recovery screen
                            Navigator.push(
                              context,
                              MaterialPageRoute(
                                builder: (context) => const AccountRecoveryScreen(),
                              ),
                            );
                          },
                          child: const Text(
                            'Forgot password?',
                            style: TextStyle(
                              color: Colors.black87,
                              fontSize: 12,
                              decoration: TextDecoration.underline,
                            ),
                          ),
                        ),
                      ),
                    ),
                    
                    const SizedBox(height: 20),
                    
                    // Login button with Animation
                    FadeTransition(
                      opacity: _buttonAnimation,
                      child: SlideTransition(
                        position: Tween<Offset>(
                          begin: const Offset(0, 0.3),
                          end: Offset.zero,
                        ).animate(
                          CurvedAnimation(
                            parent: _controller,
                            curve: const Interval(0.5, 0.8, curve: Curves.easeOut),
                          ),
                        ),
                        child: SizedBox(
                          width: double.infinity,
                          height: 50,
                          child: ElevatedButton(
                            onPressed: () async {
                              final email = _emailController.text.trim();
                              final password = _passwordController.text;

                              if (email.isEmpty || password.isEmpty) {
                                ScaffoldMessenger.of(context).showSnackBar(
                                  const SnackBar(
                                    content: Text('Please fill in all fields'),
                                    backgroundColor: Colors.red,
                                  ),
                                );
                                return;
                              }

                              final authProvider = Provider.of<AuthProvider>(context, listen: false);
                              
                              try {
                                await authProvider.login(email, password);
                                
                                if (context.mounted) {
                                  Navigator.of(context).pushAndRemoveUntil(
                                    MaterialPageRoute(
                                      builder: (context) => const HomeScreen(),
                                    ),
                                    (route) => false,
                                  );
                                }
                              } catch (e) {
                                if (context.mounted) {
                                  ScaffoldMessenger.of(context).showSnackBar(
                                    SnackBar(
                                      content: Text(e.toString().replaceAll('Exception: ', '')),
                                      backgroundColor: Colors.red,
                                    ),
                                  );
                                }
                              }
                            },
                            style: ElevatedButton.styleFrom(
                              backgroundColor: Colors.transparent,
                              foregroundColor: Colors.black,
                              elevation: 0,
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(25),
                                side: const BorderSide(color: Colors.black, width: 2),
                              ),
                            ),
                            child: Consumer<AuthProvider>(
                              builder: (context, auth, child) {
                                if (auth.isLoading) {
                                  return const SizedBox(
                                    height: 20,
                                    width: 20,
                                    child: CircularProgressIndicator(
                                      strokeWidth: 2,
                                      valueColor: AlwaysStoppedAnimation<Color>(Colors.black),
                                    ),
                                  );
                                }
                                return const Text(
                                  'Login',
                                  style: TextStyle(
                                    fontSize: 16,
                                    fontWeight: FontWeight.w600,
                                  ),
                                );
                              },
                            ),
                          ),
                        ),
                      ),
                    ),
                    
                    const SizedBox(height: 24),
                    
                    // Or continue with - with Animation
                    FadeTransition(
                      opacity: _buttonAnimation,
                      child: const Center(
                        child: Text(
                          'Or continue with',
                          style: TextStyle(
                            fontSize: 14,
                            color: Colors.black87,
                          ),
                        ),
                      ),
                    ),
                    
                    const SizedBox(height: 16),
                    
                    // Google sign in button with Animation
                    FadeTransition(
                      opacity: _buttonAnimation,
                      child: ScaleTransition(
                        scale: Tween<double>(
                          begin: 0.8,
                          end: 1.0,
                        ).animate(
                          CurvedAnimation(
                            parent: _controller,
                            curve: const Interval(0.6, 0.9, curve: Curves.easeOutBack),
                          ),
                        ),
                        child: Center(
                          child: Container(
                            width: 50,
                            height: 50,
                            decoration: BoxDecoration(
                              color: Colors.white,
                              shape: BoxShape.circle,
                              boxShadow: [
                                BoxShadow(
                                  color: Colors.black.withOpacity(0.1),
                                  blurRadius: 10,
                                  offset: const Offset(0, 4),
                                ),
                              ],
                            ),
                            child: IconButton(
                              icon: Image.asset(
                                'assets/images/google.png',
                                width: 24,
                                height: 24,
                              ),
                              onPressed: () async {
                                final authProvider = Provider.of<AuthProvider>(context, listen: false);
                                
                                try {
                                  await authProvider.signInWithGoogle();
                                  
                                  if (context.mounted) {
                                    final user = authProvider.user;
                                    
                                    // Check if user needs to set password (first-time Google login)
                                    if (user != null && user.requiresPassword) {
                                      Navigator.of(context).pushAndRemoveUntil(
                                        MaterialPageRoute(
                                          builder: (context) => CreatePasswordScreen(
                                            userId: user.userId.toString(),
                                            email: user.email,
                                            fullName: user.fullName,
                                          ),
                                        ),
                                        (route) => false,
                                      );
                                    } else {
                                      // User already has password or returning Google user
                                      Navigator.of(context).pushAndRemoveUntil(
                                        MaterialPageRoute(
                                          builder: (context) => const HomeScreen(),
                                        ),
                                        (route) => false,
                                      );
                                    }
                                  }
                                } catch (e) {
                                  if (context.mounted) {
                                    final errorMessage = e.toString().replaceAll('Exception: ', '');
                                    
                                    // Show detailed error dialog for Google Sign In setup issues
                                    if (errorMessage.contains('Failed to get ID token') || 
                                        errorMessage.contains('Google Sign In chưa được cấu hình')) {
                                      showDialog(
                                        context: context,
                                        builder: (context) => AlertDialog(
                                          title: const Text('Google Sign In chưa sẵn sàng'),
                                          content: SingleChildScrollView(
                                            child: Column(
                                              crossAxisAlignment: CrossAxisAlignment.start,
                                              mainAxisSize: MainAxisSize.min,
                                              children: [
                                                const Text(
                                                  'Để sử dụng Google Sign In, bạn cần:\n',
                                                  style: TextStyle(fontWeight: FontWeight.bold),
                                                ),
                                                const Text('1. Tạo OAuth Client ID tại Google Cloud Console'),
                                                const Text('2. Thêm SHA-1 fingerprint'),
                                                const Text('3. Cấu hình ClientId trong backend'),
                                                const SizedBox(height: 16),
                                                const SizedBox(height: 16),
                                                const Text(
                                                  'Bạn có thể dùng đăng nhập Email/Password để test app trước.',
                                                  style: TextStyle(fontWeight: FontWeight.w500),
                                                ),
                                              ],
                                            ),
                                          ),
                                          actions: [
                                            TextButton(
                                              onPressed: () => Navigator.of(context).pop(),
                                              child: const Text('OK'),
                                            ),
                                          ],
                                        ),
                                      );
                                    } else {
                                      // Show normal snackbar for other errors
                                      ScaffoldMessenger.of(context).showSnackBar(
                                        SnackBar(
                                          content: Text(errorMessage),
                                          backgroundColor: Colors.red,
                                          duration: const Duration(seconds: 4),
                                        ),
                                      );
                                    }
                                  }
                                }
                              },
                            ),
                          ),
                        ),
                      ),
                    ),
                    
                    const SizedBox(height: 24),
                    
                    // Register link with Animation
                    FadeTransition(
                      opacity: _buttonAnimation,
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          const Text(
                            "Don't have an account?  ",
                            style: TextStyle(
                              fontSize: 14,
                              color: Colors.black87,
                            ),
                          ),
                          TextButton(
                            onPressed: () {
                              // Navigate to sign up screen
                              Navigator.push(
                                context,
                                MaterialPageRoute(
                                  builder: (context) => const SignUpScreen(),
                                ),
                              );
                            },
                            style: TextButton.styleFrom(
                              padding: EdgeInsets.zero,
                              minimumSize: const Size(0, 0),
                              tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                            ),
                            child: Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 16,
                                vertical: 6,
                              ),
                              decoration: BoxDecoration(
                                border: Border.all(color: Colors.black, width: 1.5),
                                borderRadius: BorderRadius.circular(15),
                              ),
                              child: const Text(
                                'Register',
                                style: TextStyle(
                                  fontSize: 14,
                                  color: Colors.black,
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
              ),
            ],
          ),
        ),
      ),
    );
  }
}
