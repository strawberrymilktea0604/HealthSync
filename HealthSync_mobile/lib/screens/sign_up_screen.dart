import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import 'email_verification_screen.dart';
import 'sign_in_screen.dart';

class SignUpScreen extends StatefulWidget {
  const SignUpScreen({super.key});

  @override
  State<SignUpScreen> createState() => _SignUpScreenState();
}

class _SignUpScreenState extends State<SignUpScreen> with SingleTickerProviderStateMixin {
  final TextEditingController _emailController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();
  final TextEditingController _confirmPasswordController = TextEditingController();
  bool _isPasswordVisible = false;
  bool _isConfirmPasswordVisible = false;

  late AnimationController _controller;
  late Animation<double> _headerAnimation;
  late Animation<double> _formAnimation;
  late Animation<double> _buttonAnimation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(milliseconds: 1200),
      vsync: this,
    );

    _headerAnimation = CurvedAnimation(
      parent: _controller,
      curve: const Interval(0.0, 0.3, curve: Curves.easeOut),
    );

    _formAnimation = CurvedAnimation(
      parent: _controller,
      curve: const Interval(0.2, 0.6, curve: Curves.easeOut),
    );

    _buttonAnimation = CurvedAnimation(
      parent: _controller,
      curve: const Interval(0.5, 1.0, curve: Curves.easeOut),
    );

    _controller.forward();
  }

  @override
  void dispose() {
    _controller.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
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
                  
                  // Create an account text
                  FadeTransition(
                    opacity: _headerAnimation,
                    child: const Text(
                      'Create an account',
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
                                hintText: 'Choose  a password (min. 8 charecters)',
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
                    
                        const SizedBox(height: 20),
                        
                        // Confirm Password field with Animation
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
                              controller: _confirmPasswordController,
                              obscureText: !_isConfirmPasswordVisible,
                              decoration: InputDecoration(
                                hintText: 'Confirm your password',
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
                                    _isConfirmPasswordVisible
                                        ? Icons.visibility_off
                                        : Icons.visibility,
                                    color: Colors.black54,
                                    size: 22.18,
                                  ),
                                  onPressed: () {
                                    setState(() {
                                      _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
                                    });
                                  },
                                ),
                              ),
                            ),
                          ),
                        ),
                        
                        const SizedBox(height: 24),
                        
                        // Sign up button with Animation
                        FadeTransition(
                          opacity: _buttonAnimation,
                          child: Center(
                            child: SizedBox(
                              width: 120,
                              height: 41,
                              child: ElevatedButton(
                            onPressed: () async {
                              // Validate
                              final email = _emailController.text.trim();
                              final password = _passwordController.text;
                              final confirmPassword = _confirmPasswordController.text;

                              if (email.isEmpty) {
                                ScaffoldMessenger.of(context).showSnackBar(
                                  const SnackBar(
                                    content: Text('Please enter your email'),
                                    backgroundColor: Colors.red,
                                  ),
                                );
                                return;
                              }
                              
                              if (password != confirmPassword) {
                                ScaffoldMessenger.of(context).showSnackBar(
                                  const SnackBar(
                                    content: Text('Passwords do not match'),
                                    backgroundColor: Colors.red,
                                  ),
                                );
                                return;
                              }
                              
                              if (password.length < 8) {
                                ScaffoldMessenger.of(context).showSnackBar(
                                  const SnackBar(
                                    content: Text('Password must be at least 8 characters'),
                                    backgroundColor: Colors.red,
                                  ),
                                );
                                return;
                              }
                              
                              // Send verification code
                              final authProvider = Provider.of<AuthProvider>(context, listen: false);
                              
                              try {
                                await authProvider.sendVerificationCode(email);
                                
                                if (context.mounted) {
                                  ScaffoldMessenger.of(context).showSnackBar(
                                    const SnackBar(
                                      content: Text('Verification code sent to your email!'),
                                      backgroundColor: Colors.green,
                                    ),
                                  );
                                  
                                  // Navigate to email verification
                                  Navigator.push(
                                    context,
                                    MaterialPageRoute(
                                      builder: (context) => EmailVerificationScreen(
                                        email: email,
                                        password: password,
                                      ),
                                    ),
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
                                      'Sign up',
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
                        
                        const SizedBox(height: 120),
                        
                        // Sign in link with Animation
                        FadeTransition(
                          opacity: _buttonAnimation,
                          child: Row(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              const Text(
                                "Do you have an account?",
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
                                    Navigator.pushReplacement(
                                      context,
                                      MaterialPageRoute(
                                        builder: (context) => const SignInScreen(),
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
                                    'Sign in',
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
