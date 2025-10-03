import 'package:flutter/material.dart';
import 'package:healthsync_mobile/utils/app_styles.dart';
import 'package:healthsync_mobile/utils/colors.dart';
import 'package:healthsync_mobile/widgets/custom_button.dart';

class WelcomeScreen extends StatelessWidget {
  const WelcomeScreen({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          children: [
            // Illustration Section
            Expanded(
              flex: 5,
              child: Container(
                width: double.infinity,
                decoration: const BoxDecoration(
                  color: Color(0xFFF9F7E8), // Slightly different from card bg
                  borderRadius: BorderRadius.only(
                    bottomLeft: Radius.circular(60),
                    bottomRight: Radius.circular(60),
                  ),
                ),
                child: Padding(
                  padding: const EdgeInsets.only(top: 40.0, bottom: 20.0),
                  child: Image.asset(
                    'assets/images/gym.jpg', // Please add this image
                    fit: BoxFit.contain,
                  ),
                ),
              ),
            ),
            // Text and Button Section
            Expanded(
              flex: 4,
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 24.0),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    const Text("Welcome to", style: AppStyles.pageTitle),
                    const Text("healthsync", style: AppStyles.healthsyncTitle),
                    const SizedBox(height: 16),
                    Text(
                      "Excited to share this clean and modern wireframe for a fitness coaching website!",
                      style: AppStyles.bodyText.copyWith(color: Colors.grey[700]),
                    ),
                    const Spacer(),
                    CustomButton(
                      text: "Sign in",
                      onPressed: () {
                        // TODO: Navigate to Login Screen
                      },
                      type: ButtonType.primary,
                    ),
                    const SizedBox(height: 16),
                    CustomButton(
                      text: "Sign up",
                      onPressed: () {
                        // TODO: Navigate to Sign Up Screen
                      },
                      type: ButtonType.secondary,
                    ),
                    const SizedBox(height: 20),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}