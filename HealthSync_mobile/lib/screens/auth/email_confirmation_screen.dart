import 'package:flutter/material.dart';
import 'package:healthsync_mobile/utils/app_styles.dart';
import 'package:healthsync_mobile/utils/colors.dart';
import 'package:healthsync_mobile/widgets/custom_button.dart';

class EmailConfirmationScreen extends StatelessWidget {
  const EmailConfirmationScreen({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Colors.black),
          onPressed: () => Navigator.of(context).pop(),
        ),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 24.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const SizedBox(height: 20),
              const Text("Confirm your email", style: AppStyles.pageTitle, textAlign: TextAlign.center),
              const SizedBox(height: 60),
              Container(
                padding: const EdgeInsets.all(30.0),
                decoration: BoxDecoration(
                  color: AppColors.cardBackground,
                  borderRadius: BorderRadius.circular(30.0),
                ),
                child: Column(
                  children: [
                    Image.asset('assets/icons/email.jpg', height: 80),
                    const SizedBox(height: 30),
                    const Text(
                      "Please enter the verification code sent to email",
                      style: AppStyles.bodyText,
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 10),
                    const Text(
                      "lm**********@gmail.com",
                      style: AppStyles.bodyText,
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 30),
                    // OTP Input Fields
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                      children: List.generate(6, (index) => _buildOtpBox()),
                    ),
                    const SizedBox(height: 40),
                    CustomButton(
                      text: "Confirm",
                      onPressed: () {},
                      type: ButtonType.secondary,
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

  Widget _buildOtpBox() {
    return Container(
      width: 45,
      height: 50,
      decoration: BoxDecoration(
        color: AppColors.textFieldBackground,
        borderRadius: BorderRadius.circular(12.0),
        border: Border.all(color: Colors.grey[400]!),
      ),
      child: const Center(
        child: TextField(
          textAlign: TextAlign.center,
          keyboardType: TextInputType.number,
          maxLength: 1,
          decoration: InputDecoration(
            border: InputBorder.none,
            counterText: '',
          ),
          style: TextStyle(fontSize: 24),
        ),
      ),
    );
  }
}