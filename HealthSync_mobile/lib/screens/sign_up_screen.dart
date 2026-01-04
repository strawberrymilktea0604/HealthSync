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
  late Animation<Offset> _slideAnimation;

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

    _slideAnimation = Tween<Offset>(
      begin: const Offset(0, 1),
      end: Offset.zero,
    ).animate(CurvedAnimation(
      parent: _controller,
      curve: const Interval(0.1, 0.6, curve: Curves.easeOutCubic),
    ));

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
    
    return Scaffold(
      backgroundColor: const Color(0xFFD9D7B6),
      body: SafeArea(
        child: Column(
          children: [
            // Top Section: Create Account Text
            Expanded(
              flex: 3,
              child: Center(
                child: FadeTransition(
                  opacity: _headerAnimation,
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      const Text(
                        'Create an account',
                        style: TextStyle(
                          fontFamily: 'Estedad-VF',
                          fontSize: 24,
                          fontWeight: FontWeight.w700,
                          color: Colors.black,
                        ),
                      ),
                      const SizedBox(height: 16),
                      Image.asset(
                        'assets/images/logo.png',
                        height: 20,
                        fit: BoxFit.contain,
                      ),
                    ],
                  ),
                ),
              ),
            ),

            // Bottom Section: Form Card
            Expanded(
              flex: 7,
              child: SlideTransition(
                position: _slideAnimation,
                child: Container(
                  width: double.infinity,
                  decoration: const BoxDecoration(
                    color: Color(0xFFFDFBD4),
                    borderRadius: BorderRadius.only(
                      topLeft: Radius.circular(40),
                      topRight: Radius.circular(40),
                    ),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black12,
                        blurRadius: 10,
                        offset: Offset(0, -5),
                      ),
                    ],
                  ),
                  child: SingleChildScrollView(
                  physics: const BouncingScrollPhysics(),
                  padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 40),
                  child: Column(
                    children: [
                      // Email field
                      FadeTransition(
                        opacity: _formAnimation,
                        child: Container(
                          decoration: BoxDecoration(
                            color: const Color(0xFFD9D7B6),
                            borderRadius: BorderRadius.circular(15),
                          ),
                          child: TextField(
                            controller: _emailController,
                            decoration: const InputDecoration(
                              hintText: 'Your email address',
                              hintStyle: TextStyle(
                                fontFamily: 'Estedad-VF',
                                fontSize: 14,
                                color: Colors.black54,
                              ),
                              border: InputBorder.none,
                              contentPadding: EdgeInsets.symmetric(
                                horizontal: 20,
                                vertical: 16,
                              ),
                            ),
                            keyboardType: TextInputType.emailAddress,
                          ),
                        ),
                      ),
                  
                      const SizedBox(height: 20),
                      
                      // Password field
                      FadeTransition(
                        opacity: _formAnimation,
                        child: Container(
                          decoration: BoxDecoration(
                            color: const Color(0xFFD9D7B6),
                            borderRadius: BorderRadius.circular(15),
                          ),
                          child: TextField(
                            controller: _passwordController,
                            obscureText: !_isPasswordVisible,
                            decoration: InputDecoration(
                              hintText: 'Choose a password (min. 8 charecters)',
                              hintStyle: const TextStyle(
                                fontFamily: 'Estedad-VF',
                                fontSize: 14,
                                color: Colors.black54,
                              ),
                              border: InputBorder.none,
                              contentPadding: const EdgeInsets.symmetric(
                                horizontal: 20,
                                vertical: 16,
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
                        ),
                      ),

                      const SizedBox(height: 20),

                      // Confirm Password field
                      FadeTransition(
                        opacity: _formAnimation,
                        child: Container(
                          decoration: BoxDecoration(
                            color: const Color(0xFFD9D7B6),
                            borderRadius: BorderRadius.circular(15),
                          ),
                          child: TextField(
                            controller: _confirmPasswordController,
                            obscureText: !_isConfirmPasswordVisible,
                            decoration: InputDecoration(
                              hintText: 'Confirm your password',
                              hintStyle: const TextStyle(
                                fontFamily: 'Estedad-VF',
                                fontSize: 14,
                                color: Colors.black54,
                              ),
                              border: InputBorder.none,
                              contentPadding: const EdgeInsets.symmetric(
                                horizontal: 20,
                                vertical: 16,
                              ),
                              suffixIcon: IconButton(
                                icon: Icon(
                                  _isConfirmPasswordVisible
                                      ? Icons.visibility_off
                                      : Icons.visibility,
                                  color: Colors.black54,
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
                  
                      const SizedBox(height: 40),
                      
                      // Sign Up button
                      FadeTransition(
                        opacity: _buttonAnimation,
                        child: SizedBox(
                          width: 200,
                          height: 50,
                          child: OutlinedButton(
                            onPressed: () async {
                              final email = _emailController.text.trim();
                              final password = _passwordController.text;
                              final confirmPassword = _confirmPasswordController.text;

                              if (email.isEmpty || password.isEmpty || confirmPassword.isEmpty) {
                                ScaffoldMessenger.of(context).showSnackBar(
                                  const SnackBar(
                                    content: Text('Please fill in all fields'),
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

                              final authProvider = Provider.of<AuthProvider>(context, listen: false);
                              
                              try {
                                await authProvider.sendVerificationCode(email);
                                
                                if (context.mounted) {
                                  Navigator.pushReplacement(
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
                                      content: Text(e.toString()),
                                      backgroundColor: Colors.red,
                                    ),
                                  );
                                }
                              }
                            },
                            style: OutlinedButton.styleFrom(
                              side: const BorderSide(color: Colors.black, width: 1),
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(30),
                              ),
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
                                    fontWeight: FontWeight.w600,
                                    color: Colors.black,
                                  ),
                                );
                              },
                            ),
                          ),
                        ),
                      ),

                      const SizedBox(height: 100),

                      // Sign In link
                      FadeTransition(
                        opacity: _buttonAnimation,
                        child: Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const Text(
                              "Do you have an account? ",
                              style: TextStyle(
                                fontFamily: 'Inter',
                                fontSize: 14,
                                color: Colors.black87,
                              ),
                            ),
                            GestureDetector(
                              onTap: () {
                                Navigator.push(
                                  context,
                                  MaterialPageRoute(
                                    builder: (context) => const SignInScreen(),
                                  ),
                                );
                              },
                              child: Container(
                                padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
                                decoration: BoxDecoration(
                                  border: Border.all(color: Colors.black),
                                  borderRadius: BorderRadius.circular(30),
                                ),
                                child: const Text(
                                  'Sign in',
                                  style: TextStyle(
                                    fontFamily: 'Inter',
                                    fontSize: 14,
                                    fontWeight: FontWeight.w600,
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
              ),
            ),
          ),
          ],
        ),
      ),
    );
  }
}
