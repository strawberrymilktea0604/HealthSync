import 'package:flutter/material.dart';
import 'package:healthsync_mobile/utils/app_styles.dart';
import 'package:healthsync_mobile/utils/colors.dart';

class CongratulationsScreen extends StatelessWidget {
  const CongratulationsScreen({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 24.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.center,
            mainAxisAlignment: MainAxisAlignment.start,
            children: [
              const SizedBox(height: 60),
              const Text("healthsync", style: AppStyles.healthsyncTitle),
              const SizedBox(height: 60),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 30.0, vertical: 60.0),
                decoration: BoxDecoration(
                  color: AppColors.cardBackground,
                  borderRadius: BorderRadius.circular(30.0),
                ),
                child: Column(
                  children: [
                    const Text(
                      "Congratulations!",
                      style: AppStyles.pageTitle,
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 40),
                    Image.asset('assets/icons/done.jpg', height: 80),
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