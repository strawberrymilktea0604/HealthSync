import 'package:flutter/material.dart';
import 'colors.dart';

class AppStyles {
  static const TextStyle healthsyncTitle = TextStyle(
    fontFamily: 'ErasBoldITC',
    fontSize: 38.0,
    fontWeight: FontWeight.bold,
    color: AppColors.primaryText,
  );

  static const TextStyle pageTitle = TextStyle(
    fontFamily: 'EstedadVF',
    fontSize: 32.0,
    fontWeight: FontWeight.bold,
    color: AppColors.primaryText,
  );
  
  static const TextStyle bodyText = TextStyle(
    fontSize: 16.0,
    color: AppColors.primaryText,
  );

  static const TextStyle hintText = TextStyle(
    fontSize: 16.0,
    color: AppColors.textFieldHint,
  );

  static const TextStyle buttonTextPrimary = TextStyle(
    fontSize: 18.0,
    fontWeight: FontWeight.bold,
    color: Colors.white,
  );

  static const TextStyle buttonTextSecondary = TextStyle(
    fontSize: 18.0,
    fontWeight: FontWeight.bold,
    color: AppColors.buttonSecondaryText,
  );
}