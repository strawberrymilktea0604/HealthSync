import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import 'reset_password_screen.dart';
import 'home_screen.dart';
import 'complete_profile_screen.dart';

class EmailVerificationScreen extends StatefulWidget {
  final String email;
  final String? password;
  final bool isFromRecovery;
  
  const EmailVerificationScreen({
    super.key,
    required this.email,
    this.password,
    this.isFromRecovery = false,
  });

  @override
  State<EmailVerificationScreen> createState() => _EmailVerificationScreenState();
}

class _EmailVerificationScreenState extends State<EmailVerificationScreen> with SingleTickerProviderStateMixin {
  final List<TextEditingController> _controllers = List.generate(6, (index) => TextEditingController());
  final List<FocusNode> _focusNodes = List.generate(6, (index) => FocusNode());

  late AnimationController _controller;
  late Animation<double> _headerAnimation;
  late Animation<double> _iconAnimation;
  late Animation<double> _otpAnimation;
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

    _iconAnimation = CurvedAnimation(
      parent: _controller,
      curve: const Interval(0.2, 0.5, curve: Curves.easeOutBack),
    );

    _otpAnimation = CurvedAnimation(
      parent: _controller,
      curve: const Interval(0.3, 0.7, curve: Curves.easeOut),
    );

    _buttonAnimation = CurvedAnimation(
      parent: _controller,
      curve: const Interval(0.6, 1.0, curve: Curves.easeOut),
    );

    _controller.forward();
  }

  @override
  void dispose() {
    _controller.dispose();
    for (var controller in _controllers) {
      controller.dispose();
    }
    for (var node in _focusNodes) {
      node.dispose();
    }
    super.dispose();
  }

  void _onCodeChanged(int index, String value) {
    if (value.isNotEmpty && index < 5) {
      _focusNodes[index + 1].requestFocus();
    }
  }

  String _getVerificationCode() {
    return _controllers.map((c) => c.text).join();
  }

  String _getMaskedEmail() {
    final parts = widget.email.split('@');
    if (parts.length != 2) return widget.email;
    
    final username = parts[0];
    final domain = parts[1];
    
    if (username.length <= 2) {
      return '${username[0]}***@$domain';
    }
    
    final visibleChars = username.substring(0, 2);
    return '$visibleChars*****@$domain';
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
              // Header with Animation
              FadeTransition(
                opacity: _headerAnimation,
                child: SlideTransition(
                  position: Tween<Offset>(
                    begin: const Offset(0, -0.3),
                    end: Offset.zero,
                  ).animate(_headerAnimation),
                  child: Padding(
                    padding: EdgeInsets.symmetric(
                      horizontal: screenWidth * 0.06,
                      vertical: screenHeight * 0.03,
                    ),
                    child: const Column(
                      children: [
                        Text(
                          'Confirm your email',
                          style: TextStyle(
                            fontSize: 24,
                            fontWeight: FontWeight.bold,
                            color: Colors.black,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
              
              SizedBox(height: screenHeight * 0.02),
              
              // Form container - keep static
              Container(
                width: double.infinity,
                constraints: BoxConstraints(
                  minHeight: screenHeight * 0.7,
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
                      color: Colors.black.withValues(alpha: 0.15),
                      blurRadius: 30,
                      spreadRadius: 5,
                      offset: const Offset(0, -10),
                    ),
                  ],
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.center,
                  children: [
                    // Back button with Animation
                    FadeTransition(
                      opacity: _headerAnimation,
                      child: Align(
                        alignment: Alignment.centerLeft,
                        child: IconButton(
                          icon: const Icon(
                            Icons.arrow_back,
                            color: Colors.black87,
                            size: 28,
                          ),
                          onPressed: () {
                            Navigator.pop(context);
                          },
                        ),
                      ),
                    ),
                    
                    SizedBox(height: screenHeight * 0.02),
                    
                    // Email icon with Animation
                    FadeTransition(
                      opacity: _iconAnimation,
                      child: ScaleTransition(
                        scale: Tween<double>(
                          begin: 0.5,
                          end: 1.0,
                        ).animate(_iconAnimation),
                        child: Container(
                          width: 120,
                          height: 120,
                          padding: const EdgeInsets.all(25),
                          decoration: BoxDecoration(
                            color: Colors.white,
                            shape: BoxShape.circle,
                            boxShadow: [
                              BoxShadow(
                                color: Colors.black.withValues(alpha: 0.1),
                                blurRadius: 20,
                                offset: const Offset(0, 8),
                              ),
                            ],
                          ),
                          child: Image.asset(
                            'assets/images/email.png',
                            fit: BoxFit.contain,
                          ),
                        ),
                      ),
                    ),
                    
                    SizedBox(height: screenHeight * 0.04),
                    
                    // Description text with Animation
                    FadeTransition(
                      opacity: _otpAnimation,
                      child: Text(
                        'Please enter the verification code sent to email\n${_getMaskedEmail()}',
                        textAlign: TextAlign.center,
                        style: const TextStyle(
                          fontSize: 13,
                          color: Colors.black87,
                          height: 1.5,
                        ),
                      ),
                    ),
                    
                    SizedBox(height: screenHeight * 0.04),
                    
                    // OTP boxes with Animation
                    FadeTransition(
                      opacity: _otpAnimation,
                      child: SlideTransition(
                        position: Tween<Offset>(
                          begin: const Offset(0, 0.2),
                          end: Offset.zero,
                        ).animate(
                          CurvedAnimation(
                            parent: _controller,
                            curve: const Interval(0.35, 0.7, curve: Curves.easeOut),
                          ),
                        ),
                        child: Row(
                          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                          children: List.generate(6, (index) {
                            return Container(
                              width: 45,
                              height: 55,
                              decoration: BoxDecoration(
                                color: Colors.white.withValues(alpha: 0.5),
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(
                                  color: Colors.black.withValues(alpha: 0.1),
                                  width: 1.5,
                                ),
                              ),
                              child: TextField(
                                controller: _controllers[index],
                                focusNode: _focusNodes[index],
                                textAlign: TextAlign.center,
                                keyboardType: TextInputType.number,
                                maxLength: 1,
                                style: const TextStyle(
                                  fontSize: 24,
                                  fontWeight: FontWeight.bold,
                                  color: Colors.black87,
                                ),
                                decoration: const InputDecoration(
                                  border: InputBorder.none,
                                  counterText: '',
                                ),
                                inputFormatters: [
                                  FilteringTextInputFormatter.digitsOnly,
                                ],
                                onChanged: (value) {
                                  _onCodeChanged(index, value);
                                },
                                onTap: () {
                                  _controllers[index].selection = TextSelection.fromPosition(
                                    TextPosition(offset: _controllers[index].text.length),
                                  );
                                },
                              ),
                            );
                          }),
                        ),
                      ),
                    ),
                    
                    SizedBox(height: screenHeight * 0.05),
                    
                    // Confirm button with Animation
                    FadeTransition(
                      opacity: _buttonAnimation,
                      child: SlideTransition(
                        position: Tween<Offset>(
                          begin: const Offset(0, 0.3),
                          end: Offset.zero,
                        ).animate(
                          CurvedAnimation(
                            parent: _controller,
                            curve: const Interval(0.6, 0.9, curve: Curves.easeOut),
                          ),
                        ),
                        child: SizedBox(
                          width: double.infinity,
                          height: 50,
                          child: ElevatedButton(
                            onPressed: () async {
                              final code = _getVerificationCode();
                              if (code.length != 6) {
                                ScaffoldMessenger.of(context).showSnackBar(
                                  const SnackBar(
                                    content: Text('Please enter complete 6-digit code'),
                                    backgroundColor: Colors.red,
                                  ),
                                );
                                return;
                              }
                              
                              // Navigate based on flag
                              if (widget.isFromRecovery) {
                                final authProvider = Provider.of<AuthProvider>(context, listen: false);
                                try {
                                  final token = await authProvider.verifyResetOtp(widget.email, code);
                                  
                                  if (context.mounted) {
                                    Navigator.push(
                                      context,
                                      MaterialPageRoute(
                                        builder: (context) => ResetPasswordScreen(
                                          token: token,
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
                              } else {
                                // Register user
                                if (widget.password == null) {
                                  ScaffoldMessenger.of(context).showSnackBar(
                                    const SnackBar(
                                      content: Text('Password is missing'),
                                      backgroundColor: Colors.red,
                                    ),
                                  );
                                  return;
                                }

                                final authProvider = Provider.of<AuthProvider>(context, listen: false);
                                
                                try {
                                  await authProvider.register(
                                    email: widget.email,
                                    password: widget.password!,
                                    verificationCode: code,
                                  );
                                  
                                  if (context.mounted) {
                                    ScaffoldMessenger.of(context).showSnackBar(
                                      const SnackBar(
                                        content: Text('Registration successful!'),
                                        backgroundColor: Colors.green,
                                      ),
                                    );
                                    
                                    // Navigate based on profile completion
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
                                  'Confirm',
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
                    
                    SizedBox(height: screenHeight * 0.03),
                    
                    // Resend code with Animation
                    FadeTransition(
                      opacity: _buttonAnimation,
                      child: TextButton(
                        onPressed: () async {
                          final authProvider = Provider.of<AuthProvider>(context, listen: false);
                          
                          try {
                            await authProvider.sendVerificationCode(widget.email);
                            
                            if (context.mounted) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                const SnackBar(
                                  content: Text('Verification code resent!'),
                                  backgroundColor: Colors.green,
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
                        child: const Text(
                          'Resend code',
                          style: TextStyle(
                            fontSize: 14,
                            color: Colors.black87,
                            decoration: TextDecoration.underline,
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
    );
  }
}
