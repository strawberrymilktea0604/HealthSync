import 'package:flutter/material.dart';
import 'package:healthsync_mobile/utils/app_styles.dart';
import 'package:healthsync_mobile/utils/colors.dart';

class CustomTextField extends StatelessWidget {
  final String hintText;
  final bool isPassword;
  final TextEditingController controller;

  const CustomTextField({
    Key? key,
    required this.hintText,
    this.isPassword = false,
    required this.controller,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: AppColors.textFieldBackground,
        borderRadius: BorderRadius.circular(12.0),
      ),
      child: TextField(
        controller: controller,
        obscureText: isPassword,
        style: AppStyles.bodyText,
        decoration: InputDecoration(
          hintText: hintText,
          hintStyle: AppStyles.hintText,
          border: InputBorder.none,
          contentPadding: const EdgeInsets.symmetric(horizontal: 20.0, vertical: 18.0),
          suffixIcon: isPassword
              ? Icon(Icons.visibility_off_outlined, color: AppColors.textFieldHint)
              : null,
        ),
      ),
    );
  }
}