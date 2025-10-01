import 'package:flutter/material.dart';
import 'package:healthsync_mobile/utils/app_styles.dart';
import 'package:healthsync_mobile/utils/colors.dart';
import 'package:healthsync_mobile/widgets/custom_button.dart';
import 'package:healthsync_mobile/widgets/custom_textfield.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({Key? key}) : super(key: key);

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
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
                const Text("Welcome back!", style: AppStyles.pageTitle),
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
                        hintText: "Enter a password (min. 8 characters)",
                        isPassword: true,
                        controller: _passwordController,
                      ),
                      const SizedBox(height: 20),
                      Align(
                        alignment: Alignment.centerRight,
                        child: TextButton(
                          onPressed: () {
                            // TODO: Navigate to Forgot Password
                          },
                          child: const Text(
                            "Forgot password?",
                            style: AppStyles.bodyText,
                          ),
                        ),
                      ),
                      const SizedBox(height: 20),
                      CustomButton(
                        text: "Login",
                        onPressed: () {},
                        type: ButtonType.secondary,
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 30),
                const Center(
                  child: Text(
                    "Or continue with",
                    style: AppStyles.bodyText,
                  ),
                ),
                const SizedBox(height: 20),
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Image.asset('assets/icons/google.jpg', height: 50),
                    const SizedBox(width: 20),
                    Image.asset('assets/icons/facebook.jpg', height: 50),
                  ],
                ),
                const SizedBox(height: 30),
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Text("Don't have an account?", style: AppStyles.bodyText),
                    TextButton(
                      onPressed: () {
                        // TODO: Navigate to Register
                      },
                      child: const Text(
                        "Register",
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