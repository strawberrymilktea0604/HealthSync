import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import 'sign_up_screen.dart';
import 'account_recovery_screen.dart';
import 'home_screen.dart';
import 'create_password_screen.dart';
import 'complete_profile_screen.dart';

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
        child: Stack(
          children: [
            // Curved background shape
            Positioned(
              top: -50,
              left: -50,
              right: -50,
              child: Container(
                height: screenHeight * 0.35,
                decoration: BoxDecoration(
                  color: const Color(0xFFFDFBD4),
                  borderRadius: BorderRadius.circular(67),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withValues(alpha: 0.35),
                      blurRadius: 16,
                      offset: const Offset(0, -16),
                    ),
                  ],
                ),
              ),
            ),
            
            SingleChildScrollView(
              physics: const BouncingScrollPhysics(),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.center,
                children: [
                  SizedBox(height: screenHeight * 0.12),
                  
                  // healthsync logo text
                  FadeTransition(
                    opacity: _headerAnimation,
                    child: const Text(
                      'healthsync',
                      style: TextStyle(
                        fontFamily: 'Eras Bold ITC',
                        fontSize: 30,
                        fontWeight: FontWeight.w400,
                        color: Colors.black,
                      ),
                    ),
                  ),
                  
                  SizedBox(height: screenHeight * 0.02),
                  
                  // Welcome back text
                  FadeTransition(
                    opacity: _headerAnimation,
                    child: const Text(
                      'Welcome back!',
                      style: TextStyle(
                        fontFamily: 'Estedad-VF',
                        fontSize: 24,
                        fontWeight: FontWeight.w700,
                        color: Colors.black,
                        height: 1.758,
                      ),
                    ),
                  ),
                  
                  SizedBox(height: screenHeight * 0.05),
                  
                  // Form fields
                  Padding(
                    padding: EdgeInsets.symmetric(horizontal: screenWidth * 0.07),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Email field with Animation
                        FadeTransition(
                          opacity: _formAnimation,
                          child: Container(
                            decoration: BoxDecoration(
                              color: const Color(0xFFD9D7B6),
                              borderRadius: BorderRadius.circular(10),
                              border: Border.all(
                                color: Colors.white.withValues(alpha: 0.3),
                                width: 3,
                              ),
                            ),
                            child: TextField(
                              controller: _emailController,
                              decoration: const InputDecoration(
                                hintText: 'Your email address',
                                hintStyle: TextStyle(
                                  fontFamily: 'Estedad-VF',
                                  fontSize: 13,
                                  fontWeight: FontWeight.w400,
                                  color: Colors.black,
                                  height: 1.758,
                                ),
                                border: InputBorder.none,
                                contentPadding: EdgeInsets.symmetric(
                                  horizontal: 8,
                                  vertical: 9,
                                ),
                              ),
                              keyboardType: TextInputType.emailAddress,
                            ),
                          ),
                        ),
                    
                        const SizedBox(height: 20),
                        
                        // Password field with Animation
                        FadeTransition(
                          opacity: _formAnimation,
                          child: Container(
                            decoration: BoxDecoration(
                              color: const Color(0xFFD9D7B6),
                              borderRadius: BorderRadius.circular(10),
                              border: Border.all(
                                color: Colors.white.withValues(alpha: 0.3),
                                width: 3,
                              ),
                            ),
                            child: TextField(
                              controller: _passwordController,
                              obscureText: !_isPasswordVisible,
                              decoration: InputDecoration(
                                hintText: 'Enter a password (min. 8 charecters)',
                                hintStyle: const TextStyle(
                                  fontFamily: 'Estedad-VF',
                                  fontSize: 13,
                                  fontWeight: FontWeight.w400,
                                  color: Colors.black,
                                  height: 1.758,
                                ),
                                border: InputBorder.none,
                                contentPadding: const EdgeInsets.symmetric(
                                  horizontal: 8,
                                  vertical: 7,
                                ),
                                suffixIcon: IconButton(
                                  icon: Icon(
                                    _isPasswordVisible
                                        ? Icons.visibility_off
                                        : Icons.visibility,
                                    color: Colors.black54,
                                    size: 22.18,
                                  ),
                                  onPressed: () {
                                    setState(() {
                                      _isPasswordVisible = !_isPasswordVisible;
                                    });
                                  },
                                ),
                              ),
                            ),
                          ),
                        ),
                    
                        const SizedBox(height: 16),
                        
                        // Forgot password with Animation
                        FadeTransition(
                          opacity: _formAnimation,
                          child: Align(
                            alignment: Alignment.center,
                            child: TextButton(
                              onPressed: () {
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
                                  fontFamily: 'Inter',
                                  fontSize: 15,
                                  fontWeight: FontWeight.w400,
                                  color: Colors.black,
                                ),
                              ),
                            ),
                          ),
                        ),
                    
                        const SizedBox(height: 20),
                        
                        // Login button with Animation
                        FadeTransition(
                          opacity: _buttonAnimation,
                          child: Center(
                            child: SizedBox(
                              width: 120,
                              height: 41,
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
                                  final user = authProvider.user!;
                                  if (user.isProfileComplete || user.role == 'Admin') {
                                    Navigator.of(context).pushAndRemoveUntil(
                                      MaterialPageRoute(
                                        builder: (context) => const HomeScreen(),
                                      ),
                                      (route) => false,
                                    );
                                  } else {
                                    Navigator.of(context).pushAndRemoveUntil(
                                      MaterialPageRoute(
                                        builder: (context) => const CompleteProfileScreen(),
                                      ),
                                      (route) => false,
                                    );
                                  }
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
                                  backgroundColor: const Color(0xFFFDFBD4),
                                  foregroundColor: Colors.black,
                                  elevation: 0,
                                  shape: RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(50),
                                    side: const BorderSide(color: Color(0xFF2C2C2C), width: 1),
                                  ),
                                  padding: const EdgeInsets.all(12),
                                ),
                                child: Consumer<AuthProvider>(
                                  builder: (context, auth, child) {
                                    if (auth.isLoading) {
                                      return const SizedBox(
                                        height: 16,
                                        width: 16,
                                        child: CircularProgressIndicator(
                                          strokeWidth: 2,
                                          valueColor: AlwaysStoppedAnimation<Color>(Colors.black),
                                        ),
                                      );
                                    }
                                    return const Text(
                                      'Login',
                                      style: TextStyle(
                                        fontFamily: 'Inter',
                                        fontSize: 16,
                                        fontWeight: FontWeight.w400,
                                        color: Colors.black,
                                      ),
                                    );
                                  },
                                ),
                              ),
                            ),
                          ),
                        ),
                    
                        const SizedBox(height: 140),
                        
                        // Or continue with - with Animation
                        FadeTransition(
                          opacity: _buttonAnimation,
                          child: const Center(
                            child: Text(
                              'Or continue with',
                              style: TextStyle(
                                fontFamily: 'Inter',
                                fontSize: 16,
                                fontWeight: FontWeight.w400,
                                color: Colors.black,
                              ),
                            ),
                          ),
                        ),
                        
                        const SizedBox(height: 20),
                        
                        // Google sign in button with Animation
                        FadeTransition(
                          opacity: _buttonAnimation,
                          child: Center(
                            child: Container(
                              width: 67,
                              height: 46,
                              decoration: BoxDecoration(
                                color: Colors.white,
                                borderRadius: BorderRadius.circular(15),
                              ),
                              child: IconButton(
                                icon: Image.asset(
                                  'assets/images/google.png',
                                  width: 30,
                                  height: 30,
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
                        
                        const SizedBox(height: 32),
                        
                        // Register link with Animation
                        FadeTransition(
                          opacity: _buttonAnimation,
                          child: Row(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              const Text(
                                "Don't have an account?",
                                style: TextStyle(
                                  fontFamily: 'Estedad-VF',
                                  fontSize: 13,
                                  fontWeight: FontWeight.w400,
                                  color: Colors.black,
                                  height: 1.758,
                                ),
                              ),
                              const SizedBox(width: 8),
                              SizedBox(
                                width: 105,
                                height: 36,
                                child: ElevatedButton(
                                  onPressed: () {
                                    Navigator.push(
                                      context,
                                      MaterialPageRoute(
                                        builder: (context) => const SignUpScreen(),
                                      ),
                                    );
                                  },
                                  style: ElevatedButton.styleFrom(
                                    backgroundColor: const Color(0xFFFDFBD4),
                                    foregroundColor: Colors.black,
                                    elevation: 0,
                                    shape: RoundedRectangleBorder(
                                      borderRadius: BorderRadius.circular(50),
                                      side: const BorderSide(color: Color(0xFF2C2C2C), width: 1),
                                    ),
                                    padding: const EdgeInsets.all(12),
                                  ),
                                  child: const Text(
                                    'Register',
                                    style: TextStyle(
                                      fontFamily: 'Inter',
                                      fontSize: 16,
                                      fontWeight: FontWeight.w400,
                                      color: Colors.black,
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
          ],
        ),
      ),
    );
  }
}
