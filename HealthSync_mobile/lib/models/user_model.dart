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
    return User(
      userId: json['userId'] ?? json['UserId'],
      email: json['email'] ?? json['Email'],
      fullName: json['fullName'] ?? json['FullName'],
      role: json['role'] ?? json['Role'],
      token: json['token'] ?? json['Token'],
      expiresAt: DateTime.parse(json['expiresAt'] ?? json['ExpiresAt']),
      requiresPassword: json['requiresPassword'] ?? json['RequiresPassword'] ?? false,
      isProfileComplete: json['isProfileComplete'] ?? json['IsProfileComplete'] ?? false,
      avatarUrl: json['avatarUrl'] ?? json['AvatarUrl'],
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
