class User {
  final int userId;
  final String email;
  final String fullName;
  final String role;
  final String token;
  final DateTime expiresAt;
  final bool requiresPassword; // True if user needs to set password (first-time Google login)
  final bool isProfileComplete; // True if user profile is fully filled
  final String? avatarUrl;

  User({
    required this.userId,
    required this.email,
    required this.fullName,
    required this.role,
    required this.token,
    required this.expiresAt,
    this.requiresPassword = false,
    this.isProfileComplete = false,
    this.avatarUrl,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    String? avatar = json['avatarUrl'] ?? json['AvatarUrl'];
    // FIX: Android Emulator cannot access localhost directly. 
    // If running on Android and URL contains localhost, replace with 10.0.2.2
    // Since we don't have dart:io here, we'll do a string replacement that catches common localhost variants.
    if (avatar != null && avatar.contains('localhost')) {
        avatar = avatar.replaceFirst('localhost', '10.0.2.2');
    }
    
    return User(
      userId: json['userId'] ?? json['UserId'],
      email: json['email'] ?? json['Email'],
      fullName: json['fullName'] ?? json['FullName'],
      role: json['role'] ?? json['Role'],
      token: json['token'] ?? json['Token'],
      expiresAt: DateTime.parse(json['expiresAt'] ?? json['ExpiresAt']),
      requiresPassword: json['requiresPassword'] ?? json['RequiresPassword'] ?? false,
      isProfileComplete: json['isProfileComplete'] ?? json['IsProfileComplete'] ?? false,
      avatarUrl: avatar,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'userId': userId,
      'email': email,
      'fullName': fullName,
      'role': role,
      'token': token,
      'expiresAt': expiresAt.toIso8601String(),
      'requiresPassword': requiresPassword,
      'isProfileComplete': isProfileComplete,
      'avatarUrl': avatarUrl,
    };
  }
}
