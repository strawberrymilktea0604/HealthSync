import 'package:flutter/material.dart';
import 'package:healthsync_mobile/utils/app_styles.dart';
import 'package:healthsync_mobile/utils/colors.dart';
import 'package:healthsync_mobile/widgets/custom_button.dart';
import 'package:healthsync_mobile/widgets/custom_textfield.dart';

class AccountRecoveryScreen extends StatefulWidget {
  const AccountRecoveryScreen({Key? key}) : super(key: key);

  @override
  State<AccountRecoveryScreen> createState() => _AccountRecoveryScreenState();
}

class _AccountRecoveryScreenState extends State<AccountRecoveryScreen> {
  final _emailController = TextEditingController();

  @override
  void dispose() {
    _emailController.dispose();
    super.dispose();
  }

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
              const Text("Account recovery", style: AppStyles.pageTitle, textAlign: TextAlign.center),
              const SizedBox(height: 60),
              Container(
                padding: const EdgeInsets.all(30.0),
                decoration: BoxDecoration(
                  color: AppColors.cardBackground,
                  borderRadius: BorderRadius.circular(30.0),
                ),
                child: Column(
                  children: [
                    Image.asset('assets/icons/recovery.jpg', height: 100),
                    const SizedBox(height: 30),
                    const Text(
                      "Enter email to send password recovery request",
                      style: AppStyles.bodyText,
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 30),
                    CustomTextField(
                      hintText: "Your email address",
                      controller: _emailController,
                    ),
                    const SizedBox(height: 40),
                    CustomButton(
                      text: "Send request",
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
}