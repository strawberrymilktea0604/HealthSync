import 'package:flutter/material.dart';
import 'package:healthsync_mobile/screens/welcome_screen.dart';
import 'package:healthsync_mobile/utils/colors.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'HealthSync',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        scaffoldBackgroundColor: AppColors.background,
        fontFamily: 'DefaultFont', // You can set a default font here if needed
        colorScheme: ColorScheme.fromSeed(seedColor: AppColors.buttonPrimary),
        useMaterial3: true,
      ),
      home: const WelcomeScreen(),
    );
  }
}
