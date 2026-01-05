import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../screens/sign_in_screen.dart';

final GlobalKey<NavigatorState> navigatorKey = GlobalKey<NavigatorState>();

Future<void> handleAuthError() async {
  final prefs = await SharedPreferences.getInstance();
  await prefs.remove('user');
  await prefs.remove('token');
  
  if (navigatorKey.currentContext != null) {
    // Xóa hết stack và đẩy về màn hình đăng nhập
    navigatorKey.currentState?.pushAndRemoveUntil(
      MaterialPageRoute(builder: (context) => const SignInScreen()),
      (route) => false,
    );
    
    ScaffoldMessenger.of(navigatorKey.currentContext!).showSnackBar(
      const SnackBar(
        content: Text('Phiên đăng nhập hết hạn hoặc tài khoản bị khóa. Vui lòng đăng nhập lại.'),
        backgroundColor: Colors.red,
        duration: Duration(seconds: 3),
      ),
    );
  }
}
