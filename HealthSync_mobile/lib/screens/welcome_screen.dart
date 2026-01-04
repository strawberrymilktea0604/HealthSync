import 'package:flutter/material.dart';
import 'sign_in_screen.dart';
import 'sign_up_screen.dart';

// Custom clipper để tạo góc cong ở dưới phải
class CornerClipPath extends CustomClipper<Path> {
  @override
  Path getClip(Size size) {
    final path = Path();

    // Tạo hình tròn chính
    final center = Offset(size.width / 2, size.height / 2);
    final radius = size.width / 2;
    path.addOval(Rect.fromCircle(center: center, radius: radius));

    // Cắt một phần ở dưới (tạo hiệu ứng vòng tròn bị cắt nhẹ)
    final cutPath = Path()
      ..moveTo(0, size.height * 0.85)
      ..quadraticBezierTo(
        size.width * 0.5,
        size.height,
        size.width,
        size.height * 0.85,
      )
      ..lineTo(size.width, size.height)
      ..lineTo(0, size.height)
      ..close();

    // Trừ phần dưới ra khỏi hình tròn
    return Path.combine(PathOperation.difference, path, cutPath);
  }

  @override
  bool shouldReclip(CustomClipper<Path> oldClipper) => false;
}


class WelcomeScreen extends StatefulWidget {
  const WelcomeScreen({super.key});

  @override
  State<WelcomeScreen> createState() => _WelcomeScreenState();
}

class _WelcomeScreenState extends State<WelcomeScreen> with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _imageAnimation;
  late Animation<double> _textAnimation;
  late Animation<double> _buttonAnimation;

  @override
  void initState() {
    super.initState();
    
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1500),
    );

    _imageAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(
        parent: _controller,
        curve: const Interval(0.0, 0.4, curve: Curves.easeOut),
      ),
    );

    _textAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(
        parent: _controller,
        curve: const Interval(0.3, 0.7, curve: Curves.easeOut),
      ),
    );

    _buttonAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(
        parent: _controller,
        curve: const Interval(0.6, 1.0, curve: Curves.easeOut),
      ),
    );

    _controller.forward();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFD9D7B6),
      body: SafeArea(
        child: SingleChildScrollView(
          physics: const BouncingScrollPhysics(), // Scroll mượt mà với hiệu ứng bounce
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 24.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const SizedBox(height: 20),
                
                // Hình minh họa gym với background tròn lớn + Animation
                Center(
                  child: FadeTransition(
                    opacity: _imageAnimation,
                    child: ScaleTransition(
                      scale: Tween<double>(begin: 0.8, end: 1.0).animate(
                        CurvedAnimation(
                          parent: _controller,
                          curve: const Interval(0.0, 0.4, curve: Curves.easeOutBack),
                        ),
                      ),
                      child: Container(
                        width: 400,
                        height: 400,
                        decoration: BoxDecoration(
                          color: const Color(0xFFFDFBD4), // Màu nền vàng nhạt
                          shape: BoxShape.circle,
                          boxShadow: [
                            BoxShadow(
                              color: Colors.black.withValues(alpha: 0.2),
                              blurRadius: 40,
                              spreadRadius: 5,
                              offset: const Offset(0, 20),
                            ),
                          ],
                        ),
                        child: ClipPath(
                          clipper: CornerClipPath(),
                          child: Container(
                            decoration: const BoxDecoration(
                              color: Color(0xFFFDFBD4),
                              shape: BoxShape.circle,
                            ),
                            padding: const EdgeInsets.all(50),
                            child: Image.asset(
                              'assets/images/gym.png',
                              fit: BoxFit.contain,
                            ),
                          ),
                        ),
                      ),
                    ),
                  ),
                ),
                
                const SizedBox(height: 30),
                
                // Tiêu đề với logo + Animation
                FadeTransition(
                  opacity: _textAnimation,
                  child: SlideTransition(
                    position: Tween<Offset>(
                      begin: const Offset(0, 0.3),
                      end: Offset.zero,
                    ).animate(
                      CurvedAnimation(
                        parent: _controller,
                        curve: const Interval(0.3, 0.7, curve: Curves.easeOut),
                      ),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'Welcome to',
                          style: TextStyle(
                            fontSize: 32,
                            fontWeight: FontWeight.bold,
                            color: Colors.black,
                            height: 1.2,
                          ),
                        ),
                        const SizedBox(height: 8),
                        Image.asset(
                          'assets/images/logo.png',
                          width: 200,
                          height: 60,
                          fit: BoxFit.contain,
                        ),
                      ],
                    ),
                  ),
                ),
                
                const SizedBox(height: 20),
                
                // Mô tả + Animation
                FadeTransition(
                  opacity: _textAnimation,
                  child: const Text(
                    'Excited to share this clean and modern wireframe for a fitness coaching website! This design focuses on a user-friendly layout with a clear information hierarchy, guiding potential clients through the coach\'s offerings seamlessly. The homepage highlights key services, showcases services, and encourages action with strategically placed CTAs.',
                    style: TextStyle(
                      fontSize: 14,
                      color: Colors.black87,
                      height: 1.5,
                    ),
                  ),
                ),
                
                const SizedBox(height: 24),
                
                // Nút Sign in + Animation
                FadeTransition(
                  opacity: _buttonAnimation,
                  child: Center(
                    child: SizedBox(
                      width: 174,
                      height: 41,
                      child: ElevatedButton(
                        onPressed: () {
                          Navigator.push(
                            context,
                            MaterialPageRoute(
                              builder: (context) => const SignInScreen(),
                            ),
                          );
                        },
                        style: ElevatedButton.styleFrom(
                          backgroundColor: const Color(0xFF605F56),
                          foregroundColor: const Color(0xFFD9D7B6),
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
                          ),
                        ),
                      ),
                    ),
                  ),
                ),
                
                const SizedBox(height: 16),
                
                // Nút Sign up + Animation
                FadeTransition(
                  opacity: _buttonAnimation,
                  child: Center(
                    child: SizedBox(
                      width: 174,
                      height: 41,
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
                          backgroundColor: const Color(0xFFD9D7B6),
                          foregroundColor: Colors.black,
                          elevation: 0,
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(50),
                            side: const BorderSide(color: Color(0xFF2C2C2C), width: 1),
                          ),
                          padding: const EdgeInsets.all(12),
                        ),
                        child: const Text(
                          'Sign up',
                          style: TextStyle(
                            fontFamily: 'Inter',
                            fontSize: 16,
                            fontWeight: FontWeight.w400,
                            color: Colors.black,
                          ),
                        ),
                      ),
                    ),
                  ),
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