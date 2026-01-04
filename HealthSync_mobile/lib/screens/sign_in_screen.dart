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
  late Animation<Offset> _slideAnimation;

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
            // Top Section: Welcome Text
            Expanded(
              flex: 3,
              child: Center(
                child: FadeTransition(
                  opacity: _headerAnimation,
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      const Text(
                        'Welcome back!',
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
                              hintText: 'Enter a password (min. 8 charecters)',
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
                  
                      const SizedBox(height: 16),
                      
                      // Forgot password
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
                                fontSize: 14,
                                fontWeight: FontWeight.w500,
                                color: Colors.black,
                              ),
                            ),
                          ),
                        ),
                      ),
                  
                      const SizedBox(height: 30),
                      
                      // Login button
                      FadeTransition(
                        opacity: _buttonAnimation,
                        child: SizedBox(
                          width: 200,
                          height: 50,
                          child: OutlinedButton(
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
                            child: const Text(
                              'Login',
                              style: TextStyle(
                                fontFamily: 'Inter',
                                fontSize: 16,
                                fontWeight: FontWeight.w600,
                                color: Colors.black,
                              ),
                            ),
                          ),
                        ),
                      ),

                      const SizedBox(height: 40),

                      // Or continue with
                      FadeTransition(
                        opacity: _buttonAnimation,
                        child: Column(
                          children: [
                            const Text(
                              'Or continue with',
                              style: TextStyle(
                                fontFamily: 'Inter',
                                fontSize: 14,
                                color: Colors.black87,
                              ),
                            ),
                            const SizedBox(height: 20),
                            GestureDetector(
                              onTap: () async {
                                final authProvider = Provider.of<AuthProvider>(context, listen: false);
                                try {
                                  await authProvider.signInWithGoogle();
                                  if (context.mounted) {
                                    final user = authProvider.user!;
                                    
                                    if (user.requiresPassword) {
                                      Navigator.of(context).push(
                                        MaterialPageRoute(
                                          builder: (context) => CreatePasswordScreen(
                                            userId: user.userId.toString(),
                                            email: user.email,
                                            fullName: user.fullName,
                                          ),
                                        ),
                                      );
                                    } else if (user.isProfileComplete) {
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
                                        content: Text(e.toString()),
                                        backgroundColor: Colors.red,
                                      ),
                                    );
                                  }
                                }
                              },
                              child: Container(
                                padding: const EdgeInsets.all(10),
                                decoration: BoxDecoration(
                                  color: Colors.white,
                                  borderRadius: BorderRadius.circular(12),
                                  boxShadow: [
                                    BoxShadow(
                                      color: Colors.black.withOpacity(0.05),
                                      blurRadius: 10,
                                      offset: const Offset(0, 2),
                                    ),
                                  ],
                                ),
                                child: Image.asset(
                                  'assets/images/google.png',
                                  height: 24,
                                  width: 24,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),

                      const SizedBox(height: 40),

                      // Register link
                      FadeTransition(
                        opacity: _buttonAnimation,
                        child: Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const Text(
                              "Don't have an account? ",
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
                                    builder: (context) => const SignUpScreen(),
                                  ),
                                );
                              },
                              child: const Text(
                                'Register',
                                style: TextStyle(
                                  fontFamily: 'Inter',
                                  fontSize: 14,
                                  fontWeight: FontWeight.bold,
                                  color: Colors.black,
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
