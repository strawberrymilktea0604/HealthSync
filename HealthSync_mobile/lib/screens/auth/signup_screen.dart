import 'package:flutter/material.dart';
import 'package:healthsync_mobile/utils/app_styles.dart';
import 'package:healthsync_mobile/utils/colors.dart';
import 'package:healthsync_mobile/widgets/custom_button.dart';
import 'package:healthsync_mobile/widgets/custom_textfield.dart';

class SignUpScreen extends StatefulWidget {
  const SignUpScreen({Key? key}) : super(key: key);

  @override
  State<SignUpScreen> createState() => _SignUpScreenState();
}

class _SignUpScreenState extends State<SignUpScreen> {
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: SingleChildScrollView(
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 40.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Text("Create an account", style: AppStyles.pageTitle),
                const Text("healthsync", style: AppStyles.healthsyncTitle),
                const SizedBox(height: 40),
                Container(
                  padding: const EdgeInsets.all(24.0),
                  decoration: BoxDecoration(
                    color: AppColors.cardBackground,
                    borderRadius: BorderRadius.circular(30.0),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      CustomTextField(
                        hintText: "Your email address",
                        controller: _emailController,
                      ),
                      const SizedBox(height: 20),
                      CustomTextField(
                        hintText: "Choose a password (min. 8 characters)",
                        isPassword: true,
                        controller: _passwordController,
                      ),
                      const SizedBox(height: 20),
                      CustomTextField(
                        hintText: "Confirm your password",
                        isPassword: true,
                        controller: _confirmPasswordController,
                      ),
                      const SizedBox(height: 40),
                      CustomButton(
                        text: "Sign up",
                        onPressed: () {},
                        type: ButtonType.secondary,
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 30),
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Text("Do you have an account?", style: AppStyles.bodyText),
                    TextButton(
                      onPressed: () {
                        // TODO: Navigate to Sign In
                      },
                      child: const Text(
                        "Sign in",
                        style: AppStyles.bodyText,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}