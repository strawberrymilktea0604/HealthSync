import 'package.flutter/material.dart';
import 'package:healthsync_mobile/utils/app_styles.dart';
import 'package:healthsync_mobile/utils/colors.dart';

enum ButtonType { primary, secondary }

class CustomButton extends StatelessWidget {
  final String text;
  final VoidCallback onPressed;
  final ButtonType type;

  const CustomButton({
    Key? key,
    required this.text,
    required this.onPressed,
    this.type = ButtonType.primary,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final bool isPrimary = type == ButtonType.primary;

    return ElevatedButton(
      onPressed: onPressed,
      style: ElevatedButton.styleFrom(
        backgroundColor: isPrimary ? AppColors.buttonPrimary : Colors.transparent,
        foregroundColor: isPrimary ? Colors.white : AppColors.buttonSecondaryText,
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(30.0),
          side: isPrimary ? BorderSide.none : const BorderSide(color: AppColors.buttonBorder, width: 1.5),
        ),
        padding: const EdgeInsets.symmetric(vertical: 18.0),
      ),
      child: Text(
        text,
        style: isPrimary ? AppStyles.buttonTextPrimary : AppStyles.buttonTextSecondary,
      ),
    );
  }
}